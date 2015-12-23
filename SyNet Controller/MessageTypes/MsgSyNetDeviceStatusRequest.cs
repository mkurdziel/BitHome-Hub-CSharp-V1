using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for SyNet status request
  /// </summary>
  public class MsgSyNetDeviceStatusRequest : MsgSyNetTx
  {
    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetDeviceStatusRequest(Msg p_msg)
      : base(p_msg)
    {
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    /// <param name="p_req"></param>
    public MsgSyNetDeviceStatusRequest(DeviceXBee p_xbeeTarget,
                                       EsnAPIDeviceStatusRequest p_req)
      : base(p_xbeeTarget)
    {
      byte[] dataBytes = new byte[3];
      dataBytes[0] = (byte) EsnSyNetValues.SYNET_PACKET_START;
      dataBytes[1] = (byte) EsnAPI.DEVICE_STATUS_REQUEST;
      dataBytes[2] = (byte) p_req;
      base.AddData(dataBytes);
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "SyNetDeviceStatusRequest"; }
    }
  }
}