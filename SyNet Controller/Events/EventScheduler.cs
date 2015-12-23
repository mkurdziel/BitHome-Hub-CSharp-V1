#define DATETIME_TRIGGER_TEST_ACTIVE_NOT

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Xml.Serialization;
using SyNet.DataHelpers;
using SyNet.EventArguments;
using SyNet.Events.Triggers;
using SyNet.Protocol;
using Timer = System.Timers.Timer;

namespace SyNet.Events
{
  /// <summary>
  /// CLASS event scheduler
  /// 
  /// RESPONSIBILITIES:
  ///  - handle detection of need and firing of event-actions
  ///  - generate initial list of device triggers
  ///  - update device triggers on new node arrival
  /// 
  /// PATTERN: Singleton
  /// </summary>
  [XmlInclude(typeof(MessageTrigger))]
  [XmlInclude(typeof(DateTimeTrigger))]
  [XmlInclude(typeof(StatusTrigger))]
  public class EventScheduler : INotifyPropertyChanged
  {
    #region Private Static Data - Singleton Support

    // our instance lock
    private static readonly object s_objInstanceLock = new object();
    private static EventScheduler s_esInstance;

    /// <summary>
    ///   ID for Anonymous date/time trigger
    /// </summary>
    //public static UInt64 BASE_TRIGGER_ID_DATETIME = 1;

    #endregion

    #region Private Constants

    private const string STR_CLASS_NAME = "EventScheduler";

    private const string STR_DEVICE_TRIGGER_SUBSYSTEMNAME =
      "EventScheduler-DeviceTriggers";

    private const string STR_UI_TRIGGER_SUBSYSTEMNAME =
      "EventScheduler-UI-Triggers";


    #endregion

    #region Private Member Data

    private bool m_bIsRunning;
    private bool m_bIsShutdownRequested;
    private readonly Random m_rndRandom;
    private readonly Timer m_tmrSchedulerHeartbeat;
    private DateTime m_dtLatestTicTime;

    // debug support
    private string m_strDebugMsgSrc;

    /// <summary>
    /// ENUM named indexes into array of Manual Reset Events
    /// </summary>
    private enum EsnManualResetEventPurpose : int
    {
      /// <summary>
      /// system requested shutdown
      /// </summary>
      MRE_SHUTDOWN_REQUESTED = 0,
      /// <summary>
      /// A heartbeat timer has fired, this means we need to trip any DateTime triggers waiting for "Now"
      /// </summary>
      MRE_HEARTBEAT_TIC,
      /// <summary>
      /// a device return value message has arrived let's see if an event is now triggered
      /// </summary>
      MRE_MESSAGE_ARRIVED,
      /// <summary>
      /// the overall net state has changed let's see if an event is now triggered
      /// </summary>
      MRE_NET_STATE_CHANGED,
      /// <summary>
      /// an event was modified or added to our list of events, let's see if we need to set a timer or it's triggered
      /// </summary>
      MRE_EVENT_LIST_UPDATED,
    }

    // MAX_EVENTS_IN_ARRAY equates to the count of enum eMREventPurpose values
    private const int MAX_EVENTS_IN_ARRAY =
      ((int)EsnManualResetEventPurpose.MRE_EVENT_LIST_UPDATED + 1);

    private readonly ManualResetEvent[] m_mrevEventSetAr;


    // UNDONE thread safe?
    private SerializableDictionaryWithId<UInt64, Event> m_dctEventsByEventIDs;

    private SerializableDictionaryWithId<UInt64, Trigger> m_dctTriggersByTriggerIDs;
    private readonly SerializableDictionaryWithId<UInt64, Trigger> m_dctSpecialScreenTriggersByTriggerIDs;
    private UInt64[] m_nTriggerIDsAr;
    #endregion

    #region Constructors, Serialization Support

