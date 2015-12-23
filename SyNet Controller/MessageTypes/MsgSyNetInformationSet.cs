using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  internal class MsgSyNetInformationSet : MsgSyNetTx
  {
    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetInformationSet(Msg p_msg)
      : base(p_msg)
    {
    }

    public MsgSyNetInformationSet(DeviceXBee p_xbeeTarget,
                                  EsnAPIInfoValues p_infoID,
                                  byte[] p_bytes)
      : base(p_xbeeTarget)
    {
      byte[] dataBytes = new byte[3];
      dataBytes[0] = (byte) EsnSyNetValues.SYNET_PACKET_START;
      dataBytes[1] = (byte) EsnAPI.SETINFO;
      dataBytes[2] = (byte) p_infoID;
      base.AddData(dataBytes);
      base.AddData(p_bytes);
    }
  }
}