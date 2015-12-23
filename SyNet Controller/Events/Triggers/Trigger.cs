using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using SyNet.DataHelpers;

namespace SyNet.Events.Triggers
{
  /// <summary>
  /// CLASS base class of our trigger heirarchy
  /// </summary>
  [XmlInclude(typeof(MessageTrigger))]
  [XmlInclude(typeof(DateTimeTrigger))]
  [XmlInclude(typeof(StatusTrigger))]
  public class Trigger : INotifyPropertyChanged, IObjectWithID<ulong>
  {
    #region Public Constants / Enums

    /// <summary>
    /// ENUM: forms of value comparison
    /// </summary>
    public enum EsnMatchOperation
    {
      /// <summary>
      /// trigger when value is same
      /// </summary>
      VO_EQUAL,
      /// <summary>
      /// trigger when value is not same
      /// </summary>
      VO_NOT_EQUAL,
      /// <summary>
      /// trigger when value is less than given
      /// </summary>
      VO_LESS_THAN,
      /// <summary>
      /// trigger when value is greater than given
      /// </summary>
      VO_GREATER_THAN,
      /// <summary>
      /// trigger when value is less than or equal to given
      /// </summary>
      VO_LESS_THAN_OR_EQUAL,
      /// <summary>
      /// trigger when value is greater than or equal to given
      /// </summary>
      VO_GREATER_THAN_OR_EQUAL,
      /// <summary>
      /// trigger on any value (disable value match comparison, always matches)
      /// </summary>
      VO_ANYTHING
    }

    #endregion

    #region Private Member Data

    private const string STR_CLASS_NAME = "Trigger";

    private int m_nCurrentCount;

    private UInt64 m_nTriggerID;
    private bool m_bDidFire;
    private string m_strTriggerName;
    private readonly List<UInt64> m_lstSubscriberIDs;
    // debug support
    private string m_strDebugMsgSrc;
    private bool m_isValid = true; // By default is valid because... why not?

    #endregion

    #region Construction

