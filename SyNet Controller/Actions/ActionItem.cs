using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SyNet.Actions
{
  /// <summary>
  ///   Lightweight class to reference an action and hold it's actionparameters.
  ///   Used to represent sub-actions.
  /// </summary>
  public class ActionItem : INotifyPropertyChanged  
  {
    #region Public Properties

    /// <summary>
    ///   Action ID of the action represented by this action item
    /// </summary>
    [XmlAttribute]
    public UInt64 ActionID { get; set; }

    /// <summary>
    ///   List of ActionParameters copied from the action represented by 
    ///   this ActionItem
    /// </summary>
    public List<ActionParameter> ActionParameters { get; set; }

    /// <summary>
    ///   Returns the reference to the action object
    /// </summary>
    private Action Action
    {
      get
      {
        return ActionManager.Instance.GetAction(ActionID);
      }
    }

    /// <summary>
    ///   Returns the name of the action
    /// </summary>
    public String ActionName
    {
      get
      {
        string retVal = "";
        Action a = Action;
        if (a != null)
        {
          retVal = a.Name;
        }
        return retVal;
      }
    }

    /// <summary>
    ///   Information string
    /// </summary>
    public string Information
    {
      get
      {
        return this.Action.ToString();
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   Default constructor
    /// </summary>
    public ActionItem()
    {
      ActionParameters = new List<ActionParameter>();
    }

    /// <summary>
    ///   Constructs a new action item from an existing action
    /// </summary>
    /// <param name="p_action"></param>
    /// <param name="p_parentActionID"></param>
    public ActionItem(Action p_action, UInt64 p_parentActionID)
      : this()
    {
      ActionID = p_action.ActionID;

      foreach (ActionParameter parameter in p_action.InputParameters)
      {
        ActionParameters.Add(new ActionParameter(parameter, p_parentActionID));
      }
    }

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_action"></param>
    /// <remarks>
    ///   Any dependant parameters have their dependant ID set to the
    ///   parameter ID that it is copied from. This is used to relink
    ///   dependent parameters after the fact
    /// </remarks>
    public ActionItem(ActionItem p_action)
      : this()
    {
      //
      // Copy the action ID over
      //
      ActionID = p_action.ActionID;

      //
      // Duplicate the parameters
      //
      foreach (ActionParameter param in p_action.ActionParameters)
      {
        ActionParameter newParam = new ActionParameter(param);

        //
        // Reset the actions parent ID so it doesn't reflect back to the
        // parameter that we copied it from and instead reflects back to the
        // origional source
        //
        newParam.ParentParamID = param.ParentParamID;

        //
        // Add it to the collection
        //
        this.ActionParameters.Add(newParam);
      }
    }

    #endregion

    #region Events and Event Handlers

    /// <summary>
    ///   PropertyChanged event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   PropertyChanged event shortcut
    /// </summary>
    /// <param name="p_propertyName"></param>
    protected virtual void OnPropertyChanged( string p_propertyName )
    {
      if (this.PropertyChanged != null)
        this.PropertyChanged(this, new PropertyChangedEventArgs(p_propertyName));
    }

    #endregion
  }
}