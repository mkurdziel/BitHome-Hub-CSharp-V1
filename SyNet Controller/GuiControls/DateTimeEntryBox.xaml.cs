using System;
using System.Diagnostics;
using System.Drawing;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Drawing.Color;
using SystemColors = System.Drawing.SystemColors;

namespace SyNet.GuiControls
{
  /// <summary>
  /// Interaction logic for TimeEntryTextBox.xaml
  /// </summary>
  public partial class DateTimeEntryBox : UserControl
  {
    public enum EsnDateTimeType
    {
      DateTime,
      Date,
      Time
    }
    private DateTime m_dateTime = DateTime.Now;
    private TextBlock m_selected = null;
    private EsnDateTimeType m_controlType;
    private string m_entryString = string.Empty;
    private Timer m_entryTimer = new Timer();
    private bool m_entryInProgress = false;


    /// <summary>
    /// </summary>
    public static readonly DependencyProperty ControlTypeProperty =
      DependencyProperty.Register("ControlType",
                                  typeof(EsnDateTimeType), typeof(DateTimeEntryBox),
                                  new PropertyMetadata(EsnDateTimeType.DateTime, OnControlTypeChanged)
                                  );

    private static void OnControlTypeChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    {
      DateTimeEntryBox box = source as DateTimeEntryBox;

      box.SetupControlType();
    }

    /// <summary>
    ///   Gets or sets the type of this control
    /// </summary>
    public EsnDateTimeType ControlType
    {
      get { return (EsnDateTimeType)GetValue(ControlTypeProperty); }
      set
      {
        SetValue(ControlTypeProperty, value);
      }
    }

    /// <summary>
    ///   Returns the value as a datetime
    /// </summary>
    public DateTime DateTime
    {
      get
      {
        return m_dateTime;
      }
      set
      {
        if (m_dateTime != value)
        {
          m_dateTime = value;
          BuildTextBoxes();
          OnDateTimeChanged();
        }
      }
    }

    /// <summary>
    ///   Set up the control type based on its control type
    /// </summary>
    private void SetupControlType()
    {
      // 
      // Visible date
      //
      System.Windows.Visibility dateVisibility = System.Windows.Visibility.Collapsed;

      if (ControlType == EsnDateTimeType.DateTime ||
          ControlType == EsnDateTimeType.Date)
      {
        dateVisibility = System.Windows.Visibility.Visible;
      }
      x_month.Visibility = dateVisibility;
      x_day.Visibility = dateVisibility;
      x_year.Visibility = dateVisibility;
      x_dash1.Visibility = dateVisibility;
      x_dash2.Visibility = dateVisibility;

      // 
      // Visible time
      //
      System.Windows.Visibility timeVisibility = System.Windows.Visibility.Collapsed;

      if (ControlType == EsnDateTimeType.DateTime ||
          ControlType == EsnDateTimeType.Time)
      {
        timeVisibility = System.Windows.Visibility.Visible;
      }
      x_hour.Visibility = timeVisibility;
      x_minute.Visibility = timeVisibility;
      x_ampm.Visibility = timeVisibility;
      x_timeColon.Visibility = timeVisibility;
      x_timeDivider.Visibility = timeVisibility;

      if (ControlType == EsnDateTimeType.DateTime)
      {
        x_divider.Visibility = System.Windows.Visibility.Visible;
      }
      else
      {
        x_divider.Visibility = System.Windows.Visibility.Collapsed;
      }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public DateTimeEntryBox()
    {
      InitializeComponent();

      //
      // Set up the timer
      //
      m_entryTimer.Interval = 1000;
      m_entryTimer.AutoReset = false;
      m_entryTimer.Elapsed += EntryTimer_Elapsed;

      this.MouseDown += DateTimeEntryBox_MouseDown;
      this.KeyDown += DateTimeEntryBox_KeyDown;

      this.ControlType = EsnDateTimeType.DateTime;
      this.LostFocus += DateTimeEntryBox_LostFocus;
      this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(DateTimeEntryBox_IsEnabledChanged);

      BuildTextBoxes();
    }

    /// <summary>
    ///   Enable changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DateTimeEntryBox_IsEnabledChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      if (this.IsEnabled)
      {
        SetAllForegrounds(System.Windows.SystemColors.ControlTextColor);
      }
      else
      {
        SetAllForegrounds(System.Windows.Media.Color.FromArgb(255, 88, 88, 88));
        
      }
    }

