using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for SyNet bootload transmit
  /// </summary>
  public class MsgSyNetBootloadTransmit : MsgSyNetTx
  {
    private readonly EsnAPIBootloadTransmit m_state;

    /// <summary>
    ///   Copy Constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetBootloadTransmit(Msg p_msg)
      : base(p_msg)
    {
    }

    /// <summary>
    ///   Constructor with a target, state, and data
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    /// <param name="p_state"></param>
    /// <param name="p_data"></param>
    public MsgSyNetBootloadTransmit(DeviceXBee p_xbeeTarget,
                                    EsnAPIBootloadTransmit p_state,
                                    byte[] p_data)
      : base(p_xbeeTarget)
    {
      // Hold local copies for ease
      m_state = p_state;

      // Copy data into frame
      int dataLength = p_data != null ? p_data.Length : 0;

      byte[] dataBytes = new byte[3 + dataLength];
      dataBytes[0] = (byte) EsnSyNetValues.SYNET_PACKET_START;
      dataBytes[1] = (byte) EsnAPI.BOOTLOAD_TRANSMIT;
      dataBytes[2] = (byte) p_state;
      if (dataLength > 0 && p_data != null)
      {
        p_data.CopyTo(dataBytes, 3);
      }
      AddData(dataBytes);
    }

    /// <summary>
    ///   Constructor with a target and state
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    /// <param name="p_state"></param>
    public MsgSyNetBootloadTransmit(DeviceXBee p_xbeeTarget,
                                    EsnAPIBootloadTransmit p_state)
      : this(p_xbeeTarget, p_state, null)
    {
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "SyNetBootloadTransmit"; }
    }

    /// <summary>
    ///   Information about the packet
    /// </summary>
    public override string Information
    {
      get { return m_state.ToString(); }
    }

    /// <summary>
    ///   Bootload state
    /// </summary>
    public EsnAPIBootloadTransmit BootloadState
    {
      get { return m_state; }
    }
  }
}