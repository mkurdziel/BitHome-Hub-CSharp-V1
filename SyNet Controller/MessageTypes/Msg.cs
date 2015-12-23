using System;
using System.Collections.Generic;
using System.Text;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Generic Message
  /// </summary>
  [Serializable]
  public class Msg : Object {
    #region Member Variables

    private EsnMsgFormat m_msgFormat = EsnMsgFormat.INVALID;

    private DateTime          m_frameTimestamp;
    private EsnFrameStatus    m_frameFrameStatus     = EsnFrameStatus.FRAME_EMPTY;
    private EsnFrameDirection m_frameDirection  = EsnFrameDirection.RCVD;
    private bool              m_isProcessed     = false;
    private bool              m_isFinalized     = false;
    private readonly List<byte> m_msgData;

    #endregion

    #region Enumerations

    /// <summary>
    ///   Enumation for the type of message this is
    /// </summary>
    public enum EsnMsgFormat
    {
      /// <summary>
      ///   Invalid Message
      /// </summary>
      INVALID,
      /// <summary>
      ///   Zigbee Message
      /// </summary>
      ZIGBEE
    }

    /// <summary>
    ///   Enumeration for the status of the message frame
    /// </summary>
    public enum EsnFrameStatus {
      /// <summary>
      ///   Frame is empty
      /// </summary>
      FRAME_EMPTY,
      /// <summary>
      ///   Message tpe
      /// </summary>
      FRAME_TYPE,
      /// <summary>
      ///   First byte of size
      /// </summary>
      FRAME_SIZE_PARTIAL,
      /// <summary>
      ///   Message size
      /// </summary>
      FRAME_SIZE,
      /// <summary>
      ///   Message data
      /// </summary>
      FRAME_DATA,
      /// <summary>
      ///   Message checksum
      /// </summary>
      FRAME_CHECKSUM,
      /// <summary>
      ///   Message complete
      /// </summary>
      FRAME_COMPLETE
    }

    /// <summary>
    ///   Enumeration for the direction of the message
    /// </summary>
    public enum EsnFrameDirection {
      /// <summary>
      ///   Outgoing message
      /// </summary>
      SENT,
      /// <summary>
      ///   Incoming message
      /// </summary>
      RCVD
    }

    /// <summary>
    ///   Enumeration for frame errors
    /// </summary>
    public enum EsnFrameError {
      /// <summary>
      ///   No errors
      /// </summary>
      NONE,
      /// <summary>
      ///   Incorrect frame start byte
      /// </summary>
      INCORRECT_START_INDICATOR,
      /// <summary>
      ///   Incorrect frame size
      /// </summary>
      INCORRECT_FRAME_SIZE,
      /// <summary>
      ///   Extra frame data
      /// </summary>
      EXTRA_FRAME_DATA,
      /// <summary>
      ///   Invalid frame checksum
      /// </summary>
      INVALID_CHECKSUM
    }

    #endregion

    #region Properties

    /// <summary>
    ///   Returns the format of the message
    /// </summary>
    public EsnMsgFormat MsgFormat
    {
      get { return m_msgFormat; }
      protected set { m_msgFormat = value;}
    }

    /// <summary>
    ///   Returns a list of data bytes in the message
    /// </summary>
    public List<byte> MsgData
    {
      get { return m_msgData; }
    }

    /// <summary>
    ///   Setter for is the message has been parsed
    /// </summary>
    public bool IsProcessed
    {
      get { return m_isProcessed; }
      protected set { m_isProcessed = value; }
    }

    /// <summary>
    ///   Returns true if packet is IsComplete
    /// </summary>
    public bool IsComplete
    {
      get { return m_frameFrameStatus == EsnFrameStatus.FRAME_COMPLETE; }
    }

    /// <summary>
    ///   Makes sure the message is finalied
    /// </summary>
    public bool IsFinalized
    {
      get { return m_isFinalized; }
      protected set { m_isFinalized = value; }
    }

    /// <summary>
    ///   Length of the frame data payload
    /// </summary>
    public int Length
    {
      get
      {
        return m_msgData.Count;
      }
    }

    /// <summary>
    ///   Get the Frame ID
    /// </summary>
    public virtual byte? FrameID
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    ///   Return the frame direction
    /// </summary>
    public EsnFrameDirection Direction
    {
      get { return m_frameDirection; }
      protected set { m_frameDirection = value;}
    }
    /// <summary>
    ///   Return frame status
    /// </summary>
    public EsnFrameStatus FrameStatus
    {
      get { return m_frameFrameStatus; }
      protected set { m_frameFrameStatus = value;}
    }

    /// <summary>
    ///   Return the packet timestamp
    /// </summary>
    public DateTime Timestamp
    {
      get { return m_frameTimestamp; }
      protected set { m_frameTimestamp = value;}
    }

    /// <summary>
    ///   Return byte array of full frame
    /// </summary>
    public byte[] Bytes
    {
      get
      {
        return m_msgData.ToArray();
      }
    }

    /// <summary>
    ///   Returns the data of the frame as a string
    /// </summary>
    public String ByteString
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        int count = 0;
        foreach (byte b in (m_msgData)) {
          if (count++ % 8 == 0) {
            //sb.Append("\n");
          }

          sb.Append(Convert.ToString(b, 16).ToUpper().PadLeft(2, '0') + " ");
        }
        return sb.ToString();
      }
    }

    /// <summary>
    ///   Message type identifier
    /// </summary>
    public virtual String MsgType
    {
      get
      {
        return "Msg";
      }
    }

    /// <summary>
    ///   Returns the data of the frame as a string
    /// </summary>
    public virtual String Information
    {
      get
      {
        return "";
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   Default contstructor
    /// </summary>
    public Msg()
    {
      m_msgData = new List<byte>();
    }

    /// <summary>
    ///  Copy Constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public Msg(Msg p_msg) : this()
    {
      m_msgData.AddRange(p_msg.MsgData);
      m_frameTimestamp = DateTime.Now;
      m_frameFrameStatus = p_msg.FrameStatus;
      m_frameDirection = p_msg.Direction;
      m_isProcessed = p_msg.IsProcessed;
      m_isFinalized = p_msg.IsFinalized;
    }

    #endregion

    /// <summary>
    ///   Return 
    /// </summary>
    public override String ToString()
    {
      return String.Format("{0}", Information);
    }

    /// <summary>
    ///   Virtual method to finalize the frame
    /// </summary>
    public virtual void FinalizeFrame()
    {

    }
  }
}