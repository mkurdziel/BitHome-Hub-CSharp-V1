using System;
using System.Diagnostics;
using System.Text;
using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for a SyNet function receive
  /// </summary>
  public class MsgSyNetFunctionReceive : MsgSyNetRx
  {
    /// <summary>
    ///   Parameter offset
    /// </summary>
    public const int PARAM_OFFSET = PAYLOAD_OFFSET + 3;

    private readonly byte m_nFunctionID;
    private readonly EsnDataTypes m_nDataType;
    private readonly int m_nValue;
    private readonly string m_strValue;
    private readonly bool m_bValue;
    private readonly bool m_bIsInteger;
    private readonly string m_strTypeName;


    /// <summary>
    ///   Copy Constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetFunctionReceive(Msg p_msg)
      : base(p_msg)
    {
      int nPayloadIdx = PAYLOAD_OFFSET + 2;
      m_nFunctionID = MsgData[nPayloadIdx++];
      m_nDataType = (EsnDataTypes) MsgData[nPayloadIdx++];
      m_nValue = 0;
      m_strValue = string.Empty;
      m_bValue = false;
      m_bIsInteger = false;

      int nConversionLengthInBytes;

      switch (DataType)
      {
        case EsnDataTypes.BOOL:
          m_bValue = (MsgData[nPayloadIdx] == 0) ? false : true;
          m_strTypeName = "BOOL";
          break;
        case EsnDataTypes.BYTE:
          nConversionLengthInBytes = 1;
          LoadValueGivenWidth(MsgData, nPayloadIdx, nConversionLengthInBytes, out m_nValue);
          m_bIsInteger = true;
          m_strTypeName = "BYTE";
          break;
        case EsnDataTypes.STRING:
          GatherZeroTermString(MsgData, nPayloadIdx, out m_strValue);
          m_strTypeName = "STRING";
          break;
        case EsnDataTypes.DWORD:
          nConversionLengthInBytes = 3;
          LoadValueGivenWidth(MsgData, nPayloadIdx, nConversionLengthInBytes, out m_nValue);
          m_bIsInteger = true;
          m_strTypeName = "DWORD";
          break;
        case EsnDataTypes.VOID:
          m_strTypeName = "VOID";
          break;
        case EsnDataTypes.WORD:
          nConversionLengthInBytes = 2;
          LoadValueGivenWidth(MsgData, nPayloadIdx, nConversionLengthInBytes, out m_nValue);
          m_bIsInteger = true;
          m_strTypeName = "WORD";
          break;
        default:
          Debug.Assert(false, string.Format("Unexpected DATA_TYPE value parsed from SyNetFunctionReceive message"));
          m_strTypeName = "{unknown}";
          break;
      }
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "SyNetFunctionReceive"; }
    }

    /// <summary>
    ///   Return the function ID
    /// </summary>
    public byte FunctionID
    {
      get { return m_nFunctionID; }
    }

    /// <summary>
    ///   Return the Type of the Data
    /// </summary>
    public EsnDataTypes DataType
    {
      get { return m_nDataType; }
    }

    /// <summary>
    ///   Return string value
    /// </summary>
    public string ReturnStrValue
    {
      get { return m_strValue; }
    }

    /// <summary>
    ///   Return boolean value
    /// </summary>
    public bool ReturnBoolValue
    {
      get { return m_bValue; }
    }

    /// <summary>
    ///   Return integer value
    /// </summary>
    public int ReturnIntValue
    {
      get { return m_nValue; }
    }

    /// <summary>
    ///   Returns true if the datatype is a boolean
    /// </summary>
    public bool IsBoolType
    {
      get { return DataType == EsnDataTypes.BOOL; }
    }

    /// <summary>
    ///   Returns true if the datatype is a string
    /// </summary>
    public bool IsStringType
    {
      get { return DataType == EsnDataTypes.STRING; }
    }

    /// <summary>
    ///   Returns true if the datatype is an integer
    /// </summary>
    public bool IsIntegerType
    {
      get { return m_bIsInteger; }
    }

    /// <summary>
    ///   Information about the packet
    /// </summary>
    public override String Information
    {
      get
      {
        StringBuilder sbInfoText = new StringBuilder();
        sbInfoText.Append(string.Format("{0} {1}: ", FunctionID, m_strTypeName));
        if (IsBoolType)
        {
          sbInfoText.Append((m_bValue) ? "TRUE" : "FALSE");
        }
        else if (IsIntegerType)
        {
          sbInfoText.Append(m_nValue.ToString());
        }
        else if (IsStringType)
        {
          sbInfoText.Append(m_strValue);
        }
        else
        {
          sbInfoText.Append("?????");
        }
        return sbInfoText.ToString();
      }
    }
  }
}