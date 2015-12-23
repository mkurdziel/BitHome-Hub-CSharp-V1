using System;
using System.Windows;
using System.Windows.Controls;
using SyNet.Events;
using SyNet.Events.Triggers;
using Trigger=SyNet.Events.Triggers.Trigger;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for ActionConditionDialog.xaml
  /// </summary>
  public partial class ActionConditionDialog : Window
  {
    private const string STR_COND_EQUAL = "Equal To";
    private const string STR_COND_GREATER = "Greater Than";
    private const string STR_COND_LESS = "Less Than";

    private EventActionConditional m_eventActionConditional = null;

    public EventActionConditional EventActionConditional
    {
      get { return m_eventActionConditional; }
      set { m_eventActionConditional = value; }
    }

    public ActionConditionDialog(Event p_event)
    {
      InitializeComponent();

      //
      // Populate the trigger parameter list
      //
      foreach (Trigger trigger in p_event.Triggers)
      {
        foreach (TriggerParameter parameter in trigger.Parameters)
        {
          x_triggerValues.Items.Add(parameter);          
        }
      }

      if (x_triggerValues.Items.Count == 0)
      {
        x_triggerValues.Items.Add("No trigger values available");
        x_triggerValues.IsEnabled = false;
      }
      else
      {
        x_triggerValues.SelectedIndex = 0;
      }

      x_triggerValues.InvalidateMeasure();
      x_triggerValues.InvalidateArrange();
    }

    private void Trigger_SelectionChanged(object p_sender, SelectionChangedEventArgs p_e)
    {
      TriggerParameter param = x_triggerValues.SelectedItem as TriggerParameter;

      if (param != null)
      {
        x_conditional.IsEnabled = true;
        PopulateConditionalBox(param);
        PopulateValueBox(param);
      }
      else
      {
        x_conditional.IsEnabled = false;
      }
    }

    private void PopulateValueBox(TriggerParameter p_parameter)
    {
      switch (p_parameter.ValidationType)
      {
        case SyNet.Protocol.EsnParamValidationType.ENUMERATED:
        case SyNet.Protocol.EsnParamValidationType.BOOL:
        case SyNet.Protocol.EsnParamValidationType.MAX_STRING_LEN:
        case SyNet.Protocol.EsnParamValidationType.SIGNED_FULL:
        case SyNet.Protocol.EsnParamValidationType.SIGNED_RANGE:
        case SyNet.Protocol.EsnParamValidationType.UNSIGNED_FULL:
        case SyNet.Protocol.EsnParamValidationType.UNSIGNED_RANGE:
          TextBox textbox = new TextBox();
          textbox.HorizontalAlignment = HorizontalAlignment.Stretch;
          textbox.TextChanged += new TextChangedEventHandler(Textbox_TextChanged);
          x_value.Content = textbox;
          x_value.IsEnabled = true;
          break;
      }
    }

    void Textbox_TextChanged(object sender, TextChangedEventArgs e)
    {
      TextBox textbox = sender as TextBox;
      if (textbox != null)
      {
        StringValue = textbox.Text;
      }
    }

    private string StringValue
    {
      get; set;
    }

    /// <summary>
    ///   Build the conditional box based on the parameter type
    /// </summary>
    /// <param name="p_parameter"></param>
    private void PopulateConditionalBox(TriggerParameter p_parameter)
    {
      x_conditional.Items.Clear();

      switch(p_parameter.ValidationType)
      {
        case SyNet.Protocol.EsnParamValidationType.BOOL:
        case SyNet.Protocol.EsnParamValidationType.ENUMERATED:
        case SyNet.Protocol.EsnParamValidationType.MAX_STRING_LEN:
          x_conditional.Items.Add(STR_COND_EQUAL);
          break;
        case SyNet.Protocol.EsnParamValidationType.SIGNED_FULL:
        case SyNet.Protocol.EsnParamValidationType.SIGNED_RANGE:
        case SyNet.Protocol.EsnParamValidationType.UNSIGNED_FULL:
        case SyNet.Protocol.EsnParamValidationType.UNSIGNED_RANGE:
          x_conditional.Items.Add(STR_COND_EQUAL);
          x_conditional.Items.Add(STR_COND_GREATER);
          x_conditional.Items.Add(STR_COND_LESS);
          break;
        case SyNet.Protocol.EsnParamValidationType.UNKNOWN:
          x_conditional.IsEnabled = false;
          break;
      }

      x_conditional.SelectedIndex = 0;
    }

    private void OKButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      TriggerParameter param = x_triggerValues.SelectedValue as TriggerParameter;
      EventActionConditional.EsnConditionalType type = EventActionConditional.EsnConditionalType.EqualTo;
      ;
      switch (x_conditional.SelectedItem as String)
      {
        case STR_COND_GREATER:
          type = EventActionConditional.EsnConditionalType.GreaterThan;
          break;
        case STR_COND_LESS:
          type = EventActionConditional.EsnConditionalType.LessThan;
          break;
      }
      if (param != null)
      {
        this.EventActionConditional = new EventActionConditional(param, type, StringValue);
      }

      DialogResult = true;
    }
  }
}
