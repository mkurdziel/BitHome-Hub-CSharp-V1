using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using SyNet.Events;
using SyNet.Events.Triggers;

namespace SyNet.Gui.Toolbox
{
  /// <summary>
  /// Interaction logic for ToolboxTriggerPanel.xaml
  /// </summary>
  public partial class ToolboxTriggerPanel : UserControl
  {
    public ToolboxTriggerPanel()
    {
      InitializeComponent();

      LoadTriggers();
      //x_listBox.ItemsSource = EventScheduler.Instance.Triggers;
    }

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
          TreeViewItem tiDeviceName = new TreeViewItem {Header = device};
          tiMsgHeader.Items.Add(tiDeviceName);

          foreach (Trigger trigger in pair.Value)
          {
            TreeViewItem tiMsgTrigger = new TreeViewItem {Header = trigger};
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
      tiDateTimeTrigger.Tag = typeof(DateTimeTrigger);
      tiDateTimeHeader.Items.Add(tiDateTimeTrigger);

      foreach (Trigger trigger in DateTimeTriggers)
      {
        TreeViewItem ti = new TreeViewItem {Header = trigger};
        tiDateTimeHeader.Items.Add(ti);
      }

      //
      // Load other triggers
      //
      foreach (Trigger trigger in OtherTriggers)
      {
        TreeViewItem ti = new TreeViewItem {Header = trigger};
        x_treeView.Items.Add(ti);
      }
    }
  }
}
