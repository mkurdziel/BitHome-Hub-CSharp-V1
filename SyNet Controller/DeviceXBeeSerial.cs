using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Xml.Serialization;
using SyNet.MessageTypes;

namespace SyNet
{
  /// <summary>
  ///   Represents a serially attached Zigbee Device
  /// </summary>
  [XmlRoot("ZigbeeSerialDevice")]
  public class DeviceXBeeSerial : DeviceXBee
  {

    // our serial channel
    private readonly SerialPort m_spSerialPort;
    // any partial message recorded
    private MsgZigbee m_partialMsg;
    private bool m_bIsPartialMsg;
    private bool m_serialConnected;

    // lock objects
    private readonly Object m_partialMsgProxy;
    private readonly Object m_queueProxy;
    // Reset event

    ////////////////////////////////////////////////////////////////////////////
    #region Constructors

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public DeviceXBeeSerial()
    {
      Debug.WriteLine("[DBG] SyNetXBeeDeviceSerial - Constructor");
      m_spSerialPort = new SerialPort();
      m_partialMsgProxy = new Object();
      m_queueProxy = new Object();
    }

    #endregion Constructors
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region Properties

    /// <summary>
    ///   Returns true if the serial port is connected
    /// </summary>
    public bool IsSerialConnected
    {
      get { return m_serialConnected; }
      private set
      {
        m_serialConnected = value;
        OnPropertyChanged("IsSerialConnected");
      }
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region Public I/O Routines

    /// <summary>
    /// Open the XBee interface using to appropriate bus speed and message size
    /// </summary>
    public void Open()
    {
      Debug.WriteLine("[DBG] SyNetDeviceSerialXBee:Open() - ENTRY");
      // setup our hardware to talk with propCa
      InitSerialPort();

      // now initialize our propCan and verify
      OpenXBee();
    }

    /// <summary>
    /// We're done with this interface, shut it down
    /// </summary>
    public void Close()
    {
      Debug.WriteLine("[DBG] SyNetDeviceSerialXBee:Close() - ENTRY");

      // close propCan access
      CloseXBee();
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region Private I/O Functions

    private const int TIMEOUT_MS_STARTUP = 50; // 50 milliseconds

    /// <summary>
    ///   Initialize the serial port with proper parameters
    /// </summary>
    protected void InitSerialPort()
    {
      try
      {
        SyNetSettings sySettings = SyNetSettings.Instance;
        Debug.WriteLine("[DBG] SyNetDeviceSerial:InitSerialPort() - ENTRY");
        // setup our serial port
        m_spSerialPort.BaudRate = sySettings.SerialBaud;
        m_spSerialPort.Parity = Parity.None; // our traditional N81
        m_spSerialPort.DataBits = 8;
        m_spSerialPort.StopBits = StopBits.One;
        if (sySettings.SerialComPort != String.Empty)
        {
          m_spSerialPort.PortName = sySettings.SerialComPort;
        }
        else
        {
          throw new Exception("No com port selected");
        }
        m_spSerialPort.Handshake = Handshake.None; // FTDI chip requires off
        //m_spSerialPort.WriteTimeout = 2;  // on writes, 2mSec!
        //m_spSerialPort.NewLine = "\r"; // <CR> for XBee
        m_spSerialPort.ReadTimeout = TIMEOUT_MS_STARTUP; // read will timeout after 20mSec during startup
        m_spSerialPort.ErrorReceived += SerialPort_OnErrorReceived;
        m_spSerialPort.DataReceived += SerialPort_DataReceived;

        // attempt open
        // Initialize Serial hardware
        m_spSerialPort.Open();
        IsSerialConnected = true;
        // ensure start with no data in buffers
        FlushSerialPort();
      }
      catch (Exception e)
      {
        Debug.WriteLine("[ERR] Init Error: " + e.Message, "Serial port Open ERROR");
      }

    }

    /// <summary>
    ///   Close the XBee device on the serial port
    /// </summary>
    private void CloseXBee()
    {
      Debug.WriteLine("[DBG] SyNetDeviceSerialXBee:CloseXBee() - ENTRY");

      // flush contents
      m_spSerialPort.DiscardInBuffer(); // flush remaining bytes

      // write close command and wait for either <CR> or <BELL> (ok or error) (time-out is the only bad response here)
      //if(SingleCharResponseTo(STR_CMD_CLOSE, "On (C) for shutdown - Close open CAN Channel") == Char_TIMEOUT)
      //{
      // notice our non-response to close
      //  MessageBox.Show("No response received on close", "Warning on Close", MessageBoxButtons.OK,
      //                  MessageBoxIcon.Warning);
      //}
      // close serial port access
      m_spSerialPort.Close();
    }

    /// <summary>
    ///   Open the XBee device on the serial port
    /// </summary>
    private void OpenXBee()
    {
      Debug.WriteLine("[DBG] SyNetDeviceSerialXBee:OpenXBee() - ENTRY");
      //
      //  Let's reset the port to reset the propeller chip into autobaud mode
      //
      m_spSerialPort.DtrEnable = false;
      Thread.Sleep(1);
      m_spSerialPort.DtrEnable = true;
      Thread.Sleep(2000); // FTDI reset is 2 sec's long?!
    }

    /// <summary>
    ///   Flush the input and output buffer on the serial port
    /// </summary>
    protected void FlushSerialPort()
    {
      //Debug.WriteLine("[DBG] SyNetDeviceSerial:FlushSerial() - ENTRY");
      // ensure start with no data in buffers: in or out
      m_spSerialPort.DiscardOutBuffer();
      m_spSerialPort.DiscardInBuffer();
      while (m_spSerialPort.BytesToRead > 0)
      {
        Debug.WriteLine("[ERR] Bytes left to read - clearing");
        m_spSerialPort.ReadExisting();
      }
    }

    /// <summary>
    ///   Event for serial port data received
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void SerialPort_DataReceived(object p_sender, SerialDataReceivedEventArgs p_e)
    {
      // Lock the partial message object
      lock (m_partialMsgProxy)
      {
        MsgZigbee frame;
        //Debug.WriteLine("[DBG] Serial data received, reading bytes " + m_spSerialPort.BytesToRead);
        // check to see if there is a partial message
        frame = m_bIsPartialMsg ? m_partialMsg : new MsgZigbee();

        //retrieve number of bytes in the buffer
        MsgZigbee.EsnFrameError frameError;
        byte[] byteBuffer = new byte[1];

        //try
        //{
        //read the data and store it
        while (m_spSerialPort.BytesToRead > 0)
        {
          m_spSerialPort.Read(byteBuffer, 0, 1);
          frameError = frame.AddFrameByte(byteBuffer[0]);

          if (frameError != Msg.EsnFrameError.NONE)
          {
            // throw an error, flush the buffer and discard any partials
            Debug.WriteLine("[ERR] Error in frame: {0}", frameError.ToString());
            Debug.WriteLine(frame.ByteString);
            FlushSerialPort();
            m_bIsPartialMsg = false;
            return;
          }

          // If the frame is completed, add it and start a new one
          if (frame.IsComplete)
          {
            //Debug.WriteLine(string.Format("[DBG] Frame complete, Adding to list {0}", frame.FrameID));


            MsgDispatcher.Instance.ReceiveMsg(MsgFactory.CreateMessage(frame));

            frame = new MsgZigbee();
            m_bIsPartialMsg = false;

          }
        }

        // if frame is not complete, save it
        if (!frame.IsEmpty)
        {
          //Debug.WriteLine("[DBG] Have partial frame");
          m_partialMsg = frame;
          m_bIsPartialMsg = true;
        }
        //}
        //catch (Exception ex)
        //{
        //m_bIsPartialMsg = false;
        //Debug.WriteLine(string.Format("[ERR] Serial read error - {0}", ex.Message));
        //}
      }
      //Debug.WriteLine("[DBG] DeviceXBee.SerialPort_DataReceived - EXIT");
    }

    /// <summary>
    ///   Event for serial port error received 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private static void SerialPort_OnErrorReceived(object p_sender, SerialErrorReceivedEventArgs p_e)
    {
      switch (p_e.EventType)
      {
        case SerialError.Frame:
          Debug.WriteLine("[DBG] SerialPort-ERROR: Framing error.");
          break;
        case SerialError.Overrun:
          Debug.WriteLine("[DBG] SerialPort-ERROR: Character buffer overrun.");
          break;
        case SerialError.RXOver:
          Debug.WriteLine("[DBG] SerialPort-ERROR: Input buffer overflow.");
          break;
        case SerialError.RXParity:
          Debug.WriteLine("[DBG] SerialPort-ERROR: Parity error.");
          break;
        case SerialError.TXFull:
          Debug.WriteLine("[DBG] SerialPort-ERROR: Output buffer full.");
          break;
        default:
          Debug.WriteLine("[DBG] SerialPort-ERROR: [default case] ???unknown error???");
          break;
      }
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region Write Functions

    /// <summary>
    ///   Write a single frame to the device
    /// </summary>
    /// <param name="p_msg"></param>
    /// <returns></returns>
    public bool WriteXBeeFrame(Msg p_msg)
    {
      if (m_serialConnected)
      {
        lock (m_queueProxy)
        {
          try
          {
            //Debug.WriteLine(
              //String.Format(
                //"{0}:{1}", DateTime.Now.Second, DateTime.Now.Millisecond));
            byte[] b = p_msg.Bytes;
            m_spSerialPort.Write(b, 0, b.Length);
            return true;
          }
          catch (Exception)
          {
            Debug.WriteLine("[ERR] Could not write to serial port");
          }
        }
      }
      return false;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region Device Functions

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region IDisposable Members

    /// <summary>
    /// we are being destroyed, close our port if not already
    /// </summary>
    public void Dispose()
    {
      Debug.WriteLine("[DBG] SyNetDeviceSerialXBee:Dispose() - Dispose");
      Close();
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////

  }
}
