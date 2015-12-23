using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SyNet.GuiControls;

namespace SyNet.Gui.Toolbox
{
  /// <summary>
  /// Interaction logic for PanelConfig.xaml
  /// </summary>
  //public partial class PanelConfig : UserControl
  //{
  //  public GuiPanel GuiPanel { get; set; }

  //  public PanelConfig()
  //  {
  //    InitializeComponent();

  //    this.DataContextChanged += PanelConfig_DataContextChanged;
  //  }

  //  void PanelConfig_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
  //  {
  //    GuiPanel = this.DataContext as GuiPanel;

  //    if (GuiPanel != null)
  //    {
  //      x_foregroundColor.SelectedColor = GuiPanel.ForegroundColor;
  //      x_backgroundColor.SelectedColor = GuiPanel.BackgroundColor;
  //    }
  //  }

  //  private void BackgroundColor_PropertyChanged(object p_sender, PropertyChangedEventArgs p_e)
  //  {
  //    if (p_e.PropertyName == "SelectedColor" && GuiPanel != null)
  //    {
  //      GuiPanel.BackgroundColor = x_backgroundColor.SelectedColor;
  //    }
  //  }

  //  private void ForegroundColor_PropertyChanged(object p_sender, PropertyChangedEventArgs p_e)
  //  {
  //    if (p_e.PropertyName == "SelectedColor" && GuiPanel != null)
  //    {
  //      GuiPanel.ForegroundColor = x_foregroundColor.SelectedColor;
  //    }
  //  }
  //}
}
