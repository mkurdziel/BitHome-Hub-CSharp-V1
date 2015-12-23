using System;
using System.Xml.Serialization;
using SyNet.DataHelpers;

namespace SyNet
{
  /// <summary>
  ///   Class representing a device parameter
  /// </summary>
  public class DeviceParameter : Parameter, IObjectWithID<int>
  {
    /// <summary>
    ///   ID of the parameter, specified by the device
    /// </summary>
    [XmlAttribute]
    public int ID { get; set; }

    /// <summary>
    ///   Function ID that the parameter belongs to
    /// </summary>
    [XmlAttribute]
    public int FunctionID { get; set; }

    /// <summary>
    ///   Device ID that the parameter belongs to
    /// </summary>
    [XmlAttribute]
    public UInt64 DeviceID { get; set; }

    /// <summary>
    ///   Parameter ID used for run-time created ActionParameters
    /// </summary>
    [XmlAttribute]
    public UInt64 PreviousParamID { get; set; }

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public DeviceParameter()
    {

    }

    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="p_id"></param>
    /// <param name="p_functionID"></param>
    /// <param name="p_deviceID"></param>
    public DeviceParameter( int p_id, int p_functionID, UInt64 p_deviceID )
    {
      ID = p_id;
      FunctionID = p_functionID;
      DeviceID = p_deviceID;
    }

    /// <summary>
    ///   Extended name of the parameter
    /// </summary>
    [XmlIgnore]
    public override string FullName
    {
      get { return Name; }
    }

    /// <summary>
    ///   Returns the value of the parameter.
    /// </summary>
    /// <remarks>
    ///   The device parameter currently has no value logic so this 
    ///   simply returns the stringvalue.
    /// </remarks>
    [XmlIgnore]
    public override string StringValue
    {
      get
      {
        return base.Value;
      }
      set
      {
        base.Value = value;
      }
    }

  }
}