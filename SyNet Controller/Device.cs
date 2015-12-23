using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Xml.Serialization;
using SyNet.DataHelpers;
using SyNet.EventArguments;
using SyNet.MessageTypes;
using SyNet.Protocol;

namespace SyNet
{
  /// <summary>
  ///   Class representing a device in the system
  /// </summary>
  [XmlRoot("Device")]
  [XmlInclude(typeof(DeviceFunction))]
  public class Device : INotifyPropertyChanged, IObjectWithID<ulong>
  {
    /// <summary>
    ///   Name of a yet-unnamed device
    /// </summary>
    public const string UNKNOWN_NAME = "Unknown";

    ////////////////////////////////////////////////////////////////////////////

    #region Private members

    private readonly Mutex m_mtxObjectLock;
    private UInt64 m_nDeviceID;

    private DateTime m_dtLastSeen;

    private SerializableDictionaryWithId<int, DeviceFunction> m_dictFunctionByID;

    private string m_strDeviceName;
    private bool m_bIsUnknown;
    private int m_nDeviceNameFunctionID;

    private readonly Dictionary<int, FunctionReceivedEventHandler> m_dictFunctionEventHandlerById;

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////////

    #region Public Interface Constants

    /// <summary>
    ///   Enumeration representing the status of the device
    /// </summary>
    public enum StatusEnum
    {
      /// <summary>
      ///   This device has an unknown state and has not yet reported active
      /// </summary>
      UNKNOWN,
      /// <summary>
      ///   A status active message has been received and has been seen in the
      ///   last two minutes.
      /// </summary>
      ACTIVE,
      /// <summary>
      ///   This device has been seen in the last hour
      /// </summary>
      RECENT,
      /// <summary>
      ///   This device has not been seen in the last hour
      /// </summary>
      DEAD
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////////

    #region Events

    /// <summary>
    /// EVENT: subscribe to this custom device UI wishes to receive received-value notifications
    /// </summary>
    internal delegate void FunctionReceivedEventHandler(object p_objSender, FunctionReceivedEventArgs p_evArgs);

    internal event FunctionReceivedEventHandler FunctionReceived;

    // The internal OnFunctionReceived method raises the event by invoking 
    // the delegates. The sender is always this, the current instance 
    // of this class.  A device, upon receiving a Function Received message,
    // invokes this method to notify any attached UIs of the newly value
    //
    internal void OnFunctionReceived(FunctionReceivedEventArgs p_frEvArgs)
    {
      FunctionReceivedEventHandler handler = FunctionReceived;
      if (handler != null)
      {
        // Invokes the delegates. 
        handler(this, p_frEvArgs);
      }

      // Send to target delegates
      if (m_dictFunctionEventHandlerById.ContainsKey(p_frEvArgs.FunctionID))
      {
        handler = m_dictFunctionEventHandlerById[p_frEvArgs.FunctionID];
        if (handler != null)
        {
          handler(this, p_frEvArgs);
        }
      }
    }

    /// <summary>
    ///   Allows a listener to subscribe to events for a particular function
    ///   based on a function ID
    /// </summary>
    /// <param name="p_nFunctionID"></param>
    /// <param name="p_handler"></param>
    internal void SubscribeFunctionCallback(int p_nFunctionID, FunctionReceivedEventHandler p_handler)
    {
      if (!m_dictFunctionEventHandlerById.ContainsKey(p_nFunctionID))
      {
        m_dictFunctionEventHandlerById.Add(p_nFunctionID, p_handler);
      }
      else
      {
        m_dictFunctionEventHandlerById[p_nFunctionID] += p_handler;
      }
    }

