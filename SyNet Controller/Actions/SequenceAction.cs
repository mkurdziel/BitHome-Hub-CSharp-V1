using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Serialization;
using SyNet.EventArguments;
using SyNet.Protocol;

namespace SyNet.Actions
{
  /// <summary>
  ///   Represents an action that contains a collection of sub-actions in sequence
  /// </summary>
  public class SequenceAction : Action
  {
    private ObservableCollection<ActionItem> m_actionItems;

    /// <summary>
    ///   Internal action holding the loop count
    /// </summary>
    public ActionParameter InternalLoopCount { get; set; }

    /// <summary>
    ///   ActionParameter to set the number of sequence loops
    /// </summary>
    public ActionParameter InputNumLoops { get; set; }

    /// <summary>
    ///   Collection of ActionItems
    /// </summary>
    public ObservableCollection<ActionItem> ActionItems
    {
      get { return m_actionItems; }
      set { m_actionItems = value; }
    }

    /// <summary>
    ///   Gets the formal name of the action based on its type and lineage
    /// </summary>
    public override string FormalName
    {
      get
      {
        return this.Name;
      }
    }

    /// <summary>
    ///   Return a list of all parameters in this action
    /// </summary>
    [XmlIgnore]
    public override List<ActionParameter> Parameters
    {
      get
      {
        List<ActionParameter> paramList = new List<ActionParameter>();

        //
        // Add input and internal parameters
        //
        paramList.Add(InternalLoopCount);
        paramList.Add(InputNumLoops);

        //
        // Loop through each actionitem and add its list of actionparameters
        // to the list of all parameters
        //
        foreach (ActionItem item in ActionItems)
        {
          paramList.AddRange(item.ActionParameters);
        }

        return paramList;
      }
    }

    /// <summary>
    ///   Return a list of sub actions that are contained in this loop action
    /// </summary>
    [XmlIgnore]
    public List<Action> Actions
    {
      get
      {
        ActionManager mgr = ActionManager.Instance;
        List<Action> actionList = new List<Action>();
        foreach (ActionItem item in ActionItems)
        {
          Action a = mgr.GetAction(item.ActionID);
          if (a != null)
          {
            actionList.Add(a);
          }
          else
          {
            throw new Exception("SequenceAction.Actions: Null action in the list");
          }
        }
        return actionList;
      }
    }

