using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for all zigbee messages
  /// </summary>
  public class MsgZigbee : Msg
  {
    private Int16 m_frameDataIndex;
    private byte m_currentChecksum;

    /// <summary>
    ///   Payload offset for this message type
    /// </summary>
    public int PAYLOAD_OFFSET = 3;

    /// <summary>
    ///   XBee packet start byte
    /// </summary>
    public const byte XBEE_PACKET_START = 0x7E;

    private const byte XBEE_MAX_DATA_LEN = 128;

    /// <summary>
    ///   Enumeration for XBee Modem Status
    /// </summary>
    public enum EsnFrameDataModemStatus
    {
      /// <summary>
      ///   Hardware reset
      /// </summary>
      HARDWARE_RESET = 0,
      /// <summary>
      ///   Watchdog timer reset
      /// </summary>
      WATCHDOG_TIMER_RESET = 1,
      /// <summary>
      ///   Device has been associated with a PAN
      /// </summary>
      ASSOCIATED = 2,
      /// <summary>
      ///   Device has disassociated with a PAN
      /// </summary>
      DISASSOCIATED = 3,
      /// <summary>
      ///   Synchronization has been lost
      /// </summary>
      SYNCHRONIZATION_LOST = 4,
      /// <summary>
      ///   Coordinator realignment
      /// </summary>
      COORDINATOR_REALIGNMENT = 5,
      /// <summary>
      ///   Coordinator started
      /// </summary>
      COORDINATOR_STARTED = 6
    }

    /// <summary>
    ///   Enumeration for XBee AT Command Response Status
    /// </summary>
    public enum EsnFrameDataAtResposeStatus
    {
      /// <summary>
      ///   OK
      /// </summary>
      OK = 0,
      /// <summary>
      ///   Error
      /// </summary>
      ERROR = 1,
      /// <summary>
      ///   Invalid Command
      /// </summary>
      INVALID_COMMAND = 2,
      /// <summary>
      ///   Invalid Parameters
      /// </summary>
      INVALID_PARAMETER = 3
    }

    /// <summary>
    ///   XBee packet types based on API identifier
    /// </summary>
    public enum EsnXbeeAPI
    {
      /// <summary>
      ///   Invalid API
      /// </summary>
      INVALID = 0x00,
      /// <summary>
      ///   Modem status
      /// </summary>
      MODEM_STATUS = 0x8A,
      /// <summary>
      ///   AT Command
      /// </summary>
      AT_CMD = 0x08,
      /// <summary>
      ///   AT Command Parameter
      /// </summary>
      AT_CMD_PARAMETER = 0x09,
      /// <summary>
      ///   AT Command Response
      /// </summary>
      AT_CMD_RESPONSE = 0x88,
      /// <summary>
      ///   Remote Command
      /// </summary>
      REMOTE_CMD = 0x17,
      /// <summary>
      ///   Remote command respose
      /// </summary>
      REMOTE_CMD_RESPONSE = 0x97,
      /// <summary>
      ///   Zigbee TX Request
      /// </summary>
      ZIGBEE_TX_REQ = 0x10,
      /// <summary>
      ///   Zigbee COmmand Frame
      /// </summary>
      ZIGBEE_CMD_FRAME = 0x11,
      /// <summary>
      ///   Zigbee TX Status
      /// </summary>
      ZIGBEE_TX_STATUS = 0x8b,
      /// <summary>
      ///   Zigbee Receive
      /// </summary>
      ZIGBEE_RX = 0x90,
      /// <summary>
      ///   Zigbee explicit receive
      /// </summary>
      ZIGBEE_EXPLICIT_RX = 0x91,
      /// <summary>
      ///   Zigbee IO packet
      /// </summary>
      ZIGBEE_IO = 0x92,
      /// <summary>
      ///   Zigbee sensor receive
      /// </summary>
      RX_SENSOR = 0x94,
      /// <summary>
      ///   Node Ident
      /// </summary>
      NODE_IDENT = 0x95
    }

#pragma warning disable 1591
    /// <summary>
    ///   XBee AT Commands
    /// </summary>
    public enum EsnXbeeAtCmd
    {
      INVALID,
      /* Special */
      WRITE,
      WRITE_BINDING_TABLE,
      RESTORE_DEFAULTS,
      SOFTWARE_RESET,
      NETWORK_RESET,
      /* Addressing */
      DESTINATION_ADDRESS_HIGH,
      DESTINATION_ADDRESS_LOW,
      MY_16_NETWORK_ADDRESS,
      MY_16_PARENT_NETWORK_ADDRESS,
      NUMBER_OF_CHILDREN,
      SERIAL_NUMBER_HIGH,
      SERIAL_NUMBER_LOW,
      NODE_IDENTIFIER,
      DEVICE_TYPE_IDENTIFIER,
      ZIGBEE_APPLICATION_LAYER_ADDRESSING,
      SOURCE_ENDPOINT,
      DESTINATION_ENDPOINT,
      CLUSTER_IDENTIFIER,
      BINDING_TABLE_INDEX,
      /* Networking & Security */
      OPERATING_CHANNEL,
      PAN_ID,
      BROADCAST_HOPS,
      OPERATING_PAN_ID,
      NODE_DISCOVER_TIMEOUT,
      NETWORK_DISCOVERY_OPTIONS,
      NODE_DISCOVER,
      DESTINATION_NODE,
      SCAN_CHANNELS,
      SCAN_DURATION,
      NODE_JOIN_TIME,
      CHANNEL_VERIFICATION,
      AGGREGATE_ROUTING_NOTIFICATION,
      ASSOCIATION_INDICATION,
      /* Security */
      ENCRYPTION_ENABLE,
      ENCRYPTION_OPTIONS,
      ENCRYPTION_KEY,
      /* RF Interfacing */
      POWER_LEVEL,
      POWER_MODE,
      RECEIVED_SIGNAL_STRENGTH,
      /* Serial Interfacing */
      API_ENABLE,
      API_OPTIONS,
      INTERFACE_DATA_RATE,
      SERIAL_PARITY,
      PACKETIZATION_TIMEOUT,
      DIO7_CONFIGURATION,
      DIO6_CONFIGURATION,
      FORCE_SAMPLE,
      XBEE_SENSOR_SAMPLE,
      IO_SAMPLE_RATE,
      IO_DIGITAL_CHANGE_DETECTION,
      PWM0_CONFIGURATION,
      DIO11_CONFIGURATION,
      DIO12_CONFIGURATION,
      DIO13_CONFIGURATION,
      AD0_DIO0_CONFIGURATION,
      AD1_DIO1_CONFIGURATION,
      AD2_DIO2_CONFIGURATION,
      AD3_DIO3_CONFIGURATION,
      DIO4_CONFIGURATION,
      DIO5_CONFIGURATION,
      ASSOC_LED_BLINK_TIME,
      DIO8_CONFIGURATION,
      PULL_UP_RESISTOR,
      RSSI_PWM_TIMER,
      COMMISSIONING_PUSHBUTTON,
      /* Diagnostics */
      FIRMWARE_VERSION,
      HARDWARE_VERSION,
      SUPPLY_VOLTAGE,
      /* AT Command Options */
      COMMAND_MODE_TIMEOUT,
      EXIT_COMMAND_MODE,
      GUARD_TIMES,
      COMMAND_SEQUENCE_CHARACTER,
      /* Sleep Commands */
      SLEEP_MODE,
      NUMBER_OF_SLEEP_PERIODS,
      SLEEP_PERIOD,
      TIME_BEFORE_SLEEP,
      SLEEP_OPTIONS
    }
#pragma warning restore 1591

    /// <summary>
    ///   Hashtable of AT command strings
    /// </summary>
    public static Hashtable XBEE_AT_CMD_STRINGS = new Hashtable
                                                    {
                                                      {EsnXbeeAtCmd.WRITE, "WR"},
                                                      {EsnXbeeAtCmd.WRITE_BINDING_TABLE, "WB"},
                                                      {EsnXbeeAtCmd.RESTORE_DEFAULTS, "RE"},
                                                      {EsnXbeeAtCmd.SOFTWARE_RESET, "FR"},
                                                      {EsnXbeeAtCmd.NETWORK_RESET, "NR"},
                                                      {EsnXbeeAtCmd.DESTINATION_ADDRESS_HIGH, "DH"},
                                                      {EsnXbeeAtCmd.DESTINATION_ADDRESS_LOW, "DL"},
                                                      {EsnXbeeAtCmd.MY_16_NETWORK_ADDRESS, "MY"},
                                                      {EsnXbeeAtCmd.MY_16_PARENT_NETWORK_ADDRESS, "MP"},
                                                      {EsnXbeeAtCmd.NUMBER_OF_CHILDREN, "NC"},
                                                      {EsnXbeeAtCmd.SERIAL_NUMBER_HIGH, "SH"},
                                                      {EsnXbeeAtCmd.SERIAL_NUMBER_LOW, "SL"},
                                                      {EsnXbeeAtCmd.NODE_IDENTIFIER, "NI"},
                                                      {EsnXbeeAtCmd.DEVICE_TYPE_IDENTIFIER, "DD"},
                                                      {EsnXbeeAtCmd.ZIGBEE_APPLICATION_LAYER_ADDRESSING, "ZA"},
                                                      {EsnXbeeAtCmd.SOURCE_ENDPOINT, "SE"},
                                                      {EsnXbeeAtCmd.DESTINATION_ENDPOINT, "DE"},
                                                      {EsnXbeeAtCmd.CLUSTER_IDENTIFIER, "CI"},
                                                      {EsnXbeeAtCmd.BINDING_TABLE_INDEX, "BI"},
                                                      {EsnXbeeAtCmd.OPERATING_CHANNEL, "CH"},
                                                      {EsnXbeeAtCmd.PAN_ID, "ID"},
                                                      {EsnXbeeAtCmd.BROADCAST_HOPS, "BH"},
                                                      {EsnXbeeAtCmd.OPERATING_PAN_ID, "OP"},
                                                      {EsnXbeeAtCmd.NODE_DISCOVER_TIMEOUT, "NT"},
                                                      {EsnXbeeAtCmd.NETWORK_DISCOVERY_OPTIONS, "NO"},
                                                      {EsnXbeeAtCmd.NODE_DISCOVER, "ND"},
                                                      {EsnXbeeAtCmd.DESTINATION_NODE, "DN"},
                                                      {EsnXbeeAtCmd.SCAN_CHANNELS, "SC"},
                                                      {EsnXbeeAtCmd.SCAN_DURATION, "SD"},
                                                      {EsnXbeeAtCmd.NODE_JOIN_TIME, "NJ"},
                                                      {EsnXbeeAtCmd.CHANNEL_VERIFICATION, "JV"},
                                                      {EsnXbeeAtCmd.AGGREGATE_ROUTING_NOTIFICATION, "AR"},
                                                      {EsnXbeeAtCmd.ASSOCIATION_INDICATION, "AI"},
                                                      {EsnXbeeAtCmd.ENCRYPTION_ENABLE, "EE"},
                                                      {EsnXbeeAtCmd.ENCRYPTION_OPTIONS, "EO"},
                                                      {EsnXbeeAtCmd.ENCRYPTION_KEY, "KY"},
                                                      {EsnXbeeAtCmd.POWER_LEVEL, "PL"},
                                                      {EsnXbeeAtCmd.POWER_MODE, "PM"},
                                                      {EsnXbeeAtCmd.RECEIVED_SIGNAL_STRENGTH, "DB"},
                                                      {EsnXbeeAtCmd.API_ENABLE, "AP"},
                                                      {EsnXbeeAtCmd.API_OPTIONS, "AO"},
                                                      {EsnXbeeAtCmd.INTERFACE_DATA_RATE, "BD"},
                                                      {EsnXbeeAtCmd.SERIAL_PARITY, "NB"},
                                                      {EsnXbeeAtCmd.PACKETIZATION_TIMEOUT, "RO"},
                                                      {EsnXbeeAtCmd.DIO7_CONFIGURATION, "D7"},
                                                      {EsnXbeeAtCmd.DIO6_CONFIGURATION, "D6"},
                                                      {EsnXbeeAtCmd.FORCE_SAMPLE, "IS"},
                                                      {EsnXbeeAtCmd.XBEE_SENSOR_SAMPLE, "1S"},
                                                      {EsnXbeeAtCmd.IO_SAMPLE_RATE, "IR"},
                                                      {EsnXbeeAtCmd.IO_DIGITAL_CHANGE_DETECTION, "IC"},
                                                      {EsnXbeeAtCmd.PWM0_CONFIGURATION, "P0"},
                                                      {EsnXbeeAtCmd.DIO11_CONFIGURATION, "P1"},
                                                      {EsnXbeeAtCmd.DIO12_CONFIGURATION, "P2"},
                                                      {EsnXbeeAtCmd.DIO13_CONFIGURATION, "P3"},
                                                      {EsnXbeeAtCmd.AD0_DIO0_CONFIGURATION, "D0"},
                                                      {EsnXbeeAtCmd.AD1_DIO1_CONFIGURATION, "D1"},
                                                      {EsnXbeeAtCmd.AD2_DIO2_CONFIGURATION, "D2"},
                                                      {EsnXbeeAtCmd.AD3_DIO3_CONFIGURATION, "D3"},
                                                      {EsnXbeeAtCmd.DIO4_CONFIGURATION, "D4"},
                                                      {EsnXbeeAtCmd.DIO5_CONFIGURATION, "D5"},
                                                      {EsnXbeeAtCmd.ASSOC_LED_BLINK_TIME, "LT"},
                                                      {EsnXbeeAtCmd.DIO8_CONFIGURATION, "D8"},
                                                      {EsnXbeeAtCmd.PULL_UP_RESISTOR, "PR"},
                                                      {EsnXbeeAtCmd.RSSI_PWM_TIMER, "RP"},
                                                      {EsnXbeeAtCmd.COMMISSIONING_PUSHBUTTON, "CB"},
                                                      {EsnXbeeAtCmd.FIRMWARE_VERSION, "VR"},
                                                      {EsnXbeeAtCmd.HARDWARE_VERSION, "HV"},
                                                      {EsnXbeeAtCmd.SUPPLY_VOLTAGE, "%V"},
                                                      {EsnXbeeAtCmd.COMMAND_MODE_TIMEOUT, "CT"},
                                                      {EsnXbeeAtCmd.EXIT_COMMAND_MODE, "CN"},
                                                      {EsnXbeeAtCmd.GUARD_TIMES, "GT"},
                                                      {EsnXbeeAtCmd.COMMAND_SEQUENCE_CHARACTER, "CC"},
                                                      {EsnXbeeAtCmd.SLEEP_MODE, "SM"},
                                                      {EsnXbeeAtCmd.NUMBER_OF_SLEEP_PERIODS, "SN"},
                                                      {EsnXbeeAtCmd.SLEEP_PERIOD, "SP"},
                                                      {EsnXbeeAtCmd.TIME_BEFORE_SLEEP, "ST"},
                                                      {EsnXbeeAtCmd.SLEEP_OPTIONS, "SO"}
                                                    };

    /// <summary>
    ///   Default constructor
    /// </summary>
    public MsgZigbee()
    {
      // We is zigbee
      MsgFormat = EsnMsgFormat.ZIGBEE;

      // Add start byte
      MsgData.Add(XBEE_PACKET_START);
      // Add Length bytes
      MsgData.Add(0x00);
      MsgData.Add(0x00);
    }

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgZigbee(Msg p_msg) : base(p_msg)
    {
    }

    /// <summary>
    ///   Zigbee API
    /// </summary>
    public EsnXbeeAPI ZigbeeAPI
    {
      get
      {
        return (EsnXbeeAPI) Enum.ToObject(
                            typeof (EsnXbeeAPI), MsgData[3]);
      }
    }

    /// <summary>
    ///   Length of zigbee packet
    /// </summary>
    public UInt16 ZigbeeLength
    {
      get { return EBitConverter.ToUInt16(MsgData, 1); }
    }

    /// <summary>
    ///   Calculate length and checksum for frame being built
    /// </summary>
    public override void FinalizeFrame()
    {
      // size = msg length - start byte - frame sizes
      byte[] length = EBitConverter.GetBytes((UInt16) (MsgData.Count - 3));
      MsgData[1] = length[0];
      MsgData[2] = length[1];

      // calculate checksum
      byte checksum = 0x00;

      for (int i = 3; i < MsgData.Count; i++)
      {
        checksum += MsgData[i];
      }
      MsgData.Add((byte) (0xff - checksum));

      base.Direction = EsnFrameDirection.SENT;
      base.FrameStatus = EsnFrameStatus.FRAME_COMPLETE;
      base.Timestamp = DateTime.Now;
      base.IsFinalized = true;
    }

    /// <summary>
    ///   Returns true if packet is empty
    /// </summary>
    public bool IsEmpty
    {
      get { return base.FrameStatus == EsnFrameStatus.FRAME_EMPTY; }
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "Zigbee"; }
    }

    /// <summary>
    ///   Add a byte to the frame information
    /// </summary>
    /// <param name="p_bNewByteP"></param>
    /// <returns></returns>
    public EsnFrameError AddFrameByte(byte p_bNewByteP)
    {
      //Debug.WriteLine("[DBG] Frame builder - " +
      //  Convert.ToString(p_bNewByteP, 16) + " " + base.FrameStatus.ToString());

      switch (base.FrameStatus)
      {
        case EsnFrameStatus.FRAME_EMPTY:
          // if the first byte isn't the frame start, error
          if (p_bNewByteP == XBEE_PACKET_START)
          {
            base.FrameStatus = EsnFrameStatus.FRAME_SIZE_PARTIAL;
          }
          else
          {
            Debug.WriteLine(string.Format("[ERR] Frame start: {0}", Convert.ToString(p_bNewByteP, 16)));
            return EsnFrameError.INCORRECT_START_INDICATOR;
          }
          break;
        case EsnFrameStatus.FRAME_SIZE_PARTIAL:
          MsgData[1] = p_bNewByteP;
          base.FrameStatus = EsnFrameStatus.FRAME_SIZE;
          break;
        case EsnFrameStatus.FRAME_SIZE:
          MsgData[2] = p_bNewByteP;
          // make sure we have a decent frame size
          if ((ZigbeeLength - 1) <= 0)
          {
            Debug.WriteLine(string.Format("[ERR] Frame size: {0}", ZigbeeLength));
            return EsnFrameError.INCORRECT_FRAME_SIZE;
          }
          // create the data storage subtracting for type and crc
          base.FrameStatus = EsnFrameStatus.FRAME_TYPE;
          break;
        case EsnFrameStatus.FRAME_TYPE:
          base.FrameStatus = EsnFrameStatus.FRAME_DATA;
          m_currentChecksum += p_bNewByteP;
          MsgData.Add(p_bNewByteP);
          break;
        case EsnFrameStatus.FRAME_DATA:

          m_currentChecksum += p_bNewByteP;
          MsgData.Add(p_bNewByteP);
          m_frameDataIndex++;
          // if we have all bytes, move on
          if (m_frameDataIndex == (ZigbeeLength - 1))
          {
            base.FrameStatus = EsnFrameStatus.FRAME_CHECKSUM;
          }
          break;
        case EsnFrameStatus.FRAME_CHECKSUM:
          m_currentChecksum = (byte) (0xff - m_currentChecksum);

          if (m_currentChecksum != p_bNewByteP)
          {
            return EsnFrameError.INVALID_CHECKSUM;
          }
          MsgData.Add(p_bNewByteP);
          base.FrameStatus = EsnFrameStatus.FRAME_COMPLETE;
          base.Timestamp = DateTime.Now;
          break;
        case EsnFrameStatus.FRAME_COMPLETE:
          Debug.WriteLine(String.Format("[ERR] Adding {0} to complete frame", Convert.ToString(p_bNewByteP, 16)));
          return EsnFrameError.EXTRA_FRAME_DATA;
      }
      return EsnFrameError.NONE;
    }

    /// <summary>
    ///   This is used to skip the checksum byte for use with simulating messages
    /// </summary>
    /// <returns></returns>
    public bool SkipChecksum()
    {
      //
      // If we're not at the checksum, something is wrong
      //
      if (base.FrameStatus != EsnFrameStatus.FRAME_CHECKSUM)
      {
        return false;
      }

      //
      // Otherwise, finalize everything
      //
      base.FrameStatus = EsnFrameStatus.FRAME_COMPLETE;
      base.Timestamp = DateTime.Now;

      return true;
    }

    /// <summary>
    ///   Find the AT Command from the command string
    /// </summary>
    /// <returns></returns>
    public static EsnXbeeAtCmd AtCmdFromChars(char p_char1, char p_char2)
    {
      String at = new string(new[] {p_char1, p_char2});
      if (at.Length == 2)
      {
        foreach (DictionaryEntry c in XBEE_AT_CMD_STRINGS)
        {
          if (c.Value.Equals(at))
          {
            return (EsnXbeeAtCmd) c.Key;
          }
        }
      }
      return EsnXbeeAtCmd.INVALID;
    }
  }
}