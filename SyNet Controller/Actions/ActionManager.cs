using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;

namespace SyNet.Actions
{
  /// <summary>
  ///   Singleton ActionManager
  /// </summary>
  [XmlInclude(typeof(DeviceAction))]
  [XmlInclude(typeof(SequenceAction))]
  [XmlInclude(typeof(DelayAction))]
  public class ActionManager : INotifyPropertyChanged
  {
    ////////////////////////////////////////////////////////////////////////////
    #region Constants

    private static int MAX_THREADS = 5;

    /// <summary>
    ///   ID for Anonymous delay actions
    /// </summary>
    public static UInt64 BASE_ACTION_ID_DELAY = 1; // ActionID for anon delay action

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Member Variables

    private List<BackgroundWorker> m_workerThreads;

    private readonly Random m_rand = new Random();
    private static ActionManager s_amInstance;

    private List<Action> m_anonActions = new List<Action>();
    private List<Action> m_userActions = new List<Action>();
    private Dictionary<UInt64, List<DeviceAction>> m_dictDeviceActionByDeviceID =
      new Dictionary<ulong, List<DeviceAction>>();


    private static readonly object s_objInstanceLock = new object();
    private static readonly object s_objParamLock = new object();
    private static readonly object s_objActionLock = new object();

    private readonly Dictionary<UInt64, Parameter> m_dctParameterByID =
      new Dictionary<ulong, Parameter>();

    private readonly Dictionary<UInt64, Action> m_dctActionByID =
      new Dictionary<ulong, Action>();

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    

    ////////////////////////////////////////////////////////////////////////////
    #region Public Properties

    /// <summary>
    ///   Singleton instance of the ActionManager
    /// </summary>
    [XmlIgnore]
    public static ActionManager Instance
    {
      get
      {
        lock (s_objInstanceLock)
        {
          if (s_amInstance == null)
          {
            s_amInstance = new ActionManager();
          }
          return s_amInstance;
        }
      }
      set
      {
        lock (s_objInstanceLock)
        {
          s_amInstance = value;
        }
      }
    }

    /// <summary>
    /// Collection of all action objects
    /// </summary>
    [XmlIgnore]
    public List<Action> Actions
    {
      get
      {
        List<Action> actions = new List<Action>();
        actions.AddRange(UserActions);
        actions.AddRange(AnonActions);
        foreach (List<DeviceAction> list in DeviceActions.Values)
        {
          foreach (DeviceAction action in list)
          {
            actions.Add(action);
          }
        }

        return actions;
      }
    }

    /// <summary>
    ///   List of anonymous actions
    /// </summary>
    [XmlArray(ElementName = "AnonActions")]
    public List<Action> AnonActions
    {
      get { return m_anonActions; }
      set { m_anonActions = value; }
    }

    /// <summary>
    ///   Returns a list of user actions
    /// </summary>
    [XmlArray(ElementName = "UserActions")]
    public List<Action> UserActions
    {
      get { return m_userActions; }
      set { m_userActions = value; }
    }

