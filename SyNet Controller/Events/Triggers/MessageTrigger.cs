using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using SyNet.EventArguments;

namespace SyNet.Events.Triggers
{
  /// <summary>
  /// CLASS superclass of trigger (supporting signalling of new device funtion return values being heard)
  /// </summary>
  public class MessageTrigger : Trigger
  {
    #region Private Member Data (persisted)

    private UInt64 m_nDeviceID;
    private UInt64 m_nFunctionID;
    private FunctionReceivedEventArgs m_frValues;

    #endregion

    #region Private Member Data (NOT persisted)

    private Device m_dvcTriggerSource;
    private DeviceFunction m_dfFunction;

    #endregion

    #region Construction

    /// <summary>
    /// Constructor with initial data
    /// </summary>
    /// <param name="p_dvcNode">The device (node) associated with this trigger</param>
    /// <param name="p_dfFunction">The function whose return value is associated with this trigger</param>
    public MessageTrigger(Device p_dvcNode, DeviceFunction p_dfFunction)
    {
      Initialize();

      SetupTriggerSource(p_dvcNode);
      SetupTriggerFunction(p_dfFunction);
    }

    /// <summary>
    ///   Copy Constructor
    /// </summary>
    /// <param name="p_trRHS">object from which to get our </param>
    public MessageTrigger(MessageTrigger p_trRHS)
      : base(p_trRHS)
    {
      Initialize();

      SetupTriggerSource(p_trRHS.DeviceID);
      SetupTriggerFunction(p_trRHS.FunctionID);

      if (m_dvcTriggerSource != null && HaveSubscribers())
      {
        base.Subscribe(TriggerID); // re-subscribe if copy has subscribers
      }
    }

    /// <summary>
    /// Default Constructor (primarily used for deserialization)
    /// </summary>
    public MessageTrigger()
    {
      Initialize();
      SyNetSettings.Instance.DeserializingFinished += DeserializingFinished;
    }

    #region Private Methods Supporting Construction

    private void Initialize()
    {
      m_nDeviceID = UInt64.MaxValue; // device not yet specified
      m_nFunctionID = UInt64.MaxValue; // device not yet specified
      m_dvcTriggerSource = null;
      m_dfFunction = null;
      m_frValues = null;
    }

    private void DeserializingFinished()
    {
      Debug.WriteLine(string.Format("[DBG-Dser] MessageTrigger({0}).DeserializingFinished - ENTRY", TriggerID));
      if (m_nFunctionID != UInt64.MaxValue &&
          m_nDeviceID != UInt64.MaxValue)
      {
        SetupTriggerSource(m_nDeviceID);
        if (m_dvcTriggerSource != null)
        {
          SetupTriggerFunction(m_nFunctionID);
        }
        if (m_dvcTriggerSource != null && HaveSubscribers())
        {
          Subscribe(TriggerID); // re-subscribe if we were when we were persisted
        }
      }
      else
      {
        throw new ArgumentNullException();
      }
      Debug.WriteLine(string.Format("[DBG-Dser] MessageTrigger({0}).DeserializingFinished - EXIT", TriggerID));
    }

    /// <summary>
    ///  Wire up the trigger to the device
    /// </summary>
    /// <param name="p_dvcNode">starting with given device</param>
    private void SetupTriggerSource(Device p_dvcNode)
    {
      if (p_dvcNode != null)
      {
        SetupTriggerSource(p_dvcNode.ID);
      }
    }

    /// <summary>
    ///  Wire up the trigger to the device
    /// </summary>
    /// <param name="p_nDeviceID">starting from given device ID</param>
    private void SetupTriggerSource(UInt64 p_nDeviceID)
    {
      // if we haven't yet setup trigger-source and we should, then do so!
      // NOTE should this setup diff source if dvcID changes?
      if (m_dvcTriggerSource == null && p_nDeviceID != UInt64.MaxValue)
      {
        m_nDeviceID = p_nDeviceID;
        m_dvcTriggerSource = NodeManager.Instance.GetNode(m_nDeviceID);
      }
    }

    /// <summary>
    ///   Wire the trigger to the function
    /// </summary>
    private void SetupTriggerFunction(UInt64 p_nFunctionID)
    {
      if (m_dvcTriggerSource != null)
      {
        SetupTriggerFunction(m_dvcTriggerSource.GetFunction((int)p_nFunctionID));
      }
      else
      {
        Debug.WriteLine("[ERR] MessageTrigger.SetupTriggerFunction - Null Device");
      }
    }

