using System.Windows;
using SyNet.Events.Triggers;

namespace SyNet.GuiControls.DateRepeat
{
  /// <summary>
  /// Interaction logic for DateRepeatDialog.xaml
  /// </summary>
  public partial class DateRepeatDialog : Window
  {
    public DateRepeatDialog(DateTimeTrigger p_trigger)
    {
      InitializeComponent();
      x_dateRepeatControl.DataContext = p_trigger;
      x_dateRepeatControl.OKButtonClick += new RoutedEventHandler(OKButton_Click);
      x_dateRepeatControl.CancelButtonClick += new RoutedEventHandler(CancelButton_Click);
    }

    /// <summary>
    ///   Cancel button click handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
    }

    /// <summary>
    ///   OK button click handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