    /// <summary>
    ///   Unsubscribe a listener to events for a particular function ID
    /// </summary>
    /// <param name="p_nFunctionID"></param>
    /// <param name="p_handler"></param>
    internal void UnsubscribeFunctionCallback(int p_nFunctionID, FunctionReceivedEventHandler p_handler)
    {
      if (m_dictFunctionEventHandlerById.ContainsKey(p_nFunctionID))
      {
        m_dictFunctionEventHandlerById[p_nFunctionID] -= p_handler;
      }
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    //////////////////////////////////////////////////////////////////////////// 

    #region Constructors

    /// <summary>
    /// Constructor with no data
    /// </summary>
    public Device()
    {
      // Give a default ID
      m_nDeviceID = NodeManager.Instance.GenerateRandomID();

      Debug.WriteLine(string.Format("[DBG] Device({0}) - Constructor", ID));

      m_strDeviceName = UNKNOWN_NAME;
      m_bIsUnknown = true;

      m_dtLastSeen = DateTime.MinValue;
      m_mtxObjectLock = new Mutex();

      m_nDeviceNameFunctionID = -1;
      DeviceProvidesOwnName = false;

      m_dictFunctionByID = new SerializableDictionaryWithId<int, DeviceFunction>();
      m_dictFunctionEventHandlerById = new Dictionary<int, FunctionReceivedEventHandler>();
    }

    #endregion

    //////////////////////////////////////////////////////////////////////////// 


    ////////////////////////////////////////////////////////////////////////////

    #region Public Properties

    /// <summary>
    ///   64-bit unique device ID
    /// </summary>
    [XmlAttribute]
    public UInt64 ID
    {
      get
      {
        return m_nDeviceID;
      }
      set
      {
        if (m_nDeviceID != value)
        {
          Debug.WriteLine(string.Format("[DBG] Device({0}) is now renumbered to ({1}) [XML Loader]",m_nDeviceID,value));
        }
        m_nDeviceID = value;
      }
    }

    /// <summary>
    ///   Catalog of functions reported by this device
    /// </summary>
    [XmlElement(ElementName = "Catalog")]
    public SerializableDictionaryWithId<int, DeviceFunction> Catalog
    {
      get { return m_dictFunctionByID; }
      set
      {
        m_dictFunctionByID = value;
        OnPropertyChanged("Catalog");
      }
    }

    /// <summary>
    ///   Returns the activeness of the device
    /// </summary>
    [XmlAttribute]
    public StatusEnum DeviceStatus
    {
      get
      {
        if (m_bIsUnknown)
        {
          return StatusEnum.UNKNOWN;
        }
        TimeSpan active = DateTime.Now - LastSeen;
        if (active < TimeSpan.FromMinutes(5))
        {
          return StatusEnum.ACTIVE;
        }

        if (active < TimeSpan.FromHours(1))
        {
          return StatusEnum.RECENT;
        }
        return StatusEnum.DEAD;
      }
    }

    /// <summary>
    ///   Name of device. Can be set by user.
    /// </summary>
    [XmlAttribute]
    public string DeviceName
    {
      get { return m_strDeviceName; }
      set
      {
        m_strDeviceName = value;
        OnPropertyChanged("DeviceName");
      }
    }

    /// <summary>
    ///   Return the catalog of functions
    /// </summary>
    [XmlIgnore]
    public DeviceFunction[] FunctionList
    {
      get
      {
        DeviceFunction[] functList = new DeviceFunction[m_dictFunctionByID.Keys.Count];
        m_dictFunctionByID.Values.CopyTo(functList, 0);
        return functList;
      }
    }

    /// <summary>
    ///   Returns true if the device is currently being investigated
    /// </summary>
    [XmlAttribute]
    public bool BeingInvestigated { get; set; }

    /// <summary>
    ///   Returns true if the device needs light investigation
    /// </summary>
    [XmlAttribute]
    public bool NeedLightInvestigation { get; set; }

    /// <summary>
    ///   Do we have the full catalog
    /// </summary>
    [XmlAttribute]
    public bool HaveFullCatalog { get; set; }

    /// <summary>
    ///   Do we have the full parameters
    /// </summary>
    [XmlAttribute]
    public bool HaveFullParameters { get; set; }

    /// <summary>
    ///   An unknown device is one that has been seen on the network via
    ///   a valid SyNet message but cannot be determined as active 
    ///   through status info requests
    /// </summary>
    [XmlAttribute]
    public bool IsUnknown
    {
      get { return m_bIsUnknown; }
      set
      {
        // MODIFICATION: signal DeviceStatus change only if it really did,
        //   not when it may have!!!!

        StatusEnum ePriorDeviceStatus = DeviceStatus; // capture prior value
        m_bIsUnknown = value;
        OnPropertyChanged("IsUnknown");

        // if this caused a new DeviceStatus, then signal it too!
        SignalStatusChangeIfDifferent(ePriorDeviceStatus);
      }
    }

    /// <summary>
    ///   When I was last seen in minutes
    /// </summary>
    [XmlIgnore]
    public int LastSeenMinutes
    {
      get
      {
        TimeSpan tspan = DateTime.Now - LastSeen;
        return tspan.Minutes;
      }
    }

    /// <summary>
    ///   Returns a string stating when the device was last seen
    /// </summary>
    [XmlIgnore]
    public String LastSeenString
    {
      get
      {
        if (m_dtLastSeen == DateTime.MinValue)
        {
          return "Unknown";
        }
        return m_dtLastSeen.ToString();
      }
    }

    /// <summary>
    ///   When the device was last seen
    /// </summary>
    [XmlAttribute]
    public DateTime LastSeen
    {
      get { return m_dtLastSeen; }
      set
      {
        // MODIFICATION: signal DeviceStatus change only if it really did,
        //   not when it may have!!!!

        StatusEnum ePriorDeviceStatus = DeviceStatus; // capture prior value
        m_dtLastSeen = value; // set new 
        OnPropertyChanged("LastSeen"); // signal

        // if this caused a new DeviceStatus, then signal it too!
        SignalStatusChangeIfDifferent(ePriorDeviceStatus);
      }
    }

    /// <summary>
    ///   16-bit manufacturer ID
    /// </summary>
    [XmlAttribute]
    public UInt16 ManufacturerID { get; set; }

    /// <summary>
    ///   16-bit profile ID
    /// </summary>
    [XmlAttribute]
    public UInt16 ProfileID { get; set; }

    /// <summary>
    ///   16-bit revision ID
    /// </summary>
    [XmlAttribute]
    public UInt16 RevisionID { get; set; }

    /// <summary>
    ///   Returns true if the device has a function that provides a device name
    /// </summary>
    [XmlAttribute]
    public bool DeviceProvidesOwnName { get; set; }

    /// <summary>
    ///   Returnx the function ID of the device name function if it exists
    /// </summary>
    [XmlAttribute]
    public int DeviceNameFunctionID
    {
      get { return m_nDeviceNameFunctionID; }
      set
      {
        m_nDeviceNameFunctionID = value;
        OnPropertyChanged("DeviceNameFunctionID");
      }
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////////

    #region Public Methods

    /// <summary>
    ///   Locks the device.
    /// </summary>
    /// <returns></returns>
    public bool Lock()
    {
      if (m_mtxObjectLock.WaitOne(TimeSpan.FromSeconds(1)))
      {
        return true;
      }
      Debug.WriteLine("[ERR] Cannot get device lock");
      return false;
    }

    /// <summary>
    ///   Unlocks the device
    /// </summary>
    public void Unlock()
    {
      m_mtxObjectLock.ReleaseMutex();
    }

    /// <summary>
    ///   Trigers the DeviceStatus property to refresh
    /// </summary>
    public void Refresh()
    {
      OnPropertyChanged("DeviceStatus");
    }

    /// <summary>
    ///   Return a function from a function ID
    /// </summary>
    /// <param name="p_functionID"></param>
    /// <returns></returns>
    public DeviceFunction GetFunction(int p_functionID)
    {
      DeviceFunction function = null;
      if (m_dictFunctionByID != null &&
          m_dictFunctionByID.ContainsKey(p_functionID))
      {
        function = m_dictFunctionByID[p_functionID];
      }
      else
      {
        Debug.WriteLine("Device.DeviceFunction - null dictionary");
      }
      return function;
    }

    /// <summary>
    ///   Set a function parameter based on a parameter struct
    /// </summary>
    /// <param name="p_functID"></param>
    /// <param name="p_param"></param>
    public void SetParameter(int p_functID, DeviceParameter p_param)
    {
      if (m_dictFunctionByID.ContainsKey(p_functID))
      {
        Debug.WriteLine(string.Format("[DBG] Device.SetParameter - Adding {0}", p_param.ID));
        DeviceFunction f = m_dictFunctionByID[p_functID];
        f.SetParameter(p_param);
      }
    }

    /// <summary>
    ///   Set a function from a function struct
    /// </summary>
    /// <param name="p_f"></param>
    public void SetFunction(DeviceFunction p_f)
    {
      // Remove the function if it exists
      RemoveFunction(p_f.ID);

      // Add it to the table
      m_dictFunctionByID.Add(p_f.ID, p_f);

      OnPropertyChanged("FunctionList");
    }

    /// <summary>
    ///   Remove a function based on its function ID
    /// </summary>
    /// <param name="p_functionID"></param>
    public void RemoveFunction(int p_functionID)
    {
      // If we have an old copy of this function, remove it
      if (m_dictFunctionByID.ContainsKey(p_functionID))
      {
        m_dictFunctionByID.Remove(p_functionID);
      }
    }

    /// <summary>
    ///   Add parameter details to the device
    /// </summary>
    /// <param name="p_msg"></param>
    /// <returns></returns>
    public bool AddParameter(MsgSyNetParameterResponse p_msg)
    {
      if (p_msg == null)
      {
        return false;
      }

      if (m_dictFunctionByID.ContainsKey(p_msg.FunctionID))
      {
        DeviceParameter param = new DeviceParameter(
          p_msg.ParamID,
          p_msg.FunctionID,
          ID)
                                  {
                                    Name = p_msg.ParamName,
                                    ValidationType = p_msg.ValidationType,
                                    MinimumValue = p_msg.MinumumValue,
                                    MaxStringLength = p_msg.MaxStringLength,
                                    MaximumValue = p_msg.MaximumValue
                                  };
        //
        // Only copy in if there are values
        // (This helps keep the collection null and therefore not serialized)
        if (p_msg.EnumValuesByNames.Count > 0)
        {
          param.DctEnumValueByName = p_msg.EnumValuesByNames;
        }
        param.IsSigned = p_msg.IsSigned;
        param.DataType = p_msg.DataType;

        SetParameter(p_msg.FunctionID, param);
      }
      else
      {
        Debug.WriteLine("[ERR] Device.AddParameter - Adding param to unknown function");
      }
      return true;
    }

    /// <summary>
    ///   Add a function from the device function catalog to the object
    /// </summary>
    /// <param name="p_msg"></param>
    /// <returns></returns>
    public bool AddFunction(MsgSyNetCatalogResponse p_msg)
    {
      if (p_msg == null ||
          p_msg.EntryID == 0x00)
      {
        return false;
      }

      DeviceFunction dfNewFunction = new DeviceFunction(
        p_msg.EntryID,
        p_msg.My64BitAddress)
                                       {
                                         ID = p_msg.EntryID,
                                         Name = p_msg.FunctionName,
                                         ReturnType = p_msg.ReturnType
                                       };

      // if we see a function for getting the device name inform node manager to get it and set it 
      //  so that UI can use it
      if (dfNewFunction.Name.ToLower() == "devicename" ||
          dfNewFunction.Name.ToLower() == "device name")
      {
        DeviceNameFunctionID = dfNewFunction.ID;
        DeviceProvidesOwnName = true;
      }

      int nNextParamID = 1;
      foreach (EsnDataTypes type in p_msg.ParamTypes)
      {
        DeviceParameter dpNewParam = new DeviceParameter
                                       {
                                         ID = nNextParamID++,
                                         DataType = type
                                       };
        dfNewFunction.SetParameter(dpNewParam);
      }

      // If we have an old copy of this function, remove it
      if (m_dictFunctionByID.ContainsKey(dfNewFunction.ID))
      {
        m_dictFunctionByID.Remove(dfNewFunction.ID);
      }

      // Add it to the table
      m_dictFunctionByID.Add(dfNewFunction.ID, dfNewFunction);

      OnPropertyChanged("FunctionList");

      return true;
    }

    /// <summary>
    ///   Message received handler. Handles messages received by the system
    ///   for this device
    /// </summary>
    /// <param name="p_msg"></param>
    public void HandleMessage(Msg p_msg)
    {
      Debug.Assert(p_msg is MsgSyNetRx, "[CODE] received unexpected message (should be SyNet Rx only!)");

      MsgSyNetRx rxMsg = p_msg as MsgSyNetRx;

      if (rxMsg != null)
      {
        Debug.WriteLine(
          string.Format("[DBG] Device name=[{0}] ID={1} received message [{2}]!", DeviceName, ID, rxMsg.MsgType));

        // NOTE the following line is NOT: IsUnknown=false; since this and the next line would cause 
        //  back-to-back DeviceStatus PropertyChanged notifications with only partial info in the first one!
        m_bIsUnknown = false; // we are a known device if we've received a message via the radio
        LastSeen = DateTime.Now; // NOTE this causes a DeviceStatus PropertyChanged notification

        // Switch on the SyNet API
        switch (rxMsg.SyNetAPI)
        {
          case EsnAPI.DEVICE_STATUS:
            MsgSyNetDeviceStatus rxStatusMsg = rxMsg as MsgSyNetDeviceStatus;
            Debug.Assert(rxStatusMsg != null, "Type cast should have worked for typed message");
            if (rxStatusMsg.DeviceStatus ==
                EsnAPIDeviceStatusValues.INFO)
            {
              // iff any of our device/firmware ID values changed, reread the entire catalog
              // TODO: Compare IDs when we get 64 bit device IDS implemented
              // on the devices
              if ((rxStatusMsg.Revision != RevisionID) ||
                  (rxStatusMsg.Manufacturer != ManufacturerID) ||
                  (rxStatusMsg.Profile != ProfileID) /*||
                  (rxStatusMsg.SyNetID != ID)*/
                )
              {
                Debug.WriteLine(
                  string.Format(
                    "{0}: Wiping Catalog for Node hash=0x{1}", DeviceName,
                    GetHashCode().ToString("X")));
                HaveFullCatalog = false; // reset our catalog state so it is once more retrieved
                HaveFullParameters = false;
              }
              ManufacturerID = rxStatusMsg.Manufacturer;
              ProfileID = rxStatusMsg.Profile;
              // TODO: Finish work with a global ID
              //ID = rxStatusMsg.SyNetID;
              RevisionID = rxStatusMsg.Revision;
            }
            break;
          case EsnAPI.CATALOG_RESPONSE:
            // Pass along function calls from known nodes
            AddFunction(p_msg as MsgSyNetCatalogResponse);
            break;
          case EsnAPI.PARAMETER_RESPONSE:
            AddParameter(p_msg as MsgSyNetParameterResponse);
            break;
          case EsnAPI.FUNCTION_RECEIVE:
            MsgSyNetFunctionReceive snRxMsg = rxMsg as MsgSyNetFunctionReceive;
            if (snRxMsg != null)
            {
              // TEST short circuit
              FunctionReceivedEventArgs frArgs = null;
              if (snRxMsg.IsStringType)
              {
                frArgs = new FunctionReceivedEventArgs(snRxMsg.FunctionID, snRxMsg.ReturnStrValue);
              }
              else if (snRxMsg.IsIntegerType)
              {
                frArgs = new FunctionReceivedEventArgs(snRxMsg.FunctionID, snRxMsg.ReturnIntValue);
              }
              OnFunctionReceived(frArgs);
            }
            else
            {
              Debug.WriteLine(string.Format("HandleMessage() ERROR Message is not a MsgSyNetFunctionReceive!!!!"));
            }
            break;
        }
      }
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////////

    #region Device Properties

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    //////////////////////////////////////////////////////////////////////////// 

    #region Private Methods (Utility Methods)

    private void SignalStatusChangeIfDifferent(StatusEnum p_ePriorDeviceStatus)
    {
      if (p_ePriorDeviceStatus != DeviceStatus)
      {
        OnPropertyChanged("DeviceStatus");
      }
    }

#if false

    /// <summary>
    ///   Get the return parameter, if present
    /// </summary>
    /// <param name="p_functionID"></param>
    /// <returns></returns>
    private DeviceParameter GetReturnValue(int p_functionID)
    {
      return GetParameter(p_functionID, 0);
    }
    
    /// <summary>
    ///   Get a specific parameter based on its function id and parameter id
    /// </summary>
    /// <param name="p_functionID"></param>
    /// <param name="p_parameterID"></param>
    /// <returns></returns>
    private DeviceParameter GetParameter(int p_functionID, int p_parameterID)
    {
      return GetFunction(p_functionID).GetParameter(p_parameterID);
    }

#endif

    #endregion

    //////////////////////////////////////////////////////////////////////////// 


    ////////////////////////////////////////////////////////////////////////////

    #region Event Handlers

    /// <summary>
    ///  EVENT: PropertyChanged event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Norify subscribers that a named property has changed
    /// </summary>
    /// <param name="p_strPropertyName">the name of the property which now has a new value</param>
    protected void OnPropertyChanged(string p_strPropertyName)
    {
      if (PropertyChanged != null)
      {
        Debug.WriteLine(string.Format("[DBG] Device name=[{0}] Property [{1}] Changed!", DeviceName, p_strPropertyName));
        PropertyChanged(this, new PropertyChangedEventArgs(p_strPropertyName));
      }
      else
      {
        Debug.WriteLine(
          string.Format(
            "[DBG] Device name=[{0}] Property [{1}] Changed, NO SUBSCRIBERS!", DeviceName, p_strPropertyName));
      }

      // for this one device override the name...
      if (p_strPropertyName == "IsSerialConnected")
      {
        DeviceName = "XBeeCoordinator";
      }
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
  }
}