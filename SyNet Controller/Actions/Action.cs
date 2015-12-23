using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Xml.Serialization;
using SyNet.EventArguments;

namespace SyNet.Actions
{
  /// <summary>
  ///   Base class for all actions in the system
  /// </summary>
  public abstract class Action : INotifyPropertyChanged, IDisposable
  {
    ////////////////////////////////////////////////////////////////////////////
    #region Member Variables

    private ulong m_nActionID;
    private readonly Mutex m_bMutex = new Mutex();
    private bool m_isRunning;
    private string m_name;

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Constructors

    /// <summary>
    ///   Default Constructor
    /// </summary>
    protected Action()
      : this(0)
    {
      Name = "New Action";
    }

    /// <summary>
    ///   Initialization constructor with an ActionID
    /// </summary>
    /// <param name="p_actionID">ActionID to instantiate action with</param>
    protected Action(UInt64 p_actionID)
    {
      ActionID = p_actionID;

      //
      // Generate a new unique Action ID for this action
      //
      if (!SyNetSettings.Instance.IsDeserializing)
      {
        if (ActionID == 0)
        {
          ActionID = ActionManager.Instance.NewActionID();
        }

        RegisterDefaultParameters();
        Register();
      }
      else
      {
        SyNetSettings.Instance.DeserializingFinished += DeserializingFinished_Handler;
      }
    }

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_action">Action to copy</param>
    public Action(Action p_action)
      : this()
    {
      Name = p_action.Name;
      ParentActionID = p_action.ActionID;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Public Properties

    /// <summary>
    ///   Unique Id of this action
    /// </summary>
    [XmlAttribute]
    public UInt64 ActionID
    {
      get { return m_nActionID; }
      set
      {
        if (m_nActionID != 0)
        {
          throw new ApplicationException("[DBG] Action.ActionID - Cannot set non-zero ID");
        }

        m_nActionID = value;
      }
    }

    /// <summary>
    ///   Parent ID of this action, set by copy constructor
    /// </summary>
    [XmlAttribute]
    public UInt64 ParentActionID { get; set; }

    /// <summary>
    ///   String indentifying the type of action. Overriden by derived types.
    /// </summary>
    [XmlIgnore]
    public abstract string TypeString { get; }

    /// <summary>
    ///   Flag to delineate that the action is currently running
    /// </summary>
    [XmlIgnore]
    public bool IsRunning
    {
      get { return m_isRunning; }
      set
      {
        m_isRunning = value;
        OnPropertyChanged("IsRunning");
      }
    }

    /// <summary>
    ///   Name of action
    /// </summary>
    [XmlAttribute]
    public String Name
    {
      get { return m_name; }
      set
      {
        m_name = value;
        OnPropertyChanged("Name");
      }
    }

    /// <summary>
    ///   Gets the formal name of the action based on its type and lineage
    /// </summary>
    [XmlIgnore]
    public abstract String FormalName
    { get; }

    /// <summary>
    ///   List of full parameters present in the action
    /// </summary>
    [XmlIgnore]
    public abstract List<ActionParameter> Parameters { get; }

    /// <summary>
    ///   List of all input parameters in the action
    /// </summary>
    [XmlIgnore]
    public List<ActionParameter> InputParameters
    {
      get
      {
        List<ActionParameter> pList = Parameters;
        List<ActionParameter> ipList = new List<ActionParameter>();
        foreach (ActionParameter aParam in pList)
        {
          if (aParam.ParameterType == ActionParameter.EsnActionParameterType.INPUT)
          {
            ipList.Add(aParam);
          }
        }
        return ipList;
      }
    }

    /// <summary>
    ///   List of all internal parameters in the action
    /// </summary>
    [XmlIgnore]
    public List<ActionParameter> InternalParameters
    {
      get
      {
        List<ActionParameter> pList = Parameters;
        List<ActionParameter> ipList = new List<ActionParameter>();
        foreach (ActionParameter aParam in pList)
        {
          if (aParam.ParameterType == ActionParameter.EsnActionParameterType.INTERNAL)
          {
            ipList.Add(aParam);
          }
        }
        return ipList;
      }
    }

    /// <summary>
    ///   Gets a reference to the parent action
    /// </summary>
    [XmlIgnore]
    protected Action ParentAction
    {
      get
      {
        Action parentAction = null;
        if (ParentActionID != 0)
        {
          parentAction = ActionManager.Instance.GetAction(this.ParentActionID);
        }
        return parentAction;
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
      if (m_bMutex.WaitOne(TimeSpan.FromSeconds(1)))
      {
        return true;
      }
      Debug.WriteLine("[ERR] Action.Lock: Cannot get action lock");
      return false;
    }

    /// <summary>
    ///   Unlocks the device
    /// </summary>
    public void Unlock()
    {
      m_bMutex.ReleaseMutex();
    }

    /// <summary>
    ///   ToString method
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Name;
    }

    /// <summary>
    ///   Dispose method. Unregisters action.
    /// </summary>
    public void Dispose()
    {
      Debug.WriteLine("[DBG] Action - Dispose");
      ActionManager.Instance.UnregisterAction(this.ActionID);
    }

    /// <summary>
    ///   Execute the action. Overloaded by all derived actions.
    /// </summary>
    /// <returns></returns>
    public abstract bool Execute();

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Private and Protected Methods

    /// <summary>
    ///   Overridable method to register any necessary default parameters.
    /// </summary>
    protected virtual void RegisterDefaultParameters()
    {
    }

    /// <summary>
    ///   Registers this action with the ActionManager
    /// </summary>
    private void Register()
    {
      ActionManager.Instance.RegisterAction(this);
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Events and Event Handlers

    /// <summary>
    ///   Deserialization finished handler
    /// </summary>
    protected void DeserializingFinished_Handler()
    {
      Register();
    }

    /// <summary>
    ///   Fires events for a parameter change
    /// </summary>
    protected void OnActionModifiedParamChanged()
    {
      OnPropertyChanged("Parameters");
      OnPropertyChanged("InputParameters");
      OnActionModified(ActionModifiedEventArgs.EsnActionModifyType.PARAM_CHANGE);
    }

    /// <summary>
    ///   Fires events when the action has been modified
    /// </summary>
    /// <param name="p_type"></param>
    private void OnActionModified(ActionModifiedEventArgs.EsnActionModifyType p_type)
    {
      if (ActionModified != null)
      {
        ActionModifiedEventArgs args = new ActionModifiedEventArgs(this.ActionID,
                                                                   p_type);
        //Debug.WriteLine("[DBG] Action.OnActionModified - sending");
        ActionModified(this, args);
      }
    }

    internal delegate void ActionModifiedEventHandler(object p_sender,
                                                      ActionModifiedEventArgs
                                                        p_args);

    internal event ActionModifiedEventHandler ActionModified;

    /// <summary>
    ///   Property changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;


    /// <summary>
    ///   Fires events for property changed
    /// </summary>
    /// <param name="p_strPropertyName"></param>
    protected void OnPropertyChanged(string p_strPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(
          this,
          new PropertyChangedEventArgs(p_strPropertyName));
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////
  }
}