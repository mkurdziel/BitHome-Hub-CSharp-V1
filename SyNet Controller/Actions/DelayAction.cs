using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using SyNet.Protocol;

namespace SyNet.Actions
{
  /// <summary>
  ///   Action representing a time delay.
  /// </summary>
  public class DelayAction : Action
  {
    #region Public Properties

    /// <summary>
    ///   Built-in action for the number of seconds of delay
    /// </summary>
    public ActionParameter InputDelaySeconds { get; set; }
    
    /// <summary>
    ///   String type descriptor
    /// </summary>
    [XmlIgnore]
    public override string TypeString
    {
      get { return "Delay"; }
    }

    /// <summary>
    ///   Gets the formal name of the action based on its type and lineage
    /// </summary>
    public override string FormalName
    {
      get
      {
        return "Delay";
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
        List<ActionParameter> list = new List<ActionParameter>();
        list.Add(InputDelaySeconds);
        return list;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public DelayAction()
    {
      Name = "Delay";
    }

    #endregion

    /// <summary>
    ///   Register any necessary default parameters
    /// </summary>
    protected override void RegisterDefaultParameters()
    {
      base.RegisterDefaultParameters();

      // Input Parameters
      InputDelaySeconds = new ActionParameter(ActionParameter.EsnActionParameterType.INPUT);
      InputDelaySeconds.Name = "Delay In Seconds";
      InputDelaySeconds.DataType = EsnDataTypes.BYTE;
      InputDelaySeconds.ValidationType =
        EsnParamValidationType.UNSIGNED_FULL;
      InputDelaySeconds.IntValue = 0;
    }

    /// <summary>
    ///   Execute the delay action.
    /// </summary>
    /// <returns></returns>
    public override bool Execute()
    {
      Debug.WriteLine(
        String.Format(
          "[DBG] Delaying for {0} seconds", InputDelaySeconds.IntValue));
      System.Threading.Thread.Sleep(TimeSpan.FromSeconds(InputDelaySeconds.IntValue));
      return true;
    }
  }
}