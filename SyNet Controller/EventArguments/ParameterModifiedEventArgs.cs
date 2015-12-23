using System;

namespace SyNet.EventArguments
{

  /// <summary>
  ///   Parameter modification event arguments
  /// </summary>
  public class ParameterModifiedEventArgs : EventArgs
  {
    /// <summary>
    ///   Parameter ID that was modified
    /// </summary>
    public UInt64 ParamID { get; set; }

    /// <summary>
    ///   Enumeration for the type of parameter modification
    /// </summary>
    public enum EsnModifyType
    {
      /// <summary>
      ///   Parameter type was modified
      /// </summary>
      PARAM_TYPE
    };

    /// <summary>
    ///   Type of parameter modification
    /// </summary>
    public EsnModifyType ModifyType { get; set; }


    /// <summary>
    /// Constructor with initial data
    /// </summary>
    public ParameterModifiedEventArgs(UInt64 p_paramID, EsnModifyType p_type)
    {
      ParamID = p_paramID;
      ModifyType = p_type;
    }
  }
}