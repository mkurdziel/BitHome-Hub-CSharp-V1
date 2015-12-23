using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SyNet.Actions;
using SyNet.DataHelpers;
using SyNet.Events.Triggers;
using Action = SyNet.Actions.Action;

namespace SyNet.Events
{
  /// <summary>
  /// CLASS Event - an event coordinating triggers with actions
  /// </summary>
  /// <remarks>Adheres to INotifyPropertyChanged and IObjectWithID interfaces</remarks>
  public class Event
    : INotifyPropertyChanged, IObjectWithID<ulong>
  {
    #region Public Interface Constants

    /// <summary>
    /// ENUM this event can trigger in one of two forms
    /// </summary>
    public enum ETriggerMode
    {
      /// <summary>
      /// ALL triggers must fire to execute this event's actions
      /// </summary>
      AllTriggersFired = 0,
      /// <summary>
      /// At least one trigger must fire to execute this event's actions
      /// </summary>
      AnyTriggersFired = 1,
    }

    #endregion

    #region Private Member Data

    private readonly object m_objInstanceLock;

    private bool m_bIsEnabled;
    private bool m_bIsComplete;
    private bool m_bIsFinishingDeserialization;
    private ETriggerMode m_eTriggerMode;

    private List<EventAction> m_lstActions = new List<EventAction>();
    private List<Trigger> m_listTriggers = new List<Trigger>();

    private UInt64 m_nEventID;

    #endregion

    #region Constructors

    /// <summary>
    /// Default Constructor
    /// </summary>
    public Event()
    {
      m_bIsEnabled = false;
      m_bIsComplete = false;
      m_eTriggerMode = ETriggerMode.AllTriggersFired;
      m_nEventID = EventScheduler.Instance.NewEventSystemUniqueID();
      m_objInstanceLock = new object();
      SyNetSettings.Instance.DeserializingFinished += App_DeserializingFinished;
      m_bIsFinishingDeserialization = false;
    }

    /// <summary>
    /// trigger and action dictionaries are not setup completely
    ///  now let's finish the setup....
    /// </summary>
    private void App_DeserializingFinished()
    {
      Debug.WriteLine(string.Format("[DBG-Dser] Event({0}).DeserializingFinished - ENTRY", EventID));
      // begin finishing
      m_bIsFinishingDeserialization = true;

      // repopulate our trigger list
      //UInt64[] nTriggerIdAr = new ulong[m_dctTriggersByTriggerID.Count];
      //m_dctTriggersByTriggerID.Keys.CopyTo(nTriggerIdAr, 0);
      //m_dctTriggersByTriggerID.Clear();
      //foreach (UInt64 nTriggerId in nTriggerIdAr)
      //{
      //  Trigger aFullTrigger = EventScheduler.Instance.GetTrigger(nTriggerId);
      //  AddTrigger(aFullTrigger);
      //}

      UpdateCompleteness();

      UpdateSubscriptions();

      // end finishing
      m_bIsFinishingDeserialization = false;

      Debug.WriteLine(string.Format("[DBG-Dser] Event({0}).DeserializingFinished - EXIT", EventID));
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///   Name of the event
    /// </summary>
    [XmlAttribute]
    public string Name { get; set; }

    /// <summary>
    ///   Gets or sets the trigger mode of the event
    /// </summary>
    [XmlAttribute]
    public ETriggerMode TriggerMode
    {
      get { return m_eTriggerMode; }
      set { m_eTriggerMode = value; }
    }

    /// <summary>
    /// Gets or sets the list of triggers for the action
    /// </summary>
    [XmlArray]
    public List<Trigger> Triggers
    {
      get
      {
        return m_listTriggers;
      }
      set
      {
        m_listTriggers = value;
      }
    }
    /// <summary>
    /// Retrieve a list of Actions associated with this event
    /// </summary>
    [XmlArray]
    public List<EventAction> Actions
    {
      get { return m_lstActions; }
      set { m_lstActions = value; }
    }

    /// <summary>
    /// R/W PROPERTY: Get/Set the event ID for this event
    /// </summary>
    [XmlAttribute]
    public ulong EventID
    {
      get { return m_nEventID; }
      set { m_nEventID = value; }
    }

    /// <summary>
    /// R/W PROPERTY: Get/Set the 
    /// </summary>
    [XmlAttribute]
    public bool IsEnabled
    {
      get { return m_bIsEnabled; }
      set
      {
        bool bOrigValue = m_bIsEnabled;
        m_bIsEnabled = value;

        // if things have changed, then update our subscriptions
        if (bOrigValue != m_bIsEnabled)
        {
          UpdateSubscriptions();
        }
      }
    }

    /// <summary>
    /// In order to make event trigger scanning less impactful
    /// our triggers actually subscribe to notifications.
    /// 
    /// To keep our system running well, on event enable, we
    /// subscribe to triggering changes and on disable, we 
    /// remove this subscription
    /// </summary>
    private void UpdateSubscriptions()
    {
      foreach (Trigger trTrigger in Triggers)
      {
        if (m_bIsEnabled)
        {
          trTrigger.Subscribe(EventID);
        }
        else
        {
          trTrigger.Unsubscribe(EventID);
        }
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 
    /// </summary>
    public void Execute()
    {
      foreach (EventAction action in Actions)
      {
        if (action.ConditionalsPass)
        {
          ActionManager.Instance.ExecuteThreadedAction(action.ActionID);
        }
      }
    }

    /// <summary>
    /// Add a trigger to this event
    /// </summary>
    /// <param name="p_trNewTrigger">the trigger to be added</param>
    public void AddTrigger(Trigger p_trNewTrigger)
    {

      foreach (Trigger trigger in Triggers)
      {
        if (p_trNewTrigger.EqualsExceptID(trigger))
        {
          throw new ArgumentException(
        "ERROR attempting to add trigger with duplicate ID", "p_trNewTrigger");
        }
      }

      if (p_trNewTrigger is MessageTrigger)
      {
        Triggers.Add(new MessageTrigger(p_trNewTrigger as MessageTrigger));
      }
      else if (p_trNewTrigger is DateTimeTrigger)
      {
        Triggers.Add(new DateTimeTrigger(p_trNewTrigger as DateTimeTrigger));
      }
      else if (p_trNewTrigger is StatusTrigger)
      {
        Triggers.Add(new StatusTrigger(p_trNewTrigger as StatusTrigger));
      }
      else
      {
        throw new ArgumentException(
          "ERROR Invalid trigger type", "p_trNewTrigger");
      }

      if (!m_bIsFinishingDeserialization)
      {
        InvokePropertyChanged(new PropertyChangedEventArgs("Triggers"));
      }
      UpdateCompleteness();
    }

    private void UpdateCompleteness()
    {
      m_bIsComplete = (Triggers.Count > 0 &&
                       m_lstActions.Count > 0)
                        ? true
                        : false;

    }

    /// <summary>
    /// remove a trigger from this event
    /// </summary>
    /// <param name="p_trDeadTrigger">the trigger to remove</param>
    public void RemoveTrigger(Trigger p_trDeadTrigger)
    {
      if (Triggers.Contains(p_trDeadTrigger))
      {
        Triggers.Remove(p_trDeadTrigger);
        InvokePropertyChanged(new PropertyChangedEventArgs("Triggers"));
        UpdateCompleteness();
      }
    }

    /// <summary>
    /// add an action to this event
    /// </summary>
    /// <param name="p_acNewAction"></param>
    public void AddAction(Action p_acNewAction)
    {
      //
      // Add a new event action to this event
      //
      m_lstActions.Add(new EventAction(p_acNewAction));

      if (!m_bIsFinishingDeserialization)
      {
        InvokePropertyChanged(new PropertyChangedEventArgs("Actions"));
      }
      UpdateCompleteness();
    }

    /// <summary>
    /// remove an action from this event
    /// </summary>
    /// <param name="p_trDeadAction"></param>
    public void RemoveAction(EventAction p_trDeadAction)
    {
      if (m_lstActions.Contains(p_trDeadAction))
      {
        m_lstActions.Remove(p_trDeadAction);
        InvokePropertyChanged(new PropertyChangedEventArgs("Actions"));
        UpdateCompleteness();
      }
    }

    /// <summary>
    /// determine if this event is ready to be run
    /// </summary>
    /// <returns>T/F where T means the reguired amount of triggers are fired so this event should fire its action(s)</returns>
    public bool ReadyToExecute()
    {
      bool bIsReadyToExecute = false;

      if (m_bIsComplete && m_bIsEnabled)
      {
        bool bAllTriggersFired = true;

        foreach (Trigger trCurrTrigger in Triggers)
        {
          // if we need all fired then the first one that didn't is our answer
          if (!trCurrTrigger.DidFire &&
              m_eTriggerMode == ETriggerMode.AllTriggersFired)
          {
            bAllTriggersFired = false;
            break;
          }

          // if we need only one to fire then the first one that did is our answer
          if (trCurrTrigger.DidFire &&
              m_eTriggerMode == ETriggerMode.AnyTriggersFired)
          {
            bIsReadyToExecute = true;
            break;
          }
        }

        // if all fired, we don't care what mode we are in
        if (bAllTriggersFired)
        {
          bIsReadyToExecute = true;
        }
      }

      return bIsReadyToExecute;
    }

    /// <summary>
    /// Reset the triggers for this event so it is waiting to be triggered once more
    /// </summary>
    public void ResetAllTriggers()
    {
      foreach (Trigger trigger in Triggers)
      {
        trigger.Reset();
      }
    }

    /// <summary>
    /// lock this event
    /// </summary>
    /// <param name="p_refLockObject"></param>
    public void InstanceLock(out object p_refLockObject)
    {
      p_refLockObject = m_objInstanceLock;
    }

    #endregion

    #region Event Notification

    /// <summary>
    /// EVENT subscribe to this event to hear of property value changes for this instance
    /// </summary>
    /// <remarks>This is typically used signal update events to GUIs</remarks>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Notify subscribers that a property has changed
    /// </summary>
    /// <param name="p_pcEvArgs">a PropertyChangedEventArgs describing the property that changed</param>
    /// <remarks>typically used by GUIs</remarks>
    private void InvokePropertyChanged(PropertyChangedEventArgs p_pcEvArgs)
    {
      PropertyChangedEventHandler changed = PropertyChanged;
      if (changed != null) changed(this, p_pcEvArgs);
    }

    #endregion

    #region IObjectWithID<ulong> Interface

    /// <summary>
    /// The ID of this event (afford serialization in dictionary form)
    /// </summary>
    [XmlAttribute]
    public ulong ID
    {
      get { return EventID; }
    }

    #endregion

    #region IXmlSerializable Members

    /// <summary>
    /// This method is reserved and should not be used. When implementing the 
    /// IXmlSerializable interface, you should return null (Nothing in Visual 
    /// Basic) from this method, and instead, if specifying a custom schema 
    /// is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> 
    /// to the class.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML 
    /// representation of the object that is produced by the 
    /// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> 
    /// method and consumed by the 
    /// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> 
    /// method.
    /// </returns>
    public XmlSchema GetSchema()
    {
      return null;
    }

    /// <summary>
    /// Generates an object from its XML representation.
    /// </summary>
    /// <param name="p_xmlReader">The <see cref="T:System.Xml.XmlReader"/> 
    /// stream from which the object is deserialized.</param>
    public void ReadXml(XmlReader p_xmlReader)
    {
      string strFolderNameTypePrefix = "SyNet.Events";
      // Event lives in this folder
      string strClassName = GetType().ToString();
      string strShortName = strClassName.Substring(strFolderNameTypePrefix.Length + 1);
      if (p_xmlReader.MoveToContent() == XmlNodeType.Element &&
          p_xmlReader.LocalName == strShortName)
      {
        //  (a) simple properties
        //
        if (p_xmlReader.HasAttributes)
        {
          while (p_xmlReader.MoveToNextAttribute())
          {
            switch (p_xmlReader.Name)
            {
              case "Name":
                //   - event name
                Name = p_xmlReader.Value;
                break;
              case "TriggerMode":
                //   - trigger mode
                TriggerMode = (ETriggerMode)Enum.Parse(typeof(ETriggerMode), p_xmlReader.Value);
                break;
              case "ID":
                //   - event ID
                EventID = UInt64.Parse(p_xmlReader.Value);
                break;
              case "Enabled":
                //   - enable
                switch (p_xmlReader.Value)
                {
                  case "True":
                    IsEnabled = true;
                    break;
                  case "False":
                    IsEnabled = false;
                    break;
                  default:
                    throw new ArgumentOutOfRangeException();
                }
                break;
              default:
                Debug.WriteLine(
                  string.Format("(DBG) ERROR- unknown Attribute {0}={1}", p_xmlReader.Name, p_xmlReader.Value));
                break;
            }
            Debug.WriteLine(string.Format(" * Found Event Attr: {0}={1}", p_xmlReader.Name, p_xmlReader.Value));
          }
          // Move the reader back to the element node.
          p_xmlReader.MoveToElement();
        }

        //  (b) actions
        //m_dctActionsByActionID.Clear();
        // *Actions live in this folder
        p_xmlReader.Read(); // Skip ahead to next node (Our list of actions)
        if (p_xmlReader.MoveToContent() == XmlNodeType.Element &&
            p_xmlReader.LocalName == "Actions")
        {
          p_xmlReader.Read();
          // Skip ahead to next node (One of our ActionReference objects)
          // do we have at least one in our set of actions?
          if (p_xmlReader.LocalName.Equals("ActionReference"))
          {
            do
            {
              bool bHaveID = false;
              if (p_xmlReader.HasAttributes)
              {
                while (p_xmlReader.MoveToNextAttribute())
                {
                  switch (p_xmlReader.Name)
                  {
                    case "ID":
                      //   - ActionReference ID
                      Debug.WriteLine(
                        string.Format(
                          " * Found ActionReference Attribute: {0}={1}", p_xmlReader.Name, p_xmlReader.Value));
                      bHaveID = true;
                      break;
                    default:
                      Debug.WriteLine(
                        string.Format(
                          "(DBG) ERROR- unknown ActionReference Attribute {0}={1}", p_xmlReader.Name, p_xmlReader.Value));
                      break;
                  }
                }
                // Move the reader back to the element node.
                p_xmlReader.MoveToElement();
              }
              if (bHaveID)
              {
                //m_dctActionsByActionID[nActionID] = null;
              }
              else
              {
                throw new ArgumentNullException(); // Missing ActionReference ID attribute...
              }
              p_xmlReader.Read(); // Skip to next ActionReference (if there is one)
            } while (p_xmlReader.LocalName.Equals("ActionReference"));
          }
          p_xmlReader.ReadEndElement(); // Skip to next Element (leave Actions)
        }

        //  (c) eventParameters
        //m_dctEventParametersByParamID.Clear();
        strFolderNameTypePrefix = "SyNet.Events";
        // *Event objects live in this folder
        if (p_xmlReader.LocalName == "EventParameters")
        {
          p_xmlReader.Read();
          // Skip ahead to next node (One of our *Action objects)
          // do we have at least one in our set of EventParameters?
          if (p_xmlReader.LocalName.EndsWith("EventParameter"))
          {
            // have one, let's load it or them...
            do
            {
              string strCurrName = p_xmlReader.LocalName;
              Type tCurrType = Type.GetType(string.Format("{0}.{1}", strFolderNameTypePrefix, strCurrName));
              Debug.Assert(
                tCurrType != null,
                "XML Deserialize: wrong location in XML file or EventParameter class moved in folder hierarchy");
              bool bIsEventParameter = tCurrType.IsSubclassOf(typeof(EventParameter));
              if (bIsEventParameter)
              {
                //XmlSerializer xsEventParameterSerializer = new XmlSerializer(tCurrType);
                //EventParameter epNewParameter = (EventParameter) xsEventParameterSerializer.Deserialize(p_xmlReader);
                //m_dctEventParametersByParamID[(UInt64)epNewParameter.ID] = epNewParameter;
                p_xmlReader.ReadEndElement(); // Skip to next EventParameter (if there is one)
              }
            } while (p_xmlReader.LocalName.EndsWith("EventParameter"));
          }
        }
        //  (d) triggers
        Triggers.Clear();
        // *Trigger objects live in this folder
        if (p_xmlReader.LocalName == "Triggers")
        {
          p_xmlReader.Read();
          // Skip ahead to next node (One of our ActionReference objects)
          // do we have at least one in our set of actions?
          if (p_xmlReader.LocalName.Equals("TriggerReference"))
          {
            do
            {
              bool bHaveID = false;
              UInt64 nTriggerID = UInt64.MaxValue; // value NOT YET set
              if (p_xmlReader.HasAttributes)
              {
                while (p_xmlReader.MoveToNextAttribute())
                {
                  switch (p_xmlReader.Name)
                  {
                    case "ID":
                      //   - TriggerReference ID
                      nTriggerID = UInt64.Parse(p_xmlReader.Value);
                      Debug.WriteLine(
                        string.Format(
                          " * Found TriggerReference Attribute: {0}={1}", p_xmlReader.Name, p_xmlReader.Value));
                      bHaveID = true;
                      break;
                    default:
                      Debug.WriteLine(
                        string.Format(
                          "(DBG) ERROR- unknown TriggerReference Attribute {0}={1}", p_xmlReader.Name, p_xmlReader.Value));
                      break;
                  }
                }
                // Move the reader back to the element node.
                p_xmlReader.MoveToElement();
              }
              if (bHaveID)
              {
                //m_dctTriggersByTriggerID[nTriggerID] = null;
              }
              else
              {
                throw new ArgumentNullException(); // Missing TriggerReference ID attribute...
              }
              p_xmlReader.Read(); // Skip to next TriggerReference (if there is one)
            } while (p_xmlReader.LocalName.Equals("TriggerReference"));
          }
          p_xmlReader.ReadEndElement(); // Skip to next Element (leave Triggers)
        }
        // (e) close out this object
        p_xmlReader.ReadEndElement(); // and leave "Event"
      }
    }

    /// <summary>
    /// Converts an object into its XML representation.
    /// </summary>
    /// <param name="p_xmlWriter">The <see cref="T:System.Xml.XmlWriter"/> 
    /// stream to which the object is serialized.</param>
    public void WriteXml(XmlWriter p_xmlWriter)
    {
      try
      {
        // serialize our properties
        //p_xmlWriter.WriteStartElement(GetType().ToString());  // Event

        //  (a) simple properties
        //
        //   - event name
        p_xmlWriter.WriteAttributeString("Name", Name);
        //   - event ID
        p_xmlWriter.WriteAttributeString("ID", EventID.ToString());

        p_xmlWriter.WriteAttributeString("TriggerMode", TriggerMode.ToString());
        //   - enable
        p_xmlWriter.WriteAttributeString(
          "Enabled", (IsEnabled) ? "True" : "False");

        //  (b) actions
        p_xmlWriter.WriteStartElement("Actions");
        //foreach (UInt64 nActionID in m_dctActionsByActionID.Keys)
        //{
        //  p_xmlWriter.WriteStartElement("ActionReference");
        //  p_xmlWriter.WriteAttributeString("ID", nActionID.ToString());
        //  p_xmlWriter.WriteEndElement(); // ActionReference
        //}
        p_xmlWriter.WriteEndElement(); // Actions

        // Event actions
        foreach (EventAction action in m_lstActions)
        {
          XmlSerializer eventActionSerializer = new XmlSerializer(typeof(EventAction));
          eventActionSerializer.Serialize(p_xmlWriter, action);
        }

        //  (c) eventParameters
        p_xmlWriter.WriteStartElement("EventParameters");
        //XmlSerializer xsEventParameterSerializer =
        //  new XmlSerializer(typeof (EventParameter));
        //foreach (UInt64 nParamID in m_dctEventParametersByParamID.Keys)
        //{
        //  EventParameter epParameter = m_dctEventParametersByParamID[nParamID];
        //  xsEventParameterSerializer.Serialize(p_xmlWriter, epParameter);
        //}
        p_xmlWriter.WriteEndElement(); // EventParameters

        //  (d) triggers
        p_xmlWriter.WriteStartElement("Triggers");
        //foreach (UInt64 nTriggerID in m_dctTriggersByTriggerID.Keys)
        //{
        //  p_xmlWriter.WriteStartElement("TriggerReference");
        //  p_xmlWriter.WriteAttributeString("ID", nTriggerID.ToString());
        //  p_xmlWriter.WriteEndElement(); // TriggerReference
        //}
        p_xmlWriter.WriteEndElement(); // Triggers

        // (e) close out this object
        //p_xmlWriter.WriteEndElement(); // Event
      }
      catch (Exception ex)
      {
        MessageBox.Show(
          ex.InnerException.Message + ex.InnerException.StackTrace);
      }
    }

    #endregion
  }
}