    /// <summary>
    /// Default Constructor
    /// </summary>
    public Trigger()
    {
      m_nTriggerID = EventScheduler.Instance.NewEventSystemUniqueID();
      m_lstSubscriberIDs = new List<ulong>();
      CurrentCount = 0;

      InitializeTriggerParameters();
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="p_trRhs">source of our copy...</param>
    public Trigger(Trigger p_trRhs)
    {
      // values unique to new instance
      m_nTriggerID = EventScheduler.Instance.NewEventSystemUniqueID();
      m_lstSubscriberIDs = new List<ulong>();
      m_bDidFire = false;
      CurrentCount = 0;
      m_strTriggerName = p_trRhs.Name;

      // values copied from reference instance
      CounterValueParameter = new TriggerParameter(p_trRhs.CounterValueParameter);
      MatchOperationParameter = new TriggerParameter(p_trRhs.MatchOperationParameter);
      MatchValueParameter = new TriggerParameter(p_trRhs.MatchValueParameter);
      ValueParameter = new TriggerParameter(p_trRhs.ValueParameter);
    }

    /// <summary>
    ///   Initialize any parameters that are needed by the Trigger class
    /// </summary>
    private void InitializeTriggerParameters()
    {
      //
      // Initialize the Interval Count parameter
      //
      CounterValueParameter = new TriggerParameter();
      CounterValueParameter.DataType = Protocol.EsnDataTypes.DWORD;
      CounterValueParameter.ValidationType = Protocol.EsnParamValidationType.UNSIGNED_RANGE;
      CounterValueParameter.MaximumValue = int.MaxValue;
      CounterValueParameter.Name = "Counter Value";
      CounterValueParameter.IntValue = 1;

      //
      // Initialize the Value Operation parameter
      //
      MatchOperationParameter = new TriggerParameter();
      MatchOperationParameter.DataType = Protocol.EsnDataTypes.BYTE;
      MatchOperationParameter.ValidationType = Protocol.EsnParamValidationType.ENUMERATED;
      MatchOperationParameter.Name = "Match Operation";
      MatchOperationParameter.DctEnumValueByName.Add("ANYTHING", (int)EsnMatchOperation.VO_ANYTHING);
      MatchOperationParameter.DctEnumValueByName.Add("=", (int)EsnMatchOperation.VO_EQUAL);
      MatchOperationParameter.DctEnumValueByName.Add(">", (int)EsnMatchOperation.VO_GREATER_THAN);
      MatchOperationParameter.DctEnumValueByName.Add(">=", (int)EsnMatchOperation.VO_GREATER_THAN_OR_EQUAL);
      MatchOperationParameter.DctEnumValueByName.Add("<", (int)EsnMatchOperation.VO_LESS_THAN);
      MatchOperationParameter.DctEnumValueByName.Add("<=", (int)EsnMatchOperation.VO_LESS_THAN_OR_EQUAL);
      MatchOperationParameter.DctEnumValueByName.Add("!=", (int)EsnMatchOperation.VO_NOT_EQUAL);
      MatchOperationParameter.IntValue = (int)EsnMatchOperation.VO_ANYTHING;

      //
      // Initialize the Match Value parameter
      //
      MatchValueParameter = new TriggerParameter();
      MatchValueParameter.DataType = Protocol.EsnDataTypes.DWORD;
      MatchValueParameter.ValidationType = Protocol.EsnParamValidationType.UNSIGNED_FULL;
      MatchValueParameter.Name = "Match Value";
      MatchValueParameter.IntValue = 0;
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///   Returns true if the trigger setup is valid
    /// </summary>
    [XmlIgnore]
    public virtual bool IsValid
    {
      get { return m_isValid; }
      set
      {
        if (m_isValid != value)
        {
          m_isValid = value;
          InvokePropertyChanged(new PropertyChangedEventArgs("IsValid"));
        }
      }
    }

    /// <summary>
    /// R/O PROPERTY: get identifier of this trigger
    /// </summary>
    [XmlAttribute]
    public ulong TriggerID
    {
      get { return m_nTriggerID; }
      set
      {
        // UNDONE should this be mutable?  (R/O maybe better!)
        // TODO should this be number within device! (trigger 5 of device 7?) [allows devices to come/go without affecting triggerIDs]
        m_nTriggerID = value;
      }
    }

    /// <summary>
    /// The visible name of this trigger
    /// </summary>
    [XmlAttribute]
    public virtual string Name
    {
      get { return m_strTriggerName; }
      set { m_strTriggerName = value; }
    }

    /// <summary>
    /// R/W PROPERTY: Get/Set DidFire state
    /// </summary>
    /// <remarks>Signals property change when goes from false to true</remarks>
    [XmlIgnore]
    public virtual bool DidFire
    {
      get { return m_bDidFire; }
      set
      {
        bool bTriggerJustFired = (!m_bDidFire && value) ? true : false;
        m_bDidFire = value;
        if (bTriggerJustFired)
        {
          InvokePropertyChanged(new PropertyChangedEventArgs("DidFire"));
        }
      }
    }

    /// <summary>
    ///   Parameter holding the interval count for the trigger
    /// </summary>
    [XmlIgnore]
    public TriggerParameter CounterValueParameter { get; set; }

    /// <summary>
    /// R/W PROPERTY: Get/Set IntervalCount
    /// </summary>
    [XmlAttribute]
    public virtual Int64 CounterValue
    {
      get { return CounterValueParameter.IntValue; }
      set
      {
        if (CounterValueParameter.IntValue != value)
        {
          CounterValueParameter.IntValue = value;
          InvokePropertyChanged(new PropertyChangedEventArgs("CounterValue"));
        }
      }
    }

    /// <summary>
    ///   Parameter holding the occurance count parameter for the trigger
    /// </summary>
    [XmlIgnore]
    public TriggerParameter MatchOperationParameter { get; set; }

    /// <summary>
    /// R/W PROPERTY: Get/Set occurrence count
    /// </summary>
    [XmlAttribute]
    public int MatchOperation
    {
      get { return (int)MatchOperationParameter.IntValue; }
      set
      {
        if (MatchOperationParameter.IntValue != value)
        {
          EsnMatchOperation eNewType = EsnMatchOperation.VO_ANYTHING;
          switch (value)
          {
            case (int)EsnMatchOperation.VO_ANYTHING:
              eNewType = EsnMatchOperation.VO_ANYTHING;
              break;
            case (int)EsnMatchOperation.VO_EQUAL:
              eNewType = EsnMatchOperation.VO_EQUAL;
              break;
            case (int)EsnMatchOperation.VO_GREATER_THAN:
              eNewType = EsnMatchOperation.VO_GREATER_THAN;
              break;
            case (int)EsnMatchOperation.VO_GREATER_THAN_OR_EQUAL:
              eNewType = EsnMatchOperation.VO_GREATER_THAN_OR_EQUAL;
              break;
            case (int)EsnMatchOperation.VO_LESS_THAN:
              eNewType = EsnMatchOperation.VO_LESS_THAN;
              break;
            case (int)EsnMatchOperation.VO_LESS_THAN_OR_EQUAL:
              eNewType = EsnMatchOperation.VO_LESS_THAN_OR_EQUAL;
              break;
            case (int)EsnMatchOperation.VO_NOT_EQUAL:
              eNewType = EsnMatchOperation.VO_NOT_EQUAL;
              break;
            default:
              Debug.Assert(false, "[CODE] ERROR Bad value given to ValueOperation.set()");
              break;
          }
          MatchOperationParameter.IntValue = (int)eNewType;
          InvokePropertyChanged(new PropertyChangedEventArgs("MatchOperation"));
        }
      }
    }

    /// <summary>
    ///   Parameter holding the Match Value Parameter for this trigger
    /// </summary>
    [XmlIgnore]
    public TriggerParameter MatchValueParameter { get; set; }

    /// <summary>
    /// R/W PROPERTY: Get/Set value we trigger on (based on MatchOperation, of course)
    /// </summary>
    [XmlAttribute]
    public Int64 MatchValue
    {
      get { return MatchValueParameter.IntValue; }
      set
      {
        if (MatchValueParameter.IntValue != value)
        {
          MatchValueParameter.IntValue = value;
          InvokePropertyChanged(new PropertyChangedEventArgs("MatchValue"));
        }
      }
    }

    /// <summary>
    ///   Returns a readonly collection of parameters in the trigger
    /// </summary>
    [XmlIgnore]
    public virtual ReadOnlyCollection<TriggerParameter> Parameters
    {
      get
      {
        List<TriggerParameter> parameters = new List<TriggerParameter>();
        parameters.Add(CounterValueParameter);
        parameters.Add(MatchOperationParameter);
        parameters.Add(MatchValueParameter);
        return parameters.AsReadOnly();
      }
    }

    /// <summary>
    ///   Parameter holding the trigger value from the message
    /// </summary>
    [XmlIgnore]
    public TriggerParameter ValueParameter { get; set; }

    /// <summary>
    /// Return the newly arrived value (value which caused trigger DidFire)
    /// </summary>
    [XmlAttribute]
    public virtual string Value
    {
      get { return ValueParameter.StringValue; }
    }

    #endregion

    #region Public Methods - Trigger Control

    /// <summary>
    /// Reset the triggered condition
    /// </summary>
    public virtual void Reset()
    {
      DidFire = false;
      CurrentCount = 0;
    }

    /// <summary>
    /// INHERITED: call CountInstace after recording the new value as this
    /// routine will evaluate that trigger conditions have been met
    /// </summary>
    protected virtual void ConditionallyCountInstance()
    {
      bool bValueMatched = HaveValueMatch();
      if (bValueMatched)
      {
        CurrentCount++;
        if (AtCount())
        {
          DidFire = true;
        }
      }
    }

    /// <summary>
    /// will be a narrative summary of parameter values
    /// </summary>
    /// <returns>string containing setup description</returns>
    [XmlIgnore]
    public virtual string Description
    {
      get
      {
        return string.Empty;
      }
    }

    /// <summary>
    /// will be [fired last at ...|net yet fired], [will fire next at ...|will not fire again]
    /// </summary>
    /// <returns>string containing firing description</returns>
    public override string ToString()
    {
      return Name;
    }

    #endregion

    #region Public Methods - Live Notification support
    /// <summary>
    /// Tell this trigger to start listening for function return-value messages from the device
    /// </summary>
    /// <param name="p_nCallerID">id of subscriber</param>
    /// <remarks>this is typically called when the event containing this trigger is enabled</remarks>
    public virtual void Subscribe(UInt64 p_nCallerID)
    {
      // base class does nothing here...
    }

    /// <summary>
    /// Tell this trigger to stop listening for messages from the device
    /// </summary>
    /// <param name="p_nCallerID">id of subscriber</param>
    /// <remarks>this is typically called when the event containing this trigger is disabled</remarks>
    public virtual void Unsubscribe(UInt64 p_nCallerID)
    {
      // base class does nothing here...
    }

    #endregion

    #region Public Methods - Comparison Operators

    /// <summary>
    /// determine if the triggers are the same (except for their unique ID values)
    /// </summary>
    /// <param name="p_trNewTrigger">rhs trigger to compare</param>
    /// <returns>T/F where T means they are the same (without consideration of the ID)</returns>
    public virtual bool EqualsExceptID(Trigger p_trNewTrigger)
    {
      bool bEqualStatus = ((CounterValue == p_trNewTrigger.CounterValue) &&
                           (MatchValue == p_trNewTrigger.MatchValue) &&
                           (MatchOperation == p_trNewTrigger.MatchOperation))
                            ? true
                            : false;
      return bEqualStatus;
    }

    #endregion

    #region IObjectWithID<ulong> Methods

    /// <summary>
    /// The ID of this trigger (afford serialization in dictionary form)
    /// </summary>
    [XmlAttribute]
    public ulong ID
    {
      get { return TriggerID; }
    }

    #endregion

    #region Event Notification

    /// <summary>
    /// EVENT subscribe to this event to hear of property value changes for this instance
    /// </summary>
    /// <remarks>This is typically used signal update events to GUIs</remarks>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// INHERITABLE Notify subscribers that a property has changed
    /// </summary>
    /// <param name="p_pcEvArgs">a PropertyChangedEventArgs describing the property that changed</param>
    /// <remarks>typically used by GUIs</remarks>
    protected void InvokePropertyChanged(PropertyChangedEventArgs p_pcEvArgs)
    {
      PropertyChangedEventHandler changed = PropertyChanged;
      if (changed != null) changed(this, p_pcEvArgs);
    }

    #endregion

    #region Protected Properties

    /// <summary>
    /// INHERITABLE return the current count of matches that have occurred
    /// </summary>
    protected int CurrentCount
    {
      get { return m_nCurrentCount; }
      set { m_nCurrentCount = value; }
    }

    #endregion

    #region Protected Methods - Trigger Status

    /// <summary>
    /// INHERITABLE UTILITY: evaluate Value and Count settings to determine if this
    ///  trigger meets the requested conditions
    /// </summary>
    /// <remarks>value is evaluated first. If value conditions are met then count is checked!</remarks>
    protected virtual bool AtCount()
    {
      bool bAtCount = true;

      // evaluate count...
      if (CounterValue > 1)
      {
        bAtCount = (CurrentCount == CounterValue) ? true : false;
      }

      return (bAtCount) ? true : false;
    }

    /// <summary>
    /// INHERITABLE determine if Value relates to MatchValue per MatchOperation
    /// </summary>
    /// <returns>T/F where T means the value does match</returns>
    protected bool HaveValueMatch()
    {
      bool bValueMatched = false;
      switch (MatchOperation)
      {
        case (int)EsnMatchOperation.VO_ANYTHING:
          bValueMatched = true;
          break;
        case (int)EsnMatchOperation.VO_EQUAL:
          if (ValueParameter.ValidationType == Protocol.EsnParamValidationType.MAX_STRING_LEN)
          {
            bValueMatched = (MatchValueParameter.StringValue == ValueParameter.StringValue) ? true : false;
          }
          else
          {
            bValueMatched = (MatchValueParameter.IntValue == ValueParameter.IntValue) ? true : false;
          }
          break;
        case (int)EsnMatchOperation.VO_GREATER_THAN:
          if (ValueParameter.ValidationType > Protocol.EsnParamValidationType.MAX_STRING_LEN)
          {
            bValueMatched = (ValueParameter.StringValue.CompareTo(MatchValueParameter.StringValue) > 0) ? true : false;
          }
          else
          {
            bValueMatched = (ValueParameter.IntValue > MatchValueParameter.IntValue) ? true : false;
          }
          break;
        case (int)EsnMatchOperation.VO_GREATER_THAN_OR_EQUAL:
          if (ValueParameter.ValidationType == Protocol.EsnParamValidationType.MAX_STRING_LEN)
          {
            bValueMatched = (ValueParameter.StringValue.CompareTo(MatchValueParameter.StringValue) >= 0) ? true : false;
          }
          else
          {
            bValueMatched = (ValueParameter.IntValue >= MatchValueParameter.IntValue) ? true : false;
          }
          break;
        case (int)EsnMatchOperation.VO_LESS_THAN:
          if (ValueParameter.ValidationType == Protocol.EsnParamValidationType.MAX_STRING_LEN)
          {
            bValueMatched = (ValueParameter.StringValue.CompareTo(MatchValueParameter.StringValue) < 0) ? true : false;
          }
          else
          {
            bValueMatched = (ValueParameter.IntValue < MatchValueParameter.IntValue) ? true : false;
          }
          break;
        case (int)EsnMatchOperation.VO_LESS_THAN_OR_EQUAL:
          if (ValueParameter.ValidationType == Protocol.EsnParamValidationType.MAX_STRING_LEN)
          {
            bValueMatched = (ValueParameter.StringValue.CompareTo(MatchValueParameter.StringValue) <= 0) ? true : false;
          }
          else
          {
            bValueMatched = (ValueParameter.IntValue <= MatchValueParameter.IntValue) ? true : false;
          }
          break;
        case (int)EsnMatchOperation.VO_NOT_EQUAL:
          if (ValueParameter.ValidationType == Protocol.EsnParamValidationType.MAX_STRING_LEN)
          {
            bValueMatched = (MatchValueParameter.StringValue != ValueParameter.StringValue) ? true : false;
          }
          else
          {
            bValueMatched = (MatchValueParameter.IntValue != ValueParameter.IntValue) ? true : false;
          }
          break;
        default:
          Debug.Assert(false, string.Format("[CODE] ERROR Bad/Unknown match-operation value ({0}) given to Trigger.HaveMatchOrAtCount()", MatchOperationParameter.IntValue));
          break;
      }
      return bValueMatched;
    }


    #endregion

    #region Protected Methods - Subscription Reference Count Handling

    /// <summary>
    /// determine if this trigger needs to be subscribed
    /// </summary>
    /// <param name="p_nCallerID">id of subscriber</param>
    /// <returns>returns T/F where T means we need to do a Subscribe (there are now subscribers but weren't)</returns>
    protected bool NeedToSubscribe(ulong p_nCallerID)
    {
      // if going from 0 -> 1 this will be true...
      bool bNeedSubscribeStatus = HaveSubscribers() ? false : true;
      AddSubscriber(p_nCallerID);
      return bNeedSubscribeStatus;
    }

    /// <summary>
    /// determine if we can unsubscribe...
    /// </summary>
    /// <param name="p_nCallerID">id of subscriber</param>
    /// <returns>returns T/F where T means we need to unsubscribe (no more subscribers)</returns>
    protected bool NoLongerNeedToBeSubscribed(ulong p_nCallerID)
    {
      RemoveSubscriber(p_nCallerID);
      return HaveSubscribers();
    }


    /// <summary>
    /// determine if reference count is now zero
    /// </summary>
    /// <returns>return T/F where T means that reference count is NON-ZERO</returns>
    protected bool HaveSubscribers()
    {
      return (m_lstSubscriberIDs.Count > 0) ? true : false;
    }

    /// <summary>
    /// determine if subscriber is known
    /// </summary>
    /// <param name="p_nSubscriberID">id of subscriber</param>
    /// <returns>returns T/F where T means we this subscriber is alreay of file</returns>
    protected bool IsKnownSubscriber(UInt64 p_nSubscriberID)
    {
      return m_lstSubscriberIDs.Contains(p_nSubscriberID) ? true : false;
    }

    /// <summary>
    /// add a new subscriber
    /// </summary>
    /// <param name="p_nSubscriberID">id of subscriber</param>
    /// <remarks>will not add duplicates!</remarks>
    protected void AddSubscriber(UInt64 p_nSubscriberID)
    {
      if (!IsKnownSubscriber(p_nSubscriberID))
      {
        m_lstSubscriberIDs.Add(p_nSubscriberID);
      }
    }

    /// <summary>
    /// remove a subscriber
    /// </summary>
    /// <param name="p_nSubscriberID">id of subscriber</param>
    /// <remarks>simply does nothing if subscriber is not yet on list</remarks>
    protected void RemoveSubscriber(UInt64 p_nSubscriberID)
    {
      if (IsKnownSubscriber(p_nSubscriberID))
      {
        m_lstSubscriberIDs.Remove(p_nSubscriberID);
      }
    }

    #endregion

    #region Protected Methods -  Debug support

    /// <summary>
    /// R/W PROPERTY: get/set the current object debug-name string
    /// </summary>
    protected string DebugSource
    {
      get { return m_strDebugMsgSrc; }
      set { m_strDebugMsgSrc = (value == string.Empty) ? STR_CLASS_NAME : value; }
    }

    /// <summary>
    /// Write Debug message to debug output stream marking the text as being debug
    /// and coming from this object
    /// </summary>
    /// <param name="p_strMessageText">the message to be written to the debug log</param>
    protected void DebugMsg(string p_strMessageText)
    {
      DebugMsg(DebugSource, p_strMessageText);
    }

    /// <summary>
    /// Write Debug message to debug output stream marking the text as being debug
    /// and coming from the specified object
    /// </summary>
    /// <param name="p_strMessageSource">override identifying the debug-name of this object</param>
    /// <param name="p_strMessageText">the message to be written to the debug log</param>
    protected void DebugMsg(string p_strMessageSource, string p_strMessageText)
    {
      DebugSource = p_strMessageSource;
      Debug.WriteLine(string.Format("[DBG] {0}: {1}", DebugSource, p_strMessageText));
    }

    #endregion
  }
}