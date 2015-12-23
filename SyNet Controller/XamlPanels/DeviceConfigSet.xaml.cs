using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SyNet.MessageTypes;
using SyNet.Protocol;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for DeviceConfigSet.xaml
  /// </summary>
  public partial class DeviceConfigSet : UserControl
  {

    /// <summary>
    /// 
    /// </summary>
    public DeviceConfigSet()
    {
      InitializeComponent();
    }

    /// <summary>
    /// 
    /// </summary>
    public DeviceXBee XbeeTarget { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void m_StackPanelMain_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      XbeeTarget = DataContext as DeviceXBee;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_infoID"></param>
    /// <param name="p_info"></param>
    private void SetInfo(EsnAPIInfoValues p_infoID, String p_info)
    {
      if (XbeeTarget != null )
      {
        try
        {
          UInt16 info = Convert.ToUInt16(p_info);
          byte[] infoBytes = EBitConverter.GetBytes(info);
          MsgDispatcher.Instance.SendMsg(new MsgSyNetInformationSet(XbeeTarget, p_infoID, infoBytes));
        } catch(Exception e)
        {
          Debug.WriteLine(e.ToString());
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonID_Click(object sender, RoutedEventArgs e)
    {
      SetInfo(EsnAPIInfoValues.ID, m_TextBoxID.Text);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonManufac_Click(object sender, RoutedEventArgs e)
    {
      SetInfo(EsnAPIInfoValues.MANUFAC, m_TextBoxManufac.Text);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonProfile_Click(object sender, RoutedEventArgs e)
    {
      SetInfo(EsnAPIInfoValues.PROFILE, m_TextBoxProfile.Text);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonRevision_Click(object sender, RoutedEventArgs e)
    {
      SetInfo(EsnAPIInfoValues.REVISION, m_TextBoxRevision.Text);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonRemote_Click(object sender, RoutedEventArgs e)
    {
      SetInfo(EsnAPIInfoValues.REMOTE, m_TextBoxRemote.Text);
    }

  }
}