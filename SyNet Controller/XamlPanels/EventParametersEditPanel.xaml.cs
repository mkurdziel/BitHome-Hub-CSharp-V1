using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using SyNet.Actions;
using SyNet.Events;
using SyNet.Events.Triggers;
using Action = SyNet.Actions.Action;
using Trigger = SyNet.Events.Triggers.Trigger;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for EventParametersEditPanel.xaml
  /// </summary>
  public partial class EventParametersEditPanel : UserControl
  {
    /// <summary>
    ///  Gets or sets the event of the panel
    /// </summary>
    public Event Event
    {
      get;
      set;
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    /// <param name="p_event"></param>
    public EventParametersEditPanel(Event p_event)
    {
      DataContext = null;

      Event = p_event;

      InitializeComponent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ParameterSetupGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      //
      // Populate the type combo box
      //
      x_paramTypeComboBox.Items.Clear();
      foreach (
        object o in
          Enum.GetValues(typeof(EventParameter.EsnEventParameterType)))
      {
        //if ((EventParameter.EsnEventParameterType)o != EventParameter.EsnEventParameterType.INTERNAL)
        x_paramTypeComboBox.Items.Add(o);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private void BuildParameterTypeFields()
    {
      EventParameter param = DataContext as EventParameter;

      if (param != null)
      {
        x_paramTypeInternalLabel.Visibility = Visibility.Hidden;
        x_paramTypeComboBox.Visibility = Visibility.Visible;

        x_parameterTypeConstant.Visibility = Visibility.Hidden;
        x_parameterTypeConstantLabel.Visibility = Visibility.Hidden;
        x_parameterTypeDependentCombobox.Visibility = Visibility.Hidden;
        x_parameterTypeDependentLabel.Visibility = Visibility.Hidden;

        switch (param.EventParameterType)
        {
          case EventParameter.EsnEventParameterType.Constant:
            x_parameterTypeConstant.Visibility = Visibility.Visible;
            x_parameterTypeConstantLabel.Visibility = Visibility.Visible;
            break;
          case EventParameter.EsnEventParameterType.Trigger:
            x_parameterTypeDependentCombobox.Visibility = Visibility.Visible;
            x_parameterTypeDependentLabel.Visibility = Visibility.Visible;
            BuildDependentTypeField(param);
            break;
          //case EventParameter.EsnActionParameterType.INTERNAL:
          //  x_parameterTypeInternalLabel.Visibility = Visibility.Visible;
          //  x_parameterTypeInternalValue.Visibility = Visibility.Visible;
          //  x_paramTypeComboBox.Visibility = Visibility.Hidden;
          //  x_paramTypeInternalLabel.Visibility = Visibility.Visible;
          //  BuildDependentTypeField(param);
          //  break;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_param"></param>
    private void BuildDependentTypeField(EventParameter p_param)
    {
      if (Event != null)
      {
        //
        // Clear out the contents and build a new list
        //
        //x_parameterTypeDependentCombobox.Items.Clear();

        List<TriggerParameter> nonDependantParams = new List<TriggerParameter>();

        foreach (Trigger trigger in Event.Triggers)
        {
          foreach (TriggerParameter param in trigger.Parameters)
          {
            nonDependantParams.Add(param);
          }

          x_parameterTypeDependentCombobox.ItemsSource = nonDependantParams;
          SetSelectedDependentValue(p_param);
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_param"></param>
    private void SetSelectedDependentValue(EventParameter p_param)
    {
      bool bFound = false;
      foreach (TriggerParameter item in x_parameterTypeDependentCombobox.Items)
      {
        if (item != null)
        {
          if (item.ParamID == p_param.DependentParamID)
          {
            x_parameterTypeDependentCombobox.SelectedValue = item;
            bFound = true;
            break;
          }
        }
      }
      //
      // If there is no dependent param id, set it to the first
      // in the combo box
      //
      if (!bFound)
      {
        TriggerParameter firstItem =
          x_parameterTypeDependentCombobox.Items[0] as TriggerParameter;
        if (firstItem != null)
        {
          p_param.DependentParamID = firstItem.ParamID;
          x_parameterTypeDependentCombobox.SelectedIndex = 0;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ParamTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      BuildParameterTypeFields();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ParameterTypeDependentCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      TriggerParameter selectedDependant = x_parameterTypeDependentCombobox.SelectedItem as TriggerParameter;
      if (selectedDependant != null)
      {
        EventParameter param = DataContext as EventParameter;
        param.DependentParamID = selectedDependant.ParamID;
      }
    }
  }
}