    /// <summary>
    ///   Returns a dictionary of device actions
    /// </summary>
    [XmlIgnore]
    public Dictionary<UInt64, List<DeviceAction>> DeviceActions
    {
      get { return m_dictDeviceActionByDeviceID; }
      set { m_dictDeviceActionByDeviceID = value; }
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////
    


    ////////////////////////////////////////////////////////////////////////////
    #region Constructors

    /// <summary>
    ///   Default constructor
    /// </summary>
    public ActionManager()
    {
      lock (s_objInstanceLock)
      {
        if (s_amInstance == null)
        {
          s_amInstance = this;
        }
      }

      SyNetSettings.Instance.DeserializingFinished += Initialize;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Public Functions

    /// <summary>
    ///   Add an action to the system
    /// </summary>
    /// <param name="p_action"></param>
    /// <returns></returns>
    public bool AddAction(Action p_action)
    {
      bool bRetVal = true;

      // If we are a device action, we're stored seperatly
      if (p_action is DeviceAction)
      {
        Debug.WriteLine(string.Format("[DBG] ActionManager.AddAction - Adding device action {0}", p_action.Name));
        DeviceAction a = p_action as DeviceAction;

        if (!DeviceActions.ContainsKey(a.DeviceID))
        {
          DeviceActions[a.DeviceID] = new List<DeviceAction>();
        }
        DeviceActions[a.DeviceID].Add(a);
      }
      else if (p_action is DelayAction)
      {
        AnonActions.Add(p_action);
      }
      else
      {
        UserActions.Add(p_action);
      }

      OnPropertyChanged("Actions");

      return bRetVal;
    }

    /// <summary>
    ///   Remove an action from the list of actions
    /// </summary>
    /// <param name="p_action">Action to be removed</param>
    /// <returns>True if removal was successful</returns>
    public bool RemoveAction(Action p_action)
    {
      bool retVal = true;
      if (Actions.Contains(p_action))
      {
        // Unregister the action
        UnregisterAction(p_action.ActionID);

        RemoveParameters(p_action.Parameters);

        // If this is an action with sub-actions, check for anon actions to remove
        if (p_action is SequenceAction)
        {
          foreach (ActionItem item in ((SequenceAction)p_action).ActionItems)
          {
            DelayAction da = GetAction(item.ActionID) as DelayAction;
            if (da != null)
            {
              RemoveAction(da);
            }
          }
        }

        // Finally, remove it for good
        if (p_action is DeviceAction)
        {
          DeviceActions[((DeviceAction)p_action).DeviceID].Remove((DeviceAction)p_action);
          OnPropertyChanged("DeviceActions");
        }
        else if (p_action is DelayAction)
        {
          AnonActions.Remove(p_action);
        }
        else
        {
          UserActions.Remove(p_action);
          OnPropertyChanged("UserActions");
        }
      }
      else
      {
        retVal = false;
      }

      OnPropertyChanged("Actions");
      return retVal;
    }

    /// <summary>
    ///   Unregister parameters and remove dependencies
    /// </summary>
    /// <param name="p_paramaterList"></param>
    /// <returns></returns>
    public bool RemoveParameters(List<ActionParameter> p_paramaterList)
    {
      bool bRetVal = true;

      // Unregister its parameters
      foreach (ActionParameter parameter in p_paramaterList)
      {
        UnregisterParameter(parameter.ParamID);
      }
      return bRetVal;
    }


    /// <summary>
    ///   Unregisters a prameter based on its ID
    /// </summary>
    /// <param name="p_nParamID"></param>
    public void UnregisterParameter(UInt64 p_nParamID)
    {
      lock (s_objParamLock)
      {
        if (m_dctParameterByID.ContainsKey(p_nParamID))
        {
          Debug.WriteLine(
            String.Format("[DBG] ActionManager: Unregistering Parameter {0}",
                          p_nParamID));

          m_dctParameterByID.Remove(p_nParamID);
        }
        else
        {
          Debug.WriteLine(
            string.Format("[ERR] ActionManager: Unregistering invalid parameter {0}", p_nParamID));
        }
      }
    }

    /// <summary>
    ///   Registers a parameter based on its ID
    /// </summary>
    /// <param name="p_param"></param>
    public void RegisterParameter(Parameter p_param)
    {
      lock (s_objParamLock)
      {
        if (!m_dctParameterByID.ContainsKey(p_param.ParamID))
        {
          Debug.WriteLine(
            String.Format("[DBG] ActionManager: Registering Parameter {0} {1}",
                          p_param.ParamID, p_param.Name));

          m_dctParameterByID.Add(p_param.ParamID, p_param);
        }
        else
        {
          Debug.WriteLine(
            string.Format("[ERR] ActionManager: Registering duplicate parameter {0}", p_param.ParamID));
        }
      }
    }

    /// <summary>
    ///   Gets a parameter based on its ID
    /// </summary>
    /// <param name="p_nParamID"></param>
    /// <returns></returns>
    public Parameter GetParameter(UInt64 p_nParamID)
    {
      Parameter param = null;
      lock (s_objParamLock)
      {
        if (m_dctParameterByID.ContainsKey(p_nParamID))
        {
          param = m_dctParameterByID[p_nParamID];
        }
        else
        {
          Debug.WriteLine(
            string.Format("[ERR] ActionManager: Accessing invalid parameter {0}", p_nParamID));
        }
      }
      return param;
    }

    /// <summary>
    ///   Unregisters a action based on its ID
    /// </summary>
    /// <param name="p_nActionID"></param>
    public void UnregisterAction(UInt64 p_nActionID)
    {
      lock (s_objActionLock)
      {
        if (m_dctActionByID.ContainsKey(p_nActionID))
        {
          Debug.WriteLine(
            String.Format("[DBG] ActionManager: Unregistering Action {0}",
                          p_nActionID));

          m_dctActionByID.Remove(p_nActionID);
        }
        else
        {
          Debug.WriteLine(
            string.Format("[ERR] ActionManager: Unregistering invalid action {0}", p_nActionID));
        }
      }
    }

    /// <summary>
    ///   Registers a action based on its ID
    /// </summary>
    /// <param name="p_action"></param>
    public void RegisterAction(Action p_action)
    {
      lock (s_objActionLock)
      {
        if (!m_dctActionByID.ContainsKey(p_action.ActionID))
        {
          Debug.WriteLine(
            String.Format("[DBG] ActionManager: Registering Action {0} {1}",
                          p_action.ActionID, p_action.Name));

          m_dctActionByID.Add(p_action.ActionID, p_action);
        }
        else
        {
          Debug.WriteLine(
            string.Format("[ERR] ActionManager: Registering duplicate action {0}", p_action.ActionID));
        }
      }
    }

    /// <summary>
    ///   Gets a action based on its ID
    /// </summary>
    /// <param name="p_nActionID"></param>
    /// <returns></returns>
    public Action GetAction(UInt64 p_nActionID)
    {
      Action action = null;
      lock (s_objActionLock)
      {
        if (m_dctActionByID.ContainsKey(p_nActionID))
        {
          action = m_dctActionByID[p_nActionID];
        }
        else
        {
          Debug.WriteLine(
            string.Format("[ERR] ActionManager: Accessing invalid action {0}", p_nActionID));
        }
      }
      return action;
    }

    /// <summary>
    ///   Generate a new action ID that is guaranteed to be unique
    /// </summary>
    /// <returns></returns>
    public UInt64 NewActionID()
    {
      UInt64 actionID = GenerateRandomID();

      lock (s_objActionLock)
      {
        // TODO: Hack - must be a better way to prevent generation of 
        // predefined base ids
        while (m_dctActionByID.ContainsKey(actionID) &&
               actionID != 0 &&
               actionID != BASE_ACTION_ID_DELAY)
        {
          actionID = GenerateRandomID();
        }
      }
      return actionID;
    }

    /// <summary>
    ///   Generate a new parameter ID that is guaranteed to be unique
    /// </summary>
    /// <returns></returns>
    public UInt64 NewParamID()
    {
      UInt64 paramID = GenerateRandomID();

      lock (s_objParamLock)
      {
        while (m_dctParameterByID.ContainsKey(paramID) && paramID != 0)
        {
          paramID = GenerateRandomID();
        }
      }
      return paramID;
    }

    /// <summary>
    ///   Step backwards twice in parameter lineage to get the name of the
    ///   best descriptor action
    /// </summary>
    /// <param name="p_paramID"></param>
    /// <returns></returns>
    public String GetParameterBaseName(UInt64 p_paramID)
    {
      StringBuilder sb = new StringBuilder();

      ActionParameter param = GetParameter(p_paramID) as ActionParameter;
      if (param != null)
      {
        UInt64 baseParamID = param.ParentParamID;
        ActionParameter parentParam = GetParameter(baseParamID) as ActionParameter;
        if (parentParam != null)
        {
          UInt64 grandparentParamID = parentParam.ParentParamID;
          ActionParameter gpParam =
            GetParameter(grandparentParamID) as ActionParameter;
          if (gpParam != null)
          {
            Action a = GetAction(gpParam.ActionID);
            sb.Append(a.Name + " - ");
          }
        }
        sb.Append(param.Name);
      }

      return sb.ToString();
    }

    /// <summary>
    ///   Execute an action on the background worker thread pool
    /// </summary>
    /// <param name="p_actionID"></param>
    /// <returns></returns>
    public bool ExecuteThreadedAction(UInt64 p_actionID)
    {
      bool retVal = false;
      Action action = GetAction(p_actionID);
      if (action != null)
      {
        if (!action.IsRunning)
        {
          BackgroundWorker openThread = null;

          //
          // Check to see if there is a thread available
          //
          foreach (BackgroundWorker thread in m_workerThreads)
          {
            if (!thread.IsBusy)
            {
              openThread = thread;
              break;
            }
          }

          if (openThread != null)
          {
            openThread.RunWorkerAsync(action);
            retVal = true;
          }
          else
          {
            Debug.WriteLine("[ERR] ActionManager - No threads available");
          }
        }
        else
        {
          Debug.WriteLine(String.Format("[DBG] Action {0} already running", action.Name));
        }
      }
      return retVal;
    }

    /// <summary>
    ///   Generate a unique name from a base name
    /// </summary>
    /// <param name="p_strBaseName"></param>
    /// <returns></returns>
    public string GenerateUniqueName(string p_strBaseName)
    {
      string baseName = p_strBaseName;

      // Check the base for uniqueness
      if (!IsNameUnique(baseName))
      {
        string newName;
        for (int suffix = 1; ; suffix++)
        {
          newName = String.Format("{0} {1}", baseName, suffix);
          if (IsNameUnique(newName))
          {
            break;
          }
        }
        baseName = newName;
      }
      return baseName;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////
    


    ////////////////////////////////////////////////////////////////////////////
    #region Private Functions

    /// <summary>
    ///   Setup the maximum number of background worker threads
    /// </summary>
    private void SetupBackgroundWorkerThreads()
    {
      m_workerThreads = new List<BackgroundWorker>();

      for (int i = 0; i < MAX_THREADS; i++)
      {
        BackgroundWorker thread = new BackgroundWorker();
        thread.DoWork += ActionThread_DoWork;
        thread.RunWorkerCompleted += ActionThread_Completed;
        m_workerThreads.Add(thread);
      }
    }

    /// <summary>
    ///   Worker thread completed function
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="e"></param>
    void ActionThread_Completed(object p_sender, RunWorkerCompletedEventArgs e)
    {
      if ((bool)e.Result)
      {
        //Debug.WriteLine("[DBG] ActionManager - Action thread completed successfully");
      }
      else
      {
        Debug.WriteLine("[DBG] ActionManager - Action thread failed");

      }
    }

    /// <summary>
    ///   Worker function to execute an action
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="e"></param>
    private void ActionThread_DoWork(object p_sender, DoWorkEventArgs e)
    {
      Action action = e.Argument as Action;
      if (action != null)
      {
        action.IsRunning = true;
        e.Result = action.Execute();
        action.IsRunning = false;
      }
    }

    /// <summary>
    ///   Initialize routine to perform after deserialization has finished
    /// </summary>
    private void Initialize()
    {
      NodeManager.Instance.NodeDiscovered += NodeDiscovered_Handler;
      CreateDeviceActions();
      SetupBackgroundWorkerThreads();
    }

    /// <summary>
    ///   Iterate through all devices in the system and generate 
    ///   the run-time DeviceActions for each DeviceFunction
    /// </summary>
    private void CreateDeviceActions()
    {
      List<Device> nodes = NodeManager.Instance.NodeList;
      foreach (Device node in nodes)
      {
        foreach (DeviceFunction function in node.Catalog.Values)
        {
          DeviceAction action = new DeviceAction(function);
          AddAction(action);
        }
      }
    }
    /// <summary>
    ///   NodeManager NodeDiscovered handler.
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_args"></param>
    /// <remarks>
    ///   Necessary to handle node discovered events from the node manager.
    ///   This way, the ActionManager can create device actions for each
    ///   function in a new device.
    /// </remarks>
    private void NodeDiscovered_Handler(object p_sender, EventArguments.NodeDiscoveredEventArgs p_args)
    {
      Debug.WriteLine("[DBG] ActionManager.NodeDiscovered - Creating DeviceActions");

      if (p_args.FullParameters)
      {
        Device device = NodeManager.Instance.GetNode(p_args.DeviceID);
        if (device != null)
        {
          foreach (DeviceFunction function in device.FunctionList)
          {
            DeviceAction action = new DeviceAction(function);

            this.AddAction(action);
          }
        }
      }
    }

    /// <summary>
    ///   Checks to see if a particular name is already in use
    /// </summary>
    /// <param name="p_strName"></param>
    /// <returns></returns>
    private bool IsNameUnique(string p_strName)
    {
      bool retVal = true;
      foreach (Action action in Actions)
      {
        if (p_strName.Equals(action.Name))
        {
          retVal = false;
          break;
        }
      }
      return retVal;
    }

    /// <summary>
    ///   Generate a random Uint64 to be used as an ID
    /// </summary>
    /// <returns></returns>
    private UInt64 GenerateRandomID()
    {
      byte[] bytes = new byte[8];
      m_rand.NextBytes(bytes);
      return BitConverter.ToUInt64(bytes, 0);
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    

    ////////////////////////////////////////////////////////////////////////////
    #region Events

    /// <summary>
    ///   Property changed event
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
    ////////////////////////////////////////////////////////////////////////////
  }
}