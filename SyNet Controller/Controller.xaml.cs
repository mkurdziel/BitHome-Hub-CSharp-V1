namespace SyNet
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.IO;
  using System.IO.Ports;
  using System.Threading;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Controls.Primitives;
  using System.Windows.Data;
  using System.Windows.Forms;
  using System.Xml.Serialization;
  using BindingHelpers;
  using GuiControls;
  using GuiHelpers;
  using Properties;
  using Binding = System.Windows.Data.Binding;
  using ContextMenu=System.Windows.Controls.ContextMenu;
  using MenuItem=System.Windows.Controls.MenuItem;
  using MessageBox = System.Windows.MessageBox;
  using TabControl = System.Windows.Controls.TabControl;

  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class Controller : Window
  {
    private readonly String m_fileNameNodes = "../../XmlNodes.xml";
    private readonly String m_fileNameGui = "../../XmlGui.xml";

    private DeviceXBeeSerial m_xbee;

    private Thread m_threadMsgProcessor;
    private Thread m_threadMsgRouter;
    private Thread m_threadNodeManager;

    private MsgDispatcher m_msgDispatcher;
    private MsgRouter m_msgRouter;
    private NodeManager m_nodeManager;
    private SyNetSettings m_settings;
    private GuiPanelList m_guiPanels = new GuiPanelList();

    public GuiPanelList GuiPanels
    {
      get { return m_guiPanels; }
      set { m_guiPanels = value; }
    }

    public Controller()
    {
      Debug.WriteLine( "[DBG] Window1.Constructor - Entry" );

      // Load from the xml file
      m_nodeManager = new NodeManager();
      Load();
      NodeManager.Instance = m_nodeManager;

      m_settings = SyNetSettings.Instance;

      m_xbee = new DeviceXBeeSerial();
      DataContext = m_xbee;

      m_msgDispatcher = MsgDispatcher.Instance;
      m_msgRouter = MsgRouter.Instance;


      m_msgDispatcher.Device = m_xbee;

      InitializeComponent();

      //this is the wrapper collection that marshals notifications to the correct thread via our dispatcher
      DispatchingCollection<MsgList,
        Msg> dispatchingCollection =
          new DispatchingCollection<MsgList, Msg>( m_msgDispatcher.MsgList, Dispatcher );

      //this is the wrapper collection that marshals notifications to the correct thread via our dispatcher
      DispatchingCollection<SyNetNodeList, DeviceXBee> dispatchingNodeCollection =
        new DispatchingCollection<SyNetNodeList, DeviceXBee>( m_nodeManager.SyNetNodeList, Dispatcher );

      /* Binding */
      m_ListBox.ItemsSource = dispatchingCollection;
      m_ListBox.ItemContainerGenerator.ItemsChanged += new ItemsChangedEventHandler( ItemContainerGenerator_ItemsChanged );
      m_nodeList.m_ListViewNodes.ItemsSource = dispatchingNodeCollection;
      m_updater.m_ComboBoxNodeList.ItemsSource = dispatchingNodeCollection;

      m_xbee.Open();

      /* Start the message processor */
      m_threadMsgProcessor = new Thread( m_msgDispatcher.ProcessMessagesThread );
      m_threadMsgProcessor.Name = "MsgDispatcher:ProcessMessages";
      m_threadMsgProcessor.IsBackground = true;
      m_threadMsgProcessor.Start();

      /* Start the message router */
      m_threadMsgRouter = new Thread( m_msgRouter.RouteMessages );
      m_threadMsgRouter.Name = "MsgRouter:RouteMessages";
      m_threadMsgRouter.IsBackground = true;
      m_threadMsgRouter.Start();

      /* Start the node manager */
      m_threadNodeManager = new Thread( m_nodeManager.ManageNodesThread );
      m_threadNodeManager.Name = "NodeManager:ManageNodes";
      m_threadNodeManager.IsBackground = true;
      m_threadNodeManager.Start();

      /* Wire up any node list stuff */
      m_nodeList.CreateDeviceGuiEvent += CreateDeviceGui;
      m_nodeList.GuiPanels = GuiPanels;

      /* Add all stored Gui Panels */
      foreach (GuiPanel panel in GuiPanels)
      {
        AddGuiTab(panel); 
      }

    }

    /// <summary>
    ///   Used to keep the log window scrolled to the most recent entry
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
    {
      if (m_ListBox.Items.Count > 1)
      {
        m_ListBox.ScrollIntoView( m_ListBox.Items[m_ListBox.Items.Count - 1] );
      }
    }

    ////////////////////////////////////////////////////////////////////////////

    private void Button_Click_Clear(object sender, RoutedEventArgs e)
    {
      m_msgDispatcher.Clear();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      Settings.Default.Save();
      Exit();
    }

    private void MenuItemEditPreferences_Click(object sender, RoutedEventArgs e)
    {
      SyNetSettings sySettings = SyNetSettings.Instance;

      EditPreferencesDlg dialog = new EditPreferencesDlg();

      // get settings values and post to dialog
      // - lists of choices
      dialog.AvailableSerialPorts = SerialPort.GetPortNames();
      dialog.SupportedBaudRates = sySettings.SupportedBaudRates;
      // - current choices
      dialog.SelectedBaudRate = sySettings.SerialBaud;
      dialog.SelectedSerialPort = sySettings.SerialComPort;
      dialog.LogDirectory = sySettings.LogDirectory;
      // now setup syslog
      // - lists of choices
      dialog.SupportedSyslogFacilities = sySettings.SupportedFacilities;
      // - current choices
      dialog.SyslogEnabled = sySettings.SyslogEnabled;
      dialog.SyslogFacility = sySettings.SyslogFacility;
      dialog.SyslogTargetHostname = sySettings.SyslogTargetHost;

      // now populate the dialog
      dialog.LoadSettings();

      // center dialog on parent window
      dialog.Top = Top + (Height / 2) - (dialog.Height / 2);
      dialog.Left = Left + (Width / 2) - (dialog.Width / 2);

      dialog.ShowDialog();
      DialogResult drCloseType = dialog.EndResult;
      // if dialog [OK] pressed
      if (drCloseType == System.Windows.Forms.DialogResult.OK)
      {
        //   retrieve new settings from dialog and post to settings object
        sySettings.SerialBaud = dialog.SelectedBaudRate;
        sySettings.SerialComPort = dialog.SelectedSerialPort;
        sySettings.LogDirectory = dialog.LogDirectory;
        // and syslog settings...
        sySettings.SyslogEnabled = dialog.SyslogEnabled;
        sySettings.SyslogFacility = dialog.SyslogFacility;
        sySettings.SyslogTargetHost = dialog.SyslogTargetHostname;
        // save settings
        sySettings.SaveSettings();
        // TODO  notify any listeners that settings changed
      }
    }

    private void MenuItemFileExit_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    public void Exit()
    {
      Save();
      m_nodeManager.Stop();
      m_msgDispatcher.Stop();
      m_msgRouter.Stop();

      m_threadNodeManager.Join();
      m_threadMsgProcessor.Join();
      m_threadMsgRouter.Join();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (m_xbee.IsSerialConnected)
      {
        m_xbee.Close();
      }
      m_xbee.Open();
    }


    private void Load()
    {
      try
      {
        Debug.WriteLine( "[DBG] Loading XML File" );
        TextReader r = new StreamReader(m_fileNameNodes);
        XmlSerializer s = new XmlSerializer( typeof( NodeManager ) );
        m_nodeManager = (NodeManager)s.Deserialize( r );
        r.Close();

        r = new StreamReader(m_fileNameGui);
        s = new XmlSerializer(typeof(GuiPanelList));
        m_guiPanels = (GuiPanelList)s.Deserialize(r);
        r.Close();
      }
      catch (Exception e)
      {
        MessageBox.Show( "Could not load xml file\n" + e.Message + e.InnerException );
      }
    }

    private void Save()
    {
      try
      {
        Debug.WriteLine( "[DBG] Saving XML File" );
        TextWriter w = new StreamWriter(m_fileNameNodes); 
        XmlSerializer s = new XmlSerializer( typeof( NodeManager ) );
        s.Serialize( w, m_nodeManager );
        w.Close();

        w = new StreamWriter(m_fileNameGui); 
        XmlSerializer guiPanelSerializer = new XmlSerializer(typeof(List<GuiPanel>));
        guiPanelSerializer.Serialize(w, GuiPanels);
        w.Close();

        //XmlSerializer r = new XmlSerializer( typeof(MsgRouter));
        //r.Serialize(w, m_msgRouter);

      }
      catch (Exception e)
      {
        Debug.WriteLine( "[ERR] - Controller.Save: " + e.Message + e.InnerException );
      }
    }

    private void CreateDeviceGui(DeviceXBee p_device)
    {
      GuiPanel guiPanel = new GuiPanel( p_device );
      AddGuiTab(guiPanel);
    }

    private void AddGuiTab(GuiPanel p_guiPanel)
    {
      CloseableTabItem tab = new CloseableTabItem();
      tab.PreviewMouseRightButtonDown += GuiTab_PreviewMouseRightButtonDown;
      tab.Tag = p_guiPanel;
      tab.Content = new FunctionGui(p_guiPanel);

      // Bind tab header to panel title
      Binding b = new Binding();
      b.Source = p_guiPanel;
      b.Mode = BindingMode.TwoWay;
      b.Path = new PropertyPath("Title");
      tab.SetBinding(HeaderedContentControl.HeaderProperty, b);

      tab.AddHandler(CloseableTabItem.CloseTabEvent, new RoutedEventHandler(CloseTab));
      m_TabControl.Items.Add(tab);
      if (!GuiPanels.Contains(p_guiPanel))
      {
        GuiPanels.Add(p_guiPanel);
      }
    }

    void GuiTab_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      CloseableTabItem tab = sender as CloseableTabItem;
      if (tab != null)
      {
        // Make sure the tab has a menu
        if (tab.ContextMenu == null)
        {
          tab.ContextMenu = new ContextMenu();
        }

        ContextMenu tabMenu = tab.ContextMenu;
        tabMenu.Items.Clear();

        MenuItem menuItemRename = new MenuItem();
        menuItemRename.Tag = tab.Tag; // Set the gui panel as the menu tag
        menuItemRename.Header = "Rename";
        menuItemRename.Click += GuiPanelRename_Click;
        tabMenu.Items.Add(menuItemRename);
      } else
      {
        throw new Exception("Controller.GuiTab_PreviewMouseRightButtonDown - null tab");
      }
    }

    void GuiPanelRename_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menu = sender as MenuItem;
      GuiPanel panel = menu.Tag as GuiPanel;
      if (panel != null)
      {
        GuiPanelRenameDialog renameDialog = new GuiPanelRenameDialog(panel.Title);
        renameDialog.Left = System.Windows.Forms.Cursor.Position.X;
        renameDialog.Top = System.Windows.Forms.Cursor.Position.Y;
        renameDialog.ShowDialog();
        if (renameDialog.DialogResult.HasValue && renameDialog.DialogResult.Value)
        {
          panel.Title = renameDialog.PanelTitle;
        }
      } else
      {
        throw new Exception("Controller.GuiPanelRename_Click - null panel tag");
      }
    }

    private void CloseTab(object source, RoutedEventArgs args)
    {
      TabItem tabItem = args.Source as TabItem;
      if (tabItem != null)
      {
        TabControl tabControl = tabItem.Parent as TabControl;
        if (tabControl != null)
        {
          GuiPanel panel = ((FunctionGui)tabItem.Content).GuiPanel as GuiPanel;
          if (panel != null)
          {
            Debug.WriteLine("Removing GUI Panel");
            m_guiPanels.Remove(panel);
          }
          tabControl.Items.Remove(tabItem);
        }
      }
    }

    private void NewGuiItem_Click(object sender, RoutedEventArgs e)
    {
      GuiPanel panel = new GuiPanel();
      AddGuiTab(panel);
    }

    private void ContextMenuTabControl_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {

    }
  }
}
                                                                                                                        
              