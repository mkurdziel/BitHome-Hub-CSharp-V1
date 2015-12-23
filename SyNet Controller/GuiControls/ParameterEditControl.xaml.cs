using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SyNet.GuiControls
{
  /// <summary>
  /// Interaction logic for ParameterEditControl.xaml
  /// </summary>
  public partial class ParameterEditControl : UserControl
  {
    private Parameter m_parameter;


    private UIElement ContentUIElement
    {
      get
      {
        return this.Content as UIElement;
      }
    }

    /// <summary>
    ///   Initialization constructor
    /// </summary>
    /// <param name="p_parameter"></param>
    public ParameterEditControl(Parameter p_parameter)
      : this()
    {
      DataContext = p_parameter;
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public ParameterEditControl()
    {
      DataContext = null;

      InitializeComponent();

      this.DataContextChanged += ParameterEditControl_DataContextChanged;
    }

    /// <summary>
    ///   DataContext changed chandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ParameterEditControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
      // Clear the former content
      this.Content = null;

      // Make sure we have a parameter here, otherwise add a disabled text box
      m_parameter = DataContext as Parameter;

      if (m_parameter == null)
      {
        AddTextBox();

        if (ContentUIElement != null)
        {
          ContentUIElement.IsEnabled = false;
        }
      }
      else
      {
        CreateParameterContent();
      }
    }

    /// <summary>
    ///   Create the content
    /// </summary>
    private void CreateParameterContent()
    {
      switch (m_parameter.ValidationType)
      {
        case Protocol.EsnParamValidationType.BOOL:
          {
            AddCheckBox();
          }
          break;
        case Protocol.EsnParamValidationType.DATE_TIME:
          {
            AddDateTimeControl();
          }
          break;
        case Protocol.EsnParamValidationType.ENUMERATED:
          {
            AddEnumeratedCombobox();
          }
          break;
        case Protocol.EsnParamValidationType.UNSIGNED_FULL:
        case Protocol.EsnParamValidationType.UNSIGNED_RANGE:
        case Protocol.EsnParamValidationType.SIGNED_FULL:
        case Protocol.EsnParamValidationType.SIGNED_RANGE:
        case Protocol.EsnParamValidationType.UNKNOWN:
          {
            AddTextBox();
          }
          break;
      }
    }

    private void AddDateTimeControl()
    {

      DateTimeEntryBox box = new DateTimeEntryBox();

      box.DateTime = new DateTime(m_parameter.IntValue);
      box.DateTimeChanged += new RoutedEventHandler(DateTimeEntryBox_DateTimeChanged);
      this.Content = box;
    }

    /// <summary>
    ///   Add a combobox for an enumerated value
    /// </summary>
    private void AddEnumeratedCombobox()
    {
      ComboBox combobox = new ComboBox();

      if (m_parameter != null)
      {
        foreach (KeyValuePair<string, int> pair in m_parameter.DctEnumValueByName)
        {
          combobox.Items.Add(pair.Key);

          if (pair.Value == m_parameter.IntValue)
          {
            combobox.SelectedValue = pair.Key;  
          }
        }
      }

      combobox.SelectionChanged += Combobox_SelectionChanged;

      this.Content = combobox;
    }

    private void AddTextBox()
    {
      TextBox textbox = new TextBox();

      //
      // Setup the binding and validation rules
      //
      if (m_parameter != null)
      {
        Binding valueBinding = new Binding("StringValue");
        valueBinding.Source = m_parameter;
        valueBinding.ValidatesOnExceptions = true;
        valueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        textbox.SetBinding(TextBox.TextProperty, valueBinding);
      }

      this.Content = textbox;
    }

    private void AddCheckBox()
    {
      CheckBox checkbox = new CheckBox();

      if (m_parameter != null)
      {
        checkbox.Checked += Checkbox_Checked;
        checkbox.Unchecked += Checkbox_Unchecked;

        if (m_parameter.IntValue == 1)
        {
          checkbox.IsChecked = true;
        }
        else
        {
          checkbox.IsChecked = false;
        }
      }

      this.Content = checkbox;
    }

    /// <summary>
    ///   Combobox selection changed handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ComboBox combobox = sender as ComboBox;
      if (combobox != null)
      {
        if (m_parameter.DctEnumValueByName.ContainsKey(combobox.SelectedValue as String))
        {
          m_parameter.IntValue = m_parameter.DctEnumValueByName[combobox.SelectedValue as String];
        }
        else
        {
          Debug.WriteLine("[ERR] ParameterEditControl - Combobox SelectionChanged - invalid enum");
        }
      }
    }

    private void Checkbox_Unchecked(object sender, RoutedEventArgs e)
    {
      if (m_parameter != null)
      {
        m_parameter.IntValue = 0;
      }
    }

    private void Checkbox_Checked(object sender, RoutedEventArgs e)
    {
      if (m_parameter != null)
      {
        m_parameter.IntValue = 1;
      }
    }

    void DateTimeEntryBox_DateTimeChanged( object sender, RoutedEventArgs e )
    {
      DateTimeEntryBox box = sender as DateTimeEntryBox;
      if (box != null && m_parameter != null)
      {
        m_parameter.IntValue = box.DateTime.Ticks;
      }
    }
  }
}
