using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SyNet.Events.Triggers;
using SyNet.GuiControls.DateRepeat;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for EventTriggerEditPanel.xaml
  /// </summary>
  public partial class DateTimeTriggerEditPanel : UserControl
  {
    private DateTimeTrigger m_trigger;

    public DateTimeTriggerEditPanel( DateTimeTrigger p_trigger )
    {
    
      InitializeComponent();
      
      //
      // Set default values
      //
      x_endOnBox.DateTime = DateTime.Now.AddDays(1);
   
      //
      // Listen to data context changes
      //
      this.DataContextChanged += DateTimeTriggerEditPanel_DataContextChanged;

      //
      // Listen for changes
      //
      x_dateTimeEntryBox.DateTimeChanged += DateTimeEntryBox_DateTimeChanged;

      //
      // Set to datacontext for binding
      //
      this.DataContext = p_trigger;
    }

    void DateTimeTriggerEditPanel_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      m_trigger = DataContext as DateTimeTrigger;

      BuildControlFromTrigger();
    }

    /// <summary>
    ///   Start time changed handler
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void DateTimeEntryBox_DateTimeChanged(object p_sender, RoutedEventArgs p_e)
    {
      if (m_trigger != null)
      {
        m_trigger.StartTime = x_dateTimeEntryBox.DateTime;
      }
    }

    /// <summary>
    ///   Build the control values from the trigger
    /// </summary>
    private void BuildControlFromTrigger()
    {
      if (m_trigger != null)
      {
        //
        // Start time
        //
        x_dateTimeEntryBox.DateTime = m_trigger.StartTime;

        //
        // End time
        //
        if (m_trigger.DateTimeType == (int)DateTimeTrigger.EsnDateType.DT_ONCE)
        {
          x_endComboBox.SelectedIndex = 0;
        }
        else
        {
          //
          // If no counter value then we have a repeating with no end date
          //
          if (m_trigger.CounterValue == 1)
          {
            x_endComboBox.SelectedIndex = 0;
          }
          else
          {
            if (m_trigger.SourceWasCount)
            {
              x_afterTextBox.Text = m_trigger.CounterValue.ToString();
              x_endComboBox.SelectedIndex = 1;
            }
            else
            {
              x_endOnBox.DateTime = m_trigger.EndTime;
              x_endComboBox.SelectedIndex = 2;
            }
          }
        }
      }
    }

    private void Repeat_MouseDown( object p_sender, MouseButtonEventArgs p_e )
    {
      DateRepeatDialog dlg = new DateRepeatDialog(m_trigger);

      Point windowPoint = this.PointToScreen(Mouse.GetPosition(this));

      dlg.WindowStartupLocation = WindowStartupLocation.Manual;
      dlg.Top = windowPoint.Y;
      dlg.Left = windowPoint.X;
      dlg.Owner = Window.GetWindow(this);
      dlg.ShowInTaskbar = false;
      dlg.ShowDialog();
    }

    private void ResetButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      if (m_trigger != null)
      {
        m_trigger.StartTime = DateTime.Now;
        x_dateTimeEntryBox.DateTime = m_trigger.StartTime;
      }
    }

    private void End_SelectionChanged(object p_sender, SelectionChangedEventArgs p_e)
    {
      if (x_endOnBox == null) return;

      switch (x_endComboBox.SelectedIndex)
      {
        case 0:
          EndSetNone();
          x_endOnBox.Visibility = Visibility.Collapsed;
          x_afterPanel.Visibility = Visibility.Collapsed;
          break;
        case 1:
          EndSetAfter();
          x_endOnBox.Visibility = Visibility.Collapsed;
          x_afterPanel.Visibility = Visibility.Visible;
          break;
        case 2:
          EndSetOnDate();
          x_endOnBox.Visibility = Visibility.Visible;
          x_afterPanel.Visibility = Visibility.Collapsed;
          break;
      }
    }

    private void EndSetOnDate()
    {
      if (m_trigger != null)
      {
        if (m_trigger.SourceWasCount)
        {
          x_endOnBox.DateTime = DateTime.Now.AddDays(1);
        }
      }
    }

    private void EndSetAfter()
    {
      if (m_trigger != null)
      {
        m_trigger.CounterValue = Int32.Parse(x_afterTextBox.Text);
      }
    }

    private void EndSetNone()
    {
      if (m_trigger != null)
      {
        // nothing to do here...
      }
    }

    private void AfterTextBox_TextChanged( object sender, TextChangedEventArgs e )
    {
      if (m_trigger != null)
      {
        if (x_afterTextBox.Text != String.Empty)
        {
          m_trigger.CounterValue = Int32.Parse(x_afterTextBox.Text);
        }
      }
    }

    private void EndOn_Changed(object p_sender, RoutedEventArgs p_e)
    {
      if (m_trigger != null)
      {
        m_trigger.EndTime = x_endOnBox.DateTime;
      }
    }
  }
}
