using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using SyNet.Protocol;

namespace SyNet.Events.Triggers
{
  /// <summary>
  /// CLASS superclass of trigger (supporting time-release of events)
  /// </summary>
  public class DateTimeTrigger : Trigger
  {
    // -------------------------------------
    // Use of Parameters in DateTime Trigger
    // -------------------------------------
    //
    // ** INHERITED Parameters **
    //   
    //   CounterValue:  Unsigned Int32 [Default = 1] 
    //   -------------
    //    >0 = counted (fire CounterValue times only)
    //     0 = Forever (Not Counted)
    // 
    //    >1 NOT valid for DT_ONCE DateTimeType!!!
    // 
    // 
    //   Value:  Date-Time:QuadWord (stored as Ticks)
    //   ------
    //     DateTime.Now at time the trigger is being evaluated for
    //     DidFire.   This value is compared against MatchValue based
    //     on MatchOperation.  (Set by EvaluateFireStatus())
    // 
    // 
    //   MatchValue:  Date-Time:QuadWord (stored as Ticks)
    //   -----------
    //     Encoding of DateTime when trigger will next fire
    //     FACADE: NextExpiration property for get/set as DateTime value
    // 
    //     
    //   MatchOperation:  Enum [Default '>=']
    //   ---------------
    //     N/A - this operation works as set. 
    //           DO NOT change this!!
    // 
    // 
    // ** ADDED Parameters **
    // 
    //   StartTimeInTicks:  Date-Time:QuadWord (stored as Ticks) [Default = DateTime.MinValue.Ticks]
    //   ------
    //     The Start DateTime of this trigger
    //     FACADE: StartTime property for get/set as DateTime value (UI use this Method not StartTimeInTicks)
    // 
    // 
    //   EndTimeInTicks:  Date-Time:QuadWord (stored as Ticks) [Default = DateTime.MinValue.Ticks]
    //   ------
    //     The End DateTime of this trigger (alternative end specification, see also: CounterValue)
    //     FACADE: EndTime property for get/set as DateTime value (UI use this Method not EndTimeInTicks)
    //
    //     NOTE: SourceWasCount (bool) indicates which was specified by user: T=CounterValue, F=EndTime
    // 
    // 
    //   DateTimeType:  Enum [Default = DT_ONCE]
    //   -------------
    //     A number of DateTime specifications are supported.  This
    //     enum value describes which is selected for this instance.
    // 
    //       DT_ONCE              - NO Repeat Interval selected
    //       DT_REPEATING         - Repeat interval is simple TimeSpan (every N [Minutes|Hours|Days|Weeks])
    //       DT_NTH_DAY_OF_MONTH  - Repeat interval is List of {nth_day_of_week_within_Month}
    //       DT_DAY_OF_MONTH      - Repeat interval is List of {day_nbrs_within_Month}
    //       DT_DAY_OF_WEEK       - Repeat interval is List of {day_of_week_names}
    //       DT_MONTH_OF_YEAR     - Repeat interval is List of {month_names_within_year}
    //       DT_HOUR_OF_DAY       - Repeat interval is List of {HH:MM within Day}
    //  
    // 
    //   RepeatIntervalInTicks:  Date-Time:QuadWord (stored as Ticks) [Default = TimeSpan.MinValue.Ticks]
    //   ----------------------
    //     The means to set a fixed interval: Minutes to Weeks.
    //     Use CountValue to set the repeat.
    //     Valid only for DT_REPEATING
    //     FACADE: RepeatInterval property for get/set as TimeSpan value (UI use this Method not RepeatIntervalInTicks)
    // 
    //     Ex: "every 5 days" would be setup by setting up:
    //       CountValue     = 1
    //       RepeatInterval = 5 days (or 120 hours, etc.)
    // 
    //     Ex: "each of the next 5 days at 3PM" would be setup:
    //       CountValue     = 5
    //       RepeatInterval = 24 hours
    //       StartDate      = today at 3PM
    // 
    // 
    //   SkipInterval:  Unsigned Int32 [Default = 1]
    //   -------------
    //     The means to set a skip interval: (Do NOT use with DT_ONCE, DT_REPEATING, or DT_HOUR_OF_DAY)
    //    >1 = not consecutive (fire every SkipInterval times)
    //     1 = consecutive (Not skipping)
    // 
    //     * Valid only for DT_DAY_OF_WEEK, DT_NTH_DAY_OF_MONTH, DT_DAY_OF_MONTH or DT_MONTH_OF_YEAR
    // 
    // 
    //    SchedulePointsInTime:  List of Strings [Default = {empty}]
    //    ---------------------
    //      A list of one or more scheduled firings.  The entry format
    //      is unique to each interval type as selected by DateTimeType
    //      as follows:
    // 
    //       DT_ONCE              - list MUST be empty
    // 
    //       DT_REPEATING         - list MUST be empty
    // 
    //       DT_NTH_DAY_OF_MONTH  - one or more entries with each being "[1st|2nd|3rd|4th|Last]{space}[Sun|Mon|Tue|Wed|Thu|Fri|Sat|Day|Weekday|Weekend-Day]"
    // 
    //       DT_DAY_OF_MONTH      - one or more entries with each being a decimal value [1-31] indicating day of month
    // 
    //       DT_DAY_OF_WEEK       - one or more entries with each being "[Sun|Mon|Tue|Wed|Thu|Fri|Sat]"
    // 
    //       DT_MONTH_OF_YEAR     - one or more entries with each being "[Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec]"
    // 
    //         NOTE: MONTH_OF_YEAR list may contain exactly 1 NTH_DAY_OF_MONTH additional entry which applies to ALL Month-name entries in the list
    // 
    //       DT_HOUR_OF_DAY       - one or more entries with each being "HH:MM"
    // 
    //      Count > 1 implies multiple times through the given list
    //      Count = 1 is single time through the list
    // 
    // 
    //    NbrPriorFirings: Int64 [Default = 0]
    //    ----------------
    //     is added so we can track position in list and occurrence count hits
    //     in a way that survives persistence.  CurrentCount can't work since we 
    //     multiply by list-length!!!
    // 
    //    
    //    SourceWasCount: Bool (not applicable unless CounterValue > 0 -or- EndTime is set)
    //    ---------------
    //     Added so UserInterface can track how the user originally specified
    //     a trigger end-date.  Value meanings are:
    //       TRUE  = user specified an actual count number
    //       FALSE = user specified an ending Date/Time from which the count was calculated
    //
    //     [See also: CounterValue and EndTime]
    // 
    // 
    //    ValueDisplayForm: Enum [Default = DF_SHORT]
    //    -----------------
    //     A number of Value() format specifications are supported.  This
    //     enum value describes which is selected for this instance.
    // 
    //       DF_SHORT        - Short date/time format [default]
    //       DF_LONG         - Long date/time format
    //       DF_SHORT_TIME   - Short time-only format
    //       DF_LONG_TIME    - Long time-only format
    //       DF_SHORT_DATE   - Short date-only format
    //       DF_LONG_DATE    - Long date-only format
    // 
    // 
    // -------------------------------------
    // DateTime Trigger interface NOTEs
    // -------------------------------------
    //   String ToString()    
    //      [fired last at ...|net yet fired], [will fire next at ...|will not fire again]
    // 
    //   String Description() 
    //      narrative summary of parameter values
    //      ?Notification when changes cause this to change? (Hmm should be property?)
    // 
    //   String Value()
    //      [default] short-time string of when last triggered
    //   
    //   bool DidFire
    //     trigger fired updating ToString() values
    //     and possibly enabling enclosing event.
    // 
    //   bool WillNoLongerFire
    //     set when count expired (e.g., repeat of 6 expired, or there are no
    //     unprocessed scheduled items in list of items.)
    // 
    //   *Ticks PROPERTIES are for serialization/deserialization only! DO NOT USE.
    //      They will generate an exception if you do!
    // 
    // -------------------------------------
    // DateTime Trigger internal helpers 
    //   key to making trigger work
    // -------------------------------------
    // 
    // 
    // -------------------------------------

    #region Public Interface Enums / Constants

    /// <summary>
    /// ENUM: Type of this DateTime Trigger
    /// </summary>
    public enum EsnDateType
    {
      /// <summary>
      /// NO Repeat Interval selected
      /// </summary>
      DT_ONCE,
      /// <summary>
      /// repeat interval is simple TimeSpan (every N [Minutes|Hours|Days|Weeks|Months])
      /// </summary>
      DT_REPEATING,
      /// <summary>
      /// Repeat interval is List of {nth_day_of_week_within_Month}
      /// </summary>
      DT_NTH_DAY_OF_MONTH,
      /// <summary>
      /// Repeat interval is List of {day_nbrs_within_Month}
      /// </summary>
      DT_DAY_OF_MONTH,
      /// <summary>
      /// Repeat interval is List of {day_of_week_names}
      /// </summary>
      DT_DAY_OF_WEEK,
      /// <summary>
      /// Repeat interval is List of {month_names_within_year}
      /// </summary>
      DT_MONTH_OF_YEAR,
      /// <summary>
      /// Repeat interval is List of {HH:MM within Day}
      /// </summary>
      DT_HOUR_OF_DAY
    }

    /// <summary>
    /// ENUM: Display Format of the Value() of this DateTime Trigger
    /// </summary>
    public enum EsnValueDisplayFormat
    {
      /// <summary>
      /// Short date/time format [default] 
      /// </summary>
      DF_SHORT,
      /// <summary>
      /// Long date/time format
      /// </summary>
      DF_LONG,
      /// <summary>
      /// short time-only format
      /// </summary>
      DF_SHORT_TIME,
      /// <summary>
      /// Long time-only format
      /// </summary>
      DF_LONG_TIME,
      /// <summary>
      /// short date-only format
      /// </summary>
      DF_SHORT_DATE,
      /// <summary>
      /// Long date-only format
      /// </summary>
      DF_LONG_DATE,
    }

    private enum EsnNthDomDayType
    {
      DYT_DAY,
      DYT_WEEKDAY,
      DYT_WEEKEND_DAY,
      DYT_NOTSET
    }

    #endregion

    #region Class Static Member Data

    private const bool WITH_FIELD_ANNOTATION_PARM = true;
    private const bool NO_FIELD_ANNOTATION_PARM = false;

    private const string STR_LONG_DATE_FORMAT = "D"; // See DateTime ToString({formatStr});
    private const string STR_LONG_TIME_FORMAT = "T"; // See DateTime ToString({formatStr});
    private const string STR_SHORT_DATE_FORMAT = "ddd ddMMMyyyy"; // See DateTime ToString({formatStr});
    private const string STR_SHORT_TIME_FORMAT = "t"; // See DateTime ToString({formatStr});
    private const string STR_LONG_DATE_TIME_FORMAT = "F"; // See DateTime ToString({formatStr});
    private const string STR_SHORT_DATE_TIME_FORMAT = "g"; // See DateTime ToString({formatStr});
    private const string STR_DEFAULT_DATE_TIME_FORMAT = STR_SHORT_DATE_TIME_FORMAT;
    private const string STR_CLASS_NAME = "DateTimeTrigger";

    private static string[] s_strAbbrevDayNamesAr = null;
    private static string[] s_strDayTypeNamesAr = null;
    private static string[] s_strAbbrevMonthNamesAr = null;
    private static string[] s_strNthNamesAr = null;
    private static bool s_bShowTriggerTypeInDescription = false; // 'true' is USED DURING TESTING only

    #endregion

    #region Private Member Data

    // These could be automatics but when here we know what is actually persisted
    //  and maintained unique to this derivision
    private Int64 m_nStartTimeInTicks;
    private Int64 m_nEndTimeInTicks;
    private Int64 m_nRepeatIntervalInTicks;
    private UInt64 m_nNbrPriorFirings;
    private UInt64 m_nLastUsedNbrPriorFirings;
    private UInt32 m_nSkipInterval;
    private bool m_bSourceWasCount;
    private EsnValueDisplayFormat m_eValueDisplayForm;
    private DateTime m_dtLastTrigger;
    private bool m_bSchedulePointListContainsYearNthDomSpec;
    private string m_strYearNthDomSpec;
    private EsnDateType m_eNewType;

    private List<String> m_lstSchedulePointsAr;

    #endregion

    #region Construction

    /// <summary>
    /// Default Constructor
    /// </summary>
    public DateTimeTrigger()
    {
      Initialize();
      InitializeDateTimeTriggerParameters();

      SyNetSettings.Instance.DeserializingFinished += Instance_DeserializingFinished;
    }

    /// <summary>
    /// Copy Constructor
    /// </summary>
    /// <param name="p_dttRhs">like item from which to copy values</param>
    public DateTimeTrigger(DateTimeTrigger p_dttRhs)
      : base(p_dttRhs)
    {
      Initialize();

      // copy Trigger parameters... vs. initialize
      DateTimeTypeParameter = new TriggerParameter(p_dttRhs.DateTimeTypeParameter);
      StartTimeParameter = new TriggerParameter(p_dttRhs.StartTimeParameter);
      RepeatIntervalParameter = new TriggerParameter(p_dttRhs.RepeatIntervalParameter);

      // copy key values
      m_nStartTimeInTicks = p_dttRhs.m_nStartTimeInTicks;
      m_nEndTimeInTicks = p_dttRhs.m_nEndTimeInTicks;
      m_nRepeatIntervalInTicks = p_dttRhs.m_nRepeatIntervalInTicks;
      m_lstSchedulePointsAr = p_dttRhs.m_lstSchedulePointsAr;
      m_nNbrPriorFirings = p_dttRhs.m_nNbrPriorFirings;
      m_bSourceWasCount = p_dttRhs.m_bSourceWasCount;

      // force our internal variables to be setup (using deserialize code normally not called)
      Instance_DeserializingFinished();
    }

