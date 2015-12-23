using System;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SyNet.GuiControls;
using SyNet.GuiHelpers;

namespace SyNet.XamlPanels.GuiPanels
{
  /// <summary>
  /// Interaction logic for GuiPanelParameterControl.xaml
  /// </summary>
  public partial class GuiPanelParameterControl : UserControl
  {
    //private GuiPanelValueControl m_snControl;

    private bool m_isEditing;

    internal bool IsEditing
    {
      get { return m_isEditing; }
      set
      {
        m_isEditing = value;
        SetEditing(value);
      }
    }

    private void SetEditing(bool p_isEditing)
    {
      //  if (m_snControl != null)
      //  {
      //    m_snControl.IsEditing = p_isEditing;
      //  }
      //}

      /// <summary>
      ///   Reference to parent action control
      /// </summary>
      //private GuiPanelActionControl ParentActionControl { get; set; }

      /// <summary>
      ///   Reference to the parameter
      /// </summary>
      //private GuiParameter GuiParameter { get; set; }

      /// <summary>
      ///   Initializer constructor for a GuiParameter
      /// </summary>
      /// <param name="p_actionControl"></param>
      /// <param name="p_parameter"></param>
      //public GuiPanelParameterControl(GuiPanelActionControl p_actionControl,
      //                                GuiParameter p_parameter)
      //{
      //  ParentActionControl = p_actionControl;

      //  DataContext = GuiParameter = p_parameter;

      //  InitializeComponent();

      //  AddControl(p_parameter);

      //  SetupBindings();

      //  PositionLabel();

      //  this.GuiParameter.PropertyChanged += GuiParameter_PropertyChanged;
      //}

      //private void GuiParameter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      //{
      //  //if (e.PropertyName == "LabelPosition")
      //  //{
      //  //  PositionLabel();
      //  //}
      //}

      //private void SetupBindings()
      //{
      //  Binding b;

      //  ////
      //  //// Bind the Font size
      //  ////
      //  //b = new Binding
      //  //{
      //  //  Source = this.GuiParameter.LabelFont,
      //  //  Path = new PropertyPath("FontSize")
      //  //};
      //  //x_label.SetBinding(FontSizeProperty, b);


      //  ////
      //  //// Bind the foreground
      //  ////
      //  //b = new Binding
      //  //{
      //  //  Source = this.GuiParameter.LabelFont,
      //  //  Path = new PropertyPath("ForegroundColor"),
      //  //  Converter = new ColorToBrushConverter() 
      //  //};
      //  //x_label.SetBinding(ForegroundProperty, b);

      //}


      /// <summary>
      ///   Add a control to the panel based on the type in the GuiParameter
      /// </summary>
      /// <param name="p_parameter"></param>
      //private void AddControl(GuiParameter p_parameter)
      //{
      //  m_snControl = new GuiPanelValueControl(p_parameter, ParentActionControl);

      //  x_container.Content = m_snControl;
      //}

      //private void PositionLabel()
      //{
      //  switch(this.GuiParameter.LabelPosition)
      //  {
      //    case GuiControls.GuiParameter.EsnLabelPosition.Left:
      //      Grid.SetColumn(x_label, 0);
      //      Grid.SetRow(x_label, 1);
      //      break;
      //    case GuiControls.GuiParameter.EsnLabelPosition.Top:
      //      Grid.SetColumn(x_label, 1);
      //      Grid.SetRow(x_label, 0);
      //      break;
      //    case GuiControls.GuiParameter.EsnLabelPosition.Right:
      //      Grid.SetColumn(x_label, 2);
      //      Grid.SetRow(x_label, 1);
      //      break;
      //    case GuiControls.GuiParameter.EsnLabelPosition.Bottom:
      //      Grid.SetColumn(x_label, 1);
      //      Grid.SetRow(x_label, 2);
      //      break;
      //  }
      //}
    }
  }
}