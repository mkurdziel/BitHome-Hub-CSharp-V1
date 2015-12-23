using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SyNet.Actions;
using Action=SyNet.Actions.Action;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// </summary>
  public partial class ActionSelectDialog : Window
  {
    /// <summary>
    ///   If true, delay actions are included
    /// </summary>
    public bool IncludeDelay { get; set; }

    /// <summary>
    ///   Selected Action ID
    /// </summary>
    public UInt64 SelectedActionID { get; set;}

    /// <summary>
    ///   Action to omit from the dialog
    /// </summary>
    private UInt64 m_omitActionID;

    /// <summary>
    ///   Constructor woth omit ID
    /// </summary>
    /// <param name="p_omitActionID"></param>
    public ActionSelectDialog(UInt64 p_omitActionID )
    {
      m_omitActionID = p_omitActionID;

      InitializeComponent();
      this.Activated += ActionSelectDialog_Activated;
    }

    void ActionSelectDialog_Activated( object sender, EventArgs e )
    {
      LoadActions();
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public ActionSelectDialog() : this(0)
    {
    }

    /// <summary>
    ///   Load the actions from the system into a tree
    /// </summary>
    private void LoadActions()
    {
      ActionManager aMgr = ActionManager.Instance;
      NodeManager nMgr = NodeManager.Instance;

      //
      // Load the device actions
      //
      TreeViewItem tiDeviceHeader = new TreeViewItem();
      tiDeviceHeader.Header = "Device Actions";
      x_actionsTreeView.Items.Add(tiDeviceHeader);
      foreach (KeyValuePair<ulong, List<DeviceAction>> pair in aMgr.DeviceActions)
      {
        Device device = nMgr.GetNode(pair.Key);
        if (device != null)
        {
          TreeViewItem tiDeviceName = new TreeViewItem();
          tiDeviceName.Header = device.DeviceName;
          tiDeviceHeader.Items.Add(tiDeviceName);

          foreach (DeviceAction action in pair.Value)
          {
            TreeViewItem tiDeviceAction = new TreeViewItem();
            tiDeviceAction.Header = action.Name;
            tiDeviceAction.Tag = action.ActionID;
            tiDeviceName.Items.Add(tiDeviceAction);
          }
        }

      }

      //
      // Load the user actions
      //
      TreeViewItem tiUserHeader = new TreeViewItem();
      tiUserHeader.Header = "User Actions";
      foreach (Action action in aMgr.UserActions)
      {
        if (action.ActionID != m_omitActionID)
        {
          TreeViewItem tiUserAction = new TreeViewItem();
          tiUserAction.Header = action.Name;
          tiUserAction.Tag = action.ActionID;
          tiUserHeader.Items.Add(tiUserAction);
        }
      }
      if (tiUserHeader.Items.Count > 0)
      {
        x_actionsTreeView.Items.Add(tiUserHeader);
      }

      //
      // Load delay action
      //
      if (IncludeDelay)
      {
        TreeViewItem tiDelay = new TreeViewItem();
        tiDelay.Header = "Delay";
        tiDelay.Tag = ActionManager.BASE_ACTION_ID_DELAY;
        x_actionsTreeView.Items.Add(tiDelay);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OKButton_Click( object sender, RoutedEventArgs e )
    {

      SelectedActionID = GetSelectedAction();
      DialogResult = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void ActionsTreeView_SelectedItemChanged(object p_sender, RoutedPropertyChangedEventArgs<object> p_e)
    {
      UInt64 actionID = GetSelectedAction();
      x_OKButton.IsEnabled = (actionID != 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private UInt64 GetSelectedAction()
    {
      UInt64 retVal = 0;
      TreeViewItem item = x_actionsTreeView.SelectedItem as TreeViewItem;
      if (item != null)
      {
        if (item.Tag != null)
        {
          retVal = (UInt64)item.Tag;
        }
      }
      return retVal;
    }
  }
}
