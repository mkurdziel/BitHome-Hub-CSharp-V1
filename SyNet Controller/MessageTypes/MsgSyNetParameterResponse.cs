using System;
using System.Diagnostics;
using System.Text;
using SyNet.DataHelpers;
using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for SyNet Parameter Response
  /// </summary>
  public class MsgSyNetParameterResponse : MsgSyNetRx
  {
    private const int NOT_SET = -1;

    private readonly int m_functionID;
    private readonly int m_paramID;
    private readonly String m_paramName;
    private readonly EsnDataTypes m_eParamDataType;
    private readonly EsnParamValidationType m_eValidationType;
    private readonly int m_nMaxStringLength;
    private readonly int m_nMinumumValue;
    private readonly int m_nMaximumValue;
    private readonly int m_nValueWidthInBytes;
    private readonly bool m_bIsSigned;
    private readonly SerializableDictionary<string, int> m_dctEnumValueByName;

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetParameterResponse(Msg p_msg)
      : base(p_msg)
    {
      int nByteIdx = PAYLOAD_OFFSET + 2;

      m_dctEnumValueByName = new SerializableDictionary<string, int>();
      m_nMaxStringLength = NOT_SET;
      m_nMinumumValue = NOT_SET;
      m_nMaxStringLength = NOT_SET;
      m_eValidationType = EsnParamValidationType.UNKNOWN;

      // If we're a zero entry, it's a catalog info
      //if (m_entryID != 0x00)
      //{

      // get function ID
      m_functionID = MsgData[nByteIdx++];
      // get parameter ID
      m_paramID = MsgData[nByteIdx++];
      // get parameter data type
      m_eParamDataType = (EsnDataTypes) MsgData[nByteIdx++];
      switch (m_eParamDataType)
      {
        case EsnDataTypes.BOOL:
          m_nValueWidthInBytes = 1;
          break;
        case EsnDataTypes.BYTE:
          m_nValueWidthInBytes = 1;
          break;
        case EsnDataTypes.WORD:
          m_nValueWidthInBytes = 2;
          break;
        case EsnDataTypes.VOID:
          m_nValueWidthInBytes = 0;
          break;
        case EsnDataTypes.STRING:
          m_nValueWidthInBytes = 0;
          break;
        case EsnDataTypes.DWORD:
          m_nValueWidthInBytes = 4;
          break;
        default:
          Debug.WriteLine("[ERR] - Unrecognized Parameter Data Type");
          return;
      }

      // get parameter validation type
      m_eValidationType = (EsnParamValidationType) MsgData[nByteIdx++];

      // get validation values
      switch (ValidationType)
      {
        case EsnParamValidationType.UNSIGNED_FULL:
          // full-range unsigned value
          m_bIsSigned = false;
          break;
        case EsnParamValidationType.UNSIGNED_RANGE:
          // load min and max type-width values
          nByteIdx += LoadValueGivenWidth(MsgData, nByteIdx, m_nValueWidthInBytes, out m_nMinumumValue);
          nByteIdx += LoadValueGivenWidth(MsgData, nByteIdx, m_nValueWidthInBytes, out m_nMaximumValue);
          m_bIsSigned = false;
          break;
        case EsnParamValidationType.SIGNED_FULL:
          // full-range signed value
          m_bIsSigned = true;
          break;
        case EsnParamValidationType.SIGNED_RANGE:
          m_bIsSigned = true;
          // load min and max type-width values
          nByteIdx += LoadValueGivenWidth(MsgData, nByteIdx, m_nValueWidthInBytes, out m_nMinumumValue);
          nByteIdx += LoadValueGivenWidth(MsgData, nByteIdx, m_nValueWidthInBytes, out m_nMaximumValue);
          break;
        case EsnParamValidationType.ENUMERATED:
          // load count, then value-name pairs count times
          int nNbrEnumValues = MsgData[nByteIdx++];
          int nEnumValue;
          string strEnumValueName;
          for (int nEntryIdx = 0; nEntryIdx < nNbrEnumValues; nEntryIdx++)
          {
            nByteIdx += LoadValueGivenWidth(MsgData, nByteIdx, m_nValueWidthInBytes, out nEnumValue);
            nByteIdx += GatherZeroTermString(MsgData, nByteIdx, out strEnumValueName);
            EnumValuesByNames[strEnumValueName] = nEnumValue;
          }
          break;
        case EsnParamValidationType.MAX_STRING_LEN:
          // load single byte max string length
          m_nMaxStringLength = MsgData[nByteIdx++];
          break;
      }

      // get parameter name
      nByteIdx += GatherZeroTermString(MsgData, nByteIdx, out m_paramName);
      //}
    }

    /// <summary>
    ///   Function ID this parameter belongs to
    /// </summary>
    public int FunctionID
    {
      get { return m_functionID; }
    }

    /// <summary>
    ///   Parameter ID in the message
    /// </summary>
    public int ParamID
    {
      get { return m_paramID; }
    }

    /// <summary>
    ///   Name of Parameter
    /// </summary>
    public String ParamName
    {
      get { return m_paramName; }
    }

    /// <summary>
    ///   Returns true if the parameter is signed
    /// </summary>
    public bool IsSigned
    {
      get { return m_bIsSigned; }
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "SyNetParameterResponse"; }
    }

    /// <summary>
    ///   Information string
    /// </summary>
    public override string Information
    {
      get
      {
        StringBuilder sbInfoText = new StringBuilder();
        sbInfoText.Append(FunctionID);
        sbInfoText.Append("-");
        sbInfoText.Append(ParamID);
        sbInfoText.Append(" ");
        sbInfoText.Append(ParamName);
        sbInfoText.Append(": ");

        switch (ValidationType)
        {
          case EsnParamValidationType.UNSIGNED_FULL:
            // full-range unsigned value
            sbInfoText.Append("Unsigned Full");
            break;
          case EsnParamValidationType.UNSIGNED_RANGE:
            // load min and max type-width values
            sbInfoText.Append("Unsigned Range");
            sbInfoText.Append(string.Format(" [{0}-{1}]", MinumumValue, MaximumValue));
            break;
          case EsnParamValidationType.SIGNED_FULL:
            // full-range signed value
            sbInfoText.Append("Signed Full");
            break;
          case EsnParamValidationType.SIGNED_RANGE:
            sbInfoText.Append("Signed Range");
            sbInfoText.Append(string.Format(" [{0}-{1}]", MinumumValue, MaximumValue));
            break;
          case EsnParamValidationType.ENUMERATED:
            // load count, then value-name pairs count times
            sbInfoText.Append("Table [ ");
            foreach (string strEnumName in EnumValuesByNames.Keys)
            {
              int nEnumValue = EnumValuesByNames[strEnumName];
              sbInfoText.Append(string.Format("{0}={1} ", strEnumName, nEnumValue));
            }
            sbInfoText.Append("]");
            break;
          case EsnParamValidationType.MAX_STRING_LEN:
            // load single byte max string length
            sbInfoText.Append(string.Format("string:Max {0} bytes", MaxStringLength));
            break;
        }
        //}
        return sbInfoText.ToString();
      }
    }

    /// <summary>
    ///   Minimum value
    /// </summary>
    public int MinumumValue
    {
      get { return m_nMinumumValue; }
    }

    /// <summary>
    ///   Maximum value
    /// </summary>
    public int MaximumValue
    {
      get { return m_nMaximumValue; }
    }

    /// <summary>
    ///   Maximum string length
    /// </summary>
    public int MaxStringLength
    {
      get { return m_nMaxStringLength; }
    }

    /// <summary>
    ///   Datatype of parameter
    /// </summary>
    public EsnDataTypes DataType
    {
      get { return m_eParamDataType; }
    }

    /// <summary>
    ///   Parameter validation type
    /// </summary>
    public EsnParamValidationType ValidationType
    {
      get { return m_eValidationType; }
    }

    /// <summary>
    ///   Enumeration values if present
    /// </summary>
    public SerializableDictionary<string, int> EnumValuesByNames
    {
      get { return m_dctEnumValueByName; }
    }
  }
}