    /// <summary>
    /// Default Constructor
    /// </summary>
    public EventScheduler()
    {
      lock (s_objInstanceLock)
      {
        if (s_esInstance == null)
        {
          s_esInstance = this;
        }
      }

      m_tmrSchedulerHeartbeat = new Timer(60000); // 60-second interval
      m_tmrSchedulerHeartbeat.AutoReset = true;
      m_tmrSchedulerHeartbeat.Elapsed += TmrSchedulerHeartbeat_Elapsed;
      m_dtLatestTicTime = DateTime.MinValue;

      m_bIsRunning = false;
      m_bIsShutdownRequested = false;
      m_dctEventsByEventIDs = new SerializableDictionaryWithId<ulong, Event>();
      m_dctTriggersByTriggerIDs = new SerializableDictionaryWithId<ulong, Trigger>();
      m_dctSpecialScreenTriggersByTriggerIDs = new SerializableDictionaryWithId<ulong, Trigger>();
      m_nTriggerIDsAr = new ulong[0];
      m_rndRandom = new Random();
      m_strDebugMsgSrc = "Not Set";

      m_mrevEventSetAr = new ManualResetEvent[MAX_EVENTS_IN_ARRAY];
      for (int nMreIdx = 0; nMreIdx < MAX_EVENTS_IN_ARRAY; nMreIdx++)
      {
        m_mrevEventSetAr[nMreIdx] = new ManualResetEvent(false);
      }

      SyNetSettings.Instance.DeserializingFinished += InitAfterDeserialization;
    }

    private void TmrSchedulerHeartbeat_Elapsed(object p_objSender, ElapsedEventArgs p_evElapsedArgs)
    {
      m_dtLatestTicTime = p_evElapsedArgs.SignalTime;
      m_mrevEventSetAr[(int)EsnManualResetEventPurpose.MRE_HEARTBEAT_TIC].Set();
      DebugMsg(string.Format("Heartbeat Tick fired @{0}", m_dtLatestTicTime));
    }

    /// <summary>
    /// After Deserialization we need to rebuild non-serialized 
    /// parallel data, let's do so now...
    /// </summary>
    private void InitAfterDeserialization()
    {
      Debug.WriteLine(string.Format("[DBG-Dser] EventScheduler.DeserializingFinished - ENTRY"));

      NodeManager.Instance.NodeDiscovered += NodeDiscovered_Handler;
      CreateDeviceTriggers();
      CreateTimeTriggers();
      RebuildScreenTriggerList();
      SetupSchedulerEvents();

      Debug.WriteLine(string.Format("[DBG-Dser] EventScheduler.DeserializingFinished - EXIT "));
    }

    private void RebuildScreenTriggerList()
    {
      m_dctSpecialScreenTriggersByTriggerIDs.Clear();
      if (m_nTriggerIDsAr == null)
      {
        m_nTriggerIDsAr = new ulong[0];
      }
      foreach (ulong nUITriggerID in m_nTriggerIDsAr)
      {
        Debug.Assert(TriggerIDExists(nUITriggerID), string.Format("EventScheduler.RebuildScreenTriggerList: TriggerID not found in master list! [{0}]", nUITriggerID));
        m_dctSpecialScreenTriggersByTriggerIDs[nUITriggerID] = KnownTrigger(nUITriggerID);
      }
    }

    private void CreateTimeTriggers()
    {
      // create a single date time trigger that fires every minute
      TimeSpan tsSystemTickOneMinute = new TimeSpan(0, 0, 1, 0); // fire once every minute
      DateTimeTrigger dtTrigger = new DateTimeTrigger(tsSystemTickOneMinute);
      dtTrigger.Name = "Every Minute";
      DebugMsg(
        STR_DEVICE_TRIGGER_SUBSYSTEMNAME,
        string.Format("Adding Trigger for the SystemTick (Scheduler Heart beat)"));
      AddTrigger(dtTrigger);
    }

    private void SetupSchedulerEvents()
    {
      DebugMsg("** Enabling Scheduler Heartbeat **");
      m_tmrSchedulerHeartbeat.Enabled = true;
    }

    /// <summary>
    /// R/W PROPERTY: get/set the list of this event's triggers
    /// </summary>
    /// <remarks>(Serialization support ONLY)</remarks>
    [XmlElement(ElementName = "SchedulerEventSet")]
    public SerializableDictionaryWithId<ulong, Event> DctEventsByEventIDs
    {
      get { return m_dctEventsByEventIDs; }
      set
      {
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          throw new NotImplementedException(
            "Setting of this property by means other than deserialization is not supported");
        }
        m_dctEventsByEventIDs = value;
      }
    }

