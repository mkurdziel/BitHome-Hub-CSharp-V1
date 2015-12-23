using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SyNet.Events;
using SyNet.Events.Triggers;
using Trigger = SyNet.Events.Triggers.Trigger;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// </summary>
  public partial class TriggerSelectDialog : Window
  {
    /// <summary>
    ///   Trigger selected by the user
    /// </summary>
    public Trigger SelectedTrigger { get; set; }

    /// <summary>
    ///   Constructor
    /// </summary>
    public TriggerSelectDialog()
    {
      InitializeComponent();
      LoadTriggers();
    }

    /// <summary>
    ///   Load the triggers in the system into a tree
    /// </summary>
    private void LoadTriggers()
    {
      NodeManager nMgr = NodeManager.Instance;
      EventScheduler eMgr = EventScheduler.Instance;

      Trigger[] triggers = eMgr.Triggers;

      //
      // Sort the message triggers by device to build a tree
      //
      Dictionary<UInt64, List<Trigger>> dctDeviceTriggers =
        new Dictionary<ulong, List<Trigger>>();

      //
      // Keep a list of the datetime triggers and Other triggers
      //
      List<DateTimeTrigger> DateTimeTriggers = new List<DateTimeTrigger>();
      List<Trigger> OtherTriggers = new List<Trigger>();

      //
      // Iterate the triggers and sort them
      //
      foreach (Trigger trigger in triggers)
      {
        if (trigger is MessageTrigger)
        {
          MessageTrigger mtrig = trigger as MessageTrigger;
          if (!dctDeviceTriggers.ContainsKey(mtrig.DeviceID))
          {
            dctDeviceTriggers[mtrig.DeviceID] = new List<Trigger>();
          }

          dctDeviceTriggers[mtrig.DeviceID].Add(mtrig);
        }
        else if (trigger is StatusTrigger)
        {
          StatusTrigger strig = trigger as StatusTrigger;
          if (!dctDeviceTriggers.ContainsKey(strig.DeviceID))
          {
            dctDeviceTriggers[strig.DeviceID] = new List<Trigger>();
          }
          dctDeviceTriggers[strig.DeviceID].Add(strig);
        }
        else if (trigger is DateTimeTrigger)
        {
          DateTimeTriggers.Add(trigger as DateTimeTrigger);
        }
        else
        {
          OtherTriggers.Add(trigger);
        }
      }

      //
      // Build the trigger tree
      //
      TreeViewItem tiMsgHeader = new TreeViewItem();

      tiMsgHeader.Header = "Device Triggers";
      x_treeView.Items.Add(tiMsgHeader);
      foreach (KeyValuePair<ulong, List<Trigger>> pair in dctDeviceTriggers)
      {
        Device device = nMgr.GetNode(pair.Key);
        if (device != null)
        {
          TreeViewItem tiDeviceName = new TreeViewItem();
          tiDeviceName.Header = device.DeviceName;
          tiMsgHeader.Items.Add(tiDeviceName);

          foreach (Trigger trigger in pair.Value)
          {
            TreeViewItem tiMsgTrigger = new TreeViewItem();
            tiMsgTrigger.Header = trigger.Name;
            tiMsgTrigger.Tag = trigger;
            tiDeviceName.Items.Add(tiMsgTrigger);
          }
        }
        else
        {
          OtherTriggers.AddRange(pair.Value);
        }
      }

      //
      // Load the DateTime Triggers
      //

      TreeViewItem tiDateTimeHeader = new TreeViewItem();
      tiDateTimeHeader.Header = "Date/Time Triggers";
      x_treeView.Items.Add(tiDateTimeHeader);

      TreeViewItem tiDateTimeTrigger = new TreeViewItem();
      tiDateTimeTrigger.Header = "New Date/Time Trigger";
      tiDateTimeTrigger.Tag = typeof (DateTimeTrigger);
      tiDateTimeHeader.Items.Add(tiDateTimeTrigger);

      foreach (Trigger trigger in DateTimeTriggers)
      {
        TreeViewItem ti = new TreeViewItem();
        ti.Header = trigger.Name;
        ti.Tag = trigger;
        tiDateTimeHeader.Items.Add(ti);
      }

      //
      // Load other triggers
      //
      foreach (Trigger trigger in OtherTriggers)
      {
        TreeViewItem ti = new TreeViewItem();
        ti.Header = trigger.Name;
        ti.Tag = trigger;
        x_treeView.Items.Add(ti);
      }
    }

    /// <summary>
    ///   OK button click handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
      SelectedTrigger = GetSelectedTrigger();

      DialogResult = true;
    }

    /// <summary>
    ///   Get the currently selected trigger from the tree
    /// </summary>
    /// <returns></returns>
    private Trigger GetSelectedTrigger()
    {
      Trigger retVal = null;
      TreeViewItem item = x_treeView.SelectedItem as TreeViewItem;
      if (item != null)
      {
        if (item.Tag != null)
        {
          if (item.Tag is Trigger)
          {
            return item.Tag as Trigger;
          }
          if (item.Tag is Type)
          {
            retVal = Activator.CreateInstance(item.Tag as Type, null) as Trigger;
          }
        }
      }
      return retVal;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void TreeView_SelectedItemChanged(object p_sender, RoutedPropertyChangedEventArgs<object> p_e)
    {
      Trigger trigger = GetSelectedTrigger();
      x_OKButton.IsEnabled = (trigger != null);
    }
  }
}
