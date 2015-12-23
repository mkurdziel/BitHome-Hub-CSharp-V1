using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  internal class MsgSyNetCatalogRequest : MsgSyNetTx
  {
    public MsgSyNetCatalogRequest(Msg p_msg)
      : base(p_msg)
    {
    }

    /// <summary>
    ///   Default Constructor
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    /// <param name="p_RequestType"></param>
    public MsgSyNetCatalogRequest(DeviceXBee p_xbeeTarget, byte p_RequestType)
      : base(p_xbeeTarget)
    {
      byte[] dataBytes = new byte[3];
      dataBytes[0] = (byte) EsnSyNetValues.SYNET_PACKET_START;
      dataBytes[1] = (byte) EsnAPI.CATALOG_REQUEST;
      dataBytes[2] = p_RequestType;
      base.AddData(dataBytes);
    }

    public MsgSyNetCatalogRequest(DeviceXBee p_xbeeTarget)
      : this(p_xbeeTarget, 0x00)
    {
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "SyNetCatalogRequest"; }
    }
  }
}