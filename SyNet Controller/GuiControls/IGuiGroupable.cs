using System;

namespace SyNet.GuiControls
{
  /// <summary>
  ///   Interface for gui items that are groupable
  /// </summary>
  public interface IGuiGroupable
  {
    /// <summary>
    ///   Unique ID of this item
    /// </summary>
    Guid ID { get; }

    /// <summary>
    ///   Parent ID of group
    /// </summary>
    Guid ParentID { get; set; }

    /// <summary>
    ///   Returns true if this object is a group
    /// </summary>
    bool IsGroup { get; set; }
  }
}