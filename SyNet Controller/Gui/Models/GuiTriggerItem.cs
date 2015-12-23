
using System;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;
using SyNet.Events;
using SyNet.Events.Triggers;
using System.ComponentModel;
using Trigger = SyNet.Events.Triggers.Trigger;
using System.Diagnostics;

namespace SyNet.Gui.Models
{
  /// <summary>
  ///   Class representing the GUI version of a trigger
  /// </summary>
  public class GuiTriggerItem : INotifyPropertyChanged
  {

    public const string STR_XML_ROOT = "GuiTriggerItem";
    public const string STR_XML_TRIGGERID = "TriggerID";

    /// <summary>
    ///   The ID of the trigger represented by this item
    /// </summary>
    [XmlAttribute]
    public UInt64 TriggerID { get; set; }

    private string m_latestValue;

    /// <summary>
    ///   Holds the last possible value from the trigger
    /// </summary>
    [XmlIgnore]
    public String LatestValue
    {
      get { return m_latestValue; }
      set
      {
        m_latestValue = value;
        OnPropertyChanged("LatestValue");
      }
    }

    /// <summary>
    ///   Gets a reference to the Trigger object
    /// </summary>
    [XmlIgnore]
    public Trigger Trigger
    {
      get
      {
        Trigger trigger = EventScheduler.Instance.GetTrigger(TriggerID);
        return trigger;
      }
    }

    public String Name
    {
      get
      {
        if (this.Trigger != null)
        {
          return this.Trigger.Name;
        }
        return "Unknown Trigger";
      }
    }

    /// <summary>
    ///   Initialization construtor with a trigger ID
    /// </summary>
    /// <param name="p_triggerID"></param>
    public GuiTriggerItem( UInt64 p_triggerID )
    {
      TriggerID = p_triggerID;
      RegisterWithTrigger();
    }

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public GuiTriggerItem()
    {
    }

    /// <summary>
    ///   Registers this item with the actual trigger
    /// </summary>
    public void RegisterWithTrigger()
    {
      Trigger trigger = Trigger;
      if (trigger != null)
      {
        trigger.PropertyChanged += Trigger_PropertyChanged;

        //
        // If this is a message trigger then we need to make sure its subscribed
        //
        if (trigger is MessageTrigger)
        {
          try
          {
            trigger.Subscribe(TriggerID);
          }
          catch (Exception exArgs)
          {
            MessageBox.Show(
              string.Format("GuiTriggerItem.RegisterWithTrigger - Could not subscribe: {0}", exArgs.Message));
            return;

          }
        }
      }
    }

    private void Trigger_PropertyChanged( object p_objSender,
      PropertyChangedEventArgs p_pcEvArgs )
    {
      if (p_pcEvArgs.PropertyName == "Value")
      {
        Trigger sendTrigger = p_objSender as Trigger;
        if (sendTrigger != null)
        {
          TriggerFired();
        }
      }
    }

    private void TriggerFired()
    {
        LatestValue = Trigger.Value;
    }

    #region Implementation of INotifyPropertyChanged

    /// <summary>
    ///   Fires events for property changed
    /// </summary>
    /// <param name="p_strPropertyName"></param>
    protected void OnPropertyChanged( string p_strPropertyName )
    {
      if (PropertyChanged != null)
        PropertyChanged(
          this,
          new PropertyChangedEventArgs(p_strPropertyName));
    }

    /// <summary>
    /// 
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    /// <summary>
    ///   Load method
    /// </summary>
    /// <param name="p_element"></param>
    public void Load(XElement p_element)
    {
      XAttribute xTriggerID = p_element.Attribute(STR_XML_TRIGGERID);
      if (xTriggerID != null)
      {
        this.TriggerID = UInt64.Parse(xTriggerID.Value);
      }
    }

    /// <summary>
    ///   Save the item
    /// </summary>
    /// <returns></returns>
    public XElement Save()
    {
      XElement xElement = new XElement(STR_XML_ROOT);

      xElement.Add(new XAttribute(STR_XML_TRIGGERID, this.TriggerID.ToString()));

      return xElement;
    }
  }
}
