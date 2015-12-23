using System;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for a Zigbee AT Command
  /// </summary>
  public class MsgZigbeeATCmd : MsgZigbee {

    /// <summary>
    ///   Payload offset
    /// </summary>
    public new const int PAYLOAD_OFFSET = 4;

    private EsnXbeeAtCmd m_atCmd;

    /// <summary>
    ///   Default constructor
    /// </summary>
    /// <param name="p_atCmd"></param>
    public MsgZigbeeATCmd(EsnXbeeAtCmd p_atCmd) 
    {
      m_atCmd = p_atCmd;
      // Add the Xbee API
      MsgData.Add((byte) EsnXbeeAPI.AT_CMD);
      
      // HACK: Hardcoding frame ID
      // Frame ID
      MsgData.Add(0x01);

      // Copy the AT command into place
      MsgData.Add((byte)((String)XBEE_AT_CMD_STRINGS[p_atCmd])[0]);
      MsgData.Add((byte)((String)XBEE_AT_CMD_STRINGS[p_atCmd])[1]);
    }

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgZigbeeATCmd(Msg p_msg)
      : base(p_msg)
    {
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get
      {
        return "ZigbeeATCmd";
      }
    }

    /// <summary>
    ///   Information string
    /// </summary>
    public new String Information
    {
      get
      {
        return m_atCmd.ToString();
      }
    }
  }
}