    /// <summary>
    ///   Wire the trigger to the function
    /// </summary>
    private void SetupTriggerFunction(DeviceFunction p_dfFunction)
    {
      m_dfFunction = p_dfFunction;
      m_nFunctionID = (UInt64)m_dfFunction.ID;
      base.Name = m_dfFunction.Name;

      //
      // Initialize the value parameter with the return parameter of the function
      //
      DeviceParameter returnParam = p_dfFunction.GetParameter(0);
      InitializeMessageTriggerParameters(returnParam);
    }

    /// <summary>
    ///   Initialize any parameters that are needed by the MessageTrigger class
    /// </summary>
    private void InitializeMessageTriggerParameters(Parameter p_deviceParameter)
    {
      //
      // Initialize the Trigger Value parameter
      //
      ValueParameter = new TriggerParameter(p_deviceParameter);
      ValueParameter.Name = "Trigger Value";
    }

    #endregion

    #endregion

    #region Public Properties

    /// <summary>
    /// R/W PROPERTY: Get/Set Device ID for this trigger
    /// </summary>
    [XmlAttribute]
    public UInt64 DeviceID
    {
      get
      {
        Debug.Assert(m_nDeviceID != UInt64.MaxValue, "[CODE] ERROR MessageTrigger: not yet completely setup!");
        return m_nDeviceID;
      }
      set
      {
        m_nDeviceID = value;
        SetupTriggerSource(m_nDeviceID);
      }
    }

    /// <summary>
    /// R/W PROPERTY: Get/Set Function ID for this trigger
    /// </summary>
    [XmlAttribute]
    public UInt64 FunctionID
    {
      get { return m_nFunctionID; }
      set
      {
        m_nFunctionID = value;
        SetupTriggerFunction(m_nFunctionID);
      }
    }

    /// <summary>
    /// R/O PROPERTY: Returns a readonly collection of all parameters in this trigger
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
    /// R/O PROPERTY: Return the newly arrived value (value which caused trigger DidFire)
    /// </summary>
    [XmlIgnore]
    public override string Value
    {
      get
      {
        string strCurrentValue = m_frValues.IsStringDataType
                                   ? m_frValues.ChangedStringValue
                                   : m_frValues.ChangedIntValue.ToString();
        return strCurrentValue;
      }
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
      // check reference counting, update then subscribe if needed
      if (NeedToSubscribe(p_nCallerID))
      {
        Debug.Assert(m_dvcTriggerSource != null, "[CODE] ERROR Shouldn't have missing device at this point!");
        m_dvcTriggerSource.SubscribeFunctionCallback(
          m_dfFunction.ID, OnFunctionResultArrives);
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
        m_dvcTriggerSource.UnsubscribeFunctionCallback(
          m_dfFunction.ID, OnFunctionResultArrives);
      }
    }

    /// <summary>
    /// EVENT HANDLER: called when a return valued function message arrives from a device
    /// </summary>
    /// <param name="p_objSender"></param>
    /// <param name="p_evArgs"></param>
    private void OnFunctionResultArrives(
      object p_objSender, FunctionReceivedEventArgs p_evArgs)
    {
      // store the latest parameter value
      m_frValues = p_evArgs;

      // Store the lastest parameter value in the value parameter
      ValueParameter.StringValue = Value;

      // UNDONE int value is NOT setup but IS NEEDED!

      InvokePropertyChanged(new PropertyChangedEventArgs("Value"));

      ConditionallyCountInstance();  // update counters and value match data

      // indicate that this trigger has fired since last reset
      if (DidFire)
      {
        // now tell scheduler that some event may have come ready
        EventScheduler.Instance.OnDeviceTriggerMessageArrival(this, new EventArgs());
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
      MessageTrigger trNewMessageTrigger = p_trNewTrigger as MessageTrigger;

      return (trNewMessageTrigger != null &&
              (base.EqualsExceptID(p_trNewTrigger)) &&
              (DeviceID == trNewMessageTrigger.DeviceID) &&
              (FunctionID == trNewMessageTrigger.FunctionID) &&
              (Name == trNewMessageTrigger.Name))
               ? true
               : false;
    }

    #endregion
  }
}