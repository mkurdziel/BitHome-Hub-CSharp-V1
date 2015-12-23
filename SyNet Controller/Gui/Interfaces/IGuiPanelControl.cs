using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace SyNet.Gui.Interfaces
{
  public interface IGuiPanelControl
  {
    /// <summary>
    ///   Gets a string describing the control
    /// </summary>
    string ControlName { get; }

    /// <summary>
    ///   Return the GuiPanelItem as a UIElement
    /// </summary>
    /// <returns></returns>
    UIElement AsUIElement();

    /// <summary>
    ///   Return the GuiPanelControl as a FrameworkElement
    /// </summary>
    /// <returns></returns>
    FrameworkElement AsFrameworkElement();

    /// <summary>
    ///   Returns true if the width of the container can be resized
    /// </summary>
    bool CanResizeWidth { get; }

    /// <summary>
    ///   Returns true if the height of the container can be resized
    /// </summary>
    bool CanResizeHeight { get; }

    /// <summary>
    ///   Returns the configuration panel for the control
    /// </summary>
    UserControl ConigurationPanel { get; }

    /// <summary>
    ///   Returns a path to the control image
    /// </summary>
    string ControlImage { get; }

    /// <summary>
    ///   Load the settings from an XElement
    /// </summary>
    /// <param name="p_element"></param>
    bool Load(XElement p_element);

    /// <summary>
    ///   Save the control settings to an XElement
    /// </summary>
    /// <returns></returns>
    XElement Save();
  }
}
