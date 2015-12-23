using System;
using System.Diagnostics;

namespace SyNet.EventArguments
{
  /// <summary>
  /// CLASS Custom EventArgs identifying a new value received from a device
  /// </summary>
  public class FunctionReceivedEventArgs : EventArgs
  {
    private readonly int m_nFunctionID;
    private readonly string m_strValue;
    private readonly int m_nValue;
    private readonly bool m_bIsStringDataType;

    /// <summary>
    /// Constructor with initial data (string value returned)
    /// </summary>
    /// <param name="p_functionID">ID of function which just returned a new value</param>
    /// <param name="p_strValue">the new string typed value</param>
    public FunctionReceivedEventArgs(int p_functionID, string p_strValue)
    {
      m_nFunctionID = p_functionID;
      m_strValue = p_strValue;
      m_bIsStringDataType = true;
    }

    /// <summary>
    /// Constructor with initial data (integer value returned) 
    /// </summary>
    /// <param name="p_functionID">ID of function which just returned a new value</param>
    /// <param name="p_nValue">the new string typed value</param>
    public FunctionReceivedEventArgs(int p_functionID, int p_nValue)
    {
      m_nFunctionID = p_functionID;
      m_nValue = p_nValue;
      m_bIsStringDataType = false;
    }

    /// <summary>
    /// R/O PROPERTY: return T/F where T means we have a string value being passed by this class
    /// </summary>
    public bool IsStringDataType
    {
      get { return m_bIsStringDataType; }
    }

    /// <summary>
    /// R/O PROPERTY: return the string-typed changed value
    /// </summary>
    public string ChangedStringValue
    {
      get
      {
        Debug.Assert(
          IsStringDataType,
          "[CODE] Attempting to retrieve integer for string-valued return");
        return m_strValue;
      }
    }

    /// <summary>
    /// R/O PROPERTY: return the interger-typed changed value
    /// </summary>
    public int ChangedIntValue
    {
      get
      {
        Debug.Assert(
          !IsStringDataType,
          "[CODE] Attempting to retrieve string for integer-valued return");
        return m_nValue;
      }
    }

    /// <summary>
    /// R/O PROPERTY: return the ID of the function returning the value
    /// </summary>
    public int FunctionID
    {
      get { return m_nFunctionID; }
    }
  }
}