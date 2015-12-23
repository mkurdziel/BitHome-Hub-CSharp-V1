using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SyNet.Actions;
using SyNet.Events;
using SyNet.Events.Triggers;
using Action = SyNet.Actions.Action;
using Trigger = SyNet.Events.Triggers.Trigger;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for SequenceActionEditPanel.xaml
  /// </summary>
  public partial class EventEditPanel : UserControl
  {
    /// <summary>
    ///   Constructor
    /// </summary>
    public EventEditPanel()
    {
      InitializeComponent();

      this.DataContextChanged += EventEditPanel_DataContextChanged;
    }

    private void EventEditPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      Event myEvent = DataContext as Event;
      if (myEvent != null)
      {
        x_triggeringTypeComboBox.SelectedIndex = (int)myEvent.TriggerMode;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void ActionAddButton_click(object p_sender, RoutedEventArgs p_e)
    {
      Event myEvent = DataContext as Event;
      if (p_e != null)
      {

        ActionSelectDialog dlg = new ActionSelectDialog();
        dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dlg.ShowDialog();
        if (dlg.DialogResult.HasValue && dlg.DialogResult == true)
        {

          UInt64 actionID = dlg.SelectedActionID;

          if (actionID != 0)
          {
            Action action = null;

            //
            // Check for a delay action
            //
            if (actionID == ActionManager.BASE_ACTION_ID_DELAY)
            {
              action = new DelayAction();
              ActionManager.Instance.AddAction(action);
            }
            //
            // Otherwise its a normal action
            //
            else
            {
              action = ActionManager.Instance.GetAction(actionID);
            }

            if (action != null)
            {
              myEvent.AddAction(action);
            }
          }
        }
        RefreshActionTree();
      }
    }

    private void RefreshActionTree()
    {

      //BindingOperations.GetBindingExpressionBase(x_actionTree, TreeView.ItemsSourceProperty).UpdateTarget();
      x_actionTree.Items.Refresh();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ActionMinusButton_Click(object sender, RoutedEventArgs e)
    {

      Event myEvent = DataContext as Event;
      EventAction action = x_actionTree.SelectedItem as EventAction;
      if (myEvent != null && action != null)
      {
        string messageBoxText = String.Format("Do you want remove the action \"{0}\" from this event?", action.Name);
        string caption = "Confirm Delete";
        MessageBoxButton button = MessageBoxButton.YesNo;
        MessageBoxImage icon = MessageBoxImage.Warning;
        // Display message box
        MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

        // Process message box results
        switch (result)
        {
          case MessageBoxResult.Yes:
            myEvent.RemoveAction(action);
            break;
        }
      }
      RefreshActionTree();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void TriggerAddButton_click(object p_sender, RoutedEventArgs p_e)
    {
      Event e = DataContext as Event;
      if (e != null)
      {
        TriggerSelectDialog dlg = new TriggerSelectDialog();
        dlg.ShowDialog();

        if (dlg.DialogResult.HasValue && dlg.DialogResult == true)
        {
          Trigger trigger = dlg.SelectedTrigger;

          if (trigger != null)
          {
            e.AddTrigger(trigger);
          }
        }
      }
      x_triggerTree.Items.Refresh();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void TriggerMinusButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      Event myEvent = DataContext as Event;
      Trigger trigger = x_triggerTree.SelectedItem as Trigger;
      if (myEvent != null && trigger != null)
      {
        string messageBoxText = String.Format("Do you want remove the trigger \"{0}\" from this event?", trigger.Name);
        string caption = "Confirm Delete";
        MessageBoxButton button = MessageBoxButton.YesNo;
        MessageBoxImage icon = MessageBoxImage.Warning;
        // Display message box
        MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

        // Process message box results
        switch (result)
        {
          case MessageBoxResult.Yes:
            myEvent.RemoveTrigger(trigger);
            break;
        }
      }
      x_triggerTree.Items.Refresh();
    }

    private void x_triggeringTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      Event myEvent = DataContext as Event;
      if (myEvent != null)
      {
        myEvent.TriggerMode = (Event.ETriggerMode)x_triggeringTypeComboBox.SelectedIndex;
      }
    }

    private void ActionsTree_SelectedItemChanged(object p_sender, RoutedPropertyChangedEventArgs<object> p_e)
    {
      //
      // See if the selected item is an action or its parameter
      //
      EventAction action = x_actionTree.SelectedItem as EventAction;
      if (action != null)
      {
        EventActionEditPanel panel = new EventActionEditPanel(DataContext as Event);
        panel.DataContext = action;
        x_actionEditContainer.Content = panel;
        return;
      }

      EventParameter parameter = x_actionTree.SelectedItem as EventParameter;
      if (parameter != null)
      {
        EventParametersEditPanel panel = new EventParametersEditPanel(DataContext as Event);
        panel.DataContext = parameter;

        x_actionEditContainer.Content = panel;
        return;
      }

      x_actionEditContainer.Content = null;
    }

    private void TriggersTree_SelectedItemChanged(object p_sender, RoutedPropertyChangedEventArgs<object> p_e)
    {
      //
      // See if the selected item is a trigger
      //
      Trigger trigger = x_triggerTree.SelectedItem as Trigger;
      if (trigger != null)
      {
        if (trigger is DateTimeTrigger)
        {
          x_triggerEditContainer.Content = new DateTimeTriggerEditPanel(trigger as DateTimeTrigger);
        }
        else
        {
          x_triggerEditContainer.Content = new MessageTriggerEditPanel(trigger);
        }
      }
      else
      {
        x_triggerEditContainer.Content = null;
      }
    }
  }
}
