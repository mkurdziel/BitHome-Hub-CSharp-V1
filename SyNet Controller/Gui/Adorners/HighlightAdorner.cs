using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Size = System.Windows.Size;

namespace SyNet.Gui.Adorners
{
  /// <summary>
  ///   Class for a GUI Dragging Adorner for drag and drop
  /// </summary>
  public class HighlightAdorner : Adorner
  {
    private const int PADDING = 2;

    private VisualCollection m_visualChildren;
    private Border m_border;

    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="p_dragDropData"></param>
    /// <param name="p_dragDropTemplate"></param>
    /// <param name="p_adornedElement"></param>
    /// <param name="p_adornerLayer"></param>
    public HighlightAdorner(UIElement p_adornedElement)
      : base(p_adornedElement)
    {
      this.SnapsToDevicePixels = true;

      m_visualChildren = new VisualCollection(this);

      // Create the select thumb
      m_border = new Border();
      m_border.SnapsToDevicePixels = true;

      m_border.Style = FindResource("HighlightBorderStyle") as Style;

      // Add to the visual children
      m_visualChildren.Add(m_border);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size p_finalSize)
    {
      m_border.Width = this.AdornedElement.DesiredSize.Width + PADDING + PADDING;
      m_border.Height = this.AdornedElement.DesiredSize.Height + PADDING + PADDING;

      m_border.Arrange(new Rect(0 - PADDING, 0 - PADDING, m_border.Width, m_border.Height));

      return p_finalSize;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_index"></param>
    /// <returns></returns>
    protected override Visual GetVisualChild(int p_index)
    {
      return m_visualChildren[p_index];
    }

    /// <summary>
    /// 
    /// </summary>
    protected override int VisualChildrenCount
    {
      get { return m_visualChildren.Count; }
    }
  }
}