    /// <summary>
    /// Constructor with Initial Data (one-shot Date/Time)
    /// </summary>
    /// <param name="p_dtScheduledTime">DateTime for this trigger to fire</param>
    public DateTimeTrigger(DateTime p_dtScheduledTime)
    {
      Initialize();
      InitializeDateTimeTriggerParameters();

      // set the trigger type
      DateTimeType = (int)EsnDateType.DT_ONCE;

      // record the settings
      StartTime = p_dtScheduledTime;

      // schedule the first trigger firing
      UpdateFirePoint();
    }

    /// <summary>
    /// Constructor with Initial Data (repeating Date/Time)
    /// </summary>
    /// <param name="p_tsScheduledRepeatInterval">TimeSpan for this trigger to fire (from now and each interval thereafter)</param>
    public DateTimeTrigger(TimeSpan p_tsScheduledRepeatInterval)
    {
      Initialize();
      InitializeDateTimeTriggerParameters();

      // set the trigger type
      DateTimeType = (int)EsnDateType.DT_REPEATING;
      base.CounterValue = 0; // override default, request forever

      // record the settings
      StartTime = DateTime.Now;
      RepeatInterval = p_tsScheduledRepeatInterval;

      // schedule the first trigger firing
      UpdateFirePoint();
    }

    /// <summary>
    /// Special support for reusing a trigger object
    /// </summary>
    public void Clear()
    {
      // reset to default type
      m_eNewType = EsnDateType.DT_ONCE;
      m_lstSchedulePointsAr = new List<string>(); // empty list

      // set default values
      CounterValue = 1;
      m_nRepeatIntervalInTicks = 0;
      m_nSkipInterval = 1;
      m_nStartTimeInTicks = DateTime.MinValue.Ticks; // not set
      m_strYearNthDomSpec = string.Empty;

      // reset internal tracking
      m_nNbrPriorFirings = 0; // none occured
      m_nLastUsedNbrPriorFirings = UInt64.MaxValue;
      m_bSourceWasCount = false;
      m_dtLastTrigger = DateTime.MinValue;
      m_bSchedulePointListContainsYearNthDomSpec = false;
    }

    private void Initialize()
    {
      base.Name = "DateTime Trigger"; // setup our default name
      DebugSource = STR_CLASS_NAME; // init our debug messaging...

      Clear();

      m_eValueDisplayForm = EsnValueDisplayFormat.DF_SHORT;

      //
      // initialize class statics if not already...
      //
      if (s_strAbbrevDayNamesAr == null)
      {
        // exactly 7 entries: 0=Sun - 6=Sat
        s_strAbbrevDayNamesAr = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
      }

      if (s_strAbbrevMonthNamesAr == null)
      {
        // exactly 12+1 entries: 0=Jan - 11=Dec, 12="" (not sure why 13th is there but oh, well...)
        s_strAbbrevMonthNamesAr = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
      }

      if (s_strNthNamesAr == null)
      {
        s_strNthNamesAr = new string[5];
        s_strNthNamesAr[0] = "1st";
        s_strNthNamesAr[1] = "2nd";
        s_strNthNamesAr[2] = "3rd";
        s_strNthNamesAr[3] = "4th";
        s_strNthNamesAr[4] = "Last";
      }

      if (s_strDayTypeNamesAr == null)
      {
        s_strDayTypeNamesAr = new string[3];
        s_strDayTypeNamesAr[(int)EsnNthDomDayType.DYT_DAY] = "Day";
        s_strDayTypeNamesAr[(int)EsnNthDomDayType.DYT_WEEKDAY] = "Weekday";
        s_strDayTypeNamesAr[(int)EsnNthDomDayType.DYT_WEEKEND_DAY] = "Weekend-Day";
      }

      Debug.WriteLine(
        string.Format("[DBG-Dser DTTrigger] Init() DateTimeType={0}, TriggerInstanceID={1}", m_eNewType, ID));
    }

    /// <summary>
    /// Setup DateTime Trigger Parameters
    /// </summary>
    private void InitializeDateTimeTriggerParameters()
    {
      //
      // Initialize the Value Parameter
      //
      ValueParameter = new TriggerParameter();
      ValueParameter.Name = "DateTime Trigger Value";
      ValueParameter.DataType = EsnDataTypes.QWORD;
      ValueParameter.ValidationType = EsnParamValidationType.DATE_TIME;
      ValueParameter.IsSigned = true;
      ValueParameter.IntValue = 0;

      //
      // Initialize the Repeat Interval Parameter
      //
      RepeatIntervalParameter = new TriggerParameter();
      RepeatIntervalParameter.Name = "Repeat Interval Value";
      RepeatIntervalParameter.DataType = EsnDataTypes.QWORD;
      RepeatIntervalParameter.ValidationType = EsnParamValidationType.DATE_TIME;
      RepeatIntervalParameter.IsSigned = true;
      RepeatIntervalParameter.IntValue = 0;

      //
      // Initialize the Start Time Parameter
      //
      StartTimeParameter = new TriggerParameter();
      StartTimeParameter.Name = "Start Time Value";
      StartTimeParameter.DataType = EsnDataTypes.QWORD;
      StartTimeParameter.ValidationType = EsnParamValidationType.DATE_TIME;
      StartTimeParameter.IsSigned = true;
      StartTimeParameter.IntValue = 0;

      //
      // Initialize the DateTime Type parameter
      //
      DateTimeTypeParameter = new TriggerParameter();
      DateTimeTypeParameter.DataType = EsnDataTypes.BYTE;
      DateTimeTypeParameter.ValidationType = EsnParamValidationType.ENUMERATED;
      DateTimeTypeParameter.Name = "Date Type";
      DateTimeTypeParameter.DctEnumValueByName.Add("ONCE", (int)EsnDateType.DT_ONCE);
      DateTimeTypeParameter.DctEnumValueByName.Add("REPEATING", (int)EsnDateType.DT_REPEATING);
      DateTimeTypeParameter.DctEnumValueByName.Add("Month(s) of Year", (int)EsnDateType.DT_MONTH_OF_YEAR);
      DateTimeTypeParameter.DctEnumValueByName.Add("Day(s) of Month", (int)EsnDateType.DT_DAY_OF_MONTH);
      DateTimeTypeParameter.DctEnumValueByName.Add("Nth Day(s) of Month", (int)EsnDateType.DT_NTH_DAY_OF_MONTH);
      DateTimeTypeParameter.DctEnumValueByName.Add("Day(s) of Week", (int)EsnDateType.DT_DAY_OF_WEEK);
      DateTimeTypeParameter.DctEnumValueByName.Add("Hour(s) of Day", (int)EsnDateType.DT_HOUR_OF_DAY);
      DateTimeTypeParameter.IntValue = (int)EsnDateType.DT_ONCE;

      //
      // Override the Match Value parameter (so it becomes a DateTime to match this DateTime)
      //
      MatchValueParameter.ValidationType = EsnParamValidationType.DATE_TIME;
      MatchValueParameter.DataType = EsnDataTypes.QWORD;
      MatchValueParameter.IsSigned = true;
      MatchValueParameter.IntValue = 0;

      //
      // Initialize the Match Operation parameter to be >= expiration time
      //
      MatchOperationParameter.IntValue = (int)EsnMatchOperation.VO_GREATER_THAN_OR_EQUAL;
    }

    private void Instance_DeserializingFinished()
    {
      Debug.WriteLine(string.Format("[DBG-Dser] DateTimeTrigger.DeserializingFinished - ENTRY"));
      Debug.WriteLine(
        string.Format(
          "[DBG-Dser DTTrigger] Instance_DeserializingFinished DateTimeType={0}, TriggerInstanceID={1}", m_eNewType, ID));
      DateTimeTypeParameter.IntValue = (int)m_eNewType;
      // correct load from XML case
      if (m_lstSchedulePointsAr == null)
      {
        m_lstSchedulePointsAr = new List<string>(); // empty list 
      }

      RepeatInterval = new TimeSpan(RepeatIntervalInTicks);
      StartTime = new DateTime(StartTimeInTicks);

      if (!IsValidScheduleSet(m_lstSchedulePointsAr))
      {
        string strError = string.Format("DateTimeTrigger ID=({0}) LOAD ERROR: SchedulePoint list is NOT valid!", ID);
        Debug.WriteLine(strError);
        throw new ArgumentException(strError);
      }

      UpdateFirePoint();
      Debug.WriteLine(string.Format("[DBG-Dser] DateTimeTrigger.DeserializingFinished - EXIT"));
    }

    #endregion

    #region Persisted XML Attributes

    /// <summary>
    ///   Overriden to provide the correct property updates
    /// </summary>
    [XmlAttribute]
    public override long CounterValue
    {
      get { return base.CounterValue; }
      set
      {
        if (base.CounterValue != value)
        {
          base.CounterValue = value;
          if (!SyNetSettings.Instance.IsDeserializing)
          {
            m_bSourceWasCount = true; // mark that the count was given (not the end-time)

            // calc dependent values then notify of description changes
            UpdateFirePoint();
            InvokePropertyChanged(new PropertyChangedEventArgs("Description"));
            InvokePropertyChanged(new PropertyChangedEventArgs("Narrative"));
          }
        }
      }
    }

    /// <summary>
    /// R/W PROPERTY: Get/Set DateTime trigger type [DT_ONCE|DT_REPEATING|etc.]
    /// </summary>
    [XmlAttribute]
    public int DateTimeType
    {
      get
      {
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          if (m_eNewType != EDateTimeType)
          {
            //m_eNewType = EDateTimeType;
          }
        }
        return (int)m_eNewType;
      }
      set
      {
        if ((int)m_eNewType != value)
        {
          switch (value)
          {
            case (int)EsnDateType.DT_DAY_OF_MONTH:
              m_eNewType = EsnDateType.DT_DAY_OF_MONTH;
              break;
            case (int)EsnDateType.DT_DAY_OF_WEEK:
              m_eNewType = EsnDateType.DT_DAY_OF_WEEK;
              break;
            case (int)EsnDateType.DT_HOUR_OF_DAY:
              m_eNewType = EsnDateType.DT_HOUR_OF_DAY;
              break;
            case (int)EsnDateType.DT_MONTH_OF_YEAR:
              m_eNewType = EsnDateType.DT_MONTH_OF_YEAR;
              break;
            case (int)EsnDateType.DT_NTH_DAY_OF_MONTH:
              m_eNewType = EsnDateType.DT_NTH_DAY_OF_MONTH;
              break;
            case (int)EsnDateType.DT_ONCE:
              m_eNewType = EsnDateType.DT_ONCE;
              CounterValue = 1; // force the repeat to once when this is selected
              break;
            case (int)EsnDateType.DT_REPEATING:
              m_eNewType = EsnDateType.DT_REPEATING;
              break;
            default:
              Debug.Assert(false, string.Format("[CODE] ERROR Bad value given to DateTimeType.set([{0}])", value));
              break;
          }

          if (!SyNetSettings.Instance.IsDeserializing)
          {
            DateTimeTypeParameter.IntValue = (int)m_eNewType;
            InvokePropertyChanged(new PropertyChangedEventArgs("DateTimeType"));
          }
          else
          {
            Debug.WriteLine(
              string.Format(
                "[DBG-Dser DTTrigger] DateTimeType.set DateTimeType={0}, TriggerInstanceID={1}", m_eNewType, ID));
          }
        }
      }
    }

    /// <summary>
    /// R/W PROPERTY: nbr of previous firings of this trigger
    /// </summary>
    /// <remarks>this must only be set by construction/deserializer or didFire events</remarks>
    [XmlAttribute]
    public UInt64 NbrPriorFirings
    {
      get { return m_nNbrPriorFirings; }
      set { m_nNbrPriorFirings = value; }
    }

