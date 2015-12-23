using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SyNet.Events.Triggers;

namespace SyNet.GuiControls.DateRepeat
{
  /// <summary>
  /// Interaction logic for DateRepeatControl.xaml
  /// </summary>
  public partial class DateRepeatControl : UserControl
  {
    /// <summary>
    ///   Represents the frequency combobox values
    /// </summary>
    private enum EsnFrequencyValues
    {
      None = 0,
      Hourly,
      Daily,
      Weekly,
      Monthly,
      Yearly
    }
    private DateTimeTrigger m_trigger = null;

    public DateRepeatControl()
    {
      DataContext = null;

      InitializeComponent();

      DataContextChanged += DateRepeatControl_DataContextChanged;

      x_frequencyComboBox.SelectedIndex = 0;
    }

    /// <summary>
    ///   DataContextChanged handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DateRepeatControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      m_trigger = this.DataContext as DateTimeTrigger;

      BuildControlFromTrigger();
    }

    /// <summary>
    ///   Set all the parameters based on the DateTimeTrigger
    /// </summary>
    private void BuildControlFromTrigger()
    {
      if (m_trigger != null)
      {
        //
        // First set the type dropdown
        //
        switch (m_trigger.DateTimeType)
        {
          case (int)DateTimeTrigger.EsnDateType.DT_ONCE:
            x_frequencyComboBox.SelectedIndex = (int)EsnFrequencyValues.None;
            break;
          case (int)DateTimeTrigger.EsnDateType.DT_REPEATING:
            CalculateRepeatingFrequency();
            break;
          case (int)DateTimeTrigger.EsnDateType.DT_HOUR_OF_DAY:
            x_frequencyComboBox.SelectedIndex = (int)EsnFrequencyValues.Hourly;
            BuildHourlyControlFromTrigger();
            break;
          case (int)DateTimeTrigger.EsnDateType.DT_DAY_OF_WEEK:
            x_frequencyComboBox.SelectedIndex = (int)EsnFrequencyValues.Weekly;
            BuildWeeklyControlFromTrigger();
            break;
          case (int)DateTimeTrigger.EsnDateType.DT_DAY_OF_MONTH:
          case (int)DateTimeTrigger.EsnDateType.DT_NTH_DAY_OF_MONTH:
            x_frequencyComboBox.SelectedIndex = (int)EsnFrequencyValues.Monthly;
            BuildMonthlyControlFromTrigger();
            break;
          case (int)DateTimeTrigger.EsnDateType.DT_MONTH_OF_YEAR:
            x_frequencyComboBox.SelectedIndex = (int)EsnFrequencyValues.Yearly;
            BuildYearlyControlFromTrigger();
            break;
        }
      }
    }

    private void BuildYearlyControlFromTrigger()
    {
      x_yearlySkipTextBox.Text = m_trigger.SkipInterval.ToString();

      foreach (string point in m_trigger.SchedulePoints)
      {
        switch (point)
        {
          case "Jan": x_yearlyJan.IsChecked = true; break;
          case "Feb": x_yearlyFeb.IsChecked = true; break;
          case "Mar": x_yearlyMar.IsChecked = true; break;
          case "Apr": x_yearlyApr.IsChecked = true; break;
          case "May": x_yearlyMay.IsChecked = true; break;
          case "Jun": x_yearlyJun.IsChecked = true; break;
          case "Jul": x_yearlyJul.IsChecked = true; break;
          case "Aug": x_yearlyAug.IsChecked = true; break;
          case "Sep": x_yearlySep.IsChecked = true; break;
          case "Oct": x_yearlyOct.IsChecked = true; break;
          case "Nov": x_yearlyNov.IsChecked = true; break;
          case "Dec": x_yearlyDec.IsChecked = true; break;
        }
      }
    }

