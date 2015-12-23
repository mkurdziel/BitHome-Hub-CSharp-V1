using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SyNet.GuiControls;
using SyNet.GuiHelpers;

namespace SyNet.XamlPanels.GuiPanels
{
  /// <summary>
  /// Interaction logic for GuiControlConfigDialog.xaml
  /// </summary>
  public partial class GuiControlConfigDialog : UserControl
  {
    //GuiParameter GuiParameter { get; set; }

    //GuiPanelValueControl Control { get; set; }

    /// <summary>
    ///   Initialzation constructor
    /// </summary>
    //public GuiControlConfigDialog(GuiPanelValueControl p_control)
    //{
    //  //DataContext = GuiParameter = p_control.GuiParameter;

    //  Control = p_control;

    //  InitializeComponent();

    //  BuildTypeComboBox();

    //  BuildLabelPositionComboBox();

    //  //x_typeComboBox.SelectedValue = GuiParameter.ControlType;

    //  //x_labelColorComboBox.SelectedColor = GuiParameter.LabelFont.ForegroundColor;

    //  //
    //  // Bind the font size to the slider
    //  //
    //  //Binding b = new Binding();
    //  //b.Source = this.GuiParameter.LabelFont;
    //  //b.Path = new PropertyPath("FontSize");
    //  //b.Mode = BindingMode.TwoWay;
    //  //x_fontSizeSlider.SetBinding(Slider.ValueProperty, b);

    //  //LoadControlConfig();

    //  //GuiParameter.PropertyChanged += GuiParameter_PropertyChanged;
    //}

    private void LoadControlConfig()
    {
      //x_controlConfig.Content = Control.Control.GetConfigUserControl();
    }

    void GuiParameter_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "ControlType")
      {
        LoadControlConfig();
      }
    }

    private void BuildLabelPositionComboBox()
    {
      //foreach (GuiParameter.EsnLabelPosition pos in Enum.GetValues(typeof(GuiParameter.EsnLabelPosition)))
      //{
      //  x_labelPositionComboBox.Items.Add(pos);
      //}
    }

    private void BuildTypeComboBox()
    {
      //foreach (GuiParameter.EsnGuiControlType type in Enum.GetValues(typeof(GuiParameter.EsnGuiControlType)))
      //{
      //  x_typeComboBox.Items.Add(type);
      //}
    }

    private void OKButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      //this.GuiParameter.ControlType = (GuiParameter.EsnGuiControlType)x_typeComboBox.SelectedValue;
    }

    private void CancelButton_Click(object p_sender, RoutedEventArgs p_e)
    {
    }

    private void LabelColor_PropertyChanged(object p_sender, PropertyChangedEventArgs p_e)
    {
      //GuiParameter.LabelFont.ForegroundColor = x_labelColorComboBox.SelectedColor;
    }
  }
}