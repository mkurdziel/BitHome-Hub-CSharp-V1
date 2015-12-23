using System;
using System.Collections.ObjectModel;
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
using System.Windows.Input;
using System.Xml.Serialization;
using SyNet.BindingHelpers;
using SyNet.Gui;
using SyNet.Gui.Models;
using SyNet.GuiControls;
using SyNet.GuiHelpers;
using SyNet.MessageTypes;
using SyNet.Properties;
using SyNet.XamlPanels;
using SyNet.XamlPanels.GuiPanels;
using Binding = System.Windows.Data.Binding;
using ContextMenu = System.Windows.Controls.ContextMenu;
using GuiManager=SyNet.Gui.Models.GuiManager;
using GuiPanel=SyNet.Gui.Models.GuiPanel;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using TabControl = System.Windows.Controls.TabControl;

namespace SyNet
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class ControllerGUI : Window
  {
    private readonly String m_fileNameController = "../../XmlController.xml";

    private readonly DeviceXBeeSerial m_xbee;

    private readonly Thread m_threadMsgProcessor;
    private readonly Thread m_threadNodeManager;
    private readonly Thread m_threadEventScheduler;

    private readonly MsgDispatcher m_msgDispatcher;
    private SyNetSettings m_settings;
    private Controller m_controller;


    /// <summary>
    ///   Entry point for the application
    /// </summary>
    public ControllerGUI()
    {
      m_controller = new Controller();

      // Load from the xml file
      Load();

      m_settings = SyNetSettings.Instance;

      m_xbee = new DeviceXBeeSerial();
      DataContext = m_xbee;

      m_msgDispatcher = MsgDispatcher.Instance;

      m_msgDispatcher.Device = m_xbee;

      InitializeComponent();

      //this is the wrapper collection that marshals notifications to the correct thread via our dispatcher
      DispatchingCollection<ObservableCollection<Msg>,
        Msg> dispatchingCollection =
          new DispatchingCollection<ObservableCollection<Msg>, Msg>(
            m_msgDispatcher.MsgList, Dispatcher);

      //this is the wrapper collection that marshals notifications to the correct thread via our dispatcher
      DispatchingCollection<SyNetNodeList, Device> dispatchingNodeCollection =
        new DispatchingCollection<SyNetNodeList, Device>(
          m_controller.NodeManager.SyNetNodeList, Dispatcher);

      /* Binding */
      m_ListBox.ItemsSource = dispatchingCollection;
      m_ListBox.ItemContainerGenerator.ItemsChanged +=
        new ItemsChangedEventHandler(ItemContainerGenerator_ItemsChanged);
      m_nodeList.m_ListViewNodes.ItemsSource = dispatchingNodeCollection;
      m_updater.m_ComboBoxNodeList.ItemsSource = dispatchingNodeCollection;

      m_xbee.Open();

      /* Start the message processor */
      m_threadMsgProcessor = new Thread(m_msgDispatcher.ProcessMessagesThread);
      m_threadMsgProcessor.Name = "MsgDispatcher:ProcessMessages";
      m_threadMsgProcessor.IsBackground = true;
      m_threadMsgProcessor.Start();

      /* Start the node manager */
      m_threadNodeManager =
        new Thread(m_controller.NodeManager.ManageNodesThread);
      m_threadNodeManager.Name = "NodeManager:ManageNodes";
      m_threadNodeManager.IsBackground = true;
      m_threadNodeManager.Start();

      /* Start the Event Scheduler */
      m_threadEventScheduler =
        new Thread(m_controller.EventScheduler.ScheduleEventsThread);
      m_threadEventScheduler.Name = "Scheduler:ScheduleEvents";
      m_threadEventScheduler.IsBackground = true;
      m_threadEventScheduler.Start();

      //
      // Load the Gui Panels
      //
      LoadGuiPanels();

      //
      // Watch for tab change events
      //
      m_TabControl.SelectionChanged += TabControl_SelectionChanged;
    }

    /// <summary>
    ///   Handles tab selection changes and disables editing of any
    ///   tab not currently focused
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      //
      // Disable all editing
      //
      foreach (var tab in m_TabControl.Items)
      {
        if (tab is CloseableTabItem)
        {
          if (((CloseableTabItem)tab).Content is GuiPanelDesigner)
          {
            ((GuiPanelDesigner) (((CloseableTabItem) tab).Content)).IsEditing = false;
          }
        }
      }

      //
      // If the selected tab is a guipanel then enable the editing menu
      //
      CloseableTabItem item = m_TabControl.SelectedItem as CloseableTabItem;

      // By default, uncheck everything
      x_menuItemEditPanel.IsChecked = false;

      if (item != null && item.Content is GuiPanelDesigner)
      {
        x_menuItemEditPanel.IsEnabled = true;
      }
      else
      {
        x_menuItemEditPanel.IsEnabled = false;
      }
    }

    /// <summary>
    ///   Used to keep the log window scrolled to the most recent entry
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ItemContainerGenerator_ItemsChanged(
      object sender, ItemsChangedEventArgs e)
    {
      if (m_ListBox.Items.Count > 1)
      {
        m_ListBox.ScrollIntoView(m_ListBox.Items[m_ListBox.Items.Count - 1]);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItemFileExit_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    /// <summary>
    ///   Exit the application
    /// </summary>
    public void Exit()
    {
      Save();
      m_controller.NodeManager.Stop();
      m_msgDispatcher.Stop();
      m_controller.EventScheduler.Stop();

      m_threadNodeManager.Join();
      m_threadMsgProcessor.Join();
      m_threadEventScheduler.Join();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (m_xbee.IsSerialConnected)
      {
        m_xbee.Close();
      }
      m_xbee.Open();
    }


    /// <summary>
    ///   Load the controller object in from an XML file
    /// </summary>
    private void Load()
    {
      SyNetSettings.Instance.IsDeserializing = true;
      try
      {
        Debug.WriteLine("[DBG] Loading XML File");

        TextReader reader = new StreamReader(m_fileNameController);
        XmlSerializer serializer = new XmlSerializer(typeof(Controller));
        m_controller = (Controller)serializer.Deserialize(reader);
        reader.Close();
      }
      catch (Exception e)
      {
        MessageBox.Show(
          "Could not load xml file\n" + e.Message + e.InnerException);
      }
      SyNetSettings.Instance.IsDeserializing = false;
    }

    /// <summary>
    ///   Save the controller object to an XML file
    /// </summary>
    private void Save()
    {
      SyNetSettings.Instance.IsSerializing = true;
      string tmpName = String.Format("{0}.tmp", m_fileNameController);
      try
      {
        Debug.WriteLine("[DBG] Saving XML File");

        TextWriter controllerWriter = new StreamWriter(tmpName);
        XmlSerializer controllerSerializer =
          new XmlSerializer(typeof(Controller));
        controllerSerializer.Serialize(controllerWriter, m_controller);
        controllerWriter.Close();

        // Rename the temp file
        File.Delete(m_fileNameController);
        File.Move(tmpName, m_fileNameController);
      }
      catch (Exception e)
      {
        Debug.WriteLine(
          string.Format("[ERR] - Controller.Save: {0}{1}", e.Message, e.InnerException));
      }
      SyNetSettings.Instance.IsSerializing = false;
    }

    /// <summary>
    ///   Load the Gui panels from the gui manager
    /// </summary>
    private void LoadGuiPanels()
    {
      GuiManager mgr = GuiManager.Instance;
      foreach (GuiPanel panel in mgr.GuiPanels)
      {
        AddGuiTab(panel);
      }
    }

    /// <summary>
    ///   Add a GuiPanel to it's own Tab
    /// </summary>
    /// <param name="p_guiPanel"></param>
    private void AddGuiTab(GuiPanel p_guiPanel)
    {
      CloseableTabItem tab = new CloseableTabItem();
      GuiPanelDesigner designer = new GuiPanelDesigner(p_guiPanel);

      tab.PreviewMouseRightButtonDown += GuiTab_PreviewMouseRightButtonDown;
      tab.Tag = p_guiPanel;
      tab.Content = designer;
      m_TabControl.Padding = new Thickness(0);

      //
      // Bind tab header to panel title
      //
      Binding b = new Binding();
      b.Source = p_guiPanel;
      b.Mode = BindingMode.TwoWay;
      b.Path = new PropertyPath("Title");
      tab.SetBinding(HeaderedContentControl.HeaderProperty, b);

      //
      // Bind tab background color to panel
      //
      b = new Binding();
      b.Source = p_guiPanel;
      b.Path = new PropertyPath("BackgroundColor");
      b.Converter = new ColorToBrushConverter();
      tab.SetBinding(BackgroundProperty, b);

      //
      // Bind tab title color to panel
      //
      b = new Binding();
      b.Source = p_guiPanel;
      b.Path = new PropertyPath("ForegroundColor");
      b.Converter = new ColorToBrushConverter();
      tab.SetBinding(ForegroundProperty, b);
      
      //
      // Bind the editing to the
      //
      b = new Binding();
      b.Source = designer;
      b.Path = new PropertyPath("IsEditing");
      tab.SetBinding(CloseableTabItem.IsEditingProperty, b);

      //
      // Add the close handler to close the tab
      //
      tab.AddHandler(
        CloseableTabItem.CloseTabEvent, new RoutedEventHandler(CloseTab));
      m_TabControl.Items.Add(tab);
    }




    private void GuiTab_PreviewMouseRightButtonDown(
      object sender, MouseButtonEventArgs e)
    {
      CloseableTabItem tab = sender as CloseableTabItem;
      if (tab != null)
      {
        // Make sure the tab has a menu
        //if (tab.ContextMenu == null)
        //{
        //  tab.ContextMenu = new ContextMenu();
        //}

        //ContextMenu tabMenu = tab.ContextMenu;
        //tabMenu.Items.Clear();

        //MenuItem menuItemRename = new MenuItem();
        //menuItemRename.Tag = tab.Tag; // Set the gui panel as the menu tag
        //menuItemRename.Header = "Rename";
        //menuItemRename.Click += GuiPanelRename_Click;
        //tabMenu.Items.Add(menuItemRename);
      }
      else
      {
        throw new Exception(
          "Controller.GuiTab_PreviewMouseRightButtonDown - null tab");
      }
    }

    private void GuiPanelRename_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menu = sender as MenuItem;
      GuiPanel panel = menu.Tag as GuiPanel;
      if (panel != null)
      {
        RenameDialog renameDialog = new RenameDialog("Name Panel", panel.Title);
        renameDialog.Left = System.Windows.Forms.Cursor.Position.X;
        renameDialog.Top = System.Windows.Forms.Cursor.Position.Y;
        renameDialog.ShowDialog();
        if (renameDialog.DialogResult.HasValue &&
            renameDialog.DialogResult.Value)
        {
          panel.Title = renameDialog.Value;
        }
      }
      else
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
          GuiPanelDesigner designer = tabItem.Content as GuiPanelDesigner;
          GuiPanel panel = designer.GuiPanel;
          if (panel != null)
          {
            designer.IsEditing = false; 
            Debug.WriteLine("Removing GUI Panel");
            GuiManager.Instance.RemoveGuiPanel(panel);
          }
          tabControl.Items.Remove(tabItem);
        }
      }
    }

    private void ContextMenuTabControl_PreviewMouseRightButtonDown(
      object sender, MouseButtonEventArgs e)
    {
      CloseableTabItem tab = sender as CloseableTabItem;
      if (tab != null)
      {
        // Make sure the tab has a menu
        if (tab.ContextMenu == null)
        {
          tab.ContextMenu = new ContextMenu();
        }
        // Insert rename dialog
      }
      else
      {
        throw new Exception(
          "Controller.GuiTab_PreviewMouseRightButtonDown - null tab");
      }
    }

    private void AddGuiPanel_Click(object p_sender, RoutedEventArgs p_e)
    {
      GuiPanel newPanel = new GuiPanel();

      //
      // Bring up a rename dialog
      //
      RenameDialog dlg = new RenameDialog("New Panel Name", "Panel");
      dlg.WindowStartupLocation = WindowStartupLocation.Manual;
      dlg.Top = Top + (Height / 2);
      dlg.Left = Left + (Width / 2);
      dlg.ShowDialog();
      if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
      {
        newPanel.Title = dlg.Value;

        //
        // Add the panel to the GUI
        //
        AddGuiTab(newPanel);

        //
        // Register the panel with the GUI Manager
        //
        GuiManager.Instance.AddGuiPanel(newPanel);
      }
    }

    private void MenuItemEditPanel_Checked(object p_sender, RoutedEventArgs p_e)
    {
      //
      // If the selected tab is a guipanel then enable the editing menu
      //
      CloseableTabItem item = m_TabControl.SelectedItem as CloseableTabItem;

      if (item != null && item.Content is GuiPanelDesigner)
      {
        // If checked then edit the panel
        if (x_menuItemEditPanel.IsChecked)
        {
          ((GuiPanelDesigner) item.Content).IsEditing = true;
        }
        else
        {
          ((GuiPanelDesigner) item.Content).IsEditing = false;
        }
      }
    }
  }
}