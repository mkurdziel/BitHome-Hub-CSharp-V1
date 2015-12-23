using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SyNet.Gui.Dialogs
{
  /// <summary>
  /// Interaction logic for ValueEntryDialog.xaml
  /// </summary>
  public partial class ValueEntryDialog : Window
  {
    public ValueEntryDialog()
    {
      InitializeComponent();
      this.ShowInTaskbar = false;
    }

    /// <summary>
    ///   Gets or sets the entered value
    /// </summary>
    public string Value
    {
      get
      {
        return x_textBox.Text;
      }
      set
      {
        x_textBox.Text = value;
        x_textBox.SelectAll();
        x_textBox.Focus();
      }
    }

    /// <summary>
    ///   OK Button Click Handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
