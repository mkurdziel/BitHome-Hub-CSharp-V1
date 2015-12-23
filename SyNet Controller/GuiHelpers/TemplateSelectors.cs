using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SyNet.GuiHelpers
{
  /// <summary>
  ///   Action List template selector
  /// </summary>
  public class ActionListTemplateSelector : DataTemplateSelector
  {
    /// <summary>
    ///   Datatemplate for Action
    /// </summary>
    public DataTemplate ActionTemplate { get; set; }

    /// <summary>
    ///   Datatemplate for parameter
    /// </summary>
    public DataTemplate ParameterTemplate { get; set; }

    /// <summary>
    ///   SelectTemplate method
    /// </summary>
    /// <param name="item"></param>
    /// <param name="container"></param>
    /// <returns></returns>
    public override DataTemplate SelectTemplate(object item,
        DependencyObject container)
    {
      Debug.WriteLine(item);

      return base.SelectTemplate(item, container);
    }
  }
}
