using System.Xml.Serialization;

namespace SyNet.Events.Triggers
{
  /// <summary>
  ///   Class representing a parameter within the trigger
  /// </summary>
  public class TriggerParameter : Parameter
  {
    /// <summary>
    ///   Default Constructor
    /// </summary>
    public TriggerParameter()
    {
    }

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_actionParameter"></param>
    public TriggerParameter(Parameter p_actionParameter)
      : base(p_actionParameter)
    {
    }

    #region Overrides of Parameter

    /// <summary>
    ///   Extended name
    /// </summary>
    [XmlIgnore]
    public override string FullName
    {
      get { return base.Name; }
    }

    /// <summary>
    ///   Gets or sets the string value of the parameter
    /// </summary>
    [XmlIgnore]
    public override string StringValue
    {
      get { return Value; }
      set { Value = value; } // UNDONE need tryparse to set int value for this string???????
    }

    #endregion
  }
}