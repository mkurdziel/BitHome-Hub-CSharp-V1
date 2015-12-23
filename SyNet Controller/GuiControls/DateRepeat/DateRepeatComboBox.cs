using System;
using System.Windows;
using System.Windows.Controls;
using SyNet.Events.Triggers;

namespace SyNet.GuiControls.DateRepeat
{
  /// <summary>
  ///   Specialized combobox showing psosible date repeat values
  /// </summary>
  public class DateRepeatComboBox : ComboBox
  {
    private DateTimeTrigger m_trigger = null;
    public DateRepeatComboBox()
    {
      AddDefaultItems();
      AddResourceDictionary();
    }

    private void AddResourceDictionary()
    {

    }

    public DateRepeatComboBox(DateTimeTrigger p_dateTimeTrigger) : this()
    {
    }

    private void AddDefaultItems()
    {
      AddComboBoxItem("Every Hour"); 
      AddComboBoxItem("Every Day"); 
      AddComboBoxItem("Every Week"); 
      AddComboBoxItem("Every Month");
      this.Items.Add(new Separator());
      AddComboBoxItem("Custom...");

    }

    /// <summary>
    ///   Add an item to this combobox
    /// </summary>
    /// <param name="p_strText"></param>
    private void AddComboBoxItem(String p_strText)
    {
      ComboBoxItem item = new ComboBoxItem();
      item.Content = p_strText;
      this.Items.Add(item);
    }
  }
}
