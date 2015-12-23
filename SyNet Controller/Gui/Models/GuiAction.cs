using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Xml.Serialization;
using SyNet.Actions;
using Action = SyNet.Actions.Action;

namespace SyNet.Gui.Models
{
  /// <summary>
  ///   Class representing an action visible on the User-created Gui
  /// </summary>
  public class GuiAction : Action
  {
    #region Member Variables

    #endregion

    #region Public Properties

    /// <summary>
    ///   List of Gui Parameters
    /// </summary>
    [XmlElement]
    public List<GuiParameter> GuiParameters { get; set; }

    #endregion

    /// <summary>
    ///   Default constructor
    /// </summary>
    public GuiAction()
    {
      GuiParameters = new List<GuiParameter>();
    }

    /// <summary>
    ///   Initialization constructor for an action
    /// </summary>
    /// <param name="p_action"></param>
    public GuiAction(Action p_action)
      : base(p_action)
    {
      //
      // Set the name to the formal name
      //
      Name = FormalName;

      GuiParameters = new List<GuiParameter>();

      //
      // Loop through and create a Gui parameter for each input parameter
      //
      foreach (ActionParameter parameter in p_action.InputParameters)
      {
        GuiParameters.Add(new GuiParameter(parameter));
      }
    }

    ////////////////////////////////////////////////////////////////////////////
    #region Overrides of Action

    /// <summary>
    ///   String description of the action type
    /// </summary>
    public override string TypeString
    {
      get
      {
        return "Gui";
      }
    }

    /// <summary>
    ///   Gets the formal name of the action based on its type and lineage
    /// </summary>
    public override string FormalName
    {
      get
      {
        return ParentAction.FormalName;
      }
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
        foreach (GuiParameter param in GuiParameters)
        {
          paramList.Add(param);
        }
        return paramList;
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

      if (this.Lock())
      {
        Action parentAction = mgr.GetAction(this.ParentActionID);
        if (parentAction != null)
        {
          if (parentAction.Lock())
          {
            foreach (GuiParameter parameter in GuiParameters)
            {
              if (parameter.IsValid)
              {
                Parameter parentParam = mgr.GetParameter(
                  parameter.ParentParamID);
                if (parentParam != null)
                {
                  parentParam.StringValue = parameter.StringValue;
                }
                else
                {
                  Debug.WriteLine("[ERR] GuiAction.Execute - Parent param null");
                }
              }
              else
              {
                Debug.WriteLine("[ERR] GuiAction.Execute - Invalid parameter");
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
            Debug.WriteLine("[ERR] GuiAction.Execute - Could not get parent lock");
          }
          parentAction.Unlock();
        }
        else
        {
          Debug.WriteLine("[ERR] GuiAction.Execute - null paramter");
        }
      }
      this.Unlock();
      return retVal;
    }

    #endregion

    /// <summary>
    ///   Parameter changed event
    /// </summary>
    //public event ParameterChangedHandler ParameterChanged;
    ////////////////////////////////////////////////////////////////////////////
  }
}
