using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SyNet.Events;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for EventsPanel.xaml
  /// </summary>
  public partial class EventsPanel : UserControl
  {
    /// <summary>
    ///   Default Constructor
    /// </summary>
    public EventsPanel()
    {
      InitializeComponent();

      x_eventList.ItemsSource = EventScheduler.Instance.Events;
    }

    /// <summary>
    ///   Add button click event handler
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void AddButton_Click(object p_sender, RoutedEventArgs p_e)
    {

      Event e = new Event();
      // Create a new naming dialog
      RenameDialog dlg = new RenameDialog("Name Event");
      dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      dlg.Value = "Event";
      dlg.ShowDialog();
      if (dlg.DialogResult.HasValue && dlg.DialogResult.Value == true)
      {
        e.Name = dlg.Value;
        EventScheduler.Instance.AddEvent(e);
      }
      RefreshList();
    }

    /// <summary>
    ///   Minus button click event handler
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void MinusButton_Click(object p_sender, RoutedEventArgs p_e)
    {
      Event e = x_eventList.SelectedItem as Event;
      if (e != null)
      {
        string messageBoxText = String.Format("Do you want to delete the event \"{0}\"", e.Name);
        string caption = "Confirm Delete";
        MessageBoxButton button = MessageBoxButton.YesNo;
        MessageBoxImage icon = MessageBoxImage.Warning;
        // Display message box
        MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

        // Process message box results
        switch (result)
        {
          case MessageBoxResult.Yes:
            EventScheduler.Instance.RemoveEvent(e);
            break;
        }
      }
      RefreshList();
    }

    private void RefreshList()
    {
      x_eventList.ItemsSource = null;
      x_eventList.ItemsSource = EventScheduler.Instance.Events;
    }

    /// <summary>
    ///   Event list selection changed event handler
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void EventList_SelectionChanged(object p_sender, SelectionChangedEventArgs p_e)
    {
      Event e = x_eventList.SelectedItem as Event;
      if (e != null)
      {
        x_eventEditPanel.Visibility = Visibility.Visible;
        x_noSelectionLabel.Visibility = Visibility.Hidden;
      }
      else
      {
        x_eventEditPanel.Visibility = Visibility.Hidden;
        x_noSelectionLabel.Visibility = Visibility.Visible;
      }
    }
  }
}
