using System;
using System.Text;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for XBee AT Command Response
  /// </summary>
  public class MsgZigbeeATCmdResponse : MsgZigbee
  {
    /// <summary>
    ///   Payload offset of the message type
    /// </summary>
    public new const int PAYLOAD_OFFSET = 8;

    private readonly EsnXbeeAtCmd m_cmd;

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgZigbeeATCmdResponse(Msg p_msg)
      : base(p_msg)
    {
      m_cmd = AtCmdFromChars((char) MsgData[base.PAYLOAD_OFFSET + 2], (char) MsgData[base.PAYLOAD_OFFSET + 3]);
    }

    /// <summary>
    ///   AT Command in the Message
    /// </summary>
    public EsnXbeeAtCmd ATCmd
    {
      get { return m_cmd; }
    }

    /// <summary>
    ///   Status of the AT Command
    /// </summary>
    public EsnFrameDataAtResposeStatus CmdStatus
    {
      get
      {
        return (EsnFrameDataAtResposeStatus)
               Enum.ToObject(typeof (EsnFrameDataAtResposeStatus),
                             (int) MsgData[PAYLOAD_OFFSET + 4]);
      }
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "ZigbeeATCmdResponse"; }
    }

    /// <summary>
    ///   Override Msg Information property
    /// </summary>
    public override String Information
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        // appent at command
        sb.Append(ATCmd);
        sb.Append(" - ");
        sb.Append(FrameStatus);

        return sb.ToString();
      }
    }

    /// <summary>
    ///   Get the FrameID
    /// </summary>
    public override byte? FrameID
    {
      get { return MsgData[PAYLOAD_OFFSET + 1]; }
    }
  }
}