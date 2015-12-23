using System;

namespace SyNet.MessageTypes
{
  internal class MsgZigbeeModemStatus : MsgZigbee
  {
    public MsgZigbeeModemStatus(Msg p_msg) : base(p_msg)
    {
    }

    public EsnFrameDataModemStatus ModemStatus
    {
      get
      {
        return (EsnFrameDataModemStatus)
               Enum.ToObject(typeof (EsnFrameDataModemStatus),
                             MsgData[PAYLOAD_OFFSET + 1]);
      }
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "ZigbeeModemStatus"; }
    }

    public new String Information
    {
      get { return ModemStatus.ToString(); }
    }
  }
}