    /// <summary>
    ///   Build the monthly control
    /// </summary>
    private void BuildMonthlyControlFromTrigger()
    {
      if (m_trigger.DateTimeType == (int)DateTimeTrigger.EsnDateType.DT_DAY_OF_MONTH)
      {
        x_monthlyEachRadioButton.IsChecked = true;

        foreach (string point in m_trigger.SchedulePoints)
        {
          switch (point)
          {
            case "1":
              x_monthlyDay1.IsChecked = true;
              break;
            case "2":
              x_monthlyDay2.IsChecked = true;
              break;
            case "3":
              x_monthlyDay3.IsChecked = true;
              break;
            case "4":
              x_monthlyDay4.IsChecked = true;
              break;
            case "5":
              x_monthlyDay5.IsChecked = true;
              break;
            case "6":
              x_monthlyDay6.IsChecked = true;
              break;
            case "7":
              x_monthlyDay7.IsChecked = true;
              break;
            case "8":
              x_monthlyDay8.IsChecked = true;
              break;
            case "9":
              x_monthlyDay9.IsChecked = true;
              break;
            case "10":
              x_monthlyDay10.IsChecked = true;
              break;
            case "11":
              x_monthlyDay11.IsChecked = true;
              break;
            case "12":
              x_monthlyDay12.IsChecked = true;
              break;
            case "13":
              x_monthlyDay13.IsChecked = true;
              break;
            case "14":
              x_monthlyDay14.IsChecked = true;
              break;
            case "15":
              x_monthlyDay15.IsChecked = true;
              break;
            case "16":
              x_monthlyDay16.IsChecked = true;
              break;
            case "17":
              x_monthlyDay17.IsChecked = true;
              break;
            case "18":
              x_monthlyDay18.IsChecked = true;
              break;
            case "19":
              x_monthlyDay19.IsChecked = true;
              break;
            case "20":
              x_monthlyDay20.IsChecked = true;
              break;
            case "21":
              x_monthlyDay21.IsChecked = true;
              break;
            case "22":
              x_monthlyDay22.IsChecked = true;
              break;
            case "23":
              x_monthlyDay23.IsChecked = true;
              break;
            case "24":
              x_monthlyDay24.IsChecked = true;
              break;
            case "25":
              x_monthlyDay25.IsChecked = true;
              break;
            case "26":
              x_monthlyDay26.IsChecked = true;
              break;
            case "27":
              x_monthlyDay27.IsChecked = true;
              break;
            case "28":
              x_monthlyDay28.IsChecked = true;
              break;
            case "29":
              x_monthlyDay29.IsChecked = true;
              break;
            case "30":
              x_monthlyDay30.IsChecked = true;
              break;
            case "31":
              x_monthlyDay31.IsChecked = true;
              break;
          }
        }

      }
      if (m_trigger.DateTimeType == (int)DateTimeTrigger.EsnDateType.DT_NTH_DAY_OF_MONTH)
      {
        x_monthlyOnRadioButton.IsChecked = true;

        if (m_trigger.SchedulePoints.Count > 0)
        {
          string point = m_trigger.SchedulePoints[0];
          string[] parts = point.Split(' ');
          if (parts.Length == 2)
          {
            switch (parts[0])
            {
              case "1st":
                x_monthlyOnIntervalComboBox.SelectedIndex = 0;
                break;
              case "2nd":
                x_monthlyOnIntervalComboBox.SelectedIndex = 1;
                break;
              case "3rd":
                x_monthlyOnIntervalComboBox.SelectedIndex = 2;
                break;
              case "4th":
                x_monthlyOnIntervalComboBox.SelectedIndex = 3;
                break;
              case "Last":
                x_monthlyOnIntervalComboBox.SelectedIndex = 5;
                break;
            }

            switch (parts[1])
            {
              case "Sun":
                x_monthlyOnDayComboBox.SelectedIndex = 0;
                break;
              case "Mon":
                x_monthlyOnDayComboBox.SelectedIndex = 1;
                break;
              case "Tue":
                x_monthlyOnDayComboBox.SelectedIndex = 2;
                break;
              case "Wed":
                x_monthlyOnDayComboBox.SelectedIndex = 3;
                break;
              case "Thu":
                x_monthlyOnDayComboBox.SelectedIndex = 4;
                break;
              case "Fri":
                x_monthlyOnDayComboBox.SelectedIndex = 5;
                break;
              case "Sat":
                x_monthlyOnDayComboBox.SelectedIndex = 6;
                break;
              case "Day":
                x_monthlyOnDayComboBox.SelectedIndex = 8;
                break;
              case "Weekday":
                x_monthlyOnDayComboBox.SelectedIndex = 9;
                break;
              case "Weekend-Day":
                x_monthlyOnDayComboBox.SelectedIndex = 10;
                break;
            }

          }
        }
      }
      x_monthlyRepeatTextBox.Text = m_trigger.SkipInterval.ToString();
    }

