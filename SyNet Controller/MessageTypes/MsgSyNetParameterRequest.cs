using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for SyNet parameter request
  /// </summary>
  public class MsgSyNetParameterRequest : MsgSyNetTx
  {
    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetParameterRequest(Msg p_msg)
      : base(p_msg)
    {
    }

    /// <summary>
    ///   Default Constructor
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    /// <param name="p_functionRequest"></param>
    /// <param name="p_paramRequest"></param>
    public MsgSyNetParameterRequest(DeviceXBee p_xbeeTarget,
                                    byte p_functionRequest,
                                    byte p_paramRequest)
      : base(p_xbeeTarget)
    {
      byte[] dataBytes = new byte[4];
      dataBytes[0] = (byte) EsnSyNetValues.SYNET_PACKET_START;
      dataBytes[1] = (byte) EsnAPI.PARAMETER_REQUEST;
      dataBytes[2] = p_functionRequest;
      dataBytes[3] = p_paramRequest;
      base.AddData(dataBytes);
    }

    /// <summary>
    ///   Device Constructor
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    public MsgSyNetParameterRequest(DeviceXBee p_xbeeTarget)
      : this(p_xbeeTarget, 0x00, 0x00)
    {
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "SyNetParameterRequest"; }
    }
  }
}