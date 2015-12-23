using System;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for a ZigBee received message
  /// </summary>
  public class MsgZigbeeRx : MsgZigbee {

    /// <summary>
    ///   Payload offset of message
    /// </summary>
    public new const int PAYLOAD_OFFSET = 15;

    private UInt64 m_my64BitAddress;
    private UInt16 m_my16BitAddress;

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgZigbeeRx(Msg p_msg)
      :  base(p_msg)
    {
      m_my64BitAddress =  EBitConverter.ToUInt64(MsgData, base.PAYLOAD_OFFSET+1);
      m_my16BitAddress =  EBitConverter.ToUInt16(MsgData, base.PAYLOAD_OFFSET+9);
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public MsgZigbeeRx()
    {
      
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get
      {
        return "ZigbeeRx";
      }
    }

    /// <summary>
    ///   Returns 64-bit address if valid message type, otherwise null
    /// </summary>
    public UInt64 My64BitAddress
    {
      get
      {
        return m_my64BitAddress;
      }
    }

    /// <summary>
    ///   Returns 16-bit address if valid message type, otherwise null
    /// </summary>
    public UInt16 My16BitAddress
    {
      get
      {
        return m_my16BitAddress;
      }
    }
  }
}