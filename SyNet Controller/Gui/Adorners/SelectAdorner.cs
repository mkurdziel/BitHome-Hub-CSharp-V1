using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using SyNet.Gui.Thumbs;

namespace SyNet.Gui.Adorners
{
  /// <summary>
  ///   Class for a GUI Dragging Adorner for drag and drop
  /// </summary>
  public class SelectAdorner : Adorner
  {
    private const int PADDING = 2;

    private VisualCollection m_visualChildren;
    private SelectThumb m_selectThumb;

    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="p_dragDropData"></param>
    /// <param name="p_dragDropTemplate"></param>
    /// <param name="p_adornedElement"></param>
    /// <param name="p_adornerLayer"></param>
    public SelectAdorner( UIElement p_adornedElement )
      : base(p_adornedElement)
    {
      m_visualChildren = new VisualCollection(this);

      // Create the select thumb
      m_selectThumb = new SelectThumb();
      m_selectThumb.Template = FindResource("SelectThumbTemplate") as ControlTemplate;

      // Add to the visual children
      m_visualChildren.Add(m_selectThumb);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size p_finalSize)
    {
      m_selectThumb.Width = this.AdornedElement.DesiredSize.Width + PADDING + PADDING;
      m_selectThumb.Height = this.AdornedElement.DesiredSize.Height + PADDING + PADDING;

      m_selectThumb.Arrange(new Rect(0-PADDING, 0-PADDING, m_selectThumb.Width, m_selectThumb.Height));

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