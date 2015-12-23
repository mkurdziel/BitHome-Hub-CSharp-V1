using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Size = System.Windows.Size;

namespace SyNet.Gui.Adorners
{
  /// <summary>
  ///   Class for a GUI Dragging Adorner for drag and drop
  /// </summary>
  public class GuideAdorner : Adorner
  {
    public enum EsnGuideType
    {
      Guide,
      Margin
    }

    private VisualCollection m_visualChildren;
    private Line m_line;

    /// <summary>
    ///   Constructor
    /// </summary>
    public GuideAdorner( UIElement p_adornedElement, Line p_line, EsnGuideType p_type )
      : base(p_adornedElement)
    {
      m_visualChildren = new VisualCollection(this);

      m_line = p_line;

      m_line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

      switch (p_type)
      {
        case EsnGuideType.Margin:
          m_line.Style = FindResource("GuideLineMarginStyle") as Style;
          break;
        default:
          m_line.Style = FindResource("GuideLineStyle") as Style;
          break;
      }

      // Add to the visual children
      m_visualChildren.Add(m_line);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride( Size p_finalSize )
    {
      m_line.Arrange(new Rect(0, 0, p_finalSize.Width, p_finalSize.Height));

      return p_finalSize;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_index"></param>
    /// <returns></returns>
    protected override Visual GetVisualChild( int p_index )
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