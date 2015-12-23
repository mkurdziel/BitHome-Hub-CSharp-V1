using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace SyNet.Gui.Controls.SnButton
{
  /// <summary>
  /// Interaction logic for SnButtonConfig.xaml
  /// </summary>
  public partial class SnButtonConfig : UserControl
  {

    private SnButton MyControl { get; set; }

    public SnButtonConfig( SnButton p_control)
    {
      InitializeComponent();

      MyControl = p_control;

      DataContext = p_control;
   
    }
  }
}
