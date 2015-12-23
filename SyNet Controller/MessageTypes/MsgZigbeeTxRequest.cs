
namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for all zigbee transmit requests
  /// </summary>
  public class MsgZigbeeTxRequest : MsgZigbee {

    /// <summary>
    ///   Payload offset for this message type
    /// </summary>
    public new const int PAYLOAD_OFFSET = 17; 

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgZigbeeTxRequest(Msg p_msg)
      : base(p_msg)
    {
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    public MsgZigbeeTxRequest(DeviceXBee p_xbeeTarget)
      : this(p_xbeeTarget, null)
    {
    }

    /// <summary>
    ///   Constructor with payload
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    /// <param name="p_data"></param>
    public MsgZigbeeTxRequest(DeviceXBee p_xbeeTarget, byte[] p_data)
    {
      if(p_data == null)
      {
        p_data = new byte[0];
      }

      // Set the API
      MsgData.Add((byte)EsnXbeeAPI.ZIGBEE_TX_REQ);


      // Create a new frame ID
      //m_frameIDCurrent++;

      // HACK: hardcoded frame id
      // Copy Frame ID into place
      MsgData.Add(0x00);

      // copy in 64-bit address
      if (p_xbeeTarget.IsBroadcast) {
        MsgData.AddRange(EBitConverter.GetBytes(DeviceXBee.MY64_BITADDRESS_BROADCAST));
      } else {
        MsgData.AddRange(EBitConverter.GetBytes(p_xbeeTarget.SerialNumber));
      }

      // copy in 16-bit address
      if (p_xbeeTarget.IsBroadcast)
      {
        MsgData.AddRange(EBitConverter.GetBytes(DeviceXBee.MY16_BITADDRESS_BROADCAST));
      } else {
        MsgData.AddRange(EBitConverter.GetBytes(p_xbeeTarget.NetworkAddress));
      }

      // Broadcast Radius
      MsgData.Add(0x00);

      // Options
      if (p_xbeeTarget.IsBroadcast)
      {
        MsgData.Add(0x08);
      } else {
        MsgData.Add(0x00);
      }

      if (p_data.Length > 0) {
        MsgData.AddRange(p_data);
      }

      //m_queueFrameID = m_frameIDCurrent;

    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get
      {
        return "ZigbeeTxRequest";
      }
    }

    /// <summary>
    ///   Return the payload offset
    /// </summary>
    public int PayloadOffset
    {
      get { return PAYLOAD_OFFSET; }
    }

    /// <summary>
    ///   Add an array of bytes to the payload data
    /// </summary>
    /// <param name="p_data"></param>
    /// <returns></returns>
    public bool AddData(byte[] p_data)
    {
      if (p_data.Length>0)
      {
        // Create a new larger data segment
        MsgData.AddRange(p_data);
      }
      return true;
    }

  }
}