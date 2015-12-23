using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using SyNet.EventArguments;
using SyNet.Protocol;

namespace SyNet.Events.Triggers
{
  /// <summary>
  /// CLASS superclass of trigger (supporting signalling of device or node-list status changes)
  /// </summary>
  public class StatusTrigger : Trigger
  {
    #region Public Interface Constants

    /// <summary>
    ///   Enumeration representing the status of the device
    /// </summary>
    public enum StateEnum
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
      DEAD,
      /// <summary>
      ///  this device was just added to the node-list (new arrival)
      /// </summary>
      NODE_ADDED,
      /// <summary>
      ///  this device was just removed from the node-list (made to go away)
      /// </summary>
      NODE_REMOVED
    }

    #endregion

    #region Private Member Data (persisted)

    private UInt64 m_nDeviceID;

    #endregion

    #region Private Member Data (NOT persisted)

    private const string STR_STATUSTRIGGER_SUBSYSTEMNAME = "StatusTrigger";

    private Device m_dvcTriggerSource;
    private UInt64 m_nNodeListLatestDeviceID;

    #endregion

    #region Construction

    /// <summary>
    /// Default Constructor
    /// </summary>
    public StatusTrigger()
    {
      Initialize();
      SyNetSettings.Instance.DeserializingFinished += DeserializingFinished;
    }

    /// <summary>
    /// Copy Constructor
    /// </summary>
    /// <param name="p_stRhs">like item from which to copy values</param>
    public StatusTrigger(StatusTrigger p_stRhs)
      : base(p_stRhs)
    {
      Initialize();
      DeviceID = p_stRhs.m_nDeviceID;
      ValueParameter = new TriggerParameter(p_stRhs.ValueParameter);
    }

    /// <summary>
    /// Constructor with initial data
    /// </summary>
    /// <param name="p_dvcNode">The device (node) associated with this trigger or NULL for nodelist</param>
    public StatusTrigger(Device p_dvcNode)
    {
      Initialize();
      bool bIsDeviceNotNodelist = (p_dvcNode != null) ? true : false;
      if (bIsDeviceNotNodelist)
      {
        // have specific device status instance
        m_dvcTriggerSource = p_dvcNode;
        Debug.Assert(
          m_dvcTriggerSource != null, "[CODE] ERROR StatusTrigger: m_dvcTriggerSource should not have been NULL");
        m_nDeviceID = m_dvcTriggerSource.ID;
        // update status with latest from device
      }
      else
      {
        // have node-list status instance
        m_nDeviceID = 0;
      }
    }

    /// <summary>
    /// R/W PROPERTY: trigger name
    /// </summary>
    /// <remarks>this method rebuilds name on request which allows device to be renamed with trigger name changing too</remarks>
    [XmlAttribute]
    public override string Name
    {
      get
      {
        // set name. (Looks good when debugging)
        base.Name = (IsDeviceBasedTrigger)
                      ?
                        string.Format("{0}.Status", m_dvcTriggerSource.DeviceName)
                      :
                        "NodeList.Status";
        // return the name set
        return base.Name;
      }
      set { base.Name = value; }
    }

    private void DeserializingFinished()
    {
      Debug.WriteLine(string.Format("[DBG-Dser] StatusTrigger({0}).DeserializingFinished - ENTRY", TriggerID));

      if (IsDeviceBasedTrigger)
      {
        SetupTriggerSource();

        if (HaveSubscribers())
        {
          Subscribe(TriggerID); // re-subscribe if we were when persisted
        }
      }
      else
      {
        Subscribe(TriggerID); // re-subscribe we always were
      }
      Debug.WriteLine(string.Format("[DBG-Dser] StatusTrigger({0}).DeserializingFinished - EXIT", TriggerID));
    }

    private void SetupTriggerSource()
    {
      // if we haven't yet setup trigger-source and we should, then do so!
      // NOTE should this setup diff source if dvcID changes?
      if (m_dvcTriggerSource == null && IsDeviceBasedTrigger)
      {
        m_dvcTriggerSource = NodeManager.Instance.GetNode(m_nDeviceID);
      }
    }

