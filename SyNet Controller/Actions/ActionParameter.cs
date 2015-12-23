using System;
using System.Xml.Serialization;
using SyNet.DataHelpers;

namespace SyNet.Actions
{
  /// <summary>
  ///   Class representing an action parameter
  /// </summary>
  public class ActionParameter : Parameter, IObjectWithID<int>
  {

    ////////////////////////////////////////////////////////////////////////////
    #region Enumerations

    /// <summary>
    ///   Used to describe the type of action parameter
    /// </summary>
    public enum EsnActionParameterType
    {
      /// <summary>
      ///   Parameter needs to be set by user
      /// </summary>
      INPUT,
      /// <summary>
      ///   Parameter is a defined constant
      /// </summary>
      CONSTANT,
      /// <summary>
      ///   Parameter is dependent upon another parameter's value
      /// </summary>
      DEPENDENT,
      /// <summary>
      ///   Parameter value is internally set
      /// </summary>
      INTERNAL
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    private EsnActionParameterType m_parameterType;




    ////////////////////////////////////////////////////////////////////////////
    #region Constructors

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public ActionParameter()
    {
    }

    /// <summary>
    ///   Type-based constructor
    /// </summary>
    /// <param name="p_type"></param>
    public ActionParameter( EsnActionParameterType p_type )
    {
      ParameterType = p_type;
    }

    /// <summary>
    ///   Copy constructor with action ID
    /// </summary>
    /// <param name="p_parameter"></param>
    /// <param name="p_actionID"></param>
    public ActionParameter( ActionParameter p_parameter, UInt64 p_actionID )
      : this(p_parameter)
    {
      //
      // We don't need ot set the ParamID here because it was already created
      // by the base
      //
      ActionID = p_actionID;
    }

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_parameter"></param>
    public ActionParameter( ActionParameter p_parameter )
      : base(p_parameter)
    {
      ID = p_parameter.ID;
      ParameterType = p_parameter.ParameterType;
    }


    /// <summary>
    ///   Constructor to initialize with a device parameter
    /// </summary>
    /// <param name="p_param"></param>
    public ActionParameter( DeviceParameter p_param )
      : base(p_param, p_param.PreviousParamID)
    {
      ParameterType = EsnActionParameterType.INPUT;
      Name = p_param.Name;
      ID = p_param.ID;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Public Properties

    /// <summary>
    ///   ID used for serialization
    /// </summary>
    [XmlAttribute]
    public int ID { get; set; }

    /// <summary>
    ///   Type of action parameter
    /// </summary>
    [XmlAttribute]
    public EsnActionParameterType ParameterType
    {
      get { return m_parameterType; }
      set
      {
        m_parameterType = value;

        //
        // If we're not dependent, we don't need to remember the ID
        //
        if (m_parameterType != EsnActionParameterType.DEPENDENT)
        {
          DependentParamID = 0;
        }

        OnPropertyChanged("ParameterType");
        OnParameterTypeModified();
      }
    }

    /// <summary>
    ///   Action ID
    /// </summary>
    [XmlAttribute]
    public UInt64 ActionID
    { get; set; }

    /// <summary>
    ///   Extended name of the parameter
    /// </summary>
    [XmlIgnore]
    public override string FullName
    {
      get
      {
        if (ActionID != 0)
        {
          return ActionManager.Instance.GetParameterBaseName(ParamID);
        }
        return Name;
      }
    }

    /// <summary>
    ///   Get the value of this parameter using proper logic.
    /// </summary>
    [XmlIgnore]
    public override string StringValue
    {
      get
      {
        switch (ParameterType)
        {
          case EsnActionParameterType.DEPENDENT:
            base.Value = GetDependentValue();
            break;
        }
        return base.Value;
      }
      set
      {
        //
        // Prevent setting a dependent parameter
        //
        switch (ParameterType)
        {
          case EsnActionParameterType.DEPENDENT:
            throw new Exception("ActionParameter.StringValue - Cannot set dependent string");
        }
        base.Value = value;
      }
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Events

    #endregion
    ////////////////////////////////////////////////////////////////////////////
  }
}