    /// <summary>
    ///   Lost focus event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DateTimeEntryBox_LostFocus( object sender, RoutedEventArgs e )
    {
      if (m_selected != null)
      {
        UnselectTextBlock(m_selected);
      }
    }

    /// <summary>
    ///   Entry timer elapsed handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EntryTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      //
      // Publish the value and then update the text boxes accordingly
      //
      this.Dispatcher.Invoke(
             DispatcherPriority.Normal,
             new System.Action(
               delegate()
               {
                 PublishValue();
                 BuildTextBoxes();
               }
               )
               );
    }

    /// <summary>
    ///   Update the text boxes from the datetime type
    /// </summary>
    private void BuildTextBoxes()
    {
      x_month.Text = m_dateTime.ToString("MM");
      x_day.Text = m_dateTime.ToString("dd");
      x_year.Text = m_dateTime.ToString("yyyy");
      x_hour.Text = m_dateTime.ToString("hh");
      x_minute.Text = m_dateTime.ToString("mm");
      x_ampm.Text = m_dateTime.ToString("tt");
    }

    void DateTimeEntryBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Up:
          IncrementSelected();
          e.Handled = true;
          break;
        case Key.Down:
          DecrementSelected();
          e.Handled = true;
          break;
        case Key.Left:
          MoveLeft();
          e.Handled = true;
          break;
        case Key.Right:
          MoveRight();
          e.Handled = true;
          break;
        case Key.Delete:
        case Key.Back:
          Delete();
          e.Handled = true;
          break;
        case Key.Tab:
          // Shift tab
          if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
          {
            e.Handled = MoveLeft();
            if (!e.Handled)
            {
              UnselectTextBlock(m_selected);
            }
          }
          else
          {
            e.Handled = MoveRight();
            if (!e.Handled)
            {
              UnselectTextBlock(m_selected);
            }
          }
          break;
        //
        // Numbers
        //
        case Key.NumPad0:
        case Key.D0:
          EnterNumber(0);
          break;
        case Key.NumPad1:
        case Key.D1:
          EnterNumber(1);
          break;
        case Key.NumPad2:
        case Key.D2:
          EnterNumber(2);
          break;
        case Key.NumPad3:
        case Key.D3:
          EnterNumber(3);
          break;
        case Key.NumPad4:
        case Key.D4:
          EnterNumber(4);
          break;
        case Key.NumPad5:
        case Key.D5:
          EnterNumber(5);
          break;
        case Key.NumPad6:
        case Key.D6:
          EnterNumber(6);
          break;
        case Key.NumPad7:
        case Key.D7:
          EnterNumber(7);
          break;
        case Key.NumPad8:
        case Key.D8:
          EnterNumber(8);
          break;
        case Key.NumPad9:
        case Key.D9:
          EnterNumber(9);
          break;

