using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using SyNet.MessageTypes;
using SyNet.Protocol;
using Cursors = System.Windows.Forms.Cursors;
using UserControl = System.Windows.Controls.UserControl;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for SyNetNodeList.xaml
  /// </summary>
  public partial class NodeList : UserControl
  {
    private DeviceXBee m_xbeeTarget;

    /// <summary>
    ///   Constructor
    /// </summary>
    public NodeList()
    {
      InitializeComponent();
      //DataContext = new DeviceXBee();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void ButtonNodeUpdate_Click(object p_sender, RoutedEventArgs p_e)
    {
      System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
      MsgDispatcher.Instance.SendMsg(
        new MsgZigbeeATCmd(MsgZigbee.EsnXbeeAtCmd.NODE_DISCOVER));
      System.Windows.Forms.Cursor.Current = Cursors.Default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void RequestDeviceStatus(object p_sender, MouseButtonEventArgs p_e)
    {
      DeviceXBee xbeeTarget = m_ListViewNodes.SelectedItem as DeviceXBee;
      if (xbeeTarget != null)
      {
        MsgDispatcher.Instance.SendMsg(new MsgSyNetDeviceStatusRequest(xbeeTarget,
                                                                       EsnAPIDeviceStatusRequest.STATUS_REQUEST));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void RequestDeviceInfo(object p_sender, MouseButtonEventArgs p_e)
    {
      DeviceXBee xbeeTarget = m_ListViewNodes.SelectedItem as DeviceXBee;
      if (xbeeTarget != null)
      {
        MsgDispatcher.Instance.SendMsg(new MsgSyNetDeviceStatusRequest(xbeeTarget,
                                                                       EsnAPIDeviceStatusRequest.INFO_REQUEST));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void RequestDeviceCatalog(object p_sender, MouseButtonEventArgs p_e)
    {
      if (p_e == null) throw new ArgumentNullException("e");
      DeviceXBee xbeeTarget = m_ListViewNodes.SelectedItem as DeviceXBee;
      if (xbeeTarget != null)
      {
        MsgDispatcher.Instance.SendMsg(new MsgSyNetCatalogRequest(xbeeTarget, 0));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void RequestDeviceParameters(object p_sender, MouseButtonEventArgs p_e)
    {
      DeviceXBee xbeeTarget = m_ListViewNodes.SelectedItem as DeviceXBee;
      if (xbeeTarget != null)
      {
        MsgDispatcher.Instance.SendMsg(new MsgSyNetParameterRequest(xbeeTarget, 0, 0));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void RebootDevice(object p_sender, MouseButtonEventArgs p_e)
    {
      DeviceXBee xbeeTarget = m_ListViewNodes.SelectedItem as DeviceXBee;
      if (xbeeTarget != null)
      {
        MsgDispatcher.Instance.SendMsg(new MsgSyNetBootloadTransmit(xbeeTarget,
                                                                    EsnAPIBootloadTransmit.REBOOT_DEVICE));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void DeleteDevice(object p_sender, MouseButtonEventArgs p_e)
    {
      DeviceXBee xbeeTarget = m_ListViewNodes.SelectedItem as DeviceXBee;
      if (xbeeTarget != null)
      {
        if (m_ListViewFunctionCatalog.DataContext == xbeeTarget)
        {
          m_ListViewFunctionCatalog.DataContext = null;
        }

        if (m_StackPanelNodeInfo.DataContext == xbeeTarget)
        {
          m_StackPanelNodeInfo.DataContext = null;
        }

        NodeManager.Instance.RemoveNode(xbeeTarget);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void InvestigateNode(object p_sender, MouseButtonEventArgs p_e)
    {
      DeviceXBee xbeeTarget = m_ListViewNodes.SelectedItem as DeviceXBee;
      if (xbeeTarget != null)
      {
        NodeManager.Instance.AddNodeForInvestigation(xbeeTarget);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="e"></param>
    private void m_ListViewNodes_SelectionChanged(object p_sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      DeviceXBee xbeeTarget = m_ListViewNodes.SelectedItem as DeviceXBee;
      if (xbeeTarget != null)
      {
        m_StackPanelNodeInfo.DataContext = xbeeTarget;

        m_SetConfig.XbeeTarget = xbeeTarget;
        m_SetInfo.XbeeTarget = xbeeTarget;
        m_xbeeTarget = xbeeTarget;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="e"></param>
    private void ListViewFunctionCatalog_SelectionChanged(object p_sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      if (m_xbeeTarget != null)
      {
        if ((uint)m_ListViewFunctionCatalog.SelectedIndex <= m_xbeeTarget.FunctionList.Length)
        {
          //Device.FunctionStruct funct = m_xbeeTarget.FunctionList[m_ListViewFunctionCatalog.SelectedIndex];
          //m_FunctionGUI.DataContext = funct;
          //m_FunctionGUI.m_xbeeTarget = m_xbeeTarget;
          //m_FunctionGUI.BuildGUI();
        }
      }
    }

    private void Simulate_Click( object sender, RoutedEventArgs e )
    {
      DeviceFunction function = m_ListViewFunctionCatalog.SelectedItem as DeviceFunction;
      DeviceXBee device = m_ListViewNodes.SelectedItem as DeviceXBee;

      if (function != null && device != null)
      {
        Debug.WriteLine("Simulating function: " + function.Name);
        
        RenameDialog dlg = new RenameDialog("Enter Return Value", "0");
        dlg.WindowStartupLocation = WindowStartupLocation.Manual;
        Point point =
          m_ListViewFunctionCatalog.PointToScreen(new Point(0, 0));
        dlg.Top = point.Y;
        dlg.Left = point.X;
 
        dlg.ShowDialog();

        if (dlg.DialogResult.HasValue && dlg.DialogResult == true )
        {
          string msgValue = dlg.Value;

          //
          // Build the incoming message
          //
          Msg baseMsg = MessageCreator.CreateSyNetFunctionReceive(
            device.SerialNumber, device.NetworkAddress,
            (byte)function.ID,
            function.ReturnType,
            msgValue);

          MsgDispatcher.Instance.ReceiveMsg(
            new MsgSyNetFunctionReceive(baseMsg));
        }
      }
    }
  }
}