    /// <summary>
    /// Serializeable form of Interval TimeSpan (just the "Ticks" ma'am!)
    /// </summary>
    [XmlAttribute]
    public Int64 RepeatIntervalInTicks
    {
      get { return m_nRepeatIntervalInTicks; }
      set
      {
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          throw new ArgumentException("ERROR RepeatIntervalInTicks.set accessed when NOT de-serializing!");
        }
        m_nRepeatIntervalInTicks = value;
      }
    }

    /// <summary>
    /// R/W PROPERTY: get/set the list of fixed schedule points
    /// </summary>
    [XmlArray(ElementName = "SchedulePoints")]
    [XmlArrayItem(typeof(String), ElementName = "SchedulePoint")]
    public List<String> SchedulePoints
    {
      get { return m_lstSchedulePointsAr; }
      set
      {
        if (IsValidScheduleSet(value))
        {
          m_lstSchedulePointsAr = value;
          if (!SyNetSettings.Instance.IsDeserializing)
          {
            // calc dependent values then notify of description changes
            UpdateFirePoint();
            InvokePropertyChanged(new PropertyChangedEventArgs("Description"));
            InvokePropertyChanged(new PropertyChangedEventArgs("Narrative"));
          }
        }
        else
        {
          throw new ArgumentException(
            "DateTimeTrigger:SchedulePoints.set(): One or more invalid values in list.  Set aborted!");
        }
        if (SyNetSettings.Instance.IsDeserializing)
        {
          Debug.WriteLine(string.Format("[DeSer] SchedulePoints.Count={0}, TriggerInstanceID={1}", value.Count, ID));
        }
      }
    }

    /// <summary>
    /// R/W PROPERTY: fire every (SkipInterval)th time
    /// </summary>
    /// <remarks>this is intended to be set by the DateTimeTrigger editor</remarks>
    [XmlAttribute]
    public UInt32 SkipInterval
    {
      get { return m_nSkipInterval; }
      set
      {
        m_nSkipInterval = value;
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          // calc dependent values then notify of description changes
          UpdateFirePoint();
          InvokePropertyChanged(new PropertyChangedEventArgs("Description"));
          InvokePropertyChanged(new PropertyChangedEventArgs("Narrative"));
        }
      }
    }

    /// <summary>
    /// R/W PROPERTY: fire every (SkipInterval)th time
    /// </summary>
    /// <remarks>this is intended to be set by the DateTimeTrigger editor</remarks>
    [XmlAttribute]
    public bool SourceWasCount
    {
      get { return m_bSourceWasCount; }
      set
      {
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          Debug.Assert(false, "SourceWasCount.set called when NOT de-serializing!");
        }
        m_bSourceWasCount = value;
      }
    }

    /// <summary>
    /// Serializeable form of StartTime DateTime (just the "Ticks" ma'am!)
    /// </summary>
    [XmlAttribute]
    public Int64 StartTimeInTicks
    {
      get { return m_nStartTimeInTicks; }
      set
      {
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          throw new ArgumentException("ERROR StartTimeInTicks.set accessed when NOT de-serializing!");
        }
        m_nStartTimeInTicks = value;
      }
    }

    /// <summary>
    /// Serializeable form of EndTime DateTime (just the "Ticks" ma'am!)
    /// </summary>
    [XmlAttribute]
    public long EndTimeInTicks
    {
      get { return m_nEndTimeInTicks; }
      set
      {
        if (!SyNetSettings.Instance.IsDeserializing)
        {
          throw new ArgumentException("ERROR EndTimeInTicks.set accessed when NOT de-serializing!");
        }
        m_nEndTimeInTicks = value;
      }
    }


    /// <summary>
    /// R/W PROPERTY: get/set the display format of the Value() returned string 
    /// (date/time trigger last fired)
    /// </summary>
    /// <remarks>Also controls the ToString() display format</remarks>
    [XmlAttribute]
    public int ValueDisplayFormat
    {
      get { return (int)m_eValueDisplayForm; }
      set
      {
        if ((int)m_eValueDisplayForm != value)
        {
          switch (value)
          {
            case (int)EsnValueDisplayFormat.DF_LONG:
              m_eValueDisplayForm = EsnValueDisplayFormat.DF_LONG;
              break;
            case (int)EsnValueDisplayFormat.DF_LONG_TIME:
              m_eValueDisplayForm = EsnValueDisplayFormat.DF_LONG_TIME;
              break;
            case (int)EsnValueDisplayFormat.DF_SHORT:
              m_eValueDisplayForm = EsnValueDisplayFormat.DF_SHORT;
              break;
            case (int)EsnValueDisplayFormat.DF_SHORT_TIME:
              m_eValueDisplayForm = EsnValueDisplayFormat.DF_SHORT_TIME;
              break;
            default:
              Debug.Assert(false, string.Format("[CODE] ERROR Bad value given to ValueDisplayFormat.set([{0}])", value));
              break;
          }
          InvokePropertyChanged(new PropertyChangedEventArgs("ValueDisplayFormat"));
          InvokePropertyChanged(new PropertyChangedEventArgs("Value"));
        }
      }
    }

    #endregion

    #region Public Properties (not persisted)

    /// <summary>
    ///   Parameter holding the [ONCE|REPEATING] type selection for the trigger
    /// </summary>
    [XmlIgnore]
    public TriggerParameter DateTimeTypeParameter { get; set; }

    /// <summary>
    /// R/W PROPERTY: Get/Set DateTime for this trigger to fire
    /// </summary>
    [XmlIgnore]
    public DateTime StartTime
    {
      get { return new DateTime(StartTimeInTicks); }
      set
      {
        m_nStartTimeInTicks = value.Ticks;
        StartTimeParameter.IntValue = value.Ticks;
        InvokePropertyChanged(new PropertyChangedEventArgs("StartTime"));
        // calc dependent values then notify of description changes
        UpdateFirePoint();
        InvokePropertyChanged(new PropertyChangedEventArgs("Description"));
        InvokePropertyChanged(new PropertyChangedEventArgs("Narrative"));
      }
    }

    /// <summary>
    /// R/W PROPERTY: Get/Set DateTime past which this trigger will no longer fire
    /// </summary>
    /// <remarks>this is simply storage used by the UI at the moment.  It is not used internally.</remarks>
    [XmlIgnore]
    public DateTime EndTime
    {
      get { return new DateTime(EndTimeInTicks); }
      set
      {
        EndTimeInTicks = value.Ticks;
        m_bSourceWasCount = false; // mark that the end-time was given (not the count)
        InvokePropertyChanged(new PropertyChangedEventArgs("EndTime"));
        // calc dependent values then notify of description changes
        UpdateFirePoint();
        InvokePropertyChanged(new PropertyChangedEventArgs("Description"));
        InvokePropertyChanged(new PropertyChangedEventArgs("Narrative"));
      }
    }

    /// <summary>
    /// R/W PROPERTY: Get/Set DateTime trigger type [DT_ONCE|DT_REPEATING|etc.]
    /// </summary>
    /// <remarks>this accessor allows us to use the ENUM values directly...</remarks>
    [XmlIgnore]
    public EsnDateType EDateTimeType
    {
      get
      {
        EsnDateType eNewType = EsnDateType.DT_ONCE;
        switch (DateTimeTypeParameter.IntValue)
        {
          case (int)EsnDateType.DT_DAY_OF_MONTH:
            eNewType = EsnDateType.DT_DAY_OF_MONTH;
            break;
          case (int)EsnDateType.DT_DAY_OF_WEEK:
            eNewType = EsnDateType.DT_DAY_OF_WEEK;
            break;
          case (int)EsnDateType.DT_HOUR_OF_DAY:
            eNewType = EsnDateType.DT_HOUR_OF_DAY;
            break;
          case (int)EsnDateType.DT_MONTH_OF_YEAR:
            eNewType = EsnDateType.DT_MONTH_OF_YEAR;
            break;
          case (int)EsnDateType.DT_NTH_DAY_OF_MONTH:
            eNewType = EsnDateType.DT_NTH_DAY_OF_MONTH;
            break;
          case (int)EsnDateType.DT_ONCE:
            eNewType = EsnDateType.DT_ONCE;
            break;
          case (int)EsnDateType.DT_REPEATING:
            eNewType = EsnDateType.DT_REPEATING;
            break;
          default:
            Debug.Assert(
              false,
              string.Format("[CODE] ERROR Bad value given to EDateTimeType.get([{0}])", DateTimeTypeParameter.IntValue));
            break;
        }
        return eNewType;
      }
      set
      {
        DateTimeTypeParameter.IntValue = (int)value;
        InvokePropertyChanged(new PropertyChangedEventArgs("DateTimeType"));
      }
    }

    /// <summary>
    ///   Parameter holding the [ONCE|REPEATING] type selection for the trigger
    /// </summary>
    [XmlIgnore]
    public TriggerParameter StartTimeParameter { get; set; }

    /// <summary>
    /// R/W PROPERTY: Get/Set Repeat-Interval for this trigger to fire
    /// </summary>
    [XmlIgnore]
    public TimeSpan RepeatInterval
    {
      get { return new TimeSpan(RepeatIntervalInTicks); }
      set
      {
        m_nRepeatIntervalInTicks = value.Ticks; // do NOT use Property Access (not deserializing)
        RepeatIntervalParameter.IntValue = value.Ticks;
        InvokePropertyChanged(new PropertyChangedEventArgs("RepeatInterval"));

        if (!SyNetSettings.Instance.IsDeserializing)
        {
          // calc dependent values then notify of description changes
          UpdateFirePoint();
          InvokePropertyChanged(new PropertyChangedEventArgs("Description"));
          InvokePropertyChanged(new PropertyChangedEventArgs("Narrative"));
        }
      }
    }

    /// <summary>
    ///   Parameter holding the regular Repeat Interval (TimeSpan) for the trigger
    /// </summary>
    [XmlIgnore]
    public TriggerParameter RepeatIntervalParameter { get; set; }

    /// <summary>
    /// R/W PROPERTY: Get/Set DateTime for this trigger to fire
    /// </summary>
    [XmlIgnore]
    public DateTime NextExpiration
    {
      get { return new DateTime(MatchValue); }
      set
      {
        MatchValue = value.Ticks;
        InvokePropertyChanged(new PropertyChangedEventArgs("NextExpiration"));
        InvokePropertyChanged(new PropertyChangedEventArgs("Value"));
      }
    }

    /// <summary>
    /// R/O PROPERTY: (for GUI use) return description string (without annotation)
    /// </summary>
    [XmlIgnore]
    public string Narrative
    {
      get { return DescriptionWithAnnotation(NO_FIELD_ANNOTATION_PARM); }
    }

    /// <summary>
    /// R/W PROPERTY: get/set attribute indicating that trigger fired 
    /// it is overriden so that we can signal change of value
    /// </summary>
    [XmlIgnore]
    public override bool DidFire
    {
      get { return base.DidFire; }
      set
      {
        base.DidFire = value;
        if (value == true)
        {
          m_dtLastTrigger = new DateTime(MatchValueParameter.IntValue);
          InvokePropertyChanged(new PropertyChangedEventArgs("Value"));
        }
      }
    }

    /// <summary>
    /// R/O PROPERTY: (for GUI use) return field-annotated description string
    /// </summary>
    [XmlIgnore]
    public override string Description
    {
      get { return DescriptionWithAnnotation(WITH_FIELD_ANNOTATION_PARM); }
    }

    /// <summary>
    /// R/O PROPERTY: Return the newly arrived value (value which caused trigger DidFire)
    /// </summary>
    [XmlIgnore]
    public override string Value
    {
      get
      {
        string strValueInterpretation = m_dtLastTrigger == DateTime.MinValue
                                          ? "Not-yet-set"
                                          : DateTimeAsPreferredInterpretation(m_dtLastTrigger);
        return strValueInterpretation;
      }
    }

    /// <summary>
    ///   R/O PROPERTY: Returns a readonly collection of parameters in the trigger
    /// </summary>
    [XmlIgnore]
    public override ReadOnlyCollection<TriggerParameter> Parameters
    {
      get
      {
        List<TriggerParameter> parameters = new List<TriggerParameter>();
        // NOTE DateTimeTrigger does NOT expose any Params (has custom UI/Editor)
        return parameters.AsReadOnly();
      }
    }

    #endregion

    #region Public Methods - Trigger Control

    /// <summary>
    /// Fire Trigger if at next expiration...
    /// </summary>
    /// <param name="p_dtEvaluationTime">time the evaluation is being performed by scheduler</param>
    public void EvaluateFireStatus(DateTime p_dtEvaluationTime)
    {
      if (!WillNoLongerFire)
      {
        ValueParameter.IntValue = p_dtEvaluationTime.Ticks;
        ConditionallyCountInstance();
      }
    }

    /// <summary>
    /// will be a narrative summary of parameter values
    /// </summary>
    /// <param name="p_bWithAnnotation">T/F where T means inject field markers so GUI can parse</param>
    /// <returns>string containing setup description</returns>
    public string DescriptionWithAnnotation(bool p_bWithAnnotation)
    {
      StringBuilder sbDescription = new StringBuilder();

      bool bTriggerValid = IsValidScheduleSet(m_lstSchedulePointsAr);
      string strTriggerValid = (bTriggerValid) ? "Valid" : "NOT Valid";
      Debug.WriteLine(string.Format("   Trigger Setup: {0}", strTriggerValid));

      string strStartTime = StartTime.ToString(STR_SHORT_TIME_FORMAT);
      strStartTime = WrapAnnotation(strStartTime, "StartTime", p_bWithAnnotation);
      strStartTime = String.Format("At {0}", strStartTime);

      string strHowManyTimes;
      string strRepeatInterval;
      string[] strDaysAr;

      switch (DateTimeTypeParameter.IntValue)
      {
        case (int)EsnDateType.DT_DAY_OF_MONTH:
          //
          //  At {StartTime} on the {MonthList} {skipInterval} [(CountValue!=0) {countValue}] from {StartDate} [(CountValue!=0) to {EndDate}]
          //
          // Examples:
          //  At 6:00 PM on the 12th of each Month  from Wed 03Feb2010
          //  At 6:00 PM on the 1st, 31st of every 2nd Month  from Wed 03Feb2010
          //
          // decode RepeatCount
          strHowManyTimes = InterpretOccurrenceAndSkipCounts(SkipInterval, CounterValue, "Month", p_bWithAnnotation);
          // get list decode
          strDaysAr = new string[SchedulePoints.Count];
          for (int nDayIdx = 0; nDayIdx < SchedulePoints.Count; nDayIdx++)
          {
            strDaysAr[nDayIdx] = AppendNumberSuffix(SchedulePoints[nDayIdx]);
          }
          strRepeatInterval = string.Join(", ", strDaysAr);
          strRepeatInterval = WrapAnnotation(strRepeatInterval, "SchedulePoints", p_bWithAnnotation);
          if (s_bShowTriggerTypeInDescription)
          {
            sbDescription.Append("DAY_OF_MONTH: ");
          }
          sbDescription.AppendFormat("{0} on the {1} {2}", strStartTime, strRepeatInterval, strHowManyTimes);
          break;

        case (int)EsnDateType.DT_DAY_OF_WEEK:
          //
          //  At {StartTime} on {DayList} {skipInterval} [(CountValue!=0) {countValue}] from {StartDate} [(CountValue!=0) to {EndDate}]
          //
          // Examples:
          //  At 6:00 PM on Mon, Wed, Thu of each Week  from Wed 03Feb2010
          //  At 6:00 PM on Mon, Fri of each Week  from Wed 03Feb2010
          //  At 3:00 PM on Mon, Fri of each Week  from Wed 03Feb2010
          //
          // decode RepeatCount
          strHowManyTimes = InterpretOccurrenceAndSkipCounts(SkipInterval, CounterValue, "Week", p_bWithAnnotation);
          // get list decode
          strDaysAr = new string[SchedulePoints.Count];
          SchedulePoints.CopyTo(strDaysAr, 0);
          strRepeatInterval = string.Join(", ", strDaysAr);
          strRepeatInterval = WrapAnnotation(strRepeatInterval, "SchedulePoints", p_bWithAnnotation);
          if (s_bShowTriggerTypeInDescription)
          {
            sbDescription.Append("DAY_OF_WEEK: ");
          }
          sbDescription.AppendFormat("{0} on {1} {2}", strStartTime, strRepeatInterval, strHowManyTimes);
          break;

        case (int)EsnDateType.DT_HOUR_OF_DAY:
          //
          //  At {HourList} {skipInterval} [(CountValue!=0) {countValue}] from {StartDate} [(CountValue!=0) to {EndDate}]
          //
          // Examples:
          //  At 15:00, 18:00 of each Day  from Wed 03Feb2010
          //  At 15:00, 18:00 of each Day for 10 Days  from Wed 03Feb2010 to Sat 13Feb2010
          //
          // decode RepeatCount
          strHowManyTimes = InterpretOccurrenceAndSkipCounts(SkipInterval, CounterValue, "Day", p_bWithAnnotation);
          // get list decode
          strDaysAr = new string[SchedulePoints.Count];
          SchedulePoints.CopyTo(strDaysAr, 0);
          strRepeatInterval = string.Join(", ", strDaysAr);
          strRepeatInterval = WrapAnnotation(strRepeatInterval, "SchedulePoints", p_bWithAnnotation);
          if (s_bShowTriggerTypeInDescription)
          {
            sbDescription.Append("HOUR_OF_DAY: ");
          }
          sbDescription.AppendFormat("At {0} {1}", strRepeatInterval, strHowManyTimes);
          break;

        case (int)EsnDateType.DT_MONTH_OF_YEAR:
          //
          //  At {StartTime} on the {Nth DOM} of {MonthList} {skipInterval} [(CountValue!=0) {countValue}] from {StartDate} [(CountValue!=0) to {EndDate}]
          //
          // Examples:
          //  At 6:00 PM  on the 2nd Mon in Jan, Feb of each Year  from Wed 03Feb2010
          //  At 6:00 PM  on the 1st Thu in Jan, Jul of each Year  from Wed 03Feb2010
          //
          // decode RepeatCount
          strHowManyTimes = InterpretOccurrenceAndSkipCounts(SkipInterval, CounterValue, "Year", p_bWithAnnotation);
          // get list decode
          string strDayOfMonthSpec = string.Empty;
          List<string> strMonthNamesAr = new List<string>();
          for (int nEntryIdx = 0; nEntryIdx < SchedulePoints.Count; nEntryIdx++)
          {
            if (SchedulePoints[nEntryIdx].Contains(" "))
            {
              strDayOfMonthSpec = SchedulePoints[nEntryIdx];
            }
            else
            {
              strMonthNamesAr.Add(SchedulePoints[nEntryIdx]);
            }
          }
          strDaysAr = new string[strMonthNamesAr.Count];
          strMonthNamesAr.CopyTo(strDaysAr, 0);
          string strMonthList = string.Join(", ", strDaysAr);
          string strDayOfMonth = (strDayOfMonthSpec == string.Empty)
                                   ? string.Empty
                                   : string.Format(" on the {0}", strDayOfMonthSpec);
          strRepeatInterval = (strDayOfMonth == string.Empty)
                                ? string.Format("on {0}", strMonthList)
                                : string.Format("{0} in {1}", strDayOfMonth, strMonthList);
          strRepeatInterval = WrapAnnotation(strRepeatInterval, "SchedulePoints", p_bWithAnnotation);
          if (s_bShowTriggerTypeInDescription)
          {
            sbDescription.Append("MONTH_OF_YEAR: ");
          }
          sbDescription.AppendFormat("{0} {1} {2}", strStartTime, strRepeatInterval, strHowManyTimes);
          break;

        case (int)EsnDateType.DT_NTH_DAY_OF_MONTH:
          //
          //  At {StartTime} on the {Nth DOM} {skipInterval} [(CountValue!=0) {countValue}] from {StartDate} [(CountValue!=0) to {EndDate}]
          //
          // Examples:
          //  At 6:00 PM on the Last Weekday of each Month  from Wed 03Feb2010
          //  At 6:00 PM on the Last Weekend-Day of each Month  from Wed 03Feb2010
          //  At 6:00 PM on the 2nd Weekend-Day of each Month  from Wed 03Feb2010
          //  At 6:00 PM on the Last Weekday of each Month for 6 Months  from Wed 03Feb2010 to Tue 03Aug2010
          //  At 6:00 PM on the Last Weekend-Day of each Month for 12 Months  from Wed 03Feb2010 to Thu 03Feb2011
          //  At 6:00 PM on the Last Day of each Month for 6 Months  from Wed 03Feb2010 to Tue 03Aug2010
          //
          // decode RepeatCount
          strHowManyTimes = InterpretOccurrenceAndSkipCounts(SkipInterval, CounterValue, "Month", p_bWithAnnotation);
          // get list decode
          strDaysAr = new string[SchedulePoints.Count];
          SchedulePoints.CopyTo(strDaysAr, 0);
          strRepeatInterval = string.Join(", ", strDaysAr);
          strRepeatInterval = WrapAnnotation(strRepeatInterval, "SchedulePoints", p_bWithAnnotation);
          if (s_bShowTriggerTypeInDescription)
          {
            sbDescription.Append("NTH_DAY_OF_MONTH: ");
          }
          sbDescription.AppendFormat("{0} on the {1} {2}", strStartTime, strRepeatInterval, strHowManyTimes);
          break;

        case (int)EsnDateType.DT_ONCE:
          //
          //  At {StartTime} on {StartDate}
          //
          // Examples:
          //   At 1:25 AM on 2/23/2010
          //
          if (s_bShowTriggerTypeInDescription)
          {
            sbDescription.Append("ONCE: ");
          }
          string strSingleStartTime = StartTime.ToString(STR_SHORT_TIME_FORMAT);
          strSingleStartTime = WrapAnnotation(strSingleStartTime, "StartTime", p_bWithAnnotation);

          string strSingleStartDate = StartTime.ToString(STR_SHORT_DATE_FORMAT);
          strSingleStartDate = WrapAnnotation(strSingleStartDate, "StartDate", p_bWithAnnotation);

          sbDescription.AppendFormat("At {0} on {1}", strSingleStartTime, strSingleStartDate);
          break;

        case (int)EsnDateType.DT_REPEATING:
          //
          //  At {StartTime} {skipInterval} [(CountValue!=0) {countValue}] from {StartDate} [(CountValue!=0) to {EndDate}]
          //
          // Examples:
          //  At 1:23 AM every 2nd Hour for 4 Hours  from Tue 23Feb2010 to Tue 23Feb2010
          //  At 1:23 AM every 6 Hours,5 Minutes for 31 Hours  from Tue 23Feb2010 to Tue 02Mar2010
          //  At 1:23 AM of each Week  from Tue 23Feb2010
          //  At 1:23 AM of every 10th Day  from Tue 23Feb2010
          //  At 6:00 PM of each Day  from Wed 03Feb2010
          //  At 6:00 AM of every 4th Day  from Wed 03Feb2010
          //  At 6:00 PM of every 4th Day for 100 Days  from Wed 03Feb2010 to Thu 10Mar2011
          //  At 6:00 PM of every 2nd Week for 24 Weeks  from Wed 03Feb2010 to Wed 21Jul2010
          //
          // decode repeat interval
          string strUnits;
          UInt64 nSkipInterval;
          Int64 nCounterValue;
          bool bUseInterval;
          strRepeatInterval = InterpretRepeatInterval(
            RepeatInterval, out strUnits, out nSkipInterval, out nCounterValue, out bUseInterval);
          strRepeatInterval = WrapAnnotation(strRepeatInterval, "SchedulePoints", p_bWithAnnotation);
          // decode RepeatCount
          strHowManyTimes = InterpretOccurrenceAndSkipCounts(nSkipInterval, nCounterValue, strUnits, p_bWithAnnotation);
          if (s_bShowTriggerTypeInDescription)
          {
            sbDescription.Append("REPEATING: ");
          }
          if (bUseInterval)
          {
            sbDescription.AppendFormat("{0} {1} {2}", strStartTime, strRepeatInterval, strHowManyTimes);
          }
          else
          {
            sbDescription.AppendFormat("{0} {1}", strStartTime, strHowManyTimes);
          }
          break;
        default:
          sbDescription.Append("Trigger DateTime not yet decoded - TBA");
          break;
      }
      return sbDescription.ToString();
    }

    /// <summary>
    /// Reset the triggered condition (enable next interval)
    /// </summary>
    public override void Reset()
    {
      // UNDONE needs work for DateTimeTrigger Specifics
      base.Reset(); // reset DidFire and occurrence count
      UpdateFirePoint(); // setup next expiration
    }

    /// <summary>
    /// [fired last at ...|net yet fired], [will fire next at ...|will not fire again]
    /// </summary>
    /// <returns>string containing firing description</returns>
    /// <remarks>NOTE: is subject to ValueDisplayFormat</remarks>
    public override string ToString()
    {
      string strNextFiresAt = (WillNoLongerFire)
                                ? "Won't fire again."
                                : string.Format(
                                    "Will next fire: {0}.", DateTimeAsPreferredInterpretation(NextExpiration));
      return string.Format("Fired Last: {0}, {1}.", Value, strNextFiresAt);
    }

    #endregion

    #region Private Date/Time Formatting Support Routines

    private DateTime CalcEndDate(Int64 p_nCounterValue, string p_strUnits)
    {
      const int NBR_DAYS_IN_WEEK = 7;
      DateTime dtEnd = StartTime;
      switch (p_strUnits)
      {
        case "Year":
          dtEnd = dtEnd.AddYears((int)p_nCounterValue);
          break;
        case "Month":
          dtEnd = dtEnd.AddMonths((int)p_nCounterValue);
          break;
        case "Week":
          dtEnd = dtEnd.AddDays((int)p_nCounterValue * NBR_DAYS_IN_WEEK);
          break;
        default:
          for (int nIntervalCt = 0; nIntervalCt < p_nCounterValue; nIntervalCt++)
          {
            dtEnd = dtEnd.Add(RepeatInterval);
          }
          break;
      }
      return dtEnd;
    }

    private static string HowOftenByUnits(bool p_bNeedCommaSep, int p_nNbrValues, int p_nValue, string p_strUnits)
    {
      string strFormattedResult = (p_bNeedCommaSep) ? "," : string.Empty;
      string strPluralSuffix = (p_nValue == 1) ? "" : "s";
      strFormattedResult += (p_nNbrValues == 1 && p_nValue == 1)
                              ? p_strUnits
                              : string.Format("{0} {1}{2}", p_nValue, p_strUnits, strPluralSuffix);
      return strFormattedResult;
    }

    private string InterpretRepeatInterval(
      TimeSpan p_tsRepeatInterval, out string p_strUnits, out UInt64 p_nSkipInterval, out Int64 p_nCounterValue,
      out bool p_bHasInterval)
    {
      string strHowOften = string.Empty;

      int nActualWeeks = p_tsRepeatInterval.Days / 7;
      int nActualDays = p_tsRepeatInterval.Days % 7;
      int nActualHours = p_tsRepeatInterval.Hours;
      int nActualMinutes = p_tsRepeatInterval.Minutes;

      p_nSkipInterval = SkipInterval;
      p_nCounterValue = CounterValue;

      // force Weeks with remaining Days to show up as only Days
      if (nActualWeeks > 0 &&
          nActualDays > 0)
      {
        nActualDays += nActualWeeks * 7;
        nActualWeeks = 0;
      }

      // force Days with remaining Hours to show up as only Hours
      if (nActualDays > 0 &&
          nActualHours > 0)
      {
        nActualHours += nActualDays * 24;
        nActualDays = 0;
      }

      int nNbrValues = 0;

      int nValuesBitSet = 0;
      if (nActualWeeks > 0)
      {
        nValuesBitSet |= 1 << 3;
        nNbrValues++;
      }
      if (nActualDays > 0)
      {
        nValuesBitSet |= 1 << 2;
        nNbrValues++;
      }
      if (nActualHours > 0)
      {
        nValuesBitSet |= 1 << 1;
        nNbrValues++;
      }
      if (nActualMinutes > 0)
      {
        nValuesBitSet |= 1 << 0;
        nNbrValues++;
      }

      if ((nValuesBitSet & 0x08) > 0)
      {
        p_strUnits = "Week";
      }
      else if ((nValuesBitSet & 0x04) > 0)
      {
        p_strUnits = "Day";
      }
      else if ((nValuesBitSet & 0x02) > 0)
      {
        p_strUnits = "Hour";
      }
      else
      {
        p_strUnits = "Minute";
      }

      // if we've ended up with a simple interval, one of: [Weeks|Days|Hours|Minutes]
      if (nNbrValues == 1)
      {
        // then rebalance the RepeatInterval and SkipInterval Values
        switch (nValuesBitSet)
        {
          case 8:
            if (nActualWeeks > 1)
            {
              p_nSkipInterval = (UInt64)nActualWeeks;
              nActualWeeks = 1;
            }
            break;
          case 4:
            if (nActualDays > 1)
            {
              p_nSkipInterval = (UInt64)nActualDays;
              nActualDays = 1;
            }
            break;
          case 2:
            if (nActualHours > 1)
            {
              p_nSkipInterval = (UInt64)nActualHours;
              nActualHours = 1;
            }
            break;
          case 1:
            if (nActualMinutes > 1)
            {
              p_nSkipInterval = (UInt64)nActualMinutes;
              nActualMinutes = 1;
            }
            break;
        }
      }

      // in this world, we multiply the occurrence counts to get them correct
      bool bCorrectionForMultipleValues = (nNbrValues > 1) ? true : false;
      Int64 nOrigCounterValue = p_nCounterValue;
      if (p_nCounterValue > 0)
      {
        if (p_nSkipInterval > 1)
        {
          p_nCounterValue *= (Int64)p_nSkipInterval;
        }
        else if (nActualWeeks > 1)
        {
          const double DAYS_PER_WEEK = 7.0;
          p_nCounterValue *= nActualWeeks;
          if (bCorrectionForMultipleValues)
          {
            int nCorrectionValue =
              (int)Math.Round(((nOrigCounterValue * nActualDays) + (DAYS_PER_WEEK / 2)) / DAYS_PER_WEEK, 0);
            p_nCounterValue += nCorrectionValue;
          }
        }
        else if (nActualDays > 1)
        {
          const double HOURS_PER_DAY = 24.0;
          p_nCounterValue *= nActualDays;
          if (bCorrectionForMultipleValues)
          {
            int nCorrectionValue =
              (int)Math.Round(((nOrigCounterValue * nActualHours) + (HOURS_PER_DAY / 2)) / HOURS_PER_DAY, 0);
            p_nCounterValue += nCorrectionValue;
          }
        }
        else if (nActualHours > 1)
        {
          const double MINUTES_PER_HOUR = 60.0;
          p_nCounterValue *= nActualHours;
          if (bCorrectionForMultipleValues)
          {
            int nCorrectionValue =
              (int)Math.Round(((nOrigCounterValue * nActualMinutes) + (MINUTES_PER_HOUR / 2)) / MINUTES_PER_HOUR, 0);
            p_nCounterValue += nCorrectionValue;
          }
        }
        else if (nActualMinutes > 1)
        {
          p_nCounterValue *= nActualMinutes;
        }
      }

      p_bHasInterval = (nNbrValues > 1) ? true : false;

      bool bNeedCommaSep = false;
      if ((nValuesBitSet & 0x08) > 0)
      {
        strHowOften += HowOftenByUnits(bNeedCommaSep, nNbrValues, nActualWeeks, "Week");
      }

      if ((nValuesBitSet & 0x04) > 0)
      {
        bNeedCommaSep = ((nValuesBitSet & 0x08) > 0) ? true : false;
        strHowOften += HowOftenByUnits(bNeedCommaSep, nNbrValues, nActualDays, "Day");
      }

      if ((nValuesBitSet & 0x02) > 0)
      {
        bNeedCommaSep = ((nValuesBitSet & 0x0c) > 0) ? true : false;
        strHowOften += HowOftenByUnits(bNeedCommaSep, nNbrValues, nActualHours, "Hour");
      }

      if ((nValuesBitSet & 0x01) > 0)
      {
        bNeedCommaSep = ((nValuesBitSet & 0x0e) > 0) ? true : false;
        strHowOften += HowOftenByUnits(bNeedCommaSep, nNbrValues, nActualMinutes, "Minute");
      }

      return strHowOften;
    }

    private string InterpretOccurrenceAndSkipCounts(
      UInt64 p_nSkipInterval, Int64 p_nCounterValue, string p_strUnits, bool p_bWithAnnotation)
    {
      string strSkipInterval = (p_nSkipInterval == 1)
                                 ? string.Format("each {0}", p_strUnits)
                                 : string.Format(
                                     "every {0} {1}", AppendNumberSuffix(p_nSkipInterval.ToString()), p_strUnits);
      strSkipInterval = WrapAnnotation(strSkipInterval, "SkipInterval", p_bWithAnnotation); // WRAP!
      string strHowManyTimes = string.Format("of {0}", strSkipInterval);

      if (p_nCounterValue > 0)
      {
        string strCounterValueInterp = string.Format("{0} {1}s", p_nCounterValue, p_strUnits);
        strCounterValueInterp = WrapAnnotation(strCounterValueInterp, "Counter", p_bWithAnnotation); // WRAP!
        strHowManyTimes += string.Format(" for {0}", strCounterValueInterp);
      }

      string strStartDate = StartTime.ToString(STR_SHORT_DATE_FORMAT);
      strStartDate = WrapAnnotation(strStartDate, "StartDate", p_bWithAnnotation); // WRAP!
      strHowManyTimes += string.Format("  from {0}", strStartDate);

      if (p_nCounterValue > 1)
      {
        DateTime dtEndPoint = CalcEndDate(p_nCounterValue, p_strUnits);
        string strEndDate = dtEndPoint.ToString(STR_SHORT_DATE_FORMAT);
        strEndDate = WrapAnnotation(strEndDate, "EndDate", p_bWithAnnotation); // WRAP!
        strHowManyTimes += string.Format(" to {0}", strEndDate);
      }

      return strHowManyTimes;
    }

    private string DateTimeAsPreferredInterpretation(DateTime p_dtValueToShow)
    {
      string strValueInterpretation;
      switch (m_eValueDisplayForm)
      {
        case EsnValueDisplayFormat.DF_LONG:
          strValueInterpretation = p_dtValueToShow.ToString(STR_LONG_DATE_TIME_FORMAT);
          break;
        case EsnValueDisplayFormat.DF_LONG_TIME:
          strValueInterpretation = p_dtValueToShow.ToString(STR_LONG_TIME_FORMAT);
          break;
        case EsnValueDisplayFormat.DF_SHORT_TIME:
          strValueInterpretation = p_dtValueToShow.ToString(STR_SHORT_TIME_FORMAT);
          break;
        case EsnValueDisplayFormat.DF_LONG_DATE:
          strValueInterpretation = p_dtValueToShow.ToString(STR_LONG_DATE_FORMAT);
          break;
        case EsnValueDisplayFormat.DF_SHORT_DATE:
          strValueInterpretation = p_dtValueToShow.ToString(STR_SHORT_DATE_FORMAT);
          break;
        default:
          strValueInterpretation = p_dtValueToShow.ToString(STR_SHORT_DATE_TIME_FORMAT);
          break;
      }
      return strValueInterpretation;
    }

    private static string AppendNumberSuffix(string p_strNbr)
    {
      string strDaySuffix;
      switch (p_strNbr.Substring(p_strNbr.Length - 1, 1))
      {
        case "1":
          strDaySuffix = "st";
          break;
        case "2":
          strDaySuffix = "nd";
          if (p_strNbr == "12")
          {
            strDaySuffix = "th";
          }
          break;
        case "3":
          strDaySuffix = "rd";
          break;
        default:
          strDaySuffix = "th";
          break;
      }
      return string.Format("{0}{1}", p_strNbr, strDaySuffix);
    }

    private static string WrapAnnotation(string p_strValueToBeWrapped, string p_strWrapID, bool p_bDoAnnotate)
    {
      string strPossWrappedValue = p_strValueToBeWrapped;
      if (p_bDoAnnotate)
      {
        strPossWrappedValue = string.Format("<{0}>{1}</{0}>", p_strWrapID, p_strValueToBeWrapped);
      }
      return strPossWrappedValue;
    }

    #endregion

    #region Public Methods - Comparison Operators

    /// <summary>
    /// determine if the triggers are the same (except for their unique ID values)
    /// </summary>
    /// <param name="p_trNewTrigger">rhs trigger to compare</param>
    /// <returns>T/F where T means they are the same (without consideration of the ID)</returns>
    public override bool EqualsExceptID(Trigger p_trNewTrigger)
    {
      bool bEqualStatus = false;
      DateTimeTrigger trNewDateTimeTrigger = p_trNewTrigger as DateTimeTrigger;
      if (trNewDateTimeTrigger != null)
      {
        // IMPLE NOTE: exclude DateTime and ID from match!!!! (DateTime is always diff)
        bEqualStatus = ((CounterValue == trNewDateTimeTrigger.CounterValue) &&
          // IGNORE ID == trNewDateTimeTrigger.ID &&
          // IGNORE MatchValue == trNewDateTimeTrigger.MatchValue &&
          // IGNORE Name == trNewDateTimeTrigger.Name &&
                        (MatchOperation == trNewDateTimeTrigger.MatchOperation) &&
          // IGNORE StartTimeInTicks == trNewDateTimeTrigger.StartTimeInTicks &&
                        (RepeatIntervalInTicks == trNewDateTimeTrigger.RepeatIntervalInTicks) &&
                        AreSchedulePointSetsTheSame(trNewDateTimeTrigger) &&
          // IGNORE NbrPriorFirings == trNewDateTimeTrigger.NbrPriorFirings &&
                        (SkipInterval == trNewDateTimeTrigger.SkipInterval) &&
                        (SourceWasCount == trNewDateTimeTrigger.SourceWasCount) &&
          // IGNORE ValueDisplayFormat == trNewDateTimeTrigger.ValueDisplayFormat &&
                        (DateTimeType == trNewDateTimeTrigger.DateTimeType))
                         ? true
                         : false;
      }
      return bEqualStatus;
    }

    #endregion

    #region Utility Properties/Methods

    private void UpdateFirePoint()
    {
      // schedule the next (first?) trigger firing
      if (IsValidTrigger() &&
          !WillNoLongerFire)
      {
        NextExpiration = CalcNextExpiration();
      }
    }

    private bool IsValidTrigger()
    {
      bool bValidStatus = true;
      // all triggers need start-time
      if (StartTimeInTicks == DateTime.MinValue.Ticks)
      {
        bValidStatus = false;
      }
      // if schedulePoint form then we need a schedule set
      if (bValidStatus && IsSchedulePointTrigger)
      {
        if (m_lstSchedulePointsAr.Count < 1)
        {
          bValidStatus = false;
        }
        else
        {
          bValidStatus = IsValidScheduleSet(m_lstSchedulePointsAr);
        }
      }
      // else if is repeating for then we need a repeat interval
      else if (bValidStatus && IsSimpleIntervalTrigger &&
               RepeatIntervalInTicks == 0)
      {
        bValidStatus = false;
      }
      return bValidStatus;
    }

    /// <summary>
    /// R/O PROPERTY: return T/F where T means that occ count has expired or we've processed
    /// all items in our schedule (list)
    /// </summary>
    private bool WillNoLongerFire
    {
      get
      {
        bool bWontFireStatus = false;
        UInt64 nTotalEvents = TotalEventCount();

        if (nTotalEvents <= NbrPriorFirings)
        {
          bWontFireStatus = true;
        }
        return bWontFireStatus;
      }
    }

    private UInt64 TotalEventCount()
    {
      UInt64 nRealCountValue = (CounterValue == 0) ? UInt64.MaxValue : (UInt64)CounterValue;
      return (IsSchedulePointTrigger)
               ? (UInt64)m_lstSchedulePointsAr.Count * nRealCountValue
               : nRealCountValue;
    }

    private DateTime CalcNextExpiration()
    {
      DateTime dtNextExpiresAt;
      if (IsSimpleIntervalTrigger && RepeatIntervalInTicks > 0)
      {
        dtNextExpiresAt = NextIntervalExpiration();
      }
      else if (IsSchedulePointTrigger)
      {
        dtNextExpiresAt = NextScheduledExpiration();
      }
      else
      {
        dtNextExpiresAt = StartTime;
      }
      return dtNextExpiresAt;
    }

    private DateTime NextIntervalExpiration()
    {
      DateTime dtFirstOrPriorFire = (NextExpiration == DateTime.MinValue || NbrPriorFirings == 0) ? StartTime : NextExpiration;
      DateTime dtNextIntervalExpiration = dtFirstOrPriorFire.Add(RepeatInterval);
      Debug.Assert(IsSimpleIntervalTrigger, "NextIntervalExpiration() Only works with Simple-Interval Triggers");
      Debug.Assert(SkipInterval == 1, "NextIntervalExpiration() Skip is NOT allowed with this form of trigger!");
      Debug.WriteLine(
        string.Format(
          "NextIntervalExpiration(): Next [{0}] after [{1}] is [{2}]!",
          RepeatInterval,
          dtFirstOrPriorFire.ToString(STR_LONG_DATE_TIME_FORMAT),
          dtNextIntervalExpiration.ToString(STR_LONG_DATE_TIME_FORMAT)));
      return dtNextIntervalExpiration;
    }

    private DateTime NextScheduledExpiration()
    {
      const bool PARM_SEARCH_BACKWARD_IN_TIME = false;
      const bool PARM_SEARCH_FORWARD_IN_TIME = true;
      const int PARM_FIRST_DAY_OF_MONTH = 1;
      if (m_nLastUsedNbrPriorFirings == m_nNbrPriorFirings)
      {
        Debug.WriteLine(
          string.Format("NextScheduledExpiration(): ReCalculating due to setup change!"));
      }
      m_nLastUsedNbrPriorFirings = m_nNbrPriorFirings;
      DateTime dtNextScheduledExpiration = DateTime.MinValue;
      Debug.Assert(IsSchedulePointTrigger, "NextScheduledExpiration() Only works with Schedule Triggers");
      int nMaxListEntries = SchedulePoints.Count;
      if (EDateTimeType == EsnDateType.DT_MONTH_OF_YEAR && m_bSchedulePointListContainsYearNthDomSpec)
      {
        nMaxListEntries -= 1;
      }
      int nListIdx = ((int)NbrPriorFirings % nMaxListEntries);
      string strNextSchedulePoint = SchedulePoints[nListIdx];
      DateTime dtPrevFirePoint = (NextExpiration == DateTime.MinValue || NbrPriorFirings == 0) ? StartTime : NextExpiration;
      DateTime dtNextFirePoint = dtPrevFirePoint;
      switch (EDateTimeType)
      {
        case EsnDateType.DT_HOUR_OF_DAY:
          // have HH:MM locate next ahead of now
          // calculate possible next by replacing time with desired hour (seconds reset to zero)
          int nHour = int.Parse(strNextSchedulePoint.Substring(0, 2));
          int nMinute = int.Parse(strNextSchedulePoint.Substring(3, 2));
          dtNextFirePoint = new DateTime(dtPrevFirePoint.Year, dtPrevFirePoint.Month, dtPrevFirePoint.Day, nHour, nMinute, 0);
          // if the possible next is not in the future then add one day to fix it
          if (dtPrevFirePoint > dtNextFirePoint)
          {
            dtNextFirePoint = dtNextFirePoint.AddDays(1); // bump to next day
          }
          // return the new value to our caller
          dtNextScheduledExpiration = dtNextFirePoint;
          Debug.WriteLine(
            string.Format(
              "NextScheduledExpiration(): Next [{0}] after [{1}] is [{2}]!",
              strNextSchedulePoint,
              dtPrevFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT),
              dtNextFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT)));
          break;
        case EsnDateType.DT_DAY_OF_WEEK:
          // have [Sun-Sat] locate next ahead of now
          bool bFoundDay = NextMatchingDay(strNextSchedulePoint, PARM_SEARCH_FORWARD_IN_TIME, ref dtNextFirePoint);
          if (bFoundDay)
          {
            dtNextScheduledExpiration = dtNextFirePoint;
            Debug.WriteLine(
              string.Format(
                "NextScheduledExpiration(): Next [{0}] after [{1}] is [{2}]!",
                strNextSchedulePoint,
                dtPrevFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT),
                dtNextFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT)));
          }
          else
          {
            Debug.Assert(false, string.Format("[CODE] Failed to find match for day of week [{0}]!", strNextSchedulePoint));
          }
          break;
        case EsnDateType.DT_DAY_OF_MONTH:
          // have [1-31] of start month locate next ahead of now
          // calculate possible next by replacing time with desired hour (seconds reset to zero)
          int nDesiredDay = int.Parse(strNextSchedulePoint);
          dtNextFirePoint = new DateTime(dtPrevFirePoint.Year, dtPrevFirePoint.Month, nDesiredDay, dtPrevFirePoint.Hour, dtPrevFirePoint.Minute, dtPrevFirePoint.Second);
          if (dtNextFirePoint <= dtPrevFirePoint)
          {
            //Debug.Assert(false, string.Format("[CODE] days not in proper order, next desired [{0}] was beyond end of this month!", strNextSchedulePoint));
            dtNextFirePoint = dtNextFirePoint.AddYears(1);
          }
          dtNextScheduledExpiration = dtNextFirePoint;
          Debug.WriteLine(
            string.Format(
              "NextScheduledExpiration(): Next [{0}] after [{1}] is [{2}]!",
              strNextSchedulePoint,
              dtPrevFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT),
              dtNextFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT)));
          break;
        case EsnDateType.DT_NTH_DAY_OF_MONTH:
          // have Nth day of month spec locate next ahead of now
          int nNthDay;
          bool bIsLastDayRequested;
          string strDayOfWeek;
          EsnNthDomDayType eNthDOMDayType;
          ValuesFromNthSpec(
            strNextSchedulePoint, out nNthDay, out bIsLastDayRequested, out strDayOfWeek, out eNthDOMDayType);
          int nLastDayOfThisMonth = DateTime.DaysInMonth(dtPrevFirePoint.Year, dtPrevFirePoint.Month);
          if (bIsLastDayRequested && eNthDOMDayType == EsnNthDomDayType.DYT_NOTSET)
          {
            // Have  "Last [Sun-Sat]" form
            // calculate last day of this month...
            dtNextFirePoint = new DateTime(dtPrevFirePoint.Year, dtPrevFirePoint.Month, nLastDayOfThisMonth, dtPrevFirePoint.Hour, dtPrevFirePoint.Minute, dtPrevFirePoint.Second);
            // have [Sun-Sat] locate next prior to ...
            if (NextMatchingDay(strDayOfWeek, PARM_SEARCH_BACKWARD_IN_TIME, ref dtNextFirePoint))
            {
              // if we are already past the Last [Sun-Sat] of this month then select next year's day same month
              if (dtNextFirePoint <= dtPrevFirePoint)
              {
                // calculate last day of this month, next year
                dtNextFirePoint = new DateTime(dtPrevFirePoint.Year + 1, dtPrevFirePoint.Month, nLastDayOfThisMonth, dtPrevFirePoint.Hour, dtPrevFirePoint.Minute, dtPrevFirePoint.Second);
                // have [Sun-Sat] locate next prior to ...
                if (!NextMatchingDay(strDayOfWeek, PARM_SEARCH_BACKWARD_IN_TIME, ref dtNextFirePoint))
                {
                  Debug.Assert(false, string.Format("[CODE] Failed to find match for Nth DayOfMonth next year [{0}]!", strNextSchedulePoint));
                }
              }
              dtNextScheduledExpiration = dtNextFirePoint;
              Debug.WriteLine(
                string.Format(
                  "NextScheduledExpiration(): Next [{0}] after [{1}] is [{2}]!",
                  strNextSchedulePoint,
                  dtPrevFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT),
                  dtNextFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT)));
            }
            else
            {
              Debug.Assert(false, string.Format("[CODE] Failed to find match for Nth DayOfMonth [{0}]!", strNextSchedulePoint));
            }
          }
          else if (!bIsLastDayRequested && eNthDOMDayType == EsnNthDomDayType.DYT_NOTSET)
          {
            // Have  "[1st,2nd,3rd,4th] [Sun-Sat]" form
            // have [Sun-Sat] locate Nth day from 1st day of month
            dtNextFirePoint = new DateTime(dtPrevFirePoint.Year, dtPrevFirePoint.Month, PARM_FIRST_DAY_OF_MONTH, dtPrevFirePoint.Hour, dtPrevFirePoint.Minute, dtPrevFirePoint.Second);
            if (NextNthMatchingDay(strDayOfWeek, PARM_SEARCH_FORWARD_IN_TIME, nNthDay, ref dtNextFirePoint))
            {
              // if we are already past the [1st,2nd,3rd,4th] [Sun-Sat] of this month then select next year's day same month
              if (dtNextFirePoint <= dtPrevFirePoint)
              {
                // start search from 1st day of this month, next year...
                dtNextFirePoint = new DateTime(dtPrevFirePoint.Year + 1, dtPrevFirePoint.Month, PARM_FIRST_DAY_OF_MONTH, dtPrevFirePoint.Hour, dtPrevFirePoint.Minute, dtPrevFirePoint.Second);
                if (!NextNthMatchingDay(strDayOfWeek, PARM_SEARCH_FORWARD_IN_TIME, nNthDay, ref dtNextFirePoint))
                {
                  Debug.Assert(false, string.Format("[CODE] Failed to find match for Nth DayOfMonth next year [{0}]!", strNextSchedulePoint));
                }
              }
              dtNextScheduledExpiration = dtNextFirePoint;
              Debug.WriteLine(
                string.Format(
                  "NextScheduledExpiration(): Next [{0}] after [{1}] is [{2}]!",
                  strNextSchedulePoint,
                  dtPrevFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT),
                  dtNextFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT)));
            }
            else
            {
              Debug.Assert(false, string.Format("[CODE] Failed to find match for Nth DayOfMonth [{0}]!", strNextSchedulePoint));
            }
          }
          else if (bIsLastDayRequested && eNthDOMDayType != EsnNthDomDayType.DYT_NOTSET)
          {
            // Have  "Last [Day|Weekday|Weekend-Day]" form
            // calculate last day of this month...
            dtNextFirePoint = new DateTime(dtPrevFirePoint.Year, dtPrevFirePoint.Month, nLastDayOfThisMonth, dtPrevFirePoint.Hour, dtPrevFirePoint.Minute, dtPrevFirePoint.Second);
            // have [Day|Weekday|Weekend-Day] locate next prior to now
            if (NextMatchingDayType(eNthDOMDayType, PARM_SEARCH_BACKWARD_IN_TIME, ref dtNextFirePoint))
            {
              // if we are already past the Last [Day|Weekday|Weekend-Day] of this month then select next year's day same month
              if (dtNextFirePoint <= dtPrevFirePoint)
              {
                // calculate last day of this month, next year
                dtNextFirePoint = new DateTime(dtPrevFirePoint.Year + 1, dtPrevFirePoint.Month, nLastDayOfThisMonth, dtPrevFirePoint.Hour, dtPrevFirePoint.Minute, dtPrevFirePoint.Second);
                // have [Day|Weekday|Weekend-Day] locate next prior to ...
                if (!NextMatchingDayType(eNthDOMDayType, PARM_SEARCH_BACKWARD_IN_TIME, ref dtNextFirePoint))
                {
                  Debug.Assert(false, string.Format("[CODE] Failed to find match for Nth DayOfMonth next year [{0}]!", strNextSchedulePoint));
                }
              }
              dtNextScheduledExpiration = dtNextFirePoint;
              Debug.WriteLine(
                string.Format(
                  "NextScheduledExpiration(): Next [{0}] after [{1}] is [{2}]!",
                  strNextSchedulePoint,
                  dtPrevFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT),
                  dtNextFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT)));
            }
            else
            {
              Debug.Assert(false, string.Format("[CODE] Failed to find match for Nth DayOfMonth [{0}]!", strNextSchedulePoint));
            }
          }
          else // if (!bIsLastDayRequested && eNthDOMDayType != EsnNthDomDayType.DYT_NOTSET)
          {
            // Have  "[1st,2nd,3rd,4th] [Day|Weekday|Weekend-Day]" form
            // have [Day|Weekday|Weekend-Day] locate Nth day from 1st day of month
            dtNextFirePoint = new DateTime(dtPrevFirePoint.Year, dtPrevFirePoint.Month, PARM_FIRST_DAY_OF_MONTH, dtPrevFirePoint.Hour, dtPrevFirePoint.Minute, dtPrevFirePoint.Second);
            if (NextNthMatchingDayType(eNthDOMDayType, PARM_SEARCH_FORWARD_IN_TIME, nNthDay, ref dtNextFirePoint))
            {
              // if we are already past the [1st,2nd,3rd,4th] [Day|Weekday|Weekend-Day] of this month then select next year's day same month
              if (dtNextFirePoint <= dtPrevFirePoint)
              {
                // start search from 1st day of this month, next year...
                dtNextFirePoint = new DateTime(dtPrevFirePoint.Year + 1, dtPrevFirePoint.Month, PARM_FIRST_DAY_OF_MONTH, dtPrevFirePoint.Hour, dtPrevFirePoint.Minute, dtPrevFirePoint.Second);
                if (!NextNthMatchingDayType(eNthDOMDayType, PARM_SEARCH_FORWARD_IN_TIME, nNthDay, ref dtNextFirePoint))
                {
                  Debug.Assert(false, string.Format("[CODE] Failed to find match for Nth DayOfMonth next year [{0}]!", strNextSchedulePoint));
                }
              }
              dtNextScheduledExpiration = dtNextFirePoint;
              Debug.WriteLine(
                string.Format(
                  "NextScheduledExpiration(): Next [{0}] after [{1}] is [{2}]!",
                  strNextSchedulePoint,
                  dtPrevFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT),
                  dtNextFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT)));
            }
            else
            {
              Debug.Assert(false, string.Format("[CODE] Failed to find match for Nth DayOfMonth [{0}]!", strNextSchedulePoint));
            }
          }
          break;
        case EsnDateType.DT_MONTH_OF_YEAR:
          // have month of year with possible NthDay spec locate next ahead of now
          if(!m_bSchedulePointListContainsYearNthDomSpec)
          {
            // have simple Month of year list!
            // have [Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec] locate next ahead of now
            if (NextMatchingMonth(strNextSchedulePoint, PARM_SEARCH_FORWARD_IN_TIME, ref dtNextFirePoint))
            {
              dtNextScheduledExpiration = dtNextFirePoint;
              Debug.WriteLine(
                string.Format(
                  "NextScheduledExpiration(): Next [{0}] after [{1}] is [{2}]!",
                  strNextSchedulePoint,
                  dtPrevFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT),
                  dtNextFirePoint.ToString(STR_LONG_DATE_TIME_FORMAT)));
            }
            else
            {
              Debug.Assert(false, string.Format("[CODE] Failed to find match for Month of Year [{0}]!", strNextSchedulePoint));
            }
          }
          else
          {
            // have Month of year list with Nth DOM spec!
          }
          break;
        default:
          Debug.Assert(
            false, string.Format("[CODE] ERROR have unexpected DateType=[{0}], new type? bad code?", EDateTimeType));
          break;
      }
      return dtNextScheduledExpiration;
    }

    private static bool NextMatchingMonth(string p_strDesiredMonthOfYear, bool p_bSearchForward, ref DateTime p_dtNextFirePoint)
    {
      int nMaxMatchAttempts = 12; // search max of one years' worth of months
      bool bFoundMonth = true;
      while (p_dtNextFirePoint.ToString("MMM") != p_strDesiredMonthOfYear)
      {
        if (p_bSearchForward)
        {
          p_dtNextFirePoint = p_dtNextFirePoint.AddMonths(1); // add 1 month
        }
        else
        {
          p_dtNextFirePoint = p_dtNextFirePoint.AddMonths(-1); // subtract 1 month
        }
        nMaxMatchAttempts--;
        if (nMaxMatchAttempts == 0)
        {
          bFoundMonth = false;
          break;
        }
      }
      return bFoundMonth;
    }

    private static bool NextMatchingDay(string p_strDesiredDayOfWeek, bool p_bSearchForward, ref DateTime p_dtNextFirePoint)
    {
      int nMaxMatchAttempts = 7; // search max of one weeks' worth of days
      bool bFoundDay = true;
      while (p_dtNextFirePoint.ToString("ddd") != p_strDesiredDayOfWeek)
      {
        if (p_bSearchForward)
        {
          p_dtNextFirePoint = p_dtNextFirePoint.AddDays(1); // add 1 day
        }
        else
        {
          p_dtNextFirePoint = p_dtNextFirePoint.AddDays(-1); // subtract 1 day
        }
        nMaxMatchAttempts--;
        if (nMaxMatchAttempts == 0)
        {
          bFoundDay = false;
          break;
        }
      }
      return bFoundDay;
    }

    private static bool NextMatchingDayType(EsnNthDomDayType p_eNthDOMDayType, bool p_bSearchForward, ref DateTime p_dtNextFirePoint)
    {
      int nMaxMatchAttempts = 7; // search max of one weeks' worth of days
      bool bFoundDay = true;
      while (!DayIsDayType(p_dtNextFirePoint.ToString("ddd"), p_eNthDOMDayType))
      {
        if (p_bSearchForward)
        {
          p_dtNextFirePoint = p_dtNextFirePoint.AddDays(1); // add 1 day
        }
        else
        {
          p_dtNextFirePoint = p_dtNextFirePoint.AddDays(-1); // subtract 1 day
        }
        nMaxMatchAttempts--;
        if (nMaxMatchAttempts == 0)
        {
          bFoundDay = false;
          break;
        }
      }
      return bFoundDay;
    }

    private static bool NextNthMatchingDay(string p_strDesiredDayOfWeek, bool p_bSearchForward, int p_nNthDay, ref DateTime p_dtNextFirePoint)
    {
      bool bFoundDay = true;
      int nMaxDaysThisMonth = DateTime.DaysInMonth(p_dtNextFirePoint.Year, p_dtNextFirePoint.Month);
      int nFindCt = 0;
      int nDaysChecked = 0;
      do
      {
        if (p_dtNextFirePoint.ToString("ddd") == p_strDesiredDayOfWeek)
        {
          nFindCt++;
        }
        if (nFindCt < p_nNthDay)
        {
          if (p_bSearchForward)
          {
            p_dtNextFirePoint = p_dtNextFirePoint.AddDays(1); // add 1 day
          }
          else
          {
            p_dtNextFirePoint = p_dtNextFirePoint.AddDays(-1); // subtract 1 day
          }
          nDaysChecked++;
          if (nDaysChecked >= nMaxDaysThisMonth)
          {
            bFoundDay = false;
            break;
          }
        }
      } while (p_dtNextFirePoint.ToString("ddd") != p_strDesiredDayOfWeek && nFindCt < p_nNthDay);
      return bFoundDay;
    }

    private static bool NextNthMatchingDayType(EsnNthDomDayType p_eNthDOMDayType, bool p_bSearchForward, int p_nNthDay, ref DateTime p_dtNextFirePoint)
    {
      bool bFoundDay = true;
      int nMaxDaysThisMonth = DateTime.DaysInMonth(p_dtNextFirePoint.Year, p_dtNextFirePoint.Month);
      int nFindCt = 0;
      int nDaysChecked = 0;
      do
      {
        if(DayIsDayType(p_dtNextFirePoint.ToString("ddd"), p_eNthDOMDayType))
        {
          nFindCt++;
        }
        if (nFindCt < p_nNthDay)
        {
          if (p_bSearchForward)
          {
            p_dtNextFirePoint = p_dtNextFirePoint.AddDays(1); // add 1 day
          }
          else
          {
            p_dtNextFirePoint = p_dtNextFirePoint.AddDays(-1); // subtract 1 day
          }
          nDaysChecked++;
          if (nDaysChecked >= nMaxDaysThisMonth)
          {
            bFoundDay = false;
            break;
          }
        }
      } while (!DayIsDayType(p_dtNextFirePoint.ToString("ddd"), p_eNthDOMDayType) && nFindCt < p_nNthDay);
      return bFoundDay;
    }

    private static bool DayIsDayType(string p_strDayName, EsnNthDomDayType p_eDesiredDayType)
    {
      bool bFoundStatus = false;
      switch (p_eDesiredDayType)
      {
        case EsnNthDomDayType.DYT_DAY:
          bFoundStatus = true;
          break;
        case EsnNthDomDayType.DYT_WEEKEND_DAY:
          bFoundStatus = IsWeekDay(p_strDayName) ? false : true;
          break;
        case EsnNthDomDayType.DYT_WEEKDAY:
          bFoundStatus = IsWeekDay(p_strDayName);
          break;
        default:
          Debug.Assert(false, string.Format("[CODE] ERROR p_eDesiredDayType should never be {0}!", p_eDesiredDayType));
          break;
      }
      return bFoundStatus;
    }

    private static bool IsWeekDay(string p_strDayName)
    {
      bool bFoundStatus = false;
      if (p_strDayName.ToLower() != "sun" && p_strDayName.ToLower() != "sat")
      {
        bFoundStatus = true;
      }
      return bFoundStatus;
    }

    private static void ValuesFromNthSpec(string p_strNthDOMSpec, out int p_nNthNbr, out bool p_bIsLast, out string p_strDayPart, out EsnNthDomDayType p_eDayType)
    {
      Debug.Assert(p_strNthDOMSpec.Contains(" "), string.Format("[CODE] Bad NthDOM Spec [{0}]!", p_strNthDOMSpec));
      char[] cSeparatorAr = " \t".ToCharArray(); // split on space/tab
      string[] strFieldAr = p_strNthDOMSpec.Split(cSeparatorAr, StringSplitOptions.RemoveEmptyEntries);
      // do we have precisely two fields??
      Debug.Assert(strFieldAr.Length == 2, string.Format(
            "DateTimeTrigger.ValuesFromNthSpec() - Bad format Nth-Day entry [{0}], should be two fields, but is not!",
            p_strNthDOMSpec));

      // strFieldAr[0] = [1st|2nd|3rd|4th|Last]
      // strFieldAr[1] = [Sun|Mon|Tue|Wed|Thu|Fri|Sat|Day|Weekday|Weekend-Day]

      // retrieve "Last" indicator
      p_bIsLast = (strFieldAr[0].ToLower() == "last") ? true : false;
      // retrieve number from Nth field value (1st = 1, 2nd = 2, etc.)
      p_nNthNbr = (p_bIsLast) ? 0 : int.Parse(strFieldAr[0].Substring(0, 1));
      // retrieve [Sun|Mon|Tue|Wed|Thu|Fri|Sat|Day|Weekday|Weekend-Day]
      p_strDayPart = strFieldAr[1];
      // retrieve day-type indication
      p_eDayType = EsnNthDomDayType.DYT_NOTSET;
      for (int nTypeIdx = 0; nTypeIdx < s_strDayTypeNamesAr.Length; nTypeIdx++)
      {
        if (p_strDayPart.ToLower() == s_strDayTypeNamesAr[nTypeIdx].ToLower())
        {
          switch (nTypeIdx)
          {
            case (int)EsnNthDomDayType.DYT_DAY:
              p_eDayType = EsnNthDomDayType.DYT_DAY;
              break;
            case (int)EsnNthDomDayType.DYT_WEEKDAY:
              p_eDayType = EsnNthDomDayType.DYT_WEEKDAY;
              break;
            case (int)EsnNthDomDayType.DYT_WEEKEND_DAY:
              p_eDayType = EsnNthDomDayType.DYT_WEEKEND_DAY;
              break;
            default:
              Debug.Assert(false, "[CODE] at default entry of case statement... should NOT be able to get here!!!!  Code is broken somewhere...");
              break;
          }
          break;  // have answer, exit loop
        }
      }
    }

    private bool AreSchedulePointSetsTheSame(DateTimeTrigger p_trRHS)
    {
      bool bTestResult = false;
      if (m_lstSchedulePointsAr != null &&
          p_trRHS.m_lstSchedulePointsAr != null)
      {
        if (m_lstSchedulePointsAr.Count ==
            p_trRHS.m_lstSchedulePointsAr.Count)
        {
          if (m_lstSchedulePointsAr.Count == 0)
          {
            bTestResult = true;
          }
          else
          {
            bool bListsAreSame = true;
            for (int nListIdx = 0; nListIdx < m_lstSchedulePointsAr.Count; nListIdx++)
            {
              if (m_lstSchedulePointsAr[nListIdx] !=
                  p_trRHS.m_lstSchedulePointsAr[nListIdx])
              {
                bListsAreSame = false;
                break;
              }
            }
            if (bListsAreSame)
            {
              bTestResult = true;
            }
          }
        }
      }
      return bTestResult;
    }

    /// <summary>
    /// R/O PROPERTY: returns T/F where T means the trigger is using a list of trigger Date/Times? (a schedule)
    /// </summary>
    private bool IsSchedulePointTrigger
    {
      get
      {
        return (DateTimeType == (int)EsnDateType.DT_ONCE ||
                DateTimeType == (int)EsnDateType.DT_REPEATING)
                 ? false
                 : true;
      }
    }

    private bool IsValidScheduleSet(List<string> p_lstSchedulePointsAr)
    {
      // NOTE: is this routine fails to validate a set then it logs to Debug Output so you can tell why without further debugging
      m_bSchedulePointListContainsYearNthDomSpec = false;
      bool bValidStatus;
      switch (DateTimeType)
      {
        case (int)EsnDateType.DT_ONCE:
          bValidStatus = (p_lstSchedulePointsAr.Count == 0 && CounterValue == 1) ? true : false;
          if (SkipInterval > 1)
          {
            Debug.WriteLine(
              string.Format(
                "DateTimeTrigger.IsValidScheduleSet() - DT_ONCE Error SkipInterval({0}) NOT Allowed.  Must be One!",
                SkipInterval));
            bValidStatus = false;
          }
          if (bValidStatus == false)
          {
            Debug.WriteLine(
              string.Format(
                "DateTimeTrigger.IsValidScheduleSet() - DT_ONCE but Count is NOT zero -or- SchedulePoints set is NOT empty"));
          }
          break;
        case (int)EsnDateType.DT_REPEATING:
          bValidStatus = (p_lstSchedulePointsAr.Count == 0) ? true : false;
          if (bValidStatus == false)
          {
            Debug.WriteLine(
              string.Format(
                "DateTimeTrigger.IsValidScheduleSet() - DT_REPEATING but SchedulePoints set is NOT empty"));
          }
          if (SkipInterval > 1)
          {
            Debug.WriteLine(
              string.Format(
                "DateTimeTrigger.IsValidScheduleSet() - DT_REPEATING Error SkipInterval({0}) NOT Allowed.  Must be One!",
                SkipInterval));
            bValidStatus = false;
          }
          break;
        case (int)EsnDateType.DT_NTH_DAY_OF_MONTH:
          bValidStatus = (p_lstSchedulePointsAr.Count > 0) ? true : false;
          foreach (string strNthDaySpec in p_lstSchedulePointsAr)
          {
            if (!IsValidNthDomEntry(strNthDaySpec))
            {
              bValidStatus = false;
              break;
            }
          }
          break;
        case (int)EsnDateType.DT_MONTH_OF_YEAR:
          bValidStatus = (p_lstSchedulePointsAr.Count > 0) ? true : false;
          // for each month name in list, make sure it's a valid month name abbrev!
          foreach (string strPossMonthName in p_lstSchedulePointsAr)
          {
            bool bEntryFoundStatus = false; // preset didn't find this entry

            // validate MonthName entry or NthDayOfMonth entry
            //  if we have NthDayOfMonth entry it must be the only 1!

            // NthDayOfMonth has a ' ' in it so let's see if this is one...
            if (strPossMonthName.Contains(" "))
            {
              // validate NthDayOfMonth entry
              bool bHaveValidNthDomEntry = IsValidNthDomEntry(strPossMonthName);
              if (bHaveValidNthDomEntry)
              {
                // is our first NthDayOfMonth entry?
                if (!m_bSchedulePointListContainsYearNthDomSpec)
                {
                  // we have one... good, move on...
                  m_bSchedulePointListContainsYearNthDomSpec = true;
                  m_strYearNthDomSpec = strPossMonthName; // preserve for scheduler use
                  continue;
                }
                Debug.WriteLine(
                  string.Format(
                    "DateTimeTrigger.IsValidScheduleSet() - Have 2nd NthDomEntry [{0}], ONLY 1 allowed.",
                    strPossMonthName));
                bValidStatus = false;
                break; // this is an INVALID entry, kick it!
              }
              else
              {
                // NOT a ValidNthDomEntry, must have bad format MonthName entry (has space character in it)
                Debug.WriteLine(
                  string.Format(
                    "DateTimeTrigger.IsValidScheduleSet() - Have 2nd NthDomEntry [{0}], ONLY 1 allowed.",
                    strPossMonthName));
                bValidStatus = false;
                break; // this is an INVALID entry, kick it!
              }
            }
            else
            {
              foreach (string strAbbrevMonthName in s_strAbbrevMonthNamesAr)
              {
                // NOT NthDayOfMonth entry, validate MonthName entry
                // NOTE month-name table has final empty-string member, don't allow it to be used...
                if (strAbbrevMonthName != string.Empty &&
                    strAbbrevMonthName.ToLower() == strPossMonthName.ToLower())
                {
                  bEntryFoundStatus = true;
                  break;
                }
              }
            }
            if (!bEntryFoundStatus)
            {
              bValidStatus = false;
              Debug.WriteLine(
                string.Format(
                  "DateTimeTrigger.IsValidScheduleSet() - Have BAD Month Name entry [{0}].", strPossMonthName));
              break;
            }
          }
          break;
        case (int)EsnDateType.DT_HOUR_OF_DAY:
          bValidStatus = (p_lstSchedulePointsAr.Count > 0) ? true : false;
          if (bValidStatus)
          {
            // for each time in list, make sure it's a valid time specification!
            foreach (string strSchedulePoint in p_lstSchedulePointsAr)
            {
              Regex regxHHMM = new Regex(@"^[0-2][0-9]:[0-5][0-9]$"); // 00:00 to 23:59
              Match matchResult = regxHHMM.Match(strSchedulePoint);
              if (!matchResult.Success)
              {
                bValidStatus = false; // ERROR not HH:MM format
                Debug.WriteLine(
                  string.Format("DateTimeTrigger.IsValidScheduleSet() - Bad format HH:MM entry [{0}]", strSchedulePoint));
                break; // we've got our answer, exit loop
              }
              int nHour = int.Parse(strSchedulePoint.Substring(0, 2));
              if (nHour > 23)
              {
                bValidStatus = false; // ERROR not HH:MM format
                Debug.WriteLine(
                  string.Format(
                    "DateTimeTrigger.IsValidScheduleSet() - Bad Hour of HH:MM entry [{0}], > 23!", strSchedulePoint));
                break; // we've got our answer, exit loop
              }
            }
          }
          if (SkipInterval > 1)
          {
            Debug.WriteLine(
              string.Format(
                "DateTimeTrigger.IsValidScheduleSet() - DT_HOUR_OF_DAY Error SkipInterval({0}) NOT Allowed.  Must be One!",
                SkipInterval));
            bValidStatus = false;
          }
          break;
        case (int)EsnDateType.DT_DAY_OF_WEEK:
          bValidStatus = (p_lstSchedulePointsAr.Count > 0) ? true : false;
          // for each day name in list, make sure it's a valid day name abbrev!
          foreach (string strPossDayAbbrev in p_lstSchedulePointsAr)
          {
            bool bEntryFoundStatus = false; // preset didn't find this entry
            foreach (string strAbbrevDayName in s_strAbbrevDayNamesAr)
            {
              if (strAbbrevDayName.ToLower() ==
                  strPossDayAbbrev.ToLower())
              {
                bEntryFoundStatus = true;
                break;
              }
            }
            if (!bEntryFoundStatus)
            {
              Debug.WriteLine(
                string.Format(
                  "DateTimeTrigger.IsValidScheduleSet() - Bad Day of Week entry [{0}]!", strPossDayAbbrev));
              bValidStatus = false;
              break;
            }
          }
          break;
        case (int)EsnDateType.DT_DAY_OF_MONTH:
          bValidStatus = (p_lstSchedulePointsAr.Count > 0) ? true : false;
          // for each day in list, make sure it's a valid day-within-month specification!
          foreach (string strSchedulePoint in p_lstSchedulePointsAr)
          {
            if (strSchedulePoint == null)
            {
              bValidStatus = false; // ERROR day Nbr not valid!
              Debug.WriteLine(
                string.Format(
                  "DateTimeTrigger.IsValidScheduleSet() - EmptyList Entry: Missing Day Nbr [{1}], ![1-{0}]!", 31,
                  strSchedulePoint));
              break; // we've got our answer, exit loop
            }
            else
            {
              Regex regxHHMM = new Regex(@"^[0-9]+$"); // 01-31
              Match matchResult = regxHHMM.Match(strSchedulePoint);
              if (!matchResult.Success)
              {
                bValidStatus = false; // ERROR not [1-31] format
                Debug.WriteLine(
                  string.Format(
                    "DateTimeTrigger.IsValidScheduleSet() - Bad format Day Nbr (within Month) entry [{0}]  ![one or more digits]",
                    strSchedulePoint));
                break; // we've got our answer, exit loop
              }
              int nDayNbr = int.Parse(strSchedulePoint);
              if (nDayNbr < 1 &&
                  nDayNbr > 31)
              {
                bValidStatus = false; // ERROR day Nbr not valid!
                Debug.WriteLine(
                  string.Format(
                    "DateTimeTrigger.IsValidScheduleSet() - Out-of-Range: Day Nbr [{1}], ![1-{0}]!", 31,
                    strSchedulePoint));
                break; // we've got our answer, exit loop
              }
            }
          }
          break;
        default:
          bValidStatus = false;
          Debug.WriteLine(
            string.Format(
              "DateTimeTrigger.IsValidScheduleSet() - BAD CASE: DateTimeType Value={0} is NOT Known/Supported!",
              DateTimeType));
          break;
      }

      // Update the valid property
      IsValid = bValidStatus;

      return bValidStatus;
    }

    /// <summary>
    /// R/O PROPERTY: returns T/F where T means the trigger type supports using a skip interval
    /// </summary>
    private bool IsSkipSupportingTrigger
    {
      get
      {
        return (DateTimeType == (int)EsnDateType.DT_MONTH_OF_YEAR ||
                DateTimeType == (int)EsnDateType.DT_DAY_OF_MONTH ||
                DateTimeType == (int)EsnDateType.DT_NTH_DAY_OF_MONTH ||
                DateTimeType == (int)EsnDateType.DT_DAY_OF_WEEK)
                 ? true
                 : false;
      }
    }

    /// <summary>
    /// R/O PROPERTY: is trigger using simple TimeSpan?
    /// </summary>
    private bool IsSimpleIntervalTrigger
    {
      get { return (DateTimeType == (int)EsnDateType.DT_REPEATING) ? true : false; }
    }

    /// <summary>
    /// determine if this string contains a valid Nth Day-of-Month entry
    /// </summary>
    /// <param name="p_strPossNthDomEntry">the string in question</param>
    /// <returns>t/f where T means the entry is valid</returns>
    private static bool IsValidNthDomEntry(string p_strPossNthDomEntry)
    {
      bool bValidStatus = true;
      char[] cSeparatorAr = " \t".ToCharArray(); // split on space/tab
      string[] strFieldAr = p_strPossNthDomEntry.Split(cSeparatorAr, StringSplitOptions.RemoveEmptyEntries);
      // do we have precisely two fields??
      if (strFieldAr.Length != 2)
      {
        // not two fields, bad format
        Debug.WriteLine(
          string.Format(
            "DateTimeTrigger.IsValidScheduleSet() - Bad format Nth-Day entry [{0}], should be two fields, but is not!",
            p_strPossNthDomEntry));
        bValidStatus = false;
      }
      bool bEntryFoundStatus = false; // preset ERROR, not found
      // is first field a recognized Nth form? [1st|2nd|3rd|4th|Last]
      foreach (string strNthName in s_strNthNamesAr)
      {
        if (strFieldAr[0] == strNthName)
        {
          bEntryFoundStatus = true;
          break;
        }
      }
      if (!bEntryFoundStatus)
      {
        // no, not a recognized form!!
        Debug.WriteLine(
          string.Format(
            "DateTimeTrigger.IsValidScheduleSet() - Bad format Nth part of 'Nth Day' entry [{0}]", p_strPossNthDomEntry));
        bValidStatus = false;
      }

      bEntryFoundStatus = false; // preset didn't find this entry
      // is this 2nd field a day-of-week name?
      foreach (string strAbbrevDayName in s_strAbbrevDayNamesAr)
      {
        if (strAbbrevDayName.ToLower() ==
            strFieldAr[1].ToLower())
        {
          bEntryFoundStatus = true;
          break;
        }
      }
      if (!bEntryFoundStatus)
      {
        // Not a day-of-week name, is 2nd field a day-type name?
        foreach (string strDayTypeName in s_strDayTypeNamesAr)
        {
          if (strDayTypeName.ToLower() ==
              strFieldAr[1].ToLower())
          {
            bEntryFoundStatus = true;
            break;
          }
        }
      }
      if (!bEntryFoundStatus)
      {
        // 2nd field is neither form of name!!!
        bValidStatus = false;
        Debug.WriteLine(
          string.Format(
            "DateTimeTrigger.IsValidScheduleSet() - Bad format Day Name part of 'Nth Day' entry [{0}]",
            p_strPossNthDomEntry));
      }
      return bValidStatus;
    }

    /// <summary>
    /// INHERITED: call CountInstace after recording the new value as this
    /// routine will evaluate that trigger conditions have been met
    /// </summary>
    protected override void ConditionallyCountInstance()
    {
        bool bValueMatched = HaveValueMatch();
        if (bValueMatched)
        {
          DidFire = true;
          NbrPriorFirings++;
          UpdateFirePoint();
        }
    }

    #endregion

    #region Test Routines - Differnt trigger type being tried...

    /// <summary>
    /// Quick TEST code 
    /// </summary>
    public static void TestDateTimeTriggerDescriptions()
    {
      // start of tests
      Debug.WriteLine(".\n\n");
      s_bShowTriggerTypeInDescription = true; // enable TEST output

      DateTimeTrigger dtTestTrigger = new DateTimeTrigger();
      dtTestTrigger.Name = "dtTrig Test 1";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_ONCE;
      dtTestTrigger.StartTime = DateTime.Now.AddHours(2);
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test 2";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_REPEATING;
      dtTestTrigger.StartTime = DateTime.Now;
      dtTestTrigger.RepeatInterval = TimeSpan.FromHours(2);
      dtTestTrigger.CounterValue = 2;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test 3";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_REPEATING;
      dtTestTrigger.StartTime = DateTime.Now;
      dtTestTrigger.RepeatInterval = new TimeSpan(0, 6, 5, 0);
      dtTestTrigger.CounterValue = 5;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test 4";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_REPEATING;
      dtTestTrigger.StartTime = DateTime.Now;
      dtTestTrigger.RepeatInterval = new TimeSpan(7, 0, 0, 0);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test 5";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_REPEATING;
      dtTestTrigger.StartTime = DateTime.Now;
      dtTestTrigger.RepeatInterval = new TimeSpan(10, 0, 0, 0);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test 6";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_HOUR_OF_DAY;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      dtTestTrigger.RepeatInterval = new TimeSpan(1, 0, 0, 0);
      string[] strSchedule = new string[2];
      strSchedule[0] = "15:00";
      strSchedule[1] = "18:00";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex01";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_REPEATING;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      dtTestTrigger.RepeatInterval = new TimeSpan(1, 0, 0, 0);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex02";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_DAY_OF_WEEK;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[3];
      strSchedule[0] = "Mon";
      strSchedule[1] = "Wed";
      strSchedule[2] = "Thu";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex03";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_REPEATING;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 6, 0, 0);
      dtTestTrigger.RepeatInterval = new TimeSpan(4, 0, 0, 0);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex04";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_REPEATING;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      dtTestTrigger.RepeatInterval = new TimeSpan(4, 0, 0, 0);
      dtTestTrigger.CounterValue = 25;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Name = "dtTrig Test Ex05";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_REPEATING;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      dtTestTrigger.RepeatInterval = new TimeSpan(14, 0, 0, 0);
      dtTestTrigger.CounterValue = 12;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex06";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_DAY_OF_MONTH;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[1];
      strSchedule[0] = "12";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex07";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_DAY_OF_MONTH;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      dtTestTrigger.SkipInterval = 2;
      strSchedule = new string[2];
      strSchedule[0] = "1";
      strSchedule[1] = "31";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex08";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_NTH_DAY_OF_MONTH;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[1];
      strSchedule[0] = "Last Weekday";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex09";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_NTH_DAY_OF_MONTH;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[1];
      strSchedule[0] = "Last Weekend-Day";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex10";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_NTH_DAY_OF_MONTH;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[1];
      strSchedule[0] = "2nd Weekend-Day";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex11";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_MONTH_OF_YEAR;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[3];
      strSchedule[0] = "Jan";
      strSchedule[1] = "Feb";
      strSchedule[2] = "2nd Mon";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex12";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_HOUR_OF_DAY;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      dtTestTrigger.RepeatInterval = new TimeSpan(1, 0, 0, 0);
      strSchedule = new string[2];
      strSchedule[0] = "15:00";
      strSchedule[1] = "18:00";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 10;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex13a";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_DAY_OF_WEEK;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[2];
      strSchedule[0] = "Mon";
      strSchedule[1] = "Fri";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex13b";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_DAY_OF_WEEK;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 15, 0, 0);
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex14";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_MONTH_OF_YEAR;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[3];
      strSchedule[0] = "Jan";
      strSchedule[1] = "Jul";
      strSchedule[2] = "1st Thu";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 0;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex15";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_NTH_DAY_OF_MONTH;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[1];
      strSchedule[0] = "Last Weekday";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 6;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex16";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_NTH_DAY_OF_MONTH;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[1];
      strSchedule[0] = "Last Weekend-Day";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 12;
      TestShowTrigger(dtTestTrigger);

      dtTestTrigger.Clear();
      dtTestTrigger.Name = "dtTrig Test Ex17";
      dtTestTrigger.DateTimeType = (int)EsnDateType.DT_NTH_DAY_OF_MONTH;
      dtTestTrigger.StartTime = new DateTime(2010, 2, 3, 18, 0, 0);
      strSchedule = new string[1];
      strSchedule[0] = "Last Day";
      dtTestTrigger.SchedulePoints = new List<string>(strSchedule);
      dtTestTrigger.CounterValue = 6;
      TestShowTrigger(dtTestTrigger);

      // end of tests
      Debug.WriteLine(String.Format("* ------ ----- -----"));
      Debug.WriteLine(String.Empty);

      s_bShowTriggerTypeInDescription = false; // disable TEST output
    }

    private static void TestShowTrigger(DateTimeTrigger p_dtTestTrigger)
    {
      Debug.WriteLine(String.Format("* ------ {0} -----", p_dtTestTrigger.Name));
      Debug.WriteLine(
        String.Format("   Descr: {0}", p_dtTestTrigger.DescriptionWithAnnotation(NO_FIELD_ANNOTATION_PARM)));
      Debug.WriteLine(
        String.Format(" AnDescr: {0}", p_dtTestTrigger.DescriptionWithAnnotation(WITH_FIELD_ANNOTATION_PARM)));
      Debug.WriteLine(String.Empty);
    }

    #endregion
  }
}