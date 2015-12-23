using System.Windows.Controls;

namespace SyNet.Gui.Toolbox
{
  /// <summary>
  /// Interaction logic for LabelConfig.xaml
  /// </summary>
  public partial class LabelConfig : UserControl
  {
    GuiPanelItem MyItem { get; set; }

    public LabelConfig(GuiPanelItem p_pMyItem)
    {
      InitializeComponent();

      x_placementComboBox.Items.Add(GuiPanelItem.EsnLabelPosition.Left);
      x_placementComboBox.Items.Add(GuiPanelItem.EsnLabelPosition.Right);
      x_placementComboBox.Items.Add(GuiPanelItem.EsnLabelPosition.Top);
      x_placementComboBox.Items.Add(GuiPanelItem.EsnLabelPosition.Bottom);

      MyItem = p_pMyItem;

      DataContext = MyItem;
    }
  }
}
