using System.Windows.Controls;

namespace SyNet.Gui.Controls.SnGroupBox
{
  /// <summary>
  /// Interaction logic for SnButtonConfig.xaml
  /// </summary>
  public partial class SnGroupBoxConfig : UserControl
  {

    private SnGroupBox MyControl { get; set; }

    public SnGroupBoxConfig(SnGroupBox p_control)
    {
      InitializeComponent();

      MyControl = p_control;

      DataContext = p_control;
    }
  }
}
