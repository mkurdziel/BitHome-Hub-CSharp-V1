using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SyNet.Actions;
using SyNet.Events.Triggers;

namespace SyNet.Events
{
  public class EventActionConditional
  {
    public enum EsnConditionalType
    {
      EqualTo,
      GreaterThan,
      LessThan
    }

    /// <summary>
    ///   Parameterless constructor
    /// </summary>
    public EventActionConditional()
    {

    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    /// <param name="p_param"></param>
    /// <param name="p_type"></param>
    /// <param name="p_value"></param>
    public EventActionConditional(TriggerParameter p_param, EsnConditionalType p_type, String p_value)
    {
      ConditionalType = p_type;
      TriggerParameterID = p_param.ParamID;
      ConditionalValue = p_value;
    }

    [XmlAttribute]
    public string ConditionalValue
    {
      get;
      set;
    }

    [XmlAttribute]
    public ulong TriggerParameterID
    {
      get;
      set;
    }

    [XmlAttribute]
    public EsnConditionalType ConditionalType
    {
      get;
      set;
    }

    public bool DoesPass
    {
      get
      {
        Parameter param = ActionManager.Instance.GetParameter(TriggerParameterID);

        if (param == null)
        {
          Debug.WriteLine("[ERR] EventActionConditional has null parameter");
          return false;
        }

        switch (param.ValidationType)
        {
          case SyNet.Protocol.EsnParamValidationType.UNSIGNED_RANGE:
          case SyNet.Protocol.EsnParamValidationType.SIGNED_RANGE:
          case SyNet.Protocol.EsnParamValidationType.UNSIGNED_FULL:
          case SyNet.Protocol.EsnParamValidationType.SIGNED_FULL:
            return ValidateInt(param.IntValue);
            break;
          default:
            Debug.WriteLine("[ERR] EventActionConditional - unimplemented validation type");
            break;
        }


        return true;
      }
    }

    private bool ValidateInt(Int64 p_value)
    {
      Int32 validateIntValue;
      if (!Int32.TryParse(ConditionalValue, out validateIntValue))
      {
        Debug.WriteLine("[ERR] EventActionConditional - unable to parse int value");
        return false;
      }

      switch (ConditionalType)
      {
        case EventActionConditional.EsnConditionalType.EqualTo:
          return p_value == validateIntValue;
        case EventActionConditional.EsnConditionalType.GreaterThan:
          return p_value > validateIntValue;
        case EventActionConditional.EsnConditionalType.LessThan:
          return p_value < validateIntValue;
      }
      return false;
    }

    public override string ToString()
    {
      Parameter param = ActionManager.Instance.GetParameter(TriggerParameterID);
      return String.Format("{0} is {1} {2}", param.Name, ConditionalType, ConditionalValue);
    }
  }
}
