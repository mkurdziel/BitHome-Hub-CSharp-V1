using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SyNet.GuiControls;

namespace SyNet.XamlPanels.GuiPanels
{
  /// <summary>
  /// Interaction logic for GuiControlConfigDialog.xaml
  /// </summary>
  public partial class GuiPanelConfigDialog : UserControl
  {
  //  GuiPanel GuiPanel { get; set; }

  //  /// <summary>
  //  ///   Initialization constructor for a GuiAction
  //  /// </summary>
  //  public GuiPanelConfigDialog( GuiPanel p_panel )
  //  {
  //    DataContext = GuiPanel = p_panel;

  //    InitializeComponent();

  //    x_backgroundComboBox.SelectedColor = p_panel.BackgroundColor;
  //    x_titleComboBox.SelectedColor = p_panel.ForegroundColor;
  //  }

  //  private void BackgroundColor_PropertyChanged(object p_sender, PropertyChangedEventArgs p_e)
  //  {
  //    this.GuiPanel.BackgroundColor = x_backgroundComboBox.SelectedColor;
  //  }

  //  private void TitleColor_PropertyChanged(object p_sender, PropertyChangedEventArgs p_e)
  //  {
  //    this.GuiPanel.ForegroundColor = x_titleComboBox.SelectedColor;
  //  }

  //  private void AddTrigger_Click(object p_sender, RoutedEventArgs p_e)
  //  {
  //    TriggerSelectDialog dlg = new TriggerSelectDialog();
  //    dlg.WindowStartupLocation = WindowStartupLocation.Manual;
  //    Point point =
  //      x_addTriggerButton.PointToScreen(new Point(0, 0));
  //    dlg.Top = point.Y + (Height / 2);
  //    dlg.Left = point.X + (Width / 2);
  //    dlg.ShowDialog();

  //    if (dlg.DialogResult.HasValue && dlg.DialogResult == true)
  //    {
  //      UInt64 selectedTriggerID = dlg.SelectedTrigger.ID;
  //      GuiPanel.AddTrigger(selectedTriggerID);
  //    }
  //  }

  //  private void AddAction_Click(object p_sender, RoutedEventArgs p_e)
  //  {
  //    ActionSelectDialog dlg = new ActionSelectDialog();
  //    dlg.WindowStartupLocation = WindowStartupLocation.Manual;
  //    Point point =
  //      x_addActionButton.PointToScreen(new Point(0, 0));
  //    dlg.Top = point.Y + (Height / 2);
  //    dlg.Left = point.X + (Width / 2);
  //    dlg.ShowDialog();

  //    if (dlg.DialogResult.HasValue && dlg.DialogResult == true)
  //    {
  //      UInt64 selectedActionID = dlg.SelectedActionID;
  //      GuiPanel.AddAction(selectedActionID);
  //    }
  //  }

  //  private void UserControl_Loaded(object sender, RoutedEventArgs e)
  //  {

  //  }
  }
}