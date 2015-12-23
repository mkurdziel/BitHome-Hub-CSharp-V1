using System;

namespace SyNet.EventArguments
{

  /// <summary>
  ///   Event args for ActionModifiedEvent
  /// </summary>
  public class ActionModifiedEventArgs : EventArgs
  {
    /// <summary>
    ///   Enumeration for the type of action modification
    /// </summary>
    public enum EsnActionModifyType
    {
      /// <summary>
      ///   Parameter change
      /// </summary>
      PARAM_CHANGE
    } ;

    /// <summary>
    ///   Modification Type
    /// </summary>
    public EsnActionModifyType ModifyType { get; set; }

    /// <summary>
    ///   Action ID that was modified
    /// </summary>
    public UInt64 ActionID { get; set; }

    /// <summary>
    /// Constructor with initial data
    /// </summary>
    public ActionModifiedEventArgs( UInt64 p_actionID, 
                                    EsnActionModifyType p_type )
    {
      ActionID = p_actionID;
      ModifyType = p_type;
    }
  }
}