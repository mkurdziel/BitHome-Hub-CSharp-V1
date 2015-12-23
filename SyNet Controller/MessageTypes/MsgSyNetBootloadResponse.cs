using System;
using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for SyNet Booload response
  /// </summary>
  public class MsgSyNetBootloadResponse : MsgSyNetRx {

    private readonly UInt16 m_memoryAddress;
    private readonly EsnAPIBootloadResponse m_response;
    private readonly String m_information;

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetBootloadResponse(Msg p_msg)
      : base(p_msg)
    {
      m_response = (EsnAPIBootloadResponse)
                   Enum.ToObject(
                     typeof(EsnAPIBootloadResponse),
                     MsgData[PAYLOAD_OFFSET + 2]);

      
      // Memory address is only valid for a data success command
      if (m_response == EsnAPIBootloadResponse.DATA_SUCCESS)
      {
        m_memoryAddress = EBitConverter.ToUInt16(MsgData, PAYLOAD_OFFSET + 3);
      }

      m_information = Response.ToString();
    }

    #region Public Properties

    /// <summary>
    ///   Gets the bootload response from the device
    /// </summary>
    public EsnAPIBootloadResponse Response
    {
      get
      {
        return m_response;
      }
    }

    /// <summary>
    ///   Gets the memory address of the bootload response
    /// </summary>
    public UInt16 MemoryAddress
    {
      get
      {
        return m_memoryAddress;
      }
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get
      {
        return "SyNetBootloadResponse";
      }
    }

    /// <summary>
    ///   Gets information about the message
    /// </summary>
    public override string Information
    {
      get
      {
        return m_information;
      }
    }

    #endregion
  }
}
