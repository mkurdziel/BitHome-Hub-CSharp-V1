using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using SyNet.GuiControls;

namespace SyNet.XamlPanels.GuiPanels
{
  /// <summary>
  /// Interaction logic for GuiPanelItemConfigWindow.xaml
  /// </summary>
  public partial class GuiPanelItemConfigWindow : Window
  {
    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x80000;
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    /// <summary>
    ///   Default constructor
    /// </summary>
    public GuiPanelItemConfigWindow()
    {
      InitializeComponent();

      this.Loaded += GuiPanelItemConfigWindow_Loaded;
    }

    void GuiPanelItemConfigWindow_Loaded(object sender, RoutedEventArgs e)
    {
      var hwnd = new WindowInteropHelper(this).Handle;
      SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
    }

    /// <summary>
    ///   Sets the control to be configured
    /// </summary>
    /// <param name="p_configurable"></param>
    //public void SetControl(IGuiConfigurable p_configurable)
    //{
    //  //
    //  // Clear the container
    //  //
    //  x_container.Children.Clear();

    //  //
    //  // Get the new configure panel from the interface
    //  //
    //  UserControl configControl = p_configurable.GetConfigUserControl();
    //  if (configControl != null)
    //  {
    //    x_container.Children.Add(configControl);
    //  }
    //}
  }
}
