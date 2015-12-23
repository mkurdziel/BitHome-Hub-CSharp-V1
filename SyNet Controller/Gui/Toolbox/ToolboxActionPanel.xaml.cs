using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SyNet.Actions;
using Action = SyNet.Actions.Action;

namespace SyNet.Gui.Toolbox
{

  public class ImageDataTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate
        SelectTemplate(object item, DependencyObject container)
    {
      TreeViewItem treeViewItem = container as TreeViewItem;

      //if (element != null && item != null && item is Task)
      //{
      //  Task taskitem = item as Task;

      //  if (taskitem.Priority == 1)
      //    return
      //        element.FindResource("importantTaskTemplate") as DataTemplate;
      //  else
      //    return
      //        element.FindResource("myTaskTemplate") as DataTemplate;
      //}

      return null;
    }
  }

  /// <summary>
  /// Interaction logic for ToolboxActionPanel.xaml
  /// </summary>
  public partial class ToolboxActionPanel : UserControl
  {
    // caches the start point of the drag operation
    private Point? dragStartPoint = null;

    public ToolboxActionPanel()
    {
      InitializeComponent();

      LoadActions();

      //x_actionsList.ItemsSource = ActionManager.Instance.UserActions;
      //x_userActionTreeView.ItemsSource = ActionManager.Instance.DeviceActions;
    }

    private void LoadActions()
    {
      //x_actionsList.Items.Add("User Actions");

      //foreach (Action userAction in ActionManager.Instance.UserActions)
      //{
      //  x_actionsList.Items.Add(userAction);
      //}

      ////
      //// Load each device action
      ////
      //foreach (KeyValuePair<ulong, List<DeviceAction>> keyValuePair in ActionManager.Instance.DeviceActions)
      //{
      //  TreeView tv = new TreeView(); 

      //}


      // User actions
      TreeViewItem userActions = new TreeViewItem {Header = "User Actions", IsExpanded = true};

      foreach (Action userAction in ActionManager.Instance.UserActions)
      {
        userActions.Items.Add(new TreeViewItem {Header = userAction});
      }

      // Device actions
      TreeViewItem deviceActions = new TreeViewItem {Header = "Device Actions", IsExpanded = true};

      foreach (KeyValuePair<ulong, List<DeviceAction>> keyValuePair in ActionManager.Instance.DeviceActions)
      {
        TreeViewItem deviceTree = new TreeViewItem();
        Device device = NodeManager.Instance.GetNode(keyValuePair.Key);
        deviceTree.Header = device;

        // Add each device action
        foreach (DeviceAction action in keyValuePair.Value)
        {
          deviceTree.Items.Add(new TreeViewItem { Header = action });
        }

        // Add to the device tree
        deviceActions.Items.Add(deviceTree);
      }

      x_actionsTree.Items.Add(userActions);
      x_actionsTree.Items.Add(deviceActions);
    }

    private void x_actionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!(x_actionsList.SelectedValue is Action))
      {
        x_actionsList.SelectedIndex = -1;
      }
    }

    //private void ActionList_PreviewMouseMove( object p_sender, MouseEventArgs p_e )
    //{
    //  base.OnMouseMove(p_e);
    //  if (p_e.LeftButton != MouseButtonState.Pressed)
    //    this.dragStartPoint = null;

    //  if (this.dragStartPoint.HasValue)
    //  {
    //    Action action = x_actionsList.SelectedItem as Action;

    //    if (action != null)
    //    {
          
    //      //DragObject dragObject = new DragObject(action.ActionID, DragObject.EsnObjectType.Action);
    //      //DragDrop.DoDragDrop(this, dragObject, DragDropEffects.Copy);

    //      DataObject obj = new DataObject();
    //      obj.SetText("Text");
    //      DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
    //    }
        
    //    p_e.Handled = true;
    //  }
    //}

    //private void ActionList_PreviewMouseDown(object p_sender, MouseButtonEventArgs p_e)
    //{
    //  base.OnPreviewMouseDown(p_e);
    //  this.dragStartPoint = new Point?(p_e.GetPosition(this));
    //}
    private void Tree_SelectedItemChanged(object p_sender, RoutedPropertyChangedEventArgs<object> p_e)
    {
      if (x_actionsTree.SelectedItem is TreeViewItem)
      {
        if (!(((TreeViewItem)x_actionsTree.SelectedItem).Header is Action))
        {
        }
      }
    }
  }
}
