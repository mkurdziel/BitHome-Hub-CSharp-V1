using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SyNet.Actions;
using Action = SyNet.Actions.Action;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for ParameterEditPanel.xaml
  /// </summary>
  public partial class ParameterEditPanel : UserControl
  {
    /// <summary>
    ///   Constructor
    /// </summary>
    public ParameterEditPanel()
    {
      DataContext = null;

      InitializeComponent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ParameterSetupGrid_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      //
      // Populate the type combo box
      //
      x_paramTypeComboBox.Items.Clear();
      foreach (
        object o in
          Enum.GetValues(typeof(ActionParameter.EsnActionParameterType)))
      {
        if ((ActionParameter.EsnActionParameterType)o != ActionParameter.EsnActionParameterType.INTERNAL)
          x_paramTypeComboBox.Items.Add(o);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private void BuildParameterTypeFields()
    {
      ActionParameter param = DataContext as ActionParameter;

      if (param != null)
      {
        x_paramTypeInternalLabel.Visibility = Visibility.Hidden;
        x_paramTypeComboBox.Visibility = Visibility.Visible;

        x_parameterConstant.Visibility = Visibility.Hidden;
        //x_parameterTypeConstantTextbox.Visibility = Visibility.Hidden;
        x_parameterTypeConstantLabel.Visibility = Visibility.Hidden;
        x_parameterTypeDependentCombobox.Visibility = Visibility.Hidden;
        x_parameterTypeDependentLabel.Visibility = Visibility.Hidden;
        x_parameterTypeInternalLabel.Visibility = Visibility.Hidden;
        x_parameterTypeInternalValue.Visibility = Visibility.Hidden;

        switch (param.ParameterType)
        {
          case ActionParameter.EsnActionParameterType.CONSTANT:
            x_parameterConstant.Visibility = Visibility.Visible;
            //x_parameterTypeConstantTextbox.Visibility = Visibility.Visible;
            x_parameterTypeConstantLabel.Visibility = Visibility.Visible;
            break;
          case ActionParameter.EsnActionParameterType.DEPENDENT:
            x_parameterTypeDependentCombobox.Visibility = Visibility.Visible;
            x_parameterTypeDependentLabel.Visibility = Visibility.Visible;
            BuildDependentTypeField(param);
            break;
          case ActionParameter.EsnActionParameterType.INTERNAL:
            x_parameterTypeInternalLabel.Visibility = Visibility.Visible;
            x_parameterTypeInternalValue.Visibility = Visibility.Visible;
            x_paramTypeComboBox.Visibility = Visibility.Hidden;
            x_paramTypeInternalLabel.Visibility = Visibility.Visible;
            BuildDependentTypeField(param);
            break;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_param"></param>
    private void BuildDependentTypeField( ActionParameter p_param )
    {
      if (Action != null)
      {
        //
        // Clear out the contents and build a new list
        //
        //x_parameterTypeDependentCombobox.Items.Clear();

        List<ActionParameter> nonDependantParams = new List<ActionParameter>();
        foreach (ActionParameter param in Action.Parameters)
        {
          if (param.ParameterType != ActionParameter.EsnActionParameterType.DEPENDENT)
          {
            nonDependantParams.Add(param);
          }
        }
        x_parameterTypeDependentCombobox.ItemsSource = nonDependantParams;
        SetSelectedDependentValue(p_param);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_param"></param>
    private void SetSelectedDependentValue( ActionParameter p_param )
    {
      bool bFound = false;
      foreach (ActionParameter item in x_parameterTypeDependentCombobox.Items)
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
        ActionParameter firstItem =
          x_parameterTypeDependentCombobox.Items[0] as ActionParameter;
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
    private void ParamTypeComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      BuildParameterTypeFields();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ParameterTypeDependentCombobox_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      ActionParameter selectedDependant = x_parameterTypeDependentCombobox.SelectedItem as ActionParameter;
      if (selectedDependant != null)
      {
        ActionParameter param = DataContext as ActionParameter;
        param.DependentParamID = selectedDependant.ParamID;
      }
    }

    /// <summary>
    ///   Action whose parameters we're editing
    /// </summary>
    public Action Action
    {
      get { return base.GetValue(ActionProperty) as Action; }
      set { base.SetValue(ActionProperty, value); }
    }

    /// <summary>
    ///   Dependency property for the action
    /// </summary>
    public static readonly DependencyProperty ActionProperty =
      DependencyProperty.Register("Action", typeof(Action),
                                  typeof(ParameterEditPanel));
  }
}
