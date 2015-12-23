using System;
using System.Windows;
using System.Windows.Controls;
using SyNet.GuiControls;

namespace SyNet.XamlPanels.GuiPanels
{
  /// <summary>
  /// Interaction logic for GuiControlConfigDialog.xaml
  /// </summary>
  public partial class GuiTriggerConfigDialog : UserControl
  {
    //GuiPanelTriggerControl GuiPanelTriggerControl { get; set; }

    //public GuiTriggerItem GuiTriggerItem { get; set; }

    /// <summary>
    ///   Initialzation constructor
    /// </summary>
    /// <param name="p_control"></param>
    //public GuiTriggerConfigDialog( GuiPanelTriggerControl p_control)
    //{
    //  this.GuiPanelTriggerControl = p_control;

    //  //
    //  // Set the data context
    //  //
    //  //DataContext = GuiTriggerItem = p_control.GuiTriggerItem;

    //  InitializeComponent();
    //}

    private void OKButton_Click(object p_sender, RoutedEventArgs p_e)
    {
    }

    private void CancelButton_Click(object p_sender, RoutedEventArgs p_e)
    {
    }

    private void DeleteButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      //this.GuiPanelTriggerControl.GuiDelete();
    }
  }
}