using System;
using System.Diagnostics;
using System.Text;
using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for a SyNet catalog response
  /// </summary>
  public class MsgSyNetCatalogResponse : MsgSyNetRx
  {
    private readonly string         m_information;
    private readonly byte           m_entryID;
    private readonly byte           m_totalEntries;
    private readonly byte           m_numParams;
    private readonly EsnDataTypes   m_returnType;
    private readonly EsnDataTypes[] m_paramTypes;
    private readonly string         m_functionName;

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetCatalogResponse(Msg p_msg)
      : base(p_msg)
    {
      m_totalEntries = MsgData[PAYLOAD_OFFSET + 2];
      m_entryID = MsgData[PAYLOAD_OFFSET + 3];

      StringBuilder sbinfo = new StringBuilder();
      sbinfo.Append(EntryID);
      sbinfo.Append("/");
      sbinfo.Append(TotalEntries);

      // If we're a zero entry, it's a catalog info

      if (m_entryID != 0x00)
      {
        m_numParams = MsgData[PAYLOAD_OFFSET + 4];
        m_returnType = ToDataType(MsgData[PAYLOAD_OFFSET + 5]);
        m_paramTypes = new EsnDataTypes[m_numParams];

        int i;
        for (i = 0; i < m_numParams; i++)
        {
          m_paramTypes[i] = ToDataType(MsgData[PAYLOAD_OFFSET + 6 + i]);
        }

        StringBuilder sbname = new StringBuilder();
        char c;
        i = 0;
        while (
          (c = (char) MsgData[PAYLOAD_OFFSET + 6 + m_numParams + (i++)]) != 0x00)
        {
          sbname.Append(c);
          if (i >= MsgData.Count)
          {
            Debug.WriteLine("[ERR] SyNetCatalogResponse Name out of bounds");
          }
        }
        m_functionName = sbname.ToString();

        sbinfo.Append(" ");
        sbinfo.Append(ReturnType);
        sbinfo.Append(" ");
        sbinfo.Append(FunctionName);
        sbinfo.Append("(");
        i = 0;
        foreach (EsnDataTypes type in ParamTypes)
        {
          if ((i++) != 0)
          {
            sbinfo.Append(",");
          }
          sbinfo.Append(type);
        }
        sbinfo.Append(")");
      }
      m_information = sbinfo.ToString();
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "SyNetCatalogResponse"; }
    }

    /// <summary>
    ///   Information string
    /// </summary>
    public override string Information
    {
      get { return m_information; }
    }

    /// <summary>
    ///   Total entries in the response
    /// </summary>
    public byte TotalEntries
    {
      get { return m_totalEntries; }
    }

    /// <summary>
    ///   Entry ID
    /// </summary>
    public byte EntryID
    {
      get { return m_entryID; }
    }

    /// <summary>
    ///   Number of parameters for the entry
    /// </summary>
    public byte NumParams
    {
      get { return m_numParams; }
    }

    /// <summary>
    ///   Return type of the entry
    /// </summary>
    public EsnDataTypes ReturnType
    {
      get { return m_returnType; }
    }

    /// <summary>
    ///   Array of parameter types
    /// </summary>
    public EsnDataTypes[] ParamTypes
    {
      get { return m_paramTypes; }
    }

    /// <summary>
    ///   Function name
    /// </summary>
    public String FunctionName
    {
      get { return m_functionName; }
    }

    /// <summary>
    ///   Converts a byte into a data type enum
    /// </summary>
    /// <param name="p_byte"></param>
    /// <returns></returns>
    public EsnDataTypes ToDataType(byte p_byte)
    {
      return (EsnDataTypes)
             Enum.ToObject(typeof (EsnDataTypes), p_byte);
    }
  }
}