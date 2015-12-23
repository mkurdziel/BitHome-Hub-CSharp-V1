using System;
using System.Windows;
using System.Windows.Controls;
using SyNet.Events;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for EventActionEditPanel.xaml
  /// </summary>
  public partial class EventActionEditPanel : UserControl
  {
    public Event Event
    {
      get;
      set;
    }

    public EventActionEditPanel(Event p_event)
    {
      InitializeComponent();

      Event = p_event;
    }

    private void ConditionAdd_Click(object p_sender, RoutedEventArgs p_e)
    {
      ActionConditionDialog dlg = new ActionConditionDialog(Event);
      dlg.ShowDialog();

      if (dlg.DialogResult.HasValue && dlg.DialogResult.Value == true)
      {
        EventAction action = DataContext as EventAction;
        EventActionConditional conditional = dlg.EventActionConditional;

        if (conditional != null && action != null)
        {
          action.Conditionals.Add(conditional);
          x_actionConditions.Items.Refresh();
        }
      }
    }

    private void ConditionMinus_Click(object p_sender, RoutedEventArgs p_e)
    {
      EventActionConditional conditional = x_actionConditions.SelectedItem as EventActionConditional;
      EventAction action = DataContext as EventAction;

      if (conditional != null && action != null)
      {
        action.Conditionals.Remove(conditional);
        x_actionConditions.Items.Refresh();
      }
    }
  }
}
