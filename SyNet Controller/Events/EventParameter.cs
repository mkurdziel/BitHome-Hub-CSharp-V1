using System;
using System.Xml.Serialization;
using SyNet.Actions;

namespace SyNet.Events
{
  /// <summary>
  /// CLASS our event-specific parameter mapping object
  /// </summary>
  public class EventParameter : ActionParameter
  {

    /// <summary>
    ///   Enumeration for the event parameter type
    /// </summary>
    public enum EsnEventParameterType
    {
      /// <summary>
      ///   Parameter value is set to a constant
      /// </summary>
      Constant,
      /// <summary>
      ///   Parameter value is dependent on a trigger value
      /// </summary>
      Trigger
    }

    #region Private Member Data

    #endregion

    #region Construction

    private EsnEventParameterType m_parameterType = EsnEventParameterType.Constant;

    /// <summary>
    ///   Returns the parameter type
    /// </summary>
    [XmlAttribute]
    public EsnEventParameterType EventParameterType
    {
      get { return m_parameterType; }
      set
      {
        m_parameterType = value;
        OnPropertyChanged("ParameterType");
      }
    }

    /// <summary>
    /// Default Constructor
    /// </summary>
    public EventParameter()
    {
    }

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_rhsParameter"></param>
    public EventParameter(ActionParameter p_rhsParameter)
      : base(p_rhsParameter)
    {
    }

    #endregion

    #region Public Properties

    #endregion
  }
}