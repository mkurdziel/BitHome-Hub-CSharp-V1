using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using SyNet.DataHelpers;
using SyNet.MessageTypes;
using SyNet.Protocol;

namespace SyNet
{
  /// <summary>
  ///   Device Function Class
  /// </summary>
  public class DeviceFunction : IObjectWithID<int>
  {
    private ulong m_deviceID;
    private SerializableDictionaryWithId<int, DeviceParameter>
      m_parameters
        = new SerializableDictionaryWithId<int, DeviceParameter>();

    /// <summary>
    ///   ActionID used to create run-time DeviceActions
    /// </summary>
    [XmlAttribute]
    public UInt64 ActionID { get; set; }

    /// <summary>
    ///   Function ID
    /// </summary>
    [XmlAttribute]
    public int ID { get; set; }

    /// <summary>
    ///   Device ID this function belongs to
    /// </summary>
    [XmlAttribute]
    public UInt64 DeviceID
    {
      get { return m_deviceID; }
      set
      {
        m_deviceID = value;
      }
    }

    /// <summary>
    ///   Function Name
    /// </summary>
    [XmlAttribute]
    public string Name { get; set; }

    /// <summary>
    ///   Information string
    /// </summary>
    public string Information
    {
      get { return ToString(); }
    }

    /// <summary>
    ///   Returntype of the function
    /// </summary>
    [XmlAttribute]
    public EsnDataTypes ReturnType { get; set; }

    /// <summary>
    ///   Returns the number of parameters minus any return parameter
    /// </summary>
    [XmlIgnore]
    public int NumParameters
    {
      get
      {
        if (Parameters.ContainsKey(0))
        {
          return Parameters.Count - 1;
        }
        return Parameters.Count;
      }
    }

    /// <summary>
    ///   Dictionary indexing the parameter by its ID
    /// </summary>
    [XmlElement(ElementName = "Parameters")]
    public SerializableDictionaryWithId<int, DeviceParameter> Parameters
    {
      get { return m_parameters; }
      set { m_parameters = value; }
    }

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public DeviceFunction()
    {
    }

    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="p_funtionID"></param>
    /// <param name="p_deviceID"></param>
    public DeviceFunction( int p_funtionID, UInt64 p_deviceID )
    {
      ID = p_funtionID;
      DeviceID = p_deviceID;
    }


    /// <summary>
    ///   ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      List<string> strNames = new List<string>();
      foreach (DeviceParameter value in Parameters.Values)
      {
        if (value.ID != 0)
        {
          strNames.Add(value.Name);
        }
      }
      return string.Format("{0} {1}({2})", ReturnType, Name, string.Join(",", strNames.ToArray()));
    }

    /// <summary>
    ///   Sets a single parameter
    /// </summary>
    /// <param name="p_param"></param>
    public void SetParameter( DeviceParameter p_param )
    {
      if (Parameters.ContainsKey(p_param.ID))
      {
        Parameters.Remove(p_param.ID);
      }
      Parameters.Add(p_param.ID, p_param);
    }

    /// <summary>
    ///   Gets a single parameter
    /// </summary>
    /// <param name="p_nParameterID"></param>
    /// <returns></returns>
    internal DeviceParameter GetParameter( int p_nParameterID )
    {
      DeviceParameter dfpWanted = null;
      if (Parameters.ContainsKey(p_nParameterID))
      {
        dfpWanted = Parameters[p_nParameterID];
      }
      if (dfpWanted == null)
      {
        Debug.WriteLine(string.Format("GetParameter() ERROR: failed to locate requested parameter #{0}", p_nParameterID));
      }
      return dfpWanted;
    }

    /// <summary>
    ///   Construct the function from the parameters and send it
    /// </summary>
    /// <returns></returns>
    public bool Send()
    {
      bool bRetVal = true;
      DeviceXBee device = NodeManager.Instance.GetNode(DeviceID) as DeviceXBee;
      MsgSyNetFunctionTransmit msg = new MsgSyNetFunctionTransmit(device,
        (byte)ID);

      for (int i = 1; i <= this.NumParameters; i++)
      {
        Parameter p = GetParameter(i);
        if (!p.IsValid)
        {
          bRetVal = false;
          break;
        }
        msg.AddParam(p);

      }

      if (bRetVal)
      {
        MsgDispatcher.Instance.SendMsg(msg);
      }
      else
      {
        throw new Exception("DeviceFunction.Send - Failed parameter validation");
      }
      return bRetVal;
    }

    /// <summary>
    ///   Lock the device so only one person can use it at a time
    /// </summary>
    /// <returns></returns>
    public bool LockDevice()
    {
      Device device = NodeManager.Instance.GetNode(DeviceID);
      if (device != null)
      {
        return device.Lock();
      }
      else
      {
        throw new Exception("DeviceFunction.LockDevice - null device");
      }
    }

    /// <summary>
    ///   Unlock the device
    /// </summary>
    public void UnlockDevice()
    {
      Device device = NodeManager.Instance.GetNode(DeviceID);
      if (device != null)
      {
        device.Unlock();
      }
      else
      {
        throw new Exception("DeviceFunction.UnlockDevice - null device");
      }
    }
  }
}