using System;
using System.Windows;
using System.Windows.Controls;
using SyNet.GuiControls;

namespace SyNet.XamlPanels.GuiPanels
{
  /// <summary>
  /// Interaction logic for GuiControlConfigDialog.xaml
  /// </summary>
  public partial class GuiActionConfigDialog : UserControl
  {
    //GuiPanelActionControl GuiActionControl { get; set; }

    //GuiAction GuiAction { get; set; }

    /// <summary>
    ///   Initialization constructor for a GuiAction
    /// </summary>
    /// <param name="p_action"></param>
    //public GuiActionConfigDialog( GuiPanelActionControl p_action )
    //{
    //  //GuiActionControl = p_action;

    //  //DataContext = GuiAction = p_action.GuiAction;

    //  //InitializeComponent();
  
    //  //
    //  // Fill the orientation box
    //  //
    //  //FillOrientationComboBox();


    //}

    private void FillOrientationComboBox()
    {
      //foreach (Orientation type in Enum.GetValues(typeof(Orientation)))
      //{
      //  x_orientationComboBox.Items.Add(type);
      //}
    }

    private void DeleteButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      //GuiActionControl.GuiDelete();
    }
  }
}