    /// <summary>
    ///   Setup the hourly control
    /// </summary>
    private void BuildHourlyControlFromTrigger()
    {
      // Populate the hour list
      if (m_trigger.DateTimeType == (int)DateTimeTrigger.EsnDateType.DT_HOUR_OF_DAY)
      {
        x_hourlyTimesRadioButton.IsChecked = true;

        // Clear the current list
        x_hourlyTimeListbox.Items.Clear();

        foreach (string point in m_trigger.SchedulePoints)
        {
          // Convert HH:MM into a datetime
          if (point.Length == 5 && point[2] == ':')
          {
            int hour = Int32.Parse(point.Substring(0, 2));
            int min = Int32.Parse(point.Substring(3, 2));

            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 0);

            ListBoxItem item = new ListBoxItem();
            item.Tag = dt;
            item.Content = dt.ToString("hh:mm tt");

            x_hourlyTimeListbox.Items.Add(item);
          }
        }
      }

      // Populate the repeating lists
      if (m_trigger.DateTimeType == (int)DateTimeTrigger.EsnDateType.DT_REPEATING)
      {
        // If we're less than an hour, we're in minutes
        if (m_trigger.RepeatInterval < TimeSpan.FromHours(1.0))
        {
          x_hourlyMinutesRadioButton.IsChecked = true;
          x_hourlyMinutesTextBox.Text = m_trigger.RepeatInterval.TotalMinutes.ToString();
        }
        else
        {
          x_hourlyHoursRadioButton.IsChecked = true;
          x_hourlyHoursTextBox.Text = m_trigger.RepeatInterval.TotalHours.ToString();
        }
      }
    }

    private void BuildDailyControlFromTrigger()
    {
      x_dailyTextBox.Text = m_trigger.RepeatInterval.TotalDays.ToString();
    }

    private void BuildWeeklyControlFromTrigger()
    {
      // Populate the days
      if (m_trigger.DateTimeType == (int)DateTimeTrigger.EsnDateType.DT_DAY_OF_WEEK)
      {
        foreach (string point in m_trigger.SchedulePoints)
        {
          switch (point)
          {
            case "Sun":
              x_weeklyDaySun.IsChecked = true;
              break;
            case "Mon":
              x_weeklyDayMon.IsChecked = true;
              break;
            case "Tue":
              x_weeklyDayTue.IsChecked = true;
              break;
            case "Wed":
              x_weeklyDayWed.IsChecked = true;
              break;
            case "Thu":
              x_weeklyDayThu.IsChecked = true;
              break;
            case "Fri":
              x_weeklyDayFri.IsChecked = true;
              break;
            case "Sat":
              x_weeklyDaySat.IsChecked = true;
              break;
          }
        }
        // Fill skip box
        x_weeklyRepeatTextBox.Text = m_trigger.SkipInterval.ToString();
      }
      else
      {
        // Fill repeating box
        x_weeklyRepeatTextBox.Text = (m_trigger.RepeatInterval.TotalDays / 7.0).ToString();
      }
    }

    private void CalculateRepeatingFrequency()
    {
      // If repeating frequency is less than a day, we're hourly
      if (m_trigger.RepeatInterval < TimeSpan.FromDays(1.0))
      {
        x_frequencyComboBox.SelectedIndex = (int)EsnFrequencyValues.Hourly;
        BuildHourlyControlFromTrigger();
      }
      // If its a week increment
      else if (m_trigger.RepeatInterval.TotalDays % 7.0 == 0.0)
      {
        x_frequencyComboBox.SelectedIndex = (int)EsnFrequencyValues.Weekly;
        BuildWeeklyControlFromTrigger();
      }
      // See if it is in days
      else if (m_trigger.RepeatInterval.TotalHours % 24.0 == 0.0)
      {
        x_frequencyComboBox.SelectedIndex = (int)EsnFrequencyValues.Daily;
        BuildDailyControlFromTrigger();
      }
      else
      {
        x_frequencyComboBox.SelectedIndex = (int)EsnFrequencyValues.Hourly;
        BuildHourlyControlFromTrigger();
      }
    }

    private void Frequency_SelectionChanged(object p_sender, SelectionChangedEventArgs p_e)
    {
      HideAll();
      switch (x_frequencyComboBox.SelectedIndex)
      {
        case (int)EsnFrequencyValues.Hourly: // hourly
          {
            x_hourlyPanel.Visibility = Visibility.Visible;
          }
          break;
        case (int)EsnFrequencyValues.Daily: // daily
          {
            x_dailyPanel.Visibility = Visibility.Visible;
          }
          break;
        case (int)EsnFrequencyValues.Weekly: // weekly
          {
            x_weeklyPanel.Visibility = Visibility.Visible;
          }
          break;
        case (int)EsnFrequencyValues.Monthly: // monthly
          {
            x_monthlyPanel.Visibility = Visibility.Visible;
          }
          break;
        case (int)EsnFrequencyValues.Yearly: // yearly
          {
            x_yearlyPanel.Visibility = Visibility.Visible;
          }
          break;
        default:
          {

          }
          break;
      }
    }

    /// <summary>
    ///   Hide all panels
    /// </summary>
    private void HideAll()
    {
      x_hourlyPanel.Visibility = Visibility.Collapsed;
      x_dailyPanel.Visibility = Visibility.Collapsed;
      x_monthlyPanel.Visibility = Visibility.Collapsed;
      x_weeklyPanel.Visibility = Visibility.Collapsed;
      x_yearlyPanel.Visibility = Visibility.Collapsed;
    }

    private void OKButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      ApplyToTrigger();

      if (OKButtonClick != null)
      {
        OKButtonClick(this, p_e);
      }
    }

    /// <summary>
    ///   Applies the current settings to the trigger
    /// </summary>
    private void ApplyToTrigger()
    {
      if (m_trigger == null) return;


      switch (x_frequencyComboBox.SelectedIndex)
      {
        case (int)EsnFrequencyValues.None:
          ApplyNone();
          break;
        case (int)EsnFrequencyValues.Hourly:
          ApplyHourly();
          break;
        case (int)EsnFrequencyValues.Daily:
          ApplyDaily();
          break;
        case (int)EsnFrequencyValues.Weekly:
          ApplyWeekly();
          break;
        case (int)EsnFrequencyValues.Monthly:
          ApplyMonthly();
          break;
        case (int)EsnFrequencyValues.Yearly:
          ApplyYearly();
          break;
      }

      m_trigger.CounterValue = 0;

      // Debug.WriteLine(m_trigger.Description);   Show Annotated Narrative
      Debug.WriteLine(m_trigger.Narrative);
    }

    /// <summary>
    ///   Apply a non-repeating schedule
    /// </summary>
    private void ApplyNone()
    {
      m_trigger.SchedulePoints.Clear();
      m_trigger.CounterValue = 1;
      m_trigger.DateTimeType = (int) DateTimeTrigger.EsnDateType.DT_ONCE;

      m_trigger.SkipInterval = 0;
    }

    /// <summary>
    ///   Apply yearly
    /// </summary>
    private void ApplyYearly()
    {
      m_trigger.SchedulePoints.Clear();

      m_trigger.DateTimeType = (int)DateTimeTrigger.EsnDateType.DT_MONTH_OF_YEAR;

      m_trigger.SkipInterval = UInt32.Parse(x_yearlySkipTextBox.Text);

      if (x_yearlyJan.IsChecked == true) { m_trigger.SchedulePoints.Add("Jan"); }
      if (x_yearlyFeb.IsChecked == true) { m_trigger.SchedulePoints.Add("Feb"); }
      if (x_yearlyMar.IsChecked == true) { m_trigger.SchedulePoints.Add("Mar"); }
      if (x_yearlyApr.IsChecked == true) { m_trigger.SchedulePoints.Add("Apr"); }
      if (x_yearlyMay.IsChecked == true) { m_trigger.SchedulePoints.Add("May"); }
      if (x_yearlyJun.IsChecked == true) { m_trigger.SchedulePoints.Add("Jun"); }
      if (x_yearlyJul.IsChecked == true) { m_trigger.SchedulePoints.Add("Jul"); }
      if (x_yearlyAug.IsChecked == true) { m_trigger.SchedulePoints.Add("Aug"); }
      if (x_yearlySep.IsChecked == true) { m_trigger.SchedulePoints.Add("Sep"); }
      if (x_yearlyOct.IsChecked == true) { m_trigger.SchedulePoints.Add("Oct"); }
      if (x_yearlyNov.IsChecked == true) { m_trigger.SchedulePoints.Add("Nov"); }
      if (x_yearlyDec.IsChecked == true) { m_trigger.SchedulePoints.Add("Dec"); }
    }

    private void ApplyMonthly()
    {
      m_trigger.SchedulePoints.Clear();

      // Repeat monthly on each of date
      if (x_monthlyEachRadioButton.IsChecked == true)
      {
        m_trigger.DateTimeType = (int)DateTimeTrigger.EsnDateType.DT_DAY_OF_MONTH;

        if (x_monthlyDay1.IsChecked == true) { m_trigger.SchedulePoints.Add("1"); }
        if (x_monthlyDay2.IsChecked == true) { m_trigger.SchedulePoints.Add("2"); }
        if (x_monthlyDay3.IsChecked == true) { m_trigger.SchedulePoints.Add("3"); }
        if (x_monthlyDay4.IsChecked == true) { m_trigger.SchedulePoints.Add("4"); }
        if (x_monthlyDay5.IsChecked == true) { m_trigger.SchedulePoints.Add("5"); }
        if (x_monthlyDay6.IsChecked == true) { m_trigger.SchedulePoints.Add("6"); }
        if (x_monthlyDay7.IsChecked == true) { m_trigger.SchedulePoints.Add("7"); }
        if (x_monthlyDay8.IsChecked == true) { m_trigger.SchedulePoints.Add("8"); }
        if (x_monthlyDay9.IsChecked == true) { m_trigger.SchedulePoints.Add("9"); }
        if (x_monthlyDay10.IsChecked == true) { m_trigger.SchedulePoints.Add("10"); }
        if (x_monthlyDay11.IsChecked == true) { m_trigger.SchedulePoints.Add("11"); }
        if (x_monthlyDay12.IsChecked == true) { m_trigger.SchedulePoints.Add("12"); }
        if (x_monthlyDay13.IsChecked == true) { m_trigger.SchedulePoints.Add("13"); }
        if (x_monthlyDay14.IsChecked == true) { m_trigger.SchedulePoints.Add("14"); }
        if (x_monthlyDay15.IsChecked == true) { m_trigger.SchedulePoints.Add("15"); }
        if (x_monthlyDay16.IsChecked == true) { m_trigger.SchedulePoints.Add("16"); }
        if (x_monthlyDay17.IsChecked == true) { m_trigger.SchedulePoints.Add("17"); }
        if (x_monthlyDay18.IsChecked == true) { m_trigger.SchedulePoints.Add("18"); }
        if (x_monthlyDay19.IsChecked == true) { m_trigger.SchedulePoints.Add("19"); }
        if (x_monthlyDay20.IsChecked == true) { m_trigger.SchedulePoints.Add("20"); }
        if (x_monthlyDay21.IsChecked == true) { m_trigger.SchedulePoints.Add("21"); }
        if (x_monthlyDay22.IsChecked == true) { m_trigger.SchedulePoints.Add("22"); }
        if (x_monthlyDay23.IsChecked == true) { m_trigger.SchedulePoints.Add("23"); }
        if (x_monthlyDay24.IsChecked == true) { m_trigger.SchedulePoints.Add("24"); }
        if (x_monthlyDay25.IsChecked == true) { m_trigger.SchedulePoints.Add("25"); }
        if (x_monthlyDay26.IsChecked == true) { m_trigger.SchedulePoints.Add("26"); }
        if (x_monthlyDay27.IsChecked == true) { m_trigger.SchedulePoints.Add("27"); }
        if (x_monthlyDay28.IsChecked == true) { m_trigger.SchedulePoints.Add("28"); }
        if (x_monthlyDay29.IsChecked == true) { m_trigger.SchedulePoints.Add("29"); }
        if (x_monthlyDay30.IsChecked == true) { m_trigger.SchedulePoints.Add("30"); }
        if (x_monthlyDay31.IsChecked == true) { m_trigger.SchedulePoints.Add("31"); }
      }
      else
      {
        m_trigger.DateTimeType = (int)DateTimeTrigger.EsnDateType.DT_NTH_DAY_OF_MONTH;

        StringBuilder sb = new StringBuilder();

        switch (x_monthlyOnIntervalComboBox.SelectedIndex)
        {
          case 0:
            sb.Append("1st");
            break;
          case 1:
            sb.Append("2nd");
            break;
          case 2:
            sb.Append("3rd");
            break;
          case 3:
            sb.Append("4th");
            break;
          case 5:
            sb.Append("Last");
            break;
        }
        sb.Append(" ");


        switch (x_monthlyOnDayComboBox.SelectedIndex)
        {
          case 0:
            sb.Append("Sun");
            break;
          case 1:
            sb.Append("Mon");
            break;
          case 2:
            sb.Append("Tue");
            break;
          case 3:
            sb.Append("Wed");
            break;
          case 4:
            sb.Append("Thu");
            break;
          case 5:
            sb.Append("Fri");
            break;
          case 6:
            sb.Append("Sat");
            break;
          case 8:
            sb.Append("Day");
            break;
          case 9:
            sb.Append("Weekday");
            break;
          case 10:
            sb.Append("Weekend-Day");
            break;
        }

        m_trigger.SchedulePoints.Add(sb.ToString());
      }

      m_trigger.SkipInterval = UInt32.Parse(x_monthlyRepeatTextBox.Text);
    }

    private void ApplyWeekly()
    {
      m_trigger.SchedulePoints.Clear();

      //
      // See if any days are checked
      //
      if (
        x_weeklyDaySun.IsChecked == true ||
        x_weeklyDayMon.IsChecked == true ||
        x_weeklyDayTue.IsChecked == true ||
        x_weeklyDayWed.IsChecked == true ||
        x_weeklyDayThu.IsChecked == true ||
        x_weeklyDayFri.IsChecked == true ||
        x_weeklyDaySat.IsChecked == true
        )
      {
        // We are weekly scheduled days
        m_trigger.DateTimeType = (int)DateTimeTrigger.EsnDateType.DT_DAY_OF_WEEK;

        if (x_weeklyDaySun.IsChecked == true) { m_trigger.SchedulePoints.Add("Sun"); }
        if (x_weeklyDayMon.IsChecked == true) { m_trigger.SchedulePoints.Add("Mon"); }
        if (x_weeklyDayTue.IsChecked == true) { m_trigger.SchedulePoints.Add("Tue"); }
        if (x_weeklyDayWed.IsChecked == true) { m_trigger.SchedulePoints.Add("Wed"); }
        if (x_weeklyDayThu.IsChecked == true) { m_trigger.SchedulePoints.Add("Thu"); }
        if (x_weeklyDayFri.IsChecked == true) { m_trigger.SchedulePoints.Add("Fri"); }
        if (x_weeklyDaySat.IsChecked == true) { m_trigger.SchedulePoints.Add("Sat"); }

        m_trigger.SkipInterval = UInt32.Parse(x_weeklyRepeatTextBox.Text);
      }
      else
      {
        // We are just weekly repeating
        m_trigger.DateTimeType = (int)DateTimeTrigger.EsnDateType.DT_REPEATING;
        m_trigger.SkipInterval = 1;
        m_trigger.RepeatInterval = TimeSpan.FromDays(UInt32.Parse(x_weeklyRepeatTextBox.Text) * 7.0);
      }
    }

    private void ApplyDaily()
    {
      m_trigger.SchedulePoints.Clear();

      m_trigger.RepeatInterval = TimeSpan.FromDays(Int32.Parse(x_dailyTextBox.Text));
    }

    private void ApplyHourly()
    {
      m_trigger.SchedulePoints.Clear();

      //
      // Every x minutes
      //
      if (x_hourlyMinutesRadioButton.IsChecked == true)
      {
        m_trigger.DateTimeType = (int)DateTimeTrigger.EsnDateType.DT_REPEATING;
        // Parse the text into hours
        double hours = Double.Parse(x_hourlyMinutesTextBox.Text);
        // Set the interval
        m_trigger.RepeatInterval = TimeSpan.FromMinutes(hours);
      }
      //
      // Every x hours
      //
      if (x_hourlyHoursRadioButton.IsChecked == true)
      {
        m_trigger.DateTimeType = (int)DateTimeTrigger.EsnDateType.DT_REPEATING;
        // Parse the text into hours
        double hours = Double.Parse(x_hourlyHoursTextBox.Text);
        // Set the interval
        m_trigger.RepeatInterval = TimeSpan.FromHours(hours);
      }

      //
      // Hour list
      //
      if (x_hourlyTimesRadioButton.IsChecked == true)
      {
        m_trigger.DateTimeType = (int)DateTimeTrigger.EsnDateType.DT_HOUR_OF_DAY;

        // Add from the list to the trigger
        foreach (ListBoxItem item in x_hourlyTimeListbox.Items)
        {
          DateTime time = (DateTime)item.Tag;
          m_trigger.SchedulePoints.Add(time.ToString("HH:mm"));
        }
      }
    }

    private void CancelButton_Click(object p_sender, RoutedEventArgs p_e)
    {
    }

    /// <summary>
    ///   Event for the ok button click
    /// </summary>
    public event RoutedEventHandler OKButtonClick;

    /// <summary>
    ///   Event for the cancel button click
    /// </summary>
    public event RoutedEventHandler CancelButtonClick;

    /// <summary>
    ///   
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void RadioHourlyMinutes_Checked(object p_sender, RoutedEventArgs p_e)
    {
      if (x_hourlyMinutesTextBox != null)
      {
        x_hourlyMinutesTextBox.IsEnabled = (bool)x_hourlyMinutesRadioButton.IsChecked;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void RadioHourlyHours_Checked(object p_sender, RoutedEventArgs p_e)
    {
      if (x_hourlyHoursTextBox != null)
      {
        x_hourlyHoursTextBox.IsEnabled = (bool)x_hourlyHoursRadioButton.IsChecked;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void RadioHourlyTimes_Checked(object p_sender, RoutedEventArgs p_e)
    {
      if (x_hourlyTimeListbox != null && x_hourlyTimeTextBox != null &&
          x_hourlyTimesAddButton != null)
      {
        x_hourlyTimeTextBox.IsEnabled = (bool)x_hourlyTimesRadioButton.IsChecked;
        x_hourlyTimeListbox.IsEnabled = (bool)x_hourlyTimesRadioButton.IsChecked;
        x_hourlyTimesAddButton.IsEnabled = (bool)x_hourlyTimesRadioButton.IsChecked;
      }
    }

    private void HourlyTimeAdd_Click(object p_sender, RoutedEventArgs p_e)
    {
      ListBoxItem item = new ListBoxItem();
      item.Tag = x_hourlyTimeTextBox.DateTime;
      item.Content = x_hourlyTimeTextBox.DateTime.ToString("hh:mm tt");
      x_hourlyTimeListbox.Items.Add(item);
    }

    /// <summary>
    ///   Deletes the item
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void ListBoxDelete_MouseDown(object p_sender, MouseButtonEventArgs p_e)
    {
      Image deleteImage = p_sender as Image;

      if (deleteImage != null)
      {
        ListBoxItem item = GetDependencyObjectFromVisualTree(deleteImage, typeof(ListBoxItem)) as ListBoxItem;
        if (item != null)
        {
          x_hourlyTimeListbox.Items.Remove(item);
        }
      }
    }

    /// <summary>
    /// Walk visual tree to find the first DependencyObject of the specific type.
    /// </summary>
    private DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
    {
      //Iterate the visual tree to get the parent(ItemsControl) of this control
      DependencyObject parent = startObject;
      while (parent != null)
      {
        if (type.IsInstanceOfType(parent))
          break;
        else
          parent = VisualTreeHelper.GetParent(parent);
      }

      return parent;
    }

    private void RadioButtonMonthlyOn_Checked(object p_sender, RoutedEventArgs p_e)
    {
      x_monthlyOnDayComboBox.IsEnabled = (bool)x_monthlyOnRadioButton.IsChecked;
      x_monthlyOnIntervalComboBox.IsEnabled = (bool)x_monthlyOnRadioButton.IsChecked;
    }

    private void RadioButtonMonthlyEach_Checked(object p_sender, RoutedEventArgs p_e)
    {
      if (x_monthlyDay1 == null) return;

      x_monthlyDay1.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay2.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay3.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay4.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay5.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay6.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay7.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay8.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay9.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay10.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay11.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay12.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay13.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay14.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay15.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay16.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay17.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay18.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay19.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay20.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay21.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay22.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay23.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay24.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay25.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay26.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay27.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay28.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay29.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay30.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
      x_monthlyDay31.IsEnabled = (bool)x_monthlyEachRadioButton.IsChecked;
    }
  }
}