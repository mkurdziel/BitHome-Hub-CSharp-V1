using SyNet.MessageTypes;
using SyNet.Protocol;

namespace SyNet
{
  /// <summary>
  ///   Message factory to create message derivations
  /// </summary>
  public static class MsgFactory
  {
    /// <summary>
    ///   Create a message sub-class based on the zigbee API
    /// </summary>
    /// <param name="p_msg"></param>
    /// <returns></returns>
    public static Msg CreateMessage(MsgZigbee p_msg)
    {
      switch (p_msg.ZigbeeAPI)
      {
        case MsgZigbee.EsnXbeeAPI.MODEM_STATUS:
          return new MsgZigbeeModemStatus(p_msg);

        case MsgZigbee.EsnXbeeAPI.AT_CMD:
          return new MsgZigbeeATCmd(p_msg);

        case MsgZigbee.EsnXbeeAPI.AT_CMD_PARAMETER:
          return new MsgZigbeeATCmdQueueParam(p_msg);

        case MsgZigbee.EsnXbeeAPI.AT_CMD_RESPONSE:
          return new MsgZigbeeATCmdResponse(p_msg);

        case MsgZigbee.EsnXbeeAPI.ZIGBEE_TX_REQ:
          switch (p_msg.MsgData[MsgZigbeeTxRequest.PAYLOAD_OFFSET + 1])
          {
            case (byte) EsnAPI.CATALOG_REQUEST:
              return new MsgSyNetCatalogRequest(p_msg);
            case (byte) EsnAPI.DEVICE_STATUS_REQUEST:
              return new MsgSyNetDeviceStatusRequest(p_msg);
            case (byte) EsnAPI.BOOTLOAD_TRANSMIT:
              return new MsgSyNetBootloadTransmit(p_msg);
          }
          return new MsgZigbeeTxRequest(p_msg);
        case MsgZigbee.EsnXbeeAPI.ZIGBEE_TX_STATUS:
          return new MsgZigbeeTxStatus(p_msg);

        case MsgZigbee.EsnXbeeAPI.ZIGBEE_RX:
          switch (p_msg.MsgData[MsgZigbeeRx.PAYLOAD_OFFSET + 1])
          {
            case (byte) EsnAPI.DEVICE_STATUS:
              return new MsgSyNetDeviceStatus(p_msg);
            case (byte) EsnAPI.BOOTLOAD_RESPONSE:
              return new MsgSyNetBootloadResponse(p_msg);
            case (byte) EsnAPI.CATALOG_RESPONSE:
              return new MsgSyNetCatalogResponse(p_msg);
            case (byte) EsnAPI.PARAMETER_RESPONSE:
              return new MsgSyNetParameterResponse(p_msg);
            case (byte) EsnAPI.FUNCTION_RECEIVE:
              return new MsgSyNetFunctionReceive(p_msg);
          }
          return new MsgZigbeeRx(p_msg);
      }
      return new MsgZigbee(p_msg);
    }

    /// <summary>
    ///   Create a message subclass from a generic message
    /// </summary>
    /// <param name="p_msg"></param>
    /// <returns></returns>
    public static Msg CreateMessage(Msg p_msg)
    {
      if (p_msg.MsgFormat == Msg.EsnMsgFormat.ZIGBEE)
      {
        return CreateMessage(p_msg as MsgZigbee);
      }
      return new Msg(p_msg);
    }
  }
}