using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace SyNet
{

  /// <summary>
  ///   Represent a zigbee wireless device
  /// </summary>
  [Serializable]
  [XmlRoot(ElementName = "ZigbeeDevice")]
  public class DeviceXBee : Device  {

    ////////////////////////////////////////////////////////////////////////////
    #region Private members

    private UInt16 m_my16BitNetworkAddress;
    private UInt64 m_serialNumber;

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Static Fields

    /// <summary>
    ///   Static 64-bit broadcast address
    /// </summary>
    public static UInt64 MY64_BITADDRESS_BROADCAST = 0x000000000000FFFF;

    /// <summary>
    ///   Static 16-bit broadcast network address
    /// </summary>
    public static UInt16 MY16_BITADDRESS_BROADCAST = 0xFFFE;

    /// <summary>
    ///   Static method to retreive a temporary device for broadcast purposes
    /// </summary>
    /// <returns></returns>
    public static DeviceXBee BroadcastDevice()
    {
      DeviceXBee xbee = new DeviceXBee();
      xbee.IsBroadcast = true;
      return xbee;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Public Properties

    /// <summary>
    ///   16-bit zigbee network address
    /// </summary>
    [XmlAttribute]
    public UInt16 NetworkAddress
    {
      get { return m_my16BitNetworkAddress; }
      set
      {
        m_my16BitNetworkAddress = value;
        OnPropertyChanged("NetworkAddress");
        OnPropertyChanged("NetworkAddressString");
      }
    }

    /// <summary>
    ///   String representation of the 16-bit network address
    /// </summary>
    [XmlIgnore]
    public String NetworkAddressString
    {
      get { return EBitConverter.ToHex(m_my16BitNetworkAddress); }
    }

    /// <summary>
    ///   Full 64-bit serial number
    /// </summary>
    [XmlAttribute]
    public UInt64 SerialNumber
    {
      get
      {
        return m_serialNumber;
      }

      set
      {
        m_serialNumber = value;

        // Set the base ID
        ID = m_serialNumber;

        OnPropertyChanged("SerialNumber");
        OnPropertyChanged("SerialNumberString");
      }
    }

    /// <summary>
    ///   String representation of full 64-bit serial number
    /// </summary>
    [XmlIgnore]
    public String SerialNumberString
    {
      get { return EBitConverter.ToHex(SerialNumber); }
    }

    /// <summary>
    ///   Returns true if this particular object is a broadcast object
    /// </summary>
    [XmlIgnore]
    public bool IsBroadcast { get; set; }

    #endregion
    ////////////////////////////////////////////////////////////////////////////
    


    //////////////////////////////////////////////////////////////////////////// 
    #region Constructors

    /// <summary>
    /// Constructor with no data
    /// </summary>
    public DeviceXBee()
    {
      Debug.WriteLine("[DBG] SyNetXBeeDevice - Constructor");
    }
    #endregion
    //////////////////////////////////////////////////////////////////////////// 



    //////////////////////////////////////////////////////////////////////////// 
    #region Public Methods

    /// <summary>
    ///   Tostring method
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Concat(DeviceName, " - ", SerialNumberString);
    }

    #endregion
    //////////////////////////////////////////////////////////////////////////// 
  }
}