using System;
using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for all SyNet Transmit messages
  /// </summary>
  public class MsgSyNetTx : MsgZigbeeTxRequest {

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetTx(Msg p_msg)
      : base(p_msg)
    {
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    public MsgSyNetTx(DeviceXBee p_xbeeTarget)
      : base(p_xbeeTarget, null)
    {
    }

    /// <summary>
    ///   SyNet API type of this message
    /// </summary>
    public EsnAPI SyNetAPI
    {
      get
      {
        return (EsnAPI)Enum.ToObject(typeof(EsnAPI), 
          MsgData[MsgZigbeeTxRequest.PAYLOAD_OFFSET]);
      }
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get
      {
        return "SyNetTx";
      }
    }
  }
}