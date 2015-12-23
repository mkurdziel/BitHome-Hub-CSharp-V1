using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using SyNet.Actions;
using Action=SyNet.Actions.Action;

namespace SyNet.Events
{
  /// <summary>
  ///   Class representing an action visible on the User-created Gui
  /// </summary>
  public class EventAction : Action
  {
    #region Member Variables

    private string m_executeText;
    private List<EventActionConditional> m_conditionals = new List<EventActionConditional>();

    #endregion

    #region Public Properties

    /// <summary>
    ///   Text on the execute button
    /// </summary>
    [XmlAttribute]
    public String ExecuteText
    {
      get { return m_executeText; }
      set
      {
        m_executeText = value;
        OnPropertyChanged("ExecuteText");
      }
    }

    #endregion

    /// <summary>
    /// R/O PROPERTY: calculated name of this Action
    /// </summary>
    public override string FormalName
    {
      get
      {
        if (ParentAction != null)
        {
          return ParentAction.FormalName;
        }
        return String.Empty;
      }
    }

    /// <summary>
    ///   Gets or sets a list of conditionals
    /// </summary>
    [XmlElement]
    public List<EventActionConditional> Conditionals
    {
      get { return m_conditionals; }
      set { m_conditionals = value; }
    }

    /// <summary>
    ///   List of Gui Parameters
    /// </summary>
    [XmlElement]
    public List<EventParameter> EventParameters { get; set; }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public EventAction()
    {
      EventParameters = new List<EventParameter>();

      SyNetSettings.Instance.DeserializingFinished += Initialize;
    }

    /// <summary>
    ///   Initialization constructor for an action
    /// </summary>
    /// <param name="p_action"></param>
    public EventAction(Action p_action)
      : base(p_action)
    {
      Name = FormalName;

      EventParameters = new List<EventParameter>();

      //
      // Loop through and create a Gui parameter for each input parameter
      //
      foreach (ActionParameter parameter in p_action.InputParameters)
      {
        EventParameters.Add(new EventParameter(parameter));
      }

      Initialize();
    }

    /// <summary>
    ///   Initializer
    /// </summary>
    private void Initialize()
    {
      WireUpListeners();
      ListenToParameters();
    }

    private void ListenToParameters()
    {
      foreach (EventParameter parameter in EventParameters)
      {
        parameter.PropertyChanged += Parameter_PropertyChanged;
      }
    }

    private void Parameter_PropertyChanged(object p_objSender, PropertyChangedEventArgs p_pcEvArgs)
    {
#if false
		  if (p_pcEvArgs.PropertyName == "Value")
      {
        if (ParameterChanged != null)
        {
          ParameterChanged(this, new EventArgs());
        }
      }

#endif
    }

    private void WireUpListeners()
    {
      Action parentAction = ParentAction;
      if (parentAction != null)
      {
        parentAction.PropertyChanged += ParentAction_PropertyChanged;
      }
    }

    private void ParentAction_PropertyChanged(object p_objSender,PropertyChangedEventArgs p_pcEvArgs)
    {
      //
      // If the name had changed, fire the property changed
      //
      if (p_pcEvArgs.PropertyName == "Name")
      {
        OnPropertyChanged("Name");
      }
    }

    ////////////////////////////////////////////////////////////////////////////

    #region Overrides of Action

    /// <summary>
    ///   String description of the action type
    /// </summary>
    public override string TypeString
    {
      get { return "Event"; }
    }

    /// <summary>
    ///   List of all parameters in this action
    /// </summary>
    [XmlIgnore]
    public override List<ActionParameter> Parameters
    {
      get
      {
        List<ActionParameter> paramList = new List<ActionParameter>();
        foreach (EventParameter param in EventParameters)
        {
          paramList.Add(param);
        }
        return paramList;
      }
    }

    /// <summary>
    /// R/O PROPERTY: return T/F where T means that all conditionals are met
    /// </summary>
    public bool ConditionalsPass
    {
      get
      {
        foreach (EventActionConditional conditional in Conditionals)
        {
          if (!conditional.DoesPass)
          {
            return false;
          }
        }
        return true;
      }
    }

    /// <summary>
    ///   Execute function
    /// </summary>
    /// <returns></returns>
    public override bool Execute()
    {
      bool retVal = false;
      bool bIsValid = true;
      ActionManager mgr = ActionManager.Instance;

      if (Lock())
      {
        Action parentAction = mgr.GetAction(ParentActionID);
        if (parentAction != null)
        {
          if (parentAction.Lock())
          {
            foreach (EventParameter parameter in EventParameters)
            {
              if (parameter.IsValid)
              {
                Parameter parentParam = mgr.GetParameter(parameter.ParentParamID);
                if (parentParam != null)
                {
                  parentParam.StringValue = parameter.StringValue;
                }
                else
                {
                  Debug.WriteLine("[ERR] EventAction.Execute - Parent param null");
                }
              }
              else
              {
                Debug.WriteLine("[ERR] EventAction.Execute - Invalid parameter");
                bIsValid = false;
                break;
              }
            }

            if (bIsValid)
            {
              retVal = parentAction.Execute();
            }
          }
          else
          {
            Debug.WriteLine("[ERR] EventAction.Execute - Could not get parent lock");
          }
          parentAction.Unlock();
        }
        else
        {
          Debug.WriteLine("[ERR] EventAction.Execute - null paramter");
        }
      }
      Unlock();
      return retVal;
    }

    #endregion

    //public event ParameterChangedHandler ParameterChanged;
    ////////////////////////////////////////////////////////////////////////////
  }
}