    private void Initialize()
    {
      m_nDeviceID = UInt64.MaxValue; // device not yet specified
      m_nNodeListLatestDeviceID = UInt64.MaxValue; // device not yet specified

      m_dvcTriggerSource = null;

      DebugSource = STR_STATUSTRIGGER_SUBSYSTEMNAME;
      //
      // Initialize the Value Parameter
      //
      ValueParameter = new TriggerParameter();
      ValueParameter.Name = "StatusTrigger Value";
      ValueParameter.DataType = EsnDataTypes.BYTE;
      ValueParameter.ValidationType = EsnParamValidationType.ENUMERATED;
      ValueParameter.DctEnumValueByName.Add("UNKNOWN", 0);
      ValueParameter.DctEnumValueByName.Add("ACTIVE", 1);
      ValueParameter.DctEnumValueByName.Add("RECENT", 2);
      ValueParameter.DctEnumValueByName.Add("DEAD", 3);
      ValueParameter.DctEnumValueByName.Add("NODE_ADDED", 4);
      ValueParameter.DctEnumValueByName.Add("NODE_REMOVED", 5);
      ValueParameter.IntValue = (int) StateEnum.UNKNOWN;
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///   Returns a readonly collection of all parameters in this trigger
    /// </summary>
    [XmlIgnore]
    public override ReadOnlyCollection<TriggerParameter> Parameters
    {
      get
      {
        List<TriggerParameter> allParams = new List<TriggerParameter>(base.Parameters);
        allParams.Add(ValueParameter);
        return allParams.AsReadOnly();
      }
    }

    /// <summary>
    /// R/O PROPERTY: return the device which arrived in the node list or that which was removed.
    /// </summary>
    /// <remarks>VALID ONLY upon signal of node-list state change</remarks>
    [XmlIgnore]
    public UInt64 NodeListDeviceArrivedDeparted
    {
      get
      {
        Debug.Assert(
          m_nNodeListLatestDeviceID != UInt64.MaxValue,
          "[CODE] ERROR StatusTrigger: Attempted Fetch of nodelist arrivee/departee which is not yet set!");
        return m_nNodeListLatestDeviceID;
      }
    }

    /// <summary>
    /// R/W PROPERTY: Get/Set Device ID for this trigger
    /// </summary>
    [XmlAttribute]
    public UInt64 DeviceID
    {
      get
      {
        Debug.Assert(
          m_nDeviceID != UInt64.MaxValue,
          "[CODE] ERROR StatusTrigger: Attempted Fetch of DeviceID which is not yet set!");
        return m_nDeviceID;
      }
      set
      {
        m_nDeviceID = value;
        if (m_nDeviceID != 0 &&
            m_nDeviceID != UInt64.MaxValue)
        {
          SetupTriggerSource();
        }
      }
    }

    /// <summary>
    /// Return the newly arrived value (value which caused trigger DidFire)
    /// </summary>
    [XmlIgnore]
    public override string Value
    {
      get { return ValueParameter.StringValue; }
    }

    #endregion

    #region Private Properties

    private bool IsNodeListBasedTrigger
    {
      get
      {
        Debug.Assert(m_nDeviceID != UInt64.MaxValue, "[CODE] ERROR StatusTrigger: not yet completely setup!");
        return (m_nDeviceID == 0) ? true : false;
      }
    }

    private bool IsDeviceBasedTrigger
    {
      get { return (IsNodeListBasedTrigger) ? false : true; }
    }

    #endregion

    #region Methods supporting notification from device in node-list

    /// <summary>
    /// Tell this trigger to start listening for function return-value messages from the device
    /// </summary>
    /// <param name="p_nCallerID">unique id of caller (typically: event or guiAction)</param>
    /// <remarks>this is typically called when the event containing this trigger is enabled</remarks>
    public override void Subscribe(UInt64 p_nCallerID)
    {
      // if this object is the caller then we are simply re-subscribing after deserialization to just do it
      bool bIsForced = (p_nCallerID == TriggerID) ? true : false;

      // check reference counting, update then subscribe if needed
      if (bIsForced || NeedToSubscribe(p_nCallerID))
      {
        if (m_dvcTriggerSource != null)
        {
          m_dvcTriggerSource.PropertyChanged += DvcTriggerSource_PropertyChanged;
        }
        else
        {
          NodeManager.Instance.NodeDiscovered += NodeDiscovered_Handler;
          NodeManager.Instance.NodeRemoved += NodeRemoved_Handler;
        }
      }
    }

    private void NodeRemoved_Handler(object p_objSender, NodeDiscoveredEventArgs p_ndEvArgs)
    {
      // Store the lastest parameter value in the value parameter and make notifications
      SignalNodeListStateChange((int) StateEnum.NODE_REMOVED, p_ndEvArgs.DeviceID);
    }

    private void NodeDiscovered_Handler(object p_objSender, NodeDiscoveredEventArgs p_ndEvArgs)
    {
      // Store the lastest parameter value in the value parameter and make notifications
      SignalNodeListStateChange((int) StateEnum.NODE_ADDED, p_ndEvArgs.DeviceID);
    }

    private void SignalNodeListStateChange(int p_nNewValue, UInt64 p_nDeviceID)
    {
      m_nNodeListLatestDeviceID = p_nDeviceID;
      ValueParameter.IntValue = p_nNewValue;
      ValueParameter.StringValue = Value;

      InvokePropertyChanged(new PropertyChangedEventArgs("Value"));

      ConditionallyCountInstance(); // update counters and value match data

      // indicate that this trigger has fired since last reset
      if (DidFire)
      {
        // now tell scheduler that some event may have come ready
        DebugMsg("Node-list status arrived");
        EventScheduler.Instance.OnNodeStatusArrival(this, new EventArgs());
      }
      else
      {
        DebugMsg("Node-list status arrived, but count/value-match caused it to be ignored");
      }
    }


    /// <summary>
    /// Tell this trigger to stop listening for messages from the device
    /// </summary>
    /// <param name="p_nCallerID">unique id of caller (typically: event or guiAction)</param>
    /// <remarks>this is typically called when the event containing this trigger is disabled</remarks>
    public override void Unsubscribe(UInt64 p_nCallerID)
    {
      // do reference counting update then unsubscribe if no longer needed
      if (NoLongerNeedToBeSubscribed(p_nCallerID))
      {
        if (m_dvcTriggerSource != null)
        {
          m_dvcTriggerSource.PropertyChanged -= DvcTriggerSource_PropertyChanged;
        }
        else
        {
          NodeManager.Instance.NodeDiscovered -= NodeDiscovered_Handler;
          NodeManager.Instance.NodeRemoved -= NodeRemoved_Handler;
        }
      }
    }

    /// <summary>
    /// Device is telling us its status has changed
    /// </summary>
    /// <param name="p_objSender"></param>
    /// <param name="p_pcEvArgs"></param>
    private void DvcTriggerSource_PropertyChanged(object p_objSender, PropertyChangedEventArgs p_pcEvArgs)
    {
      if (p_pcEvArgs.PropertyName == "DeviceStatus")
      {
        Device dDevice = p_objSender as Device;
        if (dDevice != null)
        {
          switch (dDevice.DeviceStatus)
          {
            case Device.StatusEnum.ACTIVE:
              ValueParameter.IntValue = (int) StateEnum.ACTIVE;
              break;
            case Device.StatusEnum.RECENT:
              ValueParameter.IntValue = (int) StateEnum.RECENT;
              break;
            case Device.StatusEnum.DEAD:
              ValueParameter.IntValue = (int) StateEnum.DEAD;
              break;
            default:
              ValueParameter.IntValue = (int) StateEnum.UNKNOWN;
              break;
          }

          // Store the lastest parameter value in the value parameter
          ValueParameter.StringValue = Value;

          InvokePropertyChanged(new PropertyChangedEventArgs("Value"));

          ConditionallyCountInstance(); // update counters and value match data

          // indicate that this trigger has fired since last reset
          if (DidFire)
          {
            // now tell scheduler that some event may have come ready
            DebugMsg("Device status arrived");
            EventScheduler.Instance.OnNodeStatusArrival(this, new EventArgs());
          }
        }
      }
    }

    #endregion

    #region Public Methods - Comparison Operators

    /// <summary>
    /// determine if the triggers are the same (except for their unique ID values)
    /// </summary>
    /// <param name="p_trNewTrigger">rhs trigger to compare</param>
    /// <returns>T/F where T means they are the same (without consideration of the ID)</returns>
    public override bool EqualsExceptID(Trigger p_trNewTrigger)
    {
      StatusTrigger trNewStatusTrigger = p_trNewTrigger as StatusTrigger;
      bool bEqualStatus = false;
      if (trNewStatusTrigger != null)
      {
        // IMPLE. NOTE: Value cannot be compared....
        bEqualStatus = (base.EqualsExceptID(p_trNewTrigger) &&
                        (DeviceID == trNewStatusTrigger.DeviceID) &&
                        (Name == trNewStatusTrigger.Name))
                         ? true
                         : false;
      }

      return bEqualStatus;
    }

    #endregion
  }
}