using System;
using System.Text;
using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for received device status
  /// </summary>
  public class MsgSyNetDeviceStatus : MsgSyNetRx
  {
    private readonly string m_information;
    private readonly UInt16 m_synetID;
    private readonly UInt16 m_manufacturerID;
    private readonly UInt16 m_profile;
    private readonly UInt16 m_revision;

    private readonly EsnAPIDeviceStatusValues m_deviceStatus;

    /// <summary>
    ///   Copy Constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetDeviceStatus(Msg p_msg) : base(p_msg)
    {
      m_deviceStatus = (EsnAPIDeviceStatusValues)
                       Enum.ToObject(typeof(EsnAPIDeviceStatusValues),
                                     MsgData[PAYLOAD_OFFSET + 2]);
      
      StringBuilder sb = new StringBuilder();
      sb.Append("Status: " + DeviceStatus);

      // Parse out info if necessary
      if (DeviceStatus == EsnAPIDeviceStatusValues.INFO)
      {
        m_synetID = EBitConverter.ToUInt16(MsgData, PAYLOAD_OFFSET + 3);
        m_manufacturerID = EBitConverter.ToUInt16(MsgData, PAYLOAD_OFFSET + 5);
        m_profile = EBitConverter.ToUInt16(MsgData, PAYLOAD_OFFSET + 7);
        m_revision = EBitConverter.ToUInt16(MsgData, PAYLOAD_OFFSET + 9);

        sb.Append("\nSyNetID: " + SyNetIDString);
        sb.Append("\nManufacturerID: " + ManufacturerString);
        sb.Append("\nProfileID: " + ProfileString);
        sb.Append("\nRevisionID: " + RevisionString);
      }
      m_information =  sb.ToString();
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get
      {
        return "SyNetDeviceStatus";
      }
    }

    /// <summary>
    ///   Information about the packet
    /// </summary>
    public override String Information
    {
      get
      {
        return m_information; 
      }
    }

    /// <summary>
    ///   Unique SyNet ID
    /// </summary>
    public UInt16 SyNetID
    {
      get
      {
        return m_synetID;
      }
    }
    
    /// <summary>
    ///   String representation of the SyNet ID
    /// </summary>
    public String SyNetIDString
    {
      get
      {
        return EBitConverter.ToHex(SyNetID);
      }
    }

    /// <summary>
    ///   Device status returned in packet
    /// </summary>
    public EsnAPIDeviceStatusValues DeviceStatus
    {
      get
      {
        return m_deviceStatus;
      }
    }

    /// <summary>
    ///   Manufacturer ID
    /// </summary>
    public UInt16 Manufacturer
    {
      get
      {
        return m_manufacturerID;
      }
    }

    /// <summary>
    ///   String representation of the manufacturer ID
    /// </summary>
    public String ManufacturerString
    {
      get
      {
        return EBitConverter.ToHex(Manufacturer);
      }
    }

    /// <summary>
    ///   Device Profile
    /// </summary>
    public UInt16 Profile
    {
      get
      {
        return m_profile;
      }
    }

    /// <summary>
    ///   String representation of the device profile
    /// </summary>
    public String ProfileString
    {
      get
      {
        return EBitConverter.ToHex(Profile);
      }
    }

    /// <summary>
    ///   Revision of device
    /// </summary>
    public UInt16 Revision
    {
      get
      {
        return m_revision;
      }
    }

    /// <summary>
    ///   String representation of device revision
    /// </summary>
    public String RevisionString
    {
      get
      {
        return EBitConverter.ToHex(Revision);
      }
    }
  }
}