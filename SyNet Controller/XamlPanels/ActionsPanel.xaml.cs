using System;
using System.Windows;
using System.Windows.Controls;
using SyNet.Actions;
using Action=SyNet.Actions.Action;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for ActionsPanel.xaml
  /// </summary>
  public partial class ActionsPanel : UserControl
  {
    /// <summary>
    ///   Default constructor
    /// </summary>
    public ActionsPanel()
    {
      InitializeComponent();

      LoadActionTypes();

      x_actionList.ItemsSource = ActionManager.Instance.UserActions;
    }

    /// <summary>
    ///   Add a single action to the menu
    /// </summary>
    /// <param name="p_strName"></param>
    /// <param name="p_type"></param>
    private void AddActionTypeMenuItem( string p_strName, Type p_type )
    {
      MenuItem mi = new MenuItem();
      mi.Header = p_strName;
      mi.Tag = p_type;
      mi.Click += ActionAddContextMenu_Click;
      x_ActionAddContextMenu.Items.Add(mi);
    }

    /// <summary>
    ///   Load the different action types into the context menu
    /// </summary>
    private void LoadActionTypes()
    {
      //AddActionTypeMenuItem("New Action", typeof(SequenceAction));
    }

    /// <summary>
    ///   Action button add event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void x_ActionAddButton_Click( object sender, RoutedEventArgs e )
    {
      //x_ActionAddContextMenu.IsOpen = true;

      SequenceAction newAction = new SequenceAction();
      // Create a new naming dialog
      RenameDialog dlg = new RenameDialog("Name Action");
      dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      dlg.Value = ActionManager.Instance.GenerateUniqueName(
        String.Format("Action"));
      dlg.ShowDialog();
      if (dlg.DialogResult.HasValue && dlg.DialogResult.Value == true)
      {
        newAction.Name = dlg.Value;
        ActionManager.Instance.AddAction(newAction);
      }
      else
      {
        ActionManager.Instance.UnregisterAction(newAction.ActionID);
      }
      x_actionList.Items.Refresh();
      x_actionList.SelectedItem = newAction;
    }

    /// <summary>
    ///   Action add event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ActionAddContextMenu_Click( object sender, RoutedEventArgs e )
    {
      MenuItem mi = sender as MenuItem;
      if (mi != null)
      {
        Type type = mi.Tag as Type;
        Action newAction = (Action)Activator.CreateInstance(type);

        // Create a new naming dialog
        RenameDialog dlg = new RenameDialog("Name Action");
        dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dlg.Value = ActionManager.Instance.GenerateUniqueName(
          String.Format("Action"));
        dlg.ShowDialog();
        if (dlg.DialogResult.HasValue && dlg.DialogResult.Value == true)
        {
          newAction.Name = dlg.Value;
          ActionManager.Instance.AddAction(newAction);
        }
        else
        {
          ActionManager.Instance.UnregisterAction(newAction.ActionID);
        }
      }
      else
      {
        throw new ApplicationException("ActionsPanel.ActionAddContextMenu_Click - null sender");
      }
      x_actionList.Items.Refresh();
    }

    /// <summary>
    ///   Action deletion button event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void x_ActionMinusButton_Click( object sender, RoutedEventArgs e )
    {
      Action action = x_actionList.SelectedValue as Action;
      if (action != null)
      {
        string messageBoxText = String.Format("Do you want to delete the action \"{0}\"", action.Name);
        string caption = "Confirm Delete";
        MessageBoxButton button = MessageBoxButton.YesNo;
        MessageBoxImage icon = MessageBoxImage.Warning;
        // Display message box
        MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

        // Process message box results
        switch (result)
        {
          case MessageBoxResult.Yes:
            ActionManager.Instance.RemoveAction(action);
            break;
        }
      }
      x_actionList.Items.Refresh();
    }

    private void ActionList_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      Action action = x_actionList.SelectedValue as Action;
      if (action != null)
      {
        //x_editPanel.DataContext = action;

        //
        // Shuffle the visibility
        //
        x_noSelectionLabel.Visibility = Visibility.Collapsed;
        x_editPanel.Visibility = Visibility.Visible;
      }
      else
      {
        //
        // Shuffle the visibility
        //
        x_noSelectionLabel.Visibility = Visibility.Visible;
        x_editPanel.Visibility = Visibility.Collapsed;
      }
    }

    private void Action_Execute(object p_sender, RoutedEventArgs p_e)
    {
      Action a = x_actionList.SelectedValue as Action;
      if (a != null)
      {
        ActionManager.Instance.ExecuteThreadedAction(a.ActionID);
      }
    }

    private void Action_Duplicate(object p_sender, RoutedEventArgs p_e)
    {
      SequenceAction a = x_actionList.SelectedValue as SequenceAction;
      if (a != null)
      {
        SequenceAction copyAction = new SequenceAction(a);
        ActionManager.Instance.AddAction(copyAction);
      }
      x_actionList.Items.Refresh();
    }
  }
}
