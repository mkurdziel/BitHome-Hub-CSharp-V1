using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using SyNet.Actions;
using SyNet.DataHelpers;
using SyNet.EventArguments;
using SyNet.Protocol;

namespace SyNet
{
  /// <summary>
  ///   Base parameter class. Holds all validation information and logic
  ///   as well as its own internal value.
  /// </summary>
  /// <remarks>
  ///   Made abstract becase I see no usage of an instance of this class
  ///   being necesary when its derivations will always be used.
  /// </remarks>
  public abstract class Parameter : IDisposable, INotifyPropertyChanged
  {
    ////////////////////////////////////////////////////////////////////////////

    #region Member Variables

    private SerializableDictionary<string, int> m_dctEnumValueByName = null;
    private string m_strName;
    private string m_strValue;
    private UInt64 m_nParamID;
    private UInt64 m_nDependentParamID;

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////////

    #region Public Properties

    /// <summary>
    ///   Unique Parameter ID
    /// </summary>
    [XmlAttribute]
    public UInt64 ParamID
    {
      get { return m_nParamID; }
      set
      {
        if (m_nParamID != 0)
        {
          throw new ApplicationException("Parameter.ParamID - Cannot set nonzero ID");
        }

        m_nParamID = value;
      }
    }

    /// <summary>
    ///   Extended name
    /// </summary>
    [XmlIgnore]
    public abstract string FullName { get; }

    /// <summary>
    ///   Parent Parameter ID set by the copy constructor
    /// </summary>
    [XmlAttribute]
    public UInt64 ParentParamID { get; set; }

    /// <summary>
    ///   Gets or sets the string value of the parameter
    /// </summary>
    [XmlAttribute(AttributeName = "Value")]
    public abstract string StringValue { get; set; }

    /// <summary>
    ///   Dependent Parameter ID, if set
    /// </summary>
    [XmlAttribute]
    public UInt64 DependentParamID
    {
      get { return m_nDependentParamID; }
      set
      {
        //
        // Set the new ID
        //
        m_nDependentParamID = value;
      }
    }

    /// <summary>
    ///   Parameter Name
    /// </summary>
    [XmlAttribute]
    public virtual String Name
    {
      get { return m_strName; }
      set
      {
        m_strName = value;
        OnPropertyChanged("Name");
      }
    }

    /// <summary>
    ///   Data Type
    /// </summary>
    [XmlAttribute]
    public EsnDataTypes DataType { get; set; }

    /// <summary>
    ///   Validation Type
    /// </summary>
    [XmlAttribute]
    public EsnParamValidationType ValidationType { get; set; }

    /// <summary>
    ///   Miniumum Value
    /// </summary>
    [XmlAttribute]
    public Int64 MinimumValue { get; set; }

    /// <summary>
    ///   Maximum Value
    /// </summary>
    [XmlAttribute]
    public Int64 MaximumValue { get; set; }

    /// <summary>
    ///   Maximum string length
    /// </summary>
    [XmlAttribute]
    public int MaxStringLength { get; set; }

    /// <summary>
    ///   Returns true if the parameter is signed
    /// </summary>
    [XmlAttribute]
    public bool IsSigned { get; set; }

    /// <summary>
    ///   Returns a string with the range of valid parameter values
    /// </summary>
    [XmlIgnore]
    public string Range
    {
      get
      {
        string retVal = "unknown";
        if (IsInteger)
        {
          switch (ValidationType)
          {
            case EsnParamValidationType.BOOL:
              retVal = "True or False";
              break;
            case EsnParamValidationType.ENUMERATED:
              retVal = "Enumeration";
              break;
            case EsnParamValidationType.MAX_STRING_LEN:
              retVal = string.Format("{0}-character string", MaxStringLength);
              break;
            case EsnParamValidationType.SIGNED_FULL:
              string min = "NA";
              string max = "NA";
              switch (DataType)
              {
                case EsnDataTypes.BYTE:
                  min = sbyte.MinValue.ToString();
                  max = sbyte.MaxValue.ToString();
                  break;
                case EsnDataTypes.WORD:
                  min = Int16.MinValue.ToString();
                  max = Int16.MaxValue.ToString();
                  break;
                case EsnDataTypes.DWORD:
                  min = Int32.MinValue.ToString();
                  max = Int32.MaxValue.ToString();
                  break;
                case EsnDataTypes.QWORD:
                  min = Int64.MinValue.ToString();
                  max = Int64.MaxValue.ToString();
                  break;
                default:
                  Debug.WriteLine("Parameter.Range - something went wrong");
                  break;
              }
              retVal = string.Format("{0} - {1}", min, max);
              break;
            case EsnParamValidationType.UNSIGNED_FULL:
              min = "NA";
              max = "NA";
              switch (DataType)
              {
                case EsnDataTypes.BYTE:
                  min = byte.MinValue.ToString();
                  max = byte.MaxValue.ToString();
                  break;
                case EsnDataTypes.WORD:
                  min = UInt16.MinValue.ToString();
                  max = UInt16.MaxValue.ToString();
                  break;
                case EsnDataTypes.DWORD:
                  min = UInt32.MinValue.ToString();
                  max = UInt32.MaxValue.ToString();
                  break;
                case EsnDataTypes.QWORD:
                  min = UInt64.MinValue.ToString();
                  max = UInt64.MaxValue.ToString();
                  break;
                default:
                  Debug.WriteLine("Parameter.Range - something went wrong");
                  break;
              }
              retVal = string.Format("{0} - {1}", min, max);
              break;
            case EsnParamValidationType.UNKNOWN:
              break;
            case EsnParamValidationType.SIGNED_RANGE:
            case EsnParamValidationType.UNSIGNED_RANGE:
              retVal = string.Format("{0} - {1}", MinimumValue, MaximumValue);
              break;
          }
        }
        return retVal;
      }
    }