        //
        // AM PM
        //
        case Key.A:
        case Key.P:
          EnterAmPM(e.Key);
          break;
        case Key.Enter:
          PublishValue();
          break;
      }
    }

    private void PublishValue()
    {
      // Stop the timer
      m_entryTimer.Stop();
      m_entryInProgress = false;

      // Handle empty publish
      if (m_entryString == "")
      {
        return;
      }

      //
      // Parse to int value. Assume no incorrect strings
      //
      int enteredValue = Int32.Parse(m_entryString);

      //
      // Reset the entered string value
      //
      m_entryString = "";

      try
      {


        //
        // Year
        //
        if (m_selected == x_year)
        {
          m_dateTime = new DateTime(
            enteredValue,
            m_dateTime.Month,
            m_dateTime.Day,
            m_dateTime.Hour,
            m_dateTime.Minute,
            0);
        }
        //
        // Month
        //
        if (m_selected == x_month)
        {
          int numDays = m_dateTime.Day;
          int maxDays = System.DateTime.DaysInMonth(m_dateTime.Year, enteredValue);
          if (numDays > maxDays)
          {
            numDays = maxDays; 
          }

          m_dateTime = new DateTime(
            m_dateTime.Year,
            enteredValue,
            numDays,
            m_dateTime.Hour,
            m_dateTime.Minute,
            0);
        }
        //
        // Day
        //
        if (m_selected == x_day)
        {
          m_dateTime = new DateTime(
            m_dateTime.Year,
            m_dateTime.Month,
            enteredValue,
            m_dateTime.Hour,
            m_dateTime.Minute,
            0);
        }
        //
        // Hour
        //
        if (m_selected == x_hour)
        {
          //
          // If the current hour is PM, adjust
          //
          if (m_dateTime.Hour >= 12 && enteredValue < 12)
          {
            enteredValue += 12;
          }

          m_dateTime = new DateTime(
            m_dateTime.Year,
            m_dateTime.Month,
            m_dateTime.Day,
            enteredValue,
            m_dateTime.Minute,
            0);
        }
        //
        // Minute
        //
        if (m_selected == x_minute)
        {
          m_dateTime = new DateTime(
            m_dateTime.Year,
            m_dateTime.Month,
            m_dateTime.Day,
            m_dateTime.Hour,
            enteredValue,
            0);
        }

        //
        // Notify everyone of the change
        //
        OnDateTimeChanged();
      }
      catch (ArgumentOutOfRangeException)
      {
        Debug.WriteLine("[ERR] DateTimeEntryBox - DateTime Argument Error");
      }
    }

    /// <summary>
    ///  Handles number entry logic
    /// </summary>
    /// <param name="p_i"></param>
    private void EnterNumber(int p_i)
    {
      m_entryTimer.Stop();
      m_entryInProgress = true;

      if (m_selected == x_day)
      {
        ValidateInputValue(p_i, DateTime.DaysInMonth(m_dateTime.Year, m_dateTime.Month), x_day);
      }
      if (m_selected == x_month)
      {
        ValidateInputValue(p_i, 12, x_month);
      }
      if (m_selected == x_year)
      {
        ValidateInputValue(p_i, 9999, x_year);
      }
      if (m_selected == x_hour)
      {
        ValidateInputValue(p_i, 12, x_hour);
      }
      if (m_selected == x_minute)
      {
        ValidateInputValue(p_i, 59, x_minute);
      }
    }

    private void ValidateInputValue(int p_i, int p_max, TextBlock p_block)
    {
      int msv = Int32.Parse(p_max.ToString()[0].ToString());
      int maxDigits = p_max.ToString().Length;
      int desiredValue;
      //
      // If a value was already entered validate the second value
      //
      if (m_entryString.Length > 0)
      {
        //
        // If the total value is less than the max, publish it
        //
        desiredValue = Int32.Parse(m_entryString + p_i.ToString());
        if (desiredValue <= p_max)
        {
          m_entryString += p_i.ToString();

          if (m_entryString.Length < maxDigits)
          {
            // Set the box text
            p_block.Text = m_entryString;
            m_entryTimer.Start();
            return;
          }
        }
        else
        {
          // Reject it, start the timer and bail
          m_entryTimer.Start();
          return;
        }
      }
      else
      {
        // Set the entry string
        m_entryString = p_i.ToString();

        //
        // We have no entered value, handle msv
        //
        if (p_i <= msv)
        {
          // Set the box text
          p_block.Text = m_entryString;
          // Start the timer
          m_entryTimer.Start();
          return;
        }
      }

      //
      // Publish and build
      //
      PublishValue();
      BuildTextBoxes();
    }

    /// <summary>
    ///   Handle the AM/PM Entry
    /// </summary>
    /// <param name="p_key"></param>
    private void EnterAmPM(Key p_key)
    {
      if (ControlType == EsnDateTimeType.Time ||
          ControlType == EsnDateTimeType.DateTime)
      {
        if (p_key == Key.A)
        {
          SetToAM();
        }
        else
        {
          SetToPM();
        }
      }
      BuildTextBoxes();
    }

    /// <summary>
    ///   Set to PM
    /// </summary>
    private void SetToPM()
    {
      if (m_dateTime.Hour < 12)
      {
        m_dateTime = m_dateTime.AddHours(12);
        OnDateTimeChanged();
      }
    }

    /// <summary>
    ///   Set to AM
    /// </summary>
    private void SetToAM()
    {
      // we are pm
      if (m_dateTime.Hour >= 12)
      {
        m_dateTime = m_dateTime.AddHours(-12);
        OnDateTimeChanged();
      }
    }

    private void Delete()
    {
      if (m_selected != null)
      {
        if (m_selected.Text.Length > 0)
        {
          if (!m_entryInProgress)
          {
            m_entryInProgress = true;
            m_entryString = m_selected.Text;
          }

          m_entryString = m_entryString.Remove(m_entryString.Length - 1, 1);

          m_selected.Text = m_entryString;

        }
      }
    }

    /// <summary>
    ///   Decrement the currently selected value
    /// </summary>
    private void DecrementSelected()
    {
      if (m_selected != null)
      {
        // Publish any unfinished changes
        PublishUnfinished();

        if (m_selected == x_day)
        {
          m_dateTime = m_dateTime.AddDays(-1);
        }
        if (m_selected == x_month)
        {
          m_dateTime = m_dateTime.AddMonths(-1);
        }
        if (m_selected == x_year)
        {
          m_dateTime = m_dateTime.AddYears(-1);
        }
        if (m_selected == x_hour)
        {
          m_dateTime = m_dateTime.AddHours(-1);
        }
        if (m_selected == x_minute)
        {
          m_dateTime = m_dateTime.AddMinutes(-1);
        }
        if (m_selected == x_ampm)
        {
          if (m_dateTime.Hour < 12)
          {
            m_dateTime = m_dateTime.AddHours(12);
          }
          else
          {
            m_dateTime = m_dateTime.AddHours(-12);
          }
        }
        OnDateTimeChanged();
      }
      BuildTextBoxes();
    }

    /// <summary>
    ///   Increment the currently selected value
    /// </summary>
    private void IncrementSelected()
    {
      if (m_selected != null)
      {
        // Publish any unfinished changes
        PublishUnfinished();

        if (m_selected == x_day)
        {
          m_dateTime = m_dateTime.AddDays(1);
        }
        if (m_selected == x_month)
        {
          m_dateTime = m_dateTime.AddMonths(1);
        }
        if (m_selected == x_year)
        {
          m_dateTime = m_dateTime.AddYears(1);
        }
        if (m_selected == x_hour)
        {
          m_dateTime = m_dateTime.AddHours(1);
        }
        if (m_selected == x_minute)
        {
          m_dateTime = m_dateTime.AddMinutes(1);
        }
        if (m_selected == x_ampm)
        {
          if (m_dateTime.Hour < 12)
          {
            m_dateTime = m_dateTime.AddHours(12);
          }
          else
          {
            m_dateTime = m_dateTime.AddHours(-12);
          }
        }
        OnDateTimeChanged();
      }
      BuildTextBoxes();
    }

    /// <summary>
    ///   Move the selector cursor to the right
    /// </summary>
    private bool MoveRight()
    {
      if (m_selected != null)
      {
        // Publish any unfinished changes
        PublishUnfinished();

        if (m_selected == x_day)
        {
          SetSelection(x_year);
        }
        else if (m_selected == x_month)
        {
          SetSelection(x_day);
        }
        else if (m_selected == x_year)
        {
          if (ControlType == EsnDateTimeType.DateTime)
          {
            SetSelection(x_hour);
          }
        }
        else if (m_selected == x_hour)
        {
          SetSelection(x_minute);
        }
        else if (m_selected == x_minute)
        {
          SetSelection(x_ampm);
        }
        else if (m_selected == x_ampm)
        {
          return false;
        }
        else
        {
          return false;
        }
      }
      BuildTextBoxes();
      return true;
    }

    /// <summary>
    ///   Publish any unfinished entries
    /// </summary>
    private void PublishUnfinished()
    {
      if (m_entryString.Length > 0)
      {
        PublishValue();
      }
    }

    /// <summary>
    ///   Move the selected cursor to the left
    /// </summary>
    private bool MoveLeft()
    {
      if (m_selected != null)
      {
        // Publish any unfinished changes
        PublishUnfinished();

        if (m_selected == x_day)
        {
          SetSelection(x_month);
        }
        else if (m_selected == x_month)
        {
          return false;
        }
        else if (m_selected == x_year)
        {
          SetSelection(x_day);
        }
        else if (m_selected == x_hour)
        {
          if (ControlType == EsnDateTimeType.DateTime)
          {
            SetSelection(x_year);
          }
        }
        else if (m_selected == x_minute)
        {
          SetSelection(x_hour);
        }
        else if (m_selected == x_ampm)
        {
          SetSelection(x_minute);
        }
        else
        {
          return false;
        }
      }
      BuildTextBoxes();
      return true;
    }

    void DateTimeEntryBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      this.Focus();
    }


    /// <summary>
    ///   Mouse down handler
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void Block_MouseDown(object p_sender, MouseButtonEventArgs p_e)
    {
      TextBlock block = p_sender as TextBlock;

      if (block != null)
      {
        SetSelection(block);
      }
    }

    /// <summary>
    ///   Set the selection to a certain textblock
    /// </summary>
    /// <param name="p_selected"></param>
    private void SetSelection(TextBlock p_selected)
    {
      BuildTextBoxes();

      if (m_selected != null)
      {
        UnselectTextBlock(m_selected);
      }

      m_selected = p_selected;

      SelectTextBlock(m_selected);
    }

    /// <summary>
    ///   Visually unselect the text block
    /// </summary>
    /// <param name="p_selected"></param>
    private void UnselectTextBlock(TextBlock p_selected)
    {
      Color f = SystemColors.ControlText;
      Color b = System.Drawing.Color.Transparent;

      p_selected.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(f.A, f.R, f.G, f.B));
      p_selected.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(b.A, b.R, b.G, b.B));
    }

    /// <summary>
    ///   Visually select the text block
    /// </summary>
    /// <param name="p_selected"></param>
    private void SelectTextBlock(TextBlock p_selected)
    {
      Color f = SystemColors.HighlightText;
      Color b = SystemColors.Highlight;

      p_selected.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(f.A, f.R, f.G, f.B));
      p_selected.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(b.A, b.R, b.G, b.B));
    }

    /// <summary>
    ///   Fire the datetimechanged event
    /// </summary>
    private void OnDateTimeChanged()
    {
      if (DateTimeChanged != null)
      {
        DateTimeChanged(this, new RoutedEventArgs());
      }
    }

    /// <summary>
    ///   Set all foreground colors
    /// </summary>
    /// <param name="p_color"></param>
    private void SetAllForegrounds(System.Windows.Media.Color p_color)
    {
      x_ampm.Foreground = new SolidColorBrush(p_color); 
      x_dash1.Foreground = new SolidColorBrush(p_color); 
      x_dash2.Foreground = new SolidColorBrush(p_color); 
      x_day.Foreground = new SolidColorBrush(p_color); 
      x_divider.Foreground = new SolidColorBrush(p_color); 
      x_hour.Foreground = new SolidColorBrush(p_color); 
      x_minute.Foreground = new SolidColorBrush(p_color); 
      x_month.Foreground = new SolidColorBrush(p_color); 
      x_timeColon.Foreground = new SolidColorBrush(p_color); 
      x_timeDivider.Foreground = new SolidColorBrush(p_color); 
      x_year.Foreground = new SolidColorBrush(p_color); 
    }

    /// <summary>
    ///   Fired when the datetime value has changed
    /// </summary>
    public event RoutedEventHandler DateTimeChanged;
  }
}