    /// <summary>
    /// R/W PROPERTY: get/set the list of this event's triggers
    /// </summary>
    /// <remarks>(Serialization support ONLY)</remarks>
    [XmlElement(ElementName = "SchedulerDeviceTriggers")]
    public SerializableDictionaryWithId<UInt64, Trigger> DctTriggersByTriggerIDs
    {
      get { return m_dctTriggersByTriggerIDs; }
      set
      {
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          throw new NotImplementedException(
            "Setting of this property by means other than deserialization is not supported");
        }
        m_dctTriggersByTriggerIDs = value;
      }
    }

    /// <summary>
    /// R/W PROPERTY: get/set the list of this event's triggers
    /// </summary>
    /// <remarks>(Serialization support ONLY)</remarks>
    [XmlArray(ElementName = "SchedulerUserInterfaceTriggerIDs")]
    [XmlArrayItem(typeof(UInt64), ElementName = "UserInterfaceTriggerID")]
    public UInt64[] SchedulerUserInterfaceTriggerIDs
    {
      get
      {
        m_nTriggerIDsAr = new ulong[m_dctSpecialScreenTriggersByTriggerIDs.Count];
        if (m_nTriggerIDsAr.Count() > 0)
        {
          m_dctSpecialScreenTriggersByTriggerIDs.Keys.CopyTo(m_nTriggerIDsAr, 0);
        }
        return m_nTriggerIDsAr;
      }
      set
      {
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          throw new NotImplementedException(
            "Setting of this property by means other than deserialization is not supported");
        }
        m_nTriggerIDsAr = value;
      }
    }

    #endregion

    #region Private Device Trigger Generation Subsystem

    private void CreateDeviceTriggers()
    {
      List<Device> lstDevices = NodeManager.Instance.NodeList;
      // for all devices in device-list...
      foreach (Device dvcNode in lstDevices)
      {
        // add the set of device triggers
        AddTriggersForDevice(dvcNode);
      }

      // and finally, add our nodelist status-trigger
      //  NOTE: null parm identifies nodeList status configuration
      DebugMsg(
        STR_DEVICE_TRIGGER_SUBSYSTEMNAME,
        string.Format("Adding Trigger for NodeList"));
      AddTrigger(new StatusTrigger((Device)null));
    }

    private void AddTriggersForDevice(Device p_dvcNode)
    {
      DebugMsg(
        STR_DEVICE_TRIGGER_SUBSYSTEMNAME,
        string.Format("Adding Triggers for dvc={0}", p_dvcNode));

      // add function triggers for this device
      foreach (DeviceFunction dfFunction in p_dvcNode.Catalog.Values)
      {
        if (dfFunction.ReturnType !=
            EsnDataTypes.VOID)
        {
          MessageTrigger mtNewTrigger = new MessageTrigger(
            p_dvcNode, dfFunction);
          AddTrigger(mtNewTrigger);
        }
      }

      // add status trigger for this device
      StatusTrigger stNewTrigger = new StatusTrigger(p_dvcNode);
      AddTrigger(stNewTrigger);
    }

    private Trigger KnownTrigger(UInt64 p_nTriggerID)
    {
      Trigger trFoundTrigger = null;
      foreach (Trigger trTriggerInHand in Triggers)
      {
        if (trTriggerInHand.ID == p_nTriggerID)
        {
          trFoundTrigger = trTriggerInHand;
          break; // abort 'cause we know our answer
        }
      }
      return trFoundTrigger;
    }

    private bool TriggerIDExists(UInt64 p_nTriggerID)
    {
      bool bIsTriggerKnown = false;
      foreach (Trigger trTriggerInHand in Triggers)
      {
        if (trTriggerInHand.ID == p_nTriggerID)
        {
          bIsTriggerKnown = true;
          break; // abort 'cause we know our answer
        }
      }
      return bIsTriggerKnown;
    }

    private bool TriggerExists(Trigger p_trNewTrigger)
    {
      bool bIsTriggerKnown = false;
      foreach (Trigger trTriggerInHand in Triggers)
      {
        if (trTriggerInHand.EqualsExceptID(p_trNewTrigger))
        {
          bIsTriggerKnown = true;
          break; // abort 'cause we know our answer
        }
      }
      return bIsTriggerKnown;
    }

    private void AddTrigger(Trigger p_mtNewTrigger)
    {
      if (!TriggerExists(p_mtNewTrigger))
      {
        DebugMsg(
          STR_DEVICE_TRIGGER_SUBSYSTEMNAME,
          string.Format("- Adding Trigger name={0}", p_mtNewTrigger.Name));
        m_dctTriggersByTriggerIDs[p_mtNewTrigger.ID] = p_mtNewTrigger;

        // notify observers...
        OnPropertyChanged("Triggers");
      }
      else
      {
        DebugMsg(
          STR_DEVICE_TRIGGER_SUBSYSTEMNAME,
          string.Format(
            "- OOPs Trigger name={0} Already Exists", p_mtNewTrigger.Name));
      }
    }

    private void NodeDiscovered_Handler(
      object p_sender, NodeDiscoveredEventArgs p_args)
    {
      UInt64 nDeviceID = p_args.DeviceID;
      Device dvcNode = NodeManager.Instance.GetNode(nDeviceID);

      // if device has no triggers...
      if (!HaveTriggersForDevice(nDeviceID))
      {
        // add all triggers
        AddTriggersForDevice(dvcNode);
        // notify of updated triggerset
        OnDeviceTriggerSetChanged();
      }
      else
      {
        //   if not all triggers the same
        if (!HaveIdenticalTriggersForDevice(dvcNode))
        {
          // replace the triggers
          RemoveTriggersForDevice(nDeviceID);
          AddTriggersForDevice(dvcNode);
          // notify of updated triggerset
          OnDeviceTriggerSetChanged();
        }
      }
    }

    private bool HaveIdenticalTriggersForDevice(Device p_dvcNode)
    {
      bool bHaveIdenticalTriggerSets = true;
      // (a) get existing device triggers into dictionary
      SerializableDictionaryWithId<UInt64, Trigger> dctCurrTriggersByTriggerIDs
        = new SerializableDictionaryWithId<UInt64, Trigger>();
      foreach (Trigger trTriggerInHand in Triggers)
      {
        MessageTrigger trMessageTriggerInHand =
          trTriggerInHand as MessageTrigger;
        if (trMessageTriggerInHand != null)
        {
          dctCurrTriggersByTriggerIDs[trMessageTriggerInHand.TriggerID] =
            trMessageTriggerInHand;
        }
      }
      // (b) get new divice triggers into dictionary
      SerializableDictionaryWithId<UInt64, Trigger> dctNewTriggersByTriggerIDs =
        new SerializableDictionaryWithId<UInt64, Trigger>();
      foreach (DeviceFunction dfFunction in p_dvcNode.Catalog.Values)
      {
        if (dfFunction.ReturnType !=
            EsnDataTypes.VOID)
        {
          MessageTrigger mtNewTrigger = new MessageTrigger(
            p_dvcNode, dfFunction);
          dctNewTriggersByTriggerIDs[mtNewTrigger.TriggerID] = mtNewTrigger;
        }
      }
      // (c) if counts are not the same we can assume the lists are different
      if (dctCurrTriggersByTriggerIDs.Count !=
          dctNewTriggersByTriggerIDs.Count)
      {
        bHaveIdenticalTriggerSets = false;
      }
      else
      {
        // the number of triggers are the same, so let's compare in detail
        // for each current device trigger...
        foreach (Trigger trCurrTrigger in dctCurrTriggersByTriggerIDs.Values)
        {
          MessageTrigger trCurrMessageTrigger = trCurrTrigger as MessageTrigger;
          if (trCurrMessageTrigger != null)
          {
            // do we have a new device trigger?
            bool bFoundMatchingNewTrigger = false;
            // for each new device trigger...
            foreach (Trigger trNewTrigger in dctNewTriggersByTriggerIDs.Values)
            {
              MessageTrigger trNewMessageTrigger =
                trNewTrigger as MessageTrigger;
              if (trNewMessageTrigger != null)
              {
                // do we have a matching trigger?
                if (trCurrMessageTrigger.EqualsExceptID(trNewMessageTrigger))
                {
                  // yes we have a curr and new pair, let's check next curr
                  bFoundMatchingNewTrigger = true;
                  break; // abort 'cause we know our answer
                }
              }
            }
            // we found a current without a matching new... the lists are not the same
            if (!bFoundMatchingNewTrigger)
            {
              bHaveIdenticalTriggerSets = false;
              break; // abort 'cause we know our answer
            }
          }
        }
      }
      return bHaveIdenticalTriggerSets;
    }

    private void RemoveTriggersForDevice(UInt64 p_nDeviceID)
    {
      foreach (Trigger trTriggerInHand in Triggers)
      {
        MessageTrigger trMessageTriggerInHand =
          trTriggerInHand as MessageTrigger;
        if (trMessageTriggerInHand != null &&
            trMessageTriggerInHand.DeviceID == p_nDeviceID)
        {
          m_dctTriggersByTriggerIDs.Remove(trMessageTriggerInHand.TriggerID);
        }
      }
    }

    private bool HaveTriggersForDevice(UInt64 p_nDeviceID)
    {
      bool bHasTriggers = false;
      foreach (Trigger trTriggerInHand in Triggers)
      {
        MessageTrigger trMessageTriggerInHand =
          trTriggerInHand as MessageTrigger;
        if (trMessageTriggerInHand != null &&
            trMessageTriggerInHand.DeviceID == p_nDeviceID)
        {
          bHasTriggers = true;
          break; // abort 'cause we know our answer
        }
      }
      return bHasTriggers;
    }

    #endregion

    #region Event Notification

    /// <summary>
    /// EVENT subscribe to this event to hear of trigger-set membership changes..
    /// </summary>
    /// <remarks>This is typically used signal update events to GUIs</remarks>
    public event EventHandler DeviceTriggerSetChanged;

    /// <summary>
    /// Notify subscribers that our set of triggers has cnahg
    /// </summary>
    /// <remarks>typically used by GUIs to know when to refresh lists</remarks>
    private void OnDeviceTriggerSetChanged()
    {
      EventHandler changed = DeviceTriggerSetChanged;
      if (changed != null)
      {
        EventArgs eaDummyArguments = new EventArgs();
        changed(this, eaDummyArguments);
      }
    }

    #endregion

    #region Singleton-Access Method

    /// <summary>
    ///   Singleton instance of the ActionManager
    /// </summary>
    [XmlIgnore]
    public static EventScheduler Instance
    {
      get
      {
        lock (s_objInstanceLock)
        {
          if (s_esInstance == null)
          {
            s_esInstance = new EventScheduler();
          }
          return s_esInstance;
        }
      }
      set
      {
        lock (s_objInstanceLock)
        {
          s_esInstance = value;
        }
      }
    }

    #endregion

    #region Event-system UniqID generation methods

    /// <summary>
    ///   Generate a new trigger/event ID that is guaranteed to be unique
    /// </summary>
    /// <returns>64bit ID that is unique amongst events and triggers and non-zero</returns>
    public UInt64 NewEventSystemUniqueID()
    {
      UInt64 nNewID = GenerateRandomID();

      lock (s_objInstanceLock)
      {
        // TODO: Hack - must be a better way to prevent generation of predefined base ids
        while (m_dctEventsByEventIDs.ContainsKey(nNewID) ||
               m_dctTriggersByTriggerIDs.ContainsKey(nNewID) ||
               nNewID == 0)
        {
          nNewID = GenerateRandomID();
        }
      }
      return nNewID;
    }

    /// <summary>
    ///   Generate a random Uint64 to be used as an ID
    /// </summary>
    /// <returns></returns>
    private UInt64 GenerateRandomID()
    {
      byte[] bytes = new byte[8];
      m_rndRandom.NextBytes(bytes);
      return BitConverter.ToUInt64(bytes, 0);
    }

    #endregion

    #region Scheduler Interface for use by GUI

    /// <summary>
    /// add trigger to be scheduled which is not contained within an event
    /// </summary>
    /// <param name="p_trOnScreenTrigger">The new trigger</param>
    public void RegisterUserInterfaceOwnedTrigger(Trigger p_trOnScreenTrigger)
    {
      if (!TriggerExists(p_trOnScreenTrigger))
      {
        DebugMsg(
          STR_UI_TRIGGER_SUBSYSTEMNAME,
          string.Format("- Adding UI Trigger name={0} to master Trigger List", p_trOnScreenTrigger.Name));

        // add to master trigger list
        AddTrigger(p_trOnScreenTrigger);
        //
      }

      // add to our internal UI Trigger list (special scheduler list)
      if (!m_dctSpecialScreenTriggersByTriggerIDs.ContainsKey(p_trOnScreenTrigger.ID))
      {
        DebugMsg(
          STR_UI_TRIGGER_SUBSYSTEMNAME,
          string.Format("- Adding UI Trigger name={0} to Screen Trigger List", p_trOnScreenTrigger.Name));
        m_dctSpecialScreenTriggersByTriggerIDs[p_trOnScreenTrigger.ID] = p_trOnScreenTrigger;
      }
    }

    #endregion

    #region Event Editor Access

    /// <summary>
    ///   Returns true if the thread is running
    /// </summary>
    public bool IsRunning
    {
      get { return m_bIsRunning; }
    }

    /// <summary>
    ///   Returns a trigger based on its trigger ID. Returns null if not found.
    /// </summary>
    /// <param name="p_triggerID"></param>
    /// <returns></returns>
    public Trigger GetTrigger(UInt64 p_triggerID)
    {
      Trigger trigger = null;

      if (DctTriggersByTriggerIDs.ContainsKey(p_triggerID))
      {
        trigger = DctTriggersByTriggerIDs[p_triggerID];
      }
      else
      {
        Debug.WriteLine("[ERR] EventScheduler.GetTrigger - Could not find trigger");
      }

      return trigger;
    }

    /// <summary>
    /// Add new event to our list of known events
    /// </summary>
    /// <param name="p_evNewEvent">the new event from our editor to be added</param>
    public void AddEvent(Event p_evNewEvent)
    {
      if (!DctEventsByEventIDs.ContainsKey(p_evNewEvent.EventID))
      {
        DctEventsByEventIDs[p_evNewEvent.EventID] = p_evNewEvent;
        OnPropertyChanged("Events");
      }
      else
      {
        throw new ArgumentException(
          "ERROR attempting to add event with duplicate ID", "p_evNewEvent");
      }
    }

    /// <summary>
    ///   Remove an event from the event scheduler
    /// </summary>
    /// <param name="p_event"></param>
    public void RemoveEvent(Event p_event)
    {
      if (DctEventsByEventIDs.ContainsKey(p_event.EventID))
      {
        DctEventsByEventIDs.Remove(p_event.EventID);
        OnPropertyChanged("Events");
      }
    }

    /// <summary>
    /// Return list of Events (active and inactive)
    /// </summary>
    [XmlIgnore]
    public Event[] Events
    {
      get
      {
        Event[] evEventAr = new Event[DctEventsByEventIDs.Count];
        if (evEventAr.Count() > 0)
        {
          DctEventsByEventIDs.Values.CopyTo(evEventAr, 0);
        }
        return evEventAr;
      }
    }


    /// <summary>
    /// Return list of Device Triggers
    /// </summary>
    [XmlIgnore]
    public Trigger[] Triggers
    {
      get
      {
        Trigger[] evTriggerAr = new Trigger[DctTriggersByTriggerIDs.Count];
        if (evTriggerAr.Count() > 0)
        {
          DctTriggersByTriggerIDs.Values.CopyTo(evTriggerAr, 0);
        }
        return evTriggerAr;
      }
    }

    #endregion

    #region Scheduler Thread and Thread Control Methods

    /// <summary>
    /// THREAD the scheduler thread
    /// 
    /// This loop is designed to stall completely until
    /// an outside source indicates that work is needing to be done
    /// 
    /// Upon wakeup the reason for wakeup determines the work to be done
    /// 
    /// The work is done and then the thread returns to a waiting (stalled) condition
    /// </summary>
    public void ScheduleEventsThread()
    {
      const string STR_THREAD_NAME = "THREAD-ScheduleEvents";

      // show that our thread is looping
      m_bIsRunning = true;


#if DATETIME_TRIGGER_TEST_ACTIVE
      bool bIsFirstHeartbeatEvent = true;
#endif

      do
      {
        // reset our work needed flags
        bool bHuntForTriggeredEvent = false;
        bool bEvaluateDateTimeArrival = false;

        // stall this thread waiting for a signal that work needs to be done
        DebugMsg(STR_THREAD_NAME, "stalled...");

#if DATETIME_TRIGGER_TEST_ACTIVE
       // TEST TEST TEST code
        if (bIsFirstHeartbeatEvent)
        {
          bIsFirstHeartbeatEvent = false;
          DateTimeTrigger.TestDateTimeTriggerDescriptions();

          // stall this thread waiting for a signal that work needs to be done
          DebugMsg(STR_THREAD_NAME, "stalled...");
        }
#endif

        EsnManualResetEventPurpose nFiredEventId =
          (EsnManualResetEventPurpose)WaitHandle.WaitAny(m_mrevEventSetAr);

        // signaled, now which was it?
        switch (nFiredEventId)
        {
          case EsnManualResetEventPurpose.MRE_SHUTDOWN_REQUESTED:
            DebugMsg(STR_THREAD_NAME, "woke up due to shutdown request");
            m_mrevEventSetAr[(int)nFiredEventId].Reset();
            break;
          case EsnManualResetEventPurpose.MRE_EVENT_LIST_UPDATED:
            bHuntForTriggeredEvent = true;
            DebugMsg(STR_THREAD_NAME, "woke up due to event list shape change");
            m_mrevEventSetAr[(int)nFiredEventId].Reset();
            break;
          case EsnManualResetEventPurpose.MRE_MESSAGE_ARRIVED:
            bHuntForTriggeredEvent = true;
            DebugMsg(STR_THREAD_NAME, "woke up due to device message arrival");
            m_mrevEventSetAr[(int)nFiredEventId].Reset();
            break;
          case EsnManualResetEventPurpose.MRE_NET_STATE_CHANGED:
            bHuntForTriggeredEvent = true;
            DebugMsg(STR_THREAD_NAME, "woke up due to device/net status change");
            m_mrevEventSetAr[(int)nFiredEventId].Reset();
            break;
          case EsnManualResetEventPurpose.MRE_HEARTBEAT_TIC:
            bEvaluateDateTimeArrival = true;
            DebugMsg(STR_THREAD_NAME, "woke up due to point-in-time arrival");
            m_mrevEventSetAr[(int)nFiredEventId].Reset();
            break;
          default:
            break;
        }

        if (bEvaluateDateTimeArrival)
        {
          DebugMsg(STR_THREAD_NAME, "evaluating event-triggers at heartbeat");
          foreach (Event evCurrEvent in Events)
          {
            if (evCurrEvent.IsEnabled)
            {
              foreach (Trigger trCurrTrigger in evCurrEvent.Triggers)
              {
                DateTimeTrigger trCurrDateTimeTrigger = trCurrTrigger as DateTimeTrigger;
                if (trCurrDateTimeTrigger != null)
                {
                  trCurrDateTimeTrigger.EvaluateFireStatus(m_dtLatestTicTime);
                  if (trCurrDateTimeTrigger.DidFire)
                  {
                    DebugMsg(STR_THREAD_NAME, string.Format("Event DateTime Trigger:{0} Fired!", trCurrTrigger.Name));
                    bHuntForTriggeredEvent = true; // an event could now be ready, let's check for 'em
                  }
                }
              }
            }
          }

          DebugMsg(STR_THREAD_NAME, "evaluating UI-triggers at heartbeat");
          foreach (Trigger trCurrTrigger in m_dctSpecialScreenTriggersByTriggerIDs.Values)
          {
            DateTimeTrigger trCurrDateTimeTrigger = trCurrTrigger as DateTimeTrigger;
            if (trCurrDateTimeTrigger != null)
            {
              trCurrDateTimeTrigger.EvaluateFireStatus(m_dtLatestTicTime);
              if (trCurrDateTimeTrigger.DidFire)
              {
                DebugMsg(STR_THREAD_NAME, string.Format("UI DateTime Trigger:{0} Fired!", trCurrTrigger.Name));
              }
            }
          }
        }

        if (bHuntForTriggeredEvent)
        {
          DebugMsg(STR_THREAD_NAME, "hunting for possible triggered event");
          foreach (Event evCurrEvent in Events)
          {
            if (evCurrEvent.ReadyToExecute())
            {
              // conditions are good, let's do the execute
              object objLock;
              evCurrEvent.InstanceLock(out objLock);
              lock (objLock)
              {
                evCurrEvent.ResetAllTriggers();
                DebugMsg(string.Format(" - Firing Event={0}", evCurrEvent.Name));
                evCurrEvent.Execute();
              }
            }
            if (m_bIsShutdownRequested)
            {
              break; // abort when requested
            }
          }
        }
      } while (!m_bIsShutdownRequested);

      // show that our thread is no longer looping
      m_bIsRunning = false;
      DebugMsg(STR_THREAD_NAME, "thread stopped");
    }



    /// <summary>
    /// Signal scheduler to shutdown...
    /// </summary>
    public void Stop()
    {
      DebugMsg("requesting thread stop");
      m_bIsShutdownRequested = true;
      m_mrevEventSetAr[(int)EsnManualResetEventPurpose.MRE_SHUTDOWN_REQUESTED].Set();
    }

    /// <summary>
    /// Signal scheduler that a device trigger has fired
    /// </summary>
    /// <param name="p_objSender">the MessageTrigger object</param>
    /// <param name="p_eventArgs">dummy arguments, for now...</param>
    public void OnDeviceTriggerMessageArrival(object p_objSender, EventArgs p_eventArgs)
    {
      MessageTrigger mtSendingTrigger = p_objSender as MessageTrigger;
      Debug.Assert(
        mtSendingTrigger != null,
        "Sending object MUST be a MessageTrigger object");

      DebugMsg("received notfication of DeviceTrigger firing...");
      m_mrevEventSetAr[(int)EsnManualResetEventPurpose.MRE_MESSAGE_ARRIVED].Set();
    }

    /// <summary>
    /// we are to tell the thread that a Device or NodeList status has changed... so it
    /// should execute any events waiting for this to happen.
    /// </summary>
    /// <param name="p_objSender"></param>
    /// <param name="p_eventArgs"></param>
    public void OnNodeStatusArrival(object p_objSender, EventArgs p_eventArgs)
    {
      StatusTrigger mtSendingTrigger = p_objSender as StatusTrigger;
      Debug.Assert(
        mtSendingTrigger != null,
        "Sending object MUST be a StatusTrigger object");

      // NOTE nodeID added/removed is avail. but not used here
      // NOTE add/remove is also avail. but not used either

      DebugMsg("received notfication of StatusTrigger firing...");
      m_mrevEventSetAr[(int)EsnManualResetEventPurpose.MRE_NET_STATE_CHANGED].Set();
    }

    #endregion

    #region Utility Methods - Debug Support

    private string DebugSource
    {
      get { return m_strDebugMsgSrc; }
      set { m_strDebugMsgSrc = (value == string.Empty) ? STR_CLASS_NAME : value; }
    }

    private void DebugMsg(string p_strMessageText)
    {
      DebugMsg(string.Empty, p_strMessageText);
    }

    private void DebugMsg(string p_strMessageSource, string p_strMessageText)
    {
      DebugSource = p_strMessageSource;
      Debug.WriteLine(string.Format("[DBG] {0}: {1}", DebugSource, p_strMessageText));
    }

    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region Implementation of INotifyPropertyChanged

    /// <summary>
    /// EVENT: notify subscribers of Change in Property value
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// </summary>
    /// <param name="p_strPropertyName"></param>
    protected void OnPropertyChanged(string p_strPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(p_strPropertyName));
    }

    #endregion

    ///////////////////////////////////////////////////////////////////////////
  }
}