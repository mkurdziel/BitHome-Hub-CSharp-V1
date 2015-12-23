using System.Windows;
using System.Windows.Controls;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for DeviceInfoSet.xaml
  /// </summary>
  public partial class DeviceInfoSet : UserControl {
    /// <summary>
    ///   Constructor
    /// </summary>
    public DeviceInfoSet()
    {
      InitializeComponent();
    }

    /// <summary>
    ///   Device target
    /// </summary>
    public DeviceXBee XbeeTarget { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonName_Click(object sender, RoutedEventArgs e)
    {
      if (XbeeTarget != null)
      {
        XbeeTarget.DeviceName = m_TextBoxName.Text;
      }
    }
  }
}