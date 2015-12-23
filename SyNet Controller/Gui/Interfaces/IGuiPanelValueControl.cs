using System;

namespace SyNet.Gui.Interfaces
{
  public interface IGuiPanelValueControl : IGuiPanelControl
  {
    /// <summary>
    ///   Gets or sets the value of this control
    /// </summary>
    String Value
    {
      get;
      set;
    }

    /// <summary>
    ///   Returns true if this control can input a value from the user
    /// </summary>
    bool CanInputValue { get; }
  }
}