    /// <summary>
    ///   Gets the string value of this parameter using proper logic
    ///   (ie gets dependent values or internal values)
    /// </summary>
    [XmlIgnore]
    protected string Value
    {
      get { return m_strValue; }
      set
      {
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          if (Validate(value))
          {
            m_strValue = value;
            OnPropertyChanged("Value");
          }
          else
          {
            throw new ApplicationException(String.Format("Valid range: {0}", Range));
          }
        }
        else
        {
          m_strValue = value;
        }
      }
    }

    /// <summary>
    ///   Gets or sets the int value of the parameter
    /// </summary>
    [XmlIgnore]
    public Int64 IntValue
    {
      get
      {
        Int64 nParsedValue = 0;
        if (ValidateInt(StringValue))
        {
          Int64.TryParse(StringValue, out nParsedValue);
        }
        return nParsedValue;
      }
      set { StringValue = value.ToString(); }
    }

    /// <summary>
    ///   Gets whether or not the parameter value is valid
    /// </summary>
    [XmlIgnore]
    public virtual bool IsValid
    {
      get { return Validate(StringValue); }
    }

    /// <summary>
    ///   Enumeration dictionary
    /// </summary>
    [XmlElement(ElementName = "EnumValueByName", IsNullable = false)]
    public SerializableDictionary<string, int> DctEnumValueByName
    {
      get
      {
        //
        // If we are serializing, return null on an empty dictionary
        //
        if (SyNetSettings.Instance.IsSerializing)
        {
          if (m_dctEnumValueByName.Count == 0)
          {
            return null;
          }
        }
        return m_dctEnumValueByName;
      }
      set { m_dctEnumValueByName = value; }
    }

    /// <summary>
    ///   Returns true if the param is a string type
    /// </summary>
    [XmlIgnore] // no need to persist, this is an evalution of already persisted data
    public bool IsString
    {
      get { return (ValidationType == EsnParamValidationType.MAX_STRING_LEN ? true : false); }
    }