    /// <summary>
    ///   Duplication constructor
    /// </summary>
    /// <param name="p_action"></param>
    public SequenceAction( SequenceAction p_action )
      : base(p_action)
    {
      ActionItems = new ObservableCollection<ActionItem>();
      //
      // Copy the internal loop count
      //
      this.InputNumLoops.StringValue = p_action.InputNumLoops.StringValue;

      //
      // Duplicate all the Action Items
      //
      foreach (ActionItem item in p_action.ActionItems)
      {
        ActionItem itemCopy = new ActionItem(item);

        this.ActionItems.Add(itemCopy);
      }

      //
      // Loop through each action and relink any dependencies
      //
      for (int i = 0; i < Parameters.Count; i++)
      {
        ActionParameter param = Parameters[i];
        if (param.ParameterType == ActionParameter.EsnActionParameterType.DEPENDENT)
        {
          //
          // Get the equvalent parameter from the source action based
          // on its index in the array
          //
          ActionParameter sourceParam = p_action.Parameters[i];

          //
          // Get the source parameters dependent param ID and find
          // its index in the source action's parameter list
          //
          UInt64 sourceDependentParam = sourceParam.DependentParamID;
          for (int j = 0; j < p_action.Parameters.Count; j++)
          {
            if (p_action.Parameters[j].ParamID == sourceDependentParam)
            {
              param.DependentParamID = Parameters[j].ParamID;
              break;
            }
          }
        }
      }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public SequenceAction()
    {
      ActionItems = new ObservableCollection<ActionItem>();
      SyNetSettings.Instance.DeserializingFinished += Initialize;
    }

    /// <summary>
    ///   Initialize after deserialization
    /// </summary>
    protected void Initialize()
    {
      RegisterActionItemCallbacks();
    }

    /// <summary>
    ///   Regsiter the necessary default parameters for this action
    /// </summary>
    protected override void RegisterDefaultParameters()
    {
      base.RegisterDefaultParameters();

      // Internal parameters
      InternalLoopCount = new ActionParameter(ActionParameter.EsnActionParameterType.INTERNAL)
                            {
                              Name = "Internal Loop Count",
                              DataType = EsnDataTypes.WORD,
                              ValidationType =
                                EsnParamValidationType.UNSIGNED_FULL
                            };

      // Input parameters
      InputNumLoops = new ActionParameter(ActionParameter.EsnActionParameterType.CONSTANT)
                        {
                          Name = "Number of Loops",
                          DataType = EsnDataTypes.WORD,
                          ValidationType = EsnParamValidationType.UNSIGNED_FULL,
                          IntValue = 1
                        };
    }

    /// <summary>
    ///   String type identifier
    /// </summary>
    public override string TypeString
    {
      get { return ""; }
    }

    /// <summary>
    ///   Add a new sub-action to this action
    /// </summary>
    /// <param name="p_action"></param>
    public void AddAction( Action p_action )
    {
      ActionItems.Add(new ActionItem(p_action, ActionID));

      //
      // Register for change notifications on this action
      //
      p_action.ActionModified += new ActionModifiedEventHandler(ActionModified_Handler);
      OnPropertyChanged("Parameters");
    }

    /// <summary>
    ///   Removes a specific occurance of an action from the list
    /// </summary>
    /// <param name="p_aItem"></param>
    /// <returns></returns>
    public bool RemoveAction( ActionItem p_aItem )
    {
      bool retVal = false;

      if (ActionItems.Contains(p_aItem))
      {
        ActionItems.Remove(p_aItem);
        retVal = true;
      }

      //
      // Remove notifications of change on this action
      //
      Action action = ActionManager.Instance.GetAction(p_aItem.ActionID);
      if (action != null)
      {
        action.ActionModified -= ActionModified_Handler;
      }

      //
      // If this is a delay action then remove the instance
      //
      if (action is DelayAction)
      {
        ActionManager.Instance.RemoveAction(action);
      }

      return retVal;
    }

    /// <summary>
    ///   Executes the action
    /// </summary>
    /// <returns></returns>
    public override bool Execute()
    {
      bool retVal = true;
      int numLoops = (int)InputNumLoops.IntValue;

      for (int i = 0; i < numLoops; i++)
      {
        InternalLoopCount.IntValue = i;
        foreach (ActionItem item in ActionItems)
        {
          retVal &= ExecuteActionItem(item);
        }
      }
      return retVal;
    }

    /// <summary>
    ///   Execute a single ActionItem
    /// </summary>
    /// <param name="p_actionItem"></param>
    /// <returns></returns>
    private bool ExecuteActionItem( ActionItem p_actionItem )
    {
      bool retVal = true;

      ActionManager aMgr = ActionManager.Instance;
      Action action = aMgr.GetAction(p_actionItem.ActionID);

      if (action != null)
      {
        if (action.Lock())
        {
          //
          // Copy over all the parameter values
          //
          foreach (ActionParameter param in p_actionItem.ActionParameters)
          {
            if (param.IsValid)
            {
              //
              // Get the parent parameter from the mgr
              //
              Parameter parent = aMgr.GetParameter(param.ParentParamID);

              //
              // Copy the value over
              //
              if (parent != null)
              {
                parent.StringValue = param.StringValue;
              }
              else
              {
                Debug.WriteLine("[ERR] ActionItem.Send: Parent param null");
              }
            }
            else
            {
              retVal = false;
              break;
            }
          }

          //
          // If all parameters were valid, execute the action
          //
          if (retVal)
          {
            action.Execute();
          }

          // release the lock
          action.Unlock();
        }
        else
        {
          Debug.WriteLine("[ERR]ActionItem.Send: Could not get action lock");
        }
      }
      else
      {
        retVal = false;
        Debug.WriteLine(
          String.Format(@"[ERR] SequenceAction.Send: Executing a null action {0}", ActionID));
      }
      return retVal;
    }

    /// <summary>
    ///   Register through all the actions and register the callbacks
    /// </summary>
    private void RegisterActionItemCallbacks()
    {
      foreach (Action action in Actions)
      {
        action.ActionModified += ActionModified_Handler;
      }
    }

    /// <summary>
    ///   Handles action modification notifications on sub actions
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_args"></param>
    private void ActionModified_Handler( object p_sender, ActionModifiedEventArgs p_args )
    {
      Debug.WriteLine("SequenceAction.ActionModified_Handler");
    }
  }
}