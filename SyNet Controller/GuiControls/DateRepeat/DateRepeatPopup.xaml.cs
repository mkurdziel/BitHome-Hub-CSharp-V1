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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SyNet.GuiControls.DateRepeat
{
  /// <summary>
  /// Interaction logic for DateRepeatPopup.xaml
  /// </summary>
  public partial class DateRepeatPopup : UserControl
  {
    /// <summary>
    ///   Opens or closes the control popup
    /// </summary>
    public bool IsOpen
    {
      get { return x_popup.IsOpen; }
      set { x_popup.IsOpen = value; }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public DateRepeatPopup()
    {
      InitializeComponent();
    }
  }
}
