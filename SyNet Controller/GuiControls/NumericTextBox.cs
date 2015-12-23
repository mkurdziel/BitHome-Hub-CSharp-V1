using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace SyNet.GuiControls
{
  public class NumericTextBox : TextBox
  {
    public NumericTextBox() : base()
    {
      this.PreviewTextInput += NumericTextBox_PreviewTextInput;
    }

    void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
      e.Handled = !AreAllValidNumericChars(e.Text);
      base.OnPreviewTextInput(e);
    }

    private bool AreAllValidNumericChars(string str)
    {
      foreach (char c in str)
      {
        if (!Char.IsNumber(c)) return false;
      }

      return true;
    }

  }
}
