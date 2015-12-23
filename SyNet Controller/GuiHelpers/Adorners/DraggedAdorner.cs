using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SyNet.GuiHelpers.Adorners
{
  /// <summary>
  ///   Class for a GUI Dragging Adorner for drag and drop
  /// </summary>
  public class DraggedAdorner : Adorner
  {
    private readonly ContentPresenter m_contentPresenter;
    private double m_left;
    private double m_top;
    private readonly AdornerLayer m_adornerLayer;

    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="p_dragDropData"></param>
    /// <param name="p_dragDropTemplate"></param>
    /// <param name="p_adornedElement"></param>
    /// <param name="p_adornerLayer"></param>
    public DraggedAdorner( object p_dragDropData,
                           DataTemplate p_dragDropTemplate,
                           UIElement p_adornedElement, 
                           AdornerLayer p_adornerLayer )
      : base(p_adornedElement)
    {
      this.m_adornerLayer = p_adornerLayer;

      this.m_contentPresenter = new ContentPresenter();
      this.m_contentPresenter.Content = p_dragDropData;
      this.m_contentPresenter.ContentTemplate = p_dragDropTemplate;
      this.m_contentPresenter.Opacity = 0.7;

      this.m_adornerLayer.Add(this);
    }

    /// <summary>
    ///   Set the position of the adorner
    /// </summary>
    /// <param name="p_left"></param>
    /// <param name="p_top"></param>
    public void SetPosition( double p_left, double p_top )
    {
      this.m_left = p_left;
      this.m_top = p_top;
      if (this.m_adornerLayer != null)
      {
        this.m_adornerLayer.Update(this.AdornedElement);
      }
    }

    /// <summary>
    ///   
    /// </summary>
    /// <param name="p_constraint"></param>
    /// <returns></returns>
    protected override Size MeasureOverride( Size p_constraint )
    {
      this.m_contentPresenter.Measure(p_constraint);
      return this.m_contentPresenter.DesiredSize;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride( Size p_finalSize )
    {
      this.m_contentPresenter.Arrange(new Rect(p_finalSize));
      return p_finalSize;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_index"></param>
    /// <returns></returns>
    protected override Visual GetVisualChild( int p_index )
    {
      return this.m_contentPresenter;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override int VisualChildrenCount
    {
      get { return 1; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_transform"></param>
    /// <returns></returns>
    public override GeneralTransform GetDesiredTransform( GeneralTransform p_transform )
    {
      GeneralTransformGroup result = new GeneralTransformGroup();
      result.Children.Add(base.GetDesiredTransform(p_transform));
      result.Children.Add(new TranslateTransform(this.m_left, this.m_top));

      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Detach()
    {
      this.m_adornerLayer.Remove(this);
    }

  }
}