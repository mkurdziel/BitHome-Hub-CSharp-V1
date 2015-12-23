using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using SyNet.Gui.Models;

namespace SyNet.Gui
{
  /// <summary>
  /// Interaction logic for ToolboxWindow.xaml
  /// </summary>
  public partial class ToolboxWindow : Window
  {
    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x80000;
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public ToolboxWindow(GuiPanel p_panel)
    {
      // The panel being controlled by this
      DataContext = this.GuiPanel = p_panel;

      InitializeComponent();

      //
      // By default select the panel button
      //
      x_buttonPanel.IsChecked = true;

      //
      // When loaded, remove the close button
      //
      this.Loaded += ToolboxWindow_Loaded;
    }

    public GuiPanel GuiPanel
    {
      get; set;
    }

    /// <summary>
    ///   Set the user control for the control properties
    /// </summary>
    public UserControl ControlProperties
    {
      set
      {
        x_contentProperties.Content = value;
      }
    }

    void ToolboxWindow_Loaded(object sender, RoutedEventArgs e)
    {
      var hwnd = new WindowInteropHelper(this).Handle;
      SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
    }

    /// <summary>
    ///   Check a button
    /// </summary>
    /// <param name="p_button"></param>
    private void Check(ToggleButton p_button)
    {
      ToggleButton[] buttons = new ToggleButton[]
                                 {
                                   x_buttonAction,
                                   x_buttonControl,
                                   x_buttonPanel,
                                   x_buttonProperties,
                                   x_buttonTrigger };
      foreach (ToggleButton button in buttons)
      {
        if (p_button == button)
        {
          button.IsChecked = true;
        }
        else
        {
          button.IsChecked = false;
        }
      }
    }

    private void PanelButton_Checked(object p_sender, RoutedEventArgs p_e)
    {
      if (x_buttonPanel.IsChecked == true)
      {
        x_contentPanel.Visibility = Visibility.Visible; 
      }
      else
      {
        x_contentPanel.Visibility = Visibility.Collapsed;
      }
    }

    private void ActionButton_Checked(object p_sender, RoutedEventArgs p_e)
    {
      if (x_buttonAction.IsChecked == true)
      {
        x_contentActions.Visibility = Visibility.Visible;
      }
      else
      {
        x_contentActions.Visibility = Visibility.Collapsed;
      }
    }

    private void TriggerButton_Checked(object p_sender, RoutedEventArgs p_e)
    {
      if (x_buttonTrigger.IsChecked == true)
      {
        x_contentTriggers.Visibility = Visibility.Visible;
      }
      else
      {
        x_contentTriggers.Visibility = Visibility.Collapsed;
      }
    }

    private void ControlButton_Checked(object p_sender, RoutedEventArgs p_e)
    {
    }

    private void PropertiesButton_Checked(object p_sender, RoutedEventArgs p_e)
    {
      if (x_buttonProperties.IsChecked == true)
      {
        x_contentProperties.Visibility = Visibility.Visible;
      }
      else
      {
        x_contentProperties.Visibility = Visibility.Collapsed;
      }
    }

    private void PanelButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      if (x_buttonPanel.IsChecked == false)
      {
        Check(x_buttonPanel);
      }
      p_e.Handled = true;
    }

    private void PropertiesButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      if (x_buttonProperties.IsChecked == false)
      {
        Check(x_buttonProperties);
      }
      p_e.Handled = true;
    }

    private void ControlButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      if (x_buttonControl.IsChecked == false)
      {
        Check(x_buttonControl);
      }
      p_e.Handled = true;
    }

    private void TriggerButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      if (x_buttonTrigger.IsChecked == false)
      {
        Check(x_buttonTrigger);
      }
      p_e.Handled = true;
    }

    private void ActionButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      if (x_buttonAction.IsChecked == false)
      {
        Check(x_buttonAction);
      }
      p_e.Handled = true;
    }
  }
}
