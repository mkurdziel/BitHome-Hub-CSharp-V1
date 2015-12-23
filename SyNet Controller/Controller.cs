using System.Xml.Serialization;
using SyNet.Actions;
using SyNet.Events;
using SyNet.Gui.Models;

namespace SyNet
{
  /// <summary>
  ///  This class is an encapsulator for objects in the system that should
  ///   be persisted.
  /// </summary>
  public class Controller
  {
    /// <summary>
    /// The Version of the persisted XML
    /// </summary>
    [XmlAttribute]
    public double XMLVersion
    {
      get
      {
        return 1.0;
      }
      set
      {
        SyNetSettings.Instance.XMLVersion = value;
      }
    }
    private NodeManager m_nodeManager;

    /// <summary>
    ///   Node Manager for the system
    /// </summary>
    [XmlElement(ElementName = "NodeManager")]
    public NodeManager NodeManager
    {
      get { return m_nodeManager; }
      set { m_nodeManager = value; }
    }

    private ActionManager m_actionManager;

    /// <summary>
    ///   Action manager for the system
    /// </summary>
    [XmlElement(ElementName = "ActionManager")]
    public ActionManager ActionManager
    {
      get { return m_actionManager; }
      set { m_actionManager = value; }
    }

    private EventScheduler m_esEventScheduler;

    /// <summary>
    ///   Event Scheduler for the system
    /// </summary>
    [XmlElement(ElementName = "EventScheduler")]
    public EventScheduler EventScheduler
    {
      get { return m_esEventScheduler; }
      set { m_esEventScheduler = value; }
    }

    private GuiManager m_guiManager;

    /// <summary>
    ///   Gui manager for the system
    /// </summary>
    [XmlElement(ElementName = "Gui")]
    public GuiManager GuiManager
    {
      get { return m_guiManager; }
      set { m_guiManager = value; }
    }


    /// <summary>
    ///   Default constructor
    /// </summary>
    public Controller()
    {
      SyNetSettings.Instance.DeserializingFinished += DeserializingFinished;
    }

    private void DeserializingFinished()
    {
      m_actionManager = ActionManager.Instance;
      m_guiManager = GuiManager.Instance;
      m_nodeManager = NodeManager.Instance;
      m_esEventScheduler = EventScheduler.Instance;
    }
  }
}