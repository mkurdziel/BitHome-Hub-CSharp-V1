using System;
using System.Windows;
using System.Windows.Controls;
using SyNet.Actions;
using Action=SyNet.Actions.Action;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for SequenceActionEditPanel.xaml
  /// </summary>
  public partial class SequenceActionEditPanel : UserControl
  {
    /// <summary>
    ///   Constructor
    /// </summary>
    public SequenceActionEditPanel()
    {
      InitializeComponent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void ActionAddButton_click( object p_sender, RoutedEventArgs p_e )
    {
      SequenceAction a = DataContext as SequenceAction;
      if (a != null)
      {
        ActionSelectDialog dlg = new ActionSelectDialog(a.ActionID)
                                   {
                                     WindowStartupLocation =
                                       WindowStartupLocation.CenterScreen,
                                     IncludeDelay = true
                                   };

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
              a.AddAction(action);
              //x_actionTreeView.Items.Refresh();
            }
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ActionMinusButton_Click( object sender, RoutedEventArgs e )
    {
      ActionItem aItem = x_actionTreeView.SelectedItem as ActionItem;

      if (aItem != null)
      {
        string messageBoxText = String.Format("Do you want remove the sub-action \"{0}\" from this action?", aItem.ActionName);
        string caption = "Confirm Delete";
        MessageBoxButton button = MessageBoxButton.YesNo;
        MessageBoxImage icon = MessageBoxImage.Warning;
        // Display message box
        MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

        // Process message box results
        switch (result)
        {
          case MessageBoxResult.Yes:
            SequenceAction a = DataContext as SequenceAction;
            if (a != null)
            {
              a.RemoveAction(aItem);
              //x_actionTreeView.Items.Refresh();
            }
            break;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void ActionTreeView_SelectedItemChanged( object p_sender, RoutedPropertyChangedEventArgs<object> p_e )
    {
      ActionParameter param = x_actionTreeView.SelectedItem as ActionParameter;
      if (param != null)
      {
        x_paramEditPanel.Action = this.DataContext as Action;
        x_paramEditPanel.DataContext = param;
        x_paramEditPanel.Visibility = Visibility.Visible;
        x_noParamLabel.Visibility = Visibility.Hidden;
      }
      else
      {
        x_paramEditPanel.Visibility = Visibility.Hidden;
        x_noParamLabel.Visibility = Visibility.Visible;
      }
    }
  }  
}
