using System;
using System.Windows;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for ActionRenameDialog.xaml
  /// </summary>
  public partial class RenameDialog : Window
  {
    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="p_windowTitle"></param>
    public RenameDialog(string p_windowTitle) : this(p_windowTitle, "")
    {
    }

    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="p_windowTitle"></param>
    /// <param name="p_nameText"></param>
    public RenameDialog(string p_windowTitle, string p_nameText )
    {
      InitializeComponent();
      base.Title = p_windowTitle;
      this.Value = p_nameText;
    }

    /// <summary>
    ///   Value set by user
    /// </summary>
    public String Value
    {
      get
      {
        return x_textField.Text;
      }
      set
      {
        x_textField.Text = value;
        x_textField.Focus();
        x_textField.SelectAll();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CancelButton_Click( object sender, RoutedEventArgs e )
    {
      DialogResult = false;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OKButton_Click( object sender, RoutedEventArgs e )
    {
      DialogResult = true;
    }
  }
}