    /// <summary>
    ///   Returns true if the param is an int type
    /// </summary>
    [XmlIgnore] // no need to persist, this is an evalution of already persisted data
    public bool IsInteger
    {
      get
      {
        bool bIntStatus = true;
        switch (ValidationType)
        {
          case EsnParamValidationType.DATE_TIME:
            bIntStatus = false; // no, is quadword (64-bits) date-value
            break;
          case EsnParamValidationType.MAX_STRING_LEN:
            bIntStatus = false; // no, is string
            break;
          case EsnParamValidationType.UNKNOWN:
            bIntStatus = false;
            break;
        }
        return bIntStatus;
      }
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////////

    #region Constructors

    /// <summary>
    ///   Default constructor
    /// </summary>
    protected Parameter()
      : this(0)
    {
    }

    /// <summary>
    ///   Constructor with Parameter ID
    /// </summary>
    /// <param name="p_paramID"></param>
    protected Parameter(UInt64 p_paramID)
    {
      ValidationType = EsnParamValidationType.UNKNOWN;
      DataType = EsnDataTypes.STRING;
      IntValue = 0;
      IsSigned = false;
      m_dctEnumValueByName = new SerializableDictionary<string, int>();
      ParamID = p_paramID;

      //
      // Generate a new unique Parameter ID for this parameter
      //
      if (!SyNetSettings.Instance.IsDeserializing)
      {
        if (ParamID == 0)
        {
          ParamID = ActionManager.Instance.NewParamID();
        }
        Register();
      }
      else
      {
        SyNetSettings.Instance.DeserializingFinished += DeserializingFinished_Handler;
      }
    }

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_parameter"></param>
    /// <param name="p_paramID"></param>
    protected Parameter(Parameter p_parameter, UInt64 p_paramID)
      : this(p_paramID)
    {
      m_strName = p_parameter.Name;
      DataType = p_parameter.DataType;
      ValidationType = p_parameter.ValidationType;
      MinimumValue = p_parameter.MinimumValue;
      MaximumValue = p_parameter.MaximumValue;
      MaxStringLength = p_parameter.MaxStringLength;
      IsSigned = p_parameter.IsSigned;
      ParentParamID = p_parameter.ParamID;
      DependentParamID = p_parameter.DependentParamID;
      m_dctEnumValueByName = p_parameter.m_dctEnumValueByName;
      m_strValue = p_parameter.StringValue;
    }

    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_parameter"></param>
    protected Parameter(Parameter p_parameter)
      : this(p_parameter, 0)
    {
    }

    /// <summary>
    ///   Deserialization finished handler
    /// </summary>
    private void DeserializingFinished_Handler()
    {
      Register();
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////////

    #region Public Functions

    /// <summary>
    ///   Validates a string against the validation information in this
    ///   parameter
    /// </summary>
    /// <param name="p_value"></param>
    /// <returns></returns>
    public bool Validate(String p_value)
    {
      bool bRetVal = IsString
                       ? ValidateString(p_value)
                       :
                         ValidateInt(p_value);
      return bRetVal;
    }

    /// <summary>
    /// OVERRIDE use the Name property for the string interpretation of this object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string strValue = string.Empty;
      //return this.Name;
      switch (ValidationType)
      {
        case EsnParamValidationType.BOOL:
          if (DataType == EsnDataTypes.BOOL)
          {
            strValue = (Value == "0") ? "FALSE" : "TRUE";
          }
          break;
        case EsnParamValidationType.ENUMERATED:
          strValue = NameForEnumValue(int.Parse(Value));
          break;
        case EsnParamValidationType.SIGNED_FULL:
        case EsnParamValidationType.SIGNED_RANGE:
        case EsnParamValidationType.UNSIGNED_FULL:
        case EsnParamValidationType.UNSIGNED_RANGE:
          strValue = Value;
          break;
        case EsnParamValidationType.DATE_TIME:
          if (IntValue == 0)
          {
            strValue = "0";
          }
          else
          {
            DateTime dtValue = new DateTime(IntValue);
            strValue = dtValue.ToShortDateString(); // UNDONE ?? correct format for default ToString() ??
          }
          break;

        default:
          // If unknown, we don't know how to validate so return true.
          strValue = string.Format("?Unknown? ValType=({0})", ValidationType);
          break;
      }
      return string.Format("{0}:[{1}]", Name, strValue);
    }

    /// <summary>
    ///   Dispose method
    /// </summary>
    public void Dispose()
    {
      ActionManager.Instance.UnregisterParameter(ParamID);
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////////

    #region Internal Functions

    /// <summary>
    ///   Get the internal value from a dependent parameter if there is one
    ///   set. Otherwise, returns an empty string.
    /// </summary>
    /// <returns></returns>
    internal String GetDependentValue()
    {
      String retVal = String.Empty;
      ActionManager aMgr = ActionManager.Instance;

      if (DependentParamID != 0)
      {
        Parameter dParam = aMgr.GetParameter(DependentParamID);
        if (dParam != null)
        {
          retVal = dParam.Value;
        }
        else
        {
          Debug.WriteLine(
            "[ERR] Parameter.GetDependentValue - Accessing null parameter");
        }
      }
      else
      {
        Debug.WriteLine(
          "[ERR] Parameter.GetDependentValue - Accessing null dependent parameter");
      }
      return retVal;
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////

    #region Events and Notifications

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_o"></param>
    /// <param name="p_args"></param>
    internal delegate void ParameterModifiedDelegate(object p_o,
                                                     ParameterModifiedEventArgs
                                                       p_args);

    /// <summary>
    /// 
    /// </summary>
    internal event ParameterModifiedDelegate ParameterModified;

    /// <summary>
    ///   Call when parameter type is modified
    /// </summary>
    protected void OnParameterTypeModified()
    {
      OnParameterModified(ParameterModifiedEventArgs.EsnModifyType.PARAM_TYPE);
    }

    /// <summary>
    ///   Call when parameter modified
    /// </summary>
    /// <param name="p_type"></param>
    protected void OnParameterModified(ParameterModifiedEventArgs.EsnModifyType p_type)
    {
      ParameterModifiedEventArgs args = new ParameterModifiedEventArgs(ParamID, p_type);
      if (ParameterModified != null)
      {
        ParameterModified(this, args);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   
    /// </summary>
    /// <param name="p_strPropertyName"></param>
    protected void OnPropertyChanged(string p_strPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(p_strPropertyName));
    }

    #endregion

    #region Private Utility Methods

    /// <summary>
    ///   Register the parameter with the action manager
    /// </summary>
    private void Register()
    {
      ActionManager.Instance.RegisterParameter(this);
    }

    /// <summary>
    ///   Validates a string value
    /// </summary>
    /// <param name="p_value"></param>
    /// <returns></returns>
    private bool ValidateString(String p_value)
    {
      return p_value.Length <= MaxStringLength;
    }

    /// <summary>
    ///   Validates an integer value
    /// </summary>
    /// <param name="p_value"></param>
    /// <returns></returns>
    private bool ValidateInt(String p_value)
    {
      bool retVal = false;

      switch (ValidationType)
      {
        case EsnParamValidationType.BOOL:
          if (DataType == EsnDataTypes.BOOL)
          {
            retVal = (p_value.Equals("0") || (p_value.Equals("1")));
          }
          break;
        case EsnParamValidationType.ENUMERATED:
          retVal = m_dctEnumValueByName.ContainsValue(int.Parse(p_value));
          break;
        case EsnParamValidationType.SIGNED_FULL:
        case EsnParamValidationType.SIGNED_RANGE:
          Int64 sq = 0;
          switch (DataType)
          {
            case EsnDataTypes.BYTE:
              sbyte sb;
              retVal = sbyte.TryParse(p_value, out sb);
              sq = sb;
              break;
            case EsnDataTypes.WORD:
              Int16 sw;
              retVal = Int16.TryParse(p_value, out sw);
              sq = sw;
              break;
            case EsnDataTypes.DWORD:
              Int32 sd;
              retVal = Int32.TryParse(p_value, out sd);
              sq = sd;
              break;
            case EsnDataTypes.QWORD:
              retVal = Int64.TryParse(p_value, out sq);
              break;
          }

          // Do all range checking in one place
          if (ValidationType == EsnParamValidationType.SIGNED_RANGE && retVal)
          {
            retVal = sq <= MaximumValue && sq >= MinimumValue;
          }
          break;
        case EsnParamValidationType.UNSIGNED_FULL:
        case EsnParamValidationType.UNSIGNED_RANGE:
          UInt64 uq = 0;
          switch (DataType)
          {
            case EsnDataTypes.BYTE:
              byte b;
              retVal = byte.TryParse(p_value, out b);
              uq = b;
              break;
            case EsnDataTypes.WORD:
              UInt16 w;
              retVal = UInt16.TryParse(p_value, out w);
              uq = w;
              break;
            case EsnDataTypes.DWORD:
              UInt32 ud;
              retVal = UInt32.TryParse(p_value, out ud);
              uq = ud;
              break;
            case EsnDataTypes.QWORD:
              retVal = UInt64.TryParse(p_value, out uq);
              break;
          }

          // Do all range checking in one place
          if (ValidationType == EsnParamValidationType.UNSIGNED_RANGE && retVal)
          {
            retVal = uq <= (UInt64)MaximumValue && uq >= (UInt64)MinimumValue;
          }
          break;
        case EsnParamValidationType.DATE_TIME:
          switch (DataType)
          {
            case EsnDataTypes.QWORD:
              UInt64 dt;
              retVal = UInt64.TryParse(p_value, out dt);
              break;
            default:
              retVal = false; // any other data types for DATE_TIME fail validation
              break;
          }
          break;
        case EsnParamValidationType.UNKNOWN:
          // If unknown, we don't know how to validate so return true.
          retVal = true;
          break;
      }
      return retVal;
    }

    private string NameForEnumValue(int p_nEnumValue)
    {
      string strEnumName = string.Empty;
      foreach (string strPossEnumName in m_dctEnumValueByName.Keys)
      {
        int nEnumValue = m_dctEnumValueByName[strPossEnumName];
        if (nEnumValue == p_nEnumValue)
        {
          strEnumName = strPossEnumName;
          break;
        }
      }
      return strEnumName;
    }

    #endregion
  }
}