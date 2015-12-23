using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SyNet.Gui.Interfaces
{
  public interface IControlContainer : IGuiPanelControl
  {
    /// <summary>
    ///   Returns a readonly list of child controls
    /// </summary>
    ReadOnlyCollection<IGuiPanelControl> Children { get; }

    /// <summary>
    ///   Add a child control
    /// </summary>
    /// <param name="p_control"></param>
    bool AddChildControl(IGuiPanelControl p_control);

    /// <summary>
    ///   Remove a child control
    /// </summary>
    /// <param name="p_control"></param>
    /// <returns></returns>
    bool RemoveChildControl(IGuiPanelControl p_control);

    /// <summary>
    ///   Get the location of the actual container within the control
    /// </summary>
    Rect ContainerLocation { get; }

    /// <summary>
    ///   Returns any additional inside top margin
    /// </summary>
    double MarginInsideTop { get; }

    /// <summary>
    ///   Returns any additional inside bottom margin
    /// </summary>
    double MarginInsideBottom { get; }

    /// <summary>
    ///   Returns any additional inside left margin
    /// </summary>
    double MarginInsideLeft { get; }

    /// <summary>
    ///   Returns any addition inside right margin
    /// </summary>
    double MarginInsideRight { get; }

    /// <summary>
    ///   Returns the control configuration panel
    /// </summary>
    /// <returns></returns>
    UserControl GetConfigPanel();

  }
}
