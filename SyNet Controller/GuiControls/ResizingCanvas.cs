using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SyNet.GuiHelpers;

namespace SyNet.GuiControls
{
  /// <summary>
  ///   Derives from Canvas and provides designing functionality
  /// </summary>
  public partial class ResizingCanvas : Canvas
  {
    //private IGuiMovable m_movableParent;
    private List<double> m_horizontalGuides = new List<double>();
    private List<double> m_verticalGuides = new List<double>();

    #region Public Properties

    public List<double> HorizontalGuides
    {
      get
      {
        return m_horizontalGuides;
      }
      set
      {
        m_horizontalGuides = value;
      }
    }

    public List<double> VerticalGuides
    {
      get
      {
        return m_verticalGuides;
      }
      set
      {
        m_verticalGuides = value;
      }
    }

    /// <summary>
    ///   Gets the maximum movement space that can occur on the left
    /// </summary>
    public double SpaceOnLeft
    {
      get
      {
        //if (m_movableParent != null)
        //{
        //  return m_movableParent.PositionLeft;
        //}
        return 0;
      }
    }

    /// <summary>
    ///   Gets the maximum movement space that can occur on the right
    /// </summary>
    public double SpaceOnTop
    {
      get
      {
        //if (m_movableParent != null)
        //{
        //  return m_movableParent.PositionTop;
        //}
        return 0;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   Default constructor
    /// </summary>
    public ResizingCanvas()
    {
      this.Loaded += ResizingCanvas_Loaded;
    }

    /// <summary>
    ///   Loaded event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ResizingCanvas_Loaded( object sender, RoutedEventArgs e )
    {
      //m_movableParent = Utilities.FindAncestor(typeof(IGuiMovable), this) as IGuiMovable;
    }

    #endregion

    #region Overrides

    /// <summary>
    ///   MeasureOverride override. Used to grow canvas when child objects
    ///   get too close to the edges
    /// </summary>
    /// <param name="constraint"></param>
    /// <returns></returns>
    protected override Size MeasureOverride( Size constraint )
    {
      Size size = new Size();

      double leftMost = double.NaN;
      double topMost = double.NaN;
      double rightMost = double.NaN;
      double bottomMost = double.NaN;

      foreach (UIElement element in this.InternalChildren)
      {
        double left = Canvas.GetLeft(element);
        double top = Canvas.GetTop(element);


        left = double.IsNaN(left) ? 0 : left;
        top = double.IsNaN(top) ? 0 : top;

        if (leftMost > left || double.IsNaN(leftMost))
        {
          leftMost = left;
        }
        if (topMost > top || double.IsNaN(topMost))
        {
          topMost = top;
        }

        //measure desired size for each child
        element.Measure(constraint);

        Size desiredSize = element.DesiredSize;
        if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
        {
          size.Width = Math.Max(size.Width, left + desiredSize.Width);
          size.Height = Math.Max(size.Height, top + desiredSize.Height);
        }
      }


      //
      // If the leftmost or topmost is off the left or topside of the canvas,
      // then adjust appropriatly
      //
      //if ((leftMost < 0 || topMost < 0) && m_movableParent != null)
      //{
      //  double left = (leftMost < 0) ? (0 - leftMost) : 0;
      //  double top = (topMost < 0) ? (0 - topMost) : 0;

      //  //
      //  // Make sure that the parent doesn't go out of bounds of its own parent
      //  //
      //  //if (movableParent.PositionLeft < left)
      //  //{
      //  //  left = movableParent.PositionLeft;
      //  //}

      //  //if (movableParent.PositionTop < top)
      //  //{
      //  //  top = movableParent.PositionTop;
      //  //}

      //  //
      //  // Move the parent left and up by the difference
      //  //
      //  m_movableParent.PositionLeft -= left;
      //  m_movableParent.PositionTop -= top;

      //  //
      //  // Move each item left by the difference
      //  //
      //  foreach (UIElement element in this.InternalChildren)
      //  {
      //    Canvas.SetLeft(element, Canvas.GetLeft(element) + left);
      //    Canvas.SetTop(element, Canvas.GetTop(element) + top);
      //  }

      //  //
      //  // Expand the parent by the difference
      //  size.Width += left;
      //  size.Height += top;
      //}

      //if ((leftMost > 0 || topMost > 0) && m_movableParent != null)
      //{
      //  double left = leftMost - 0;
      //  double top = topMost - 0;

      //  //
      //  // Move each item left by the difference
      //  //
      //  foreach (UIElement element in this.InternalChildren)
      //  {
      //    Canvas.SetLeft(element, Canvas.GetLeft(element) - left);
      //    Canvas.SetTop(element, Canvas.GetTop(element) - top);
      //  }

      //  //
      //  // Move the parent right by the difference
      //  //
      //  m_movableParent.PositionLeft += left;
      //  m_movableParent.PositionTop += top;
      //}
      return size;
    }

    protected override void OnRender( System.Windows.Media.DrawingContext dc )
    {
      base.OnRender(dc);

      foreach (double y in m_horizontalGuides)
      {
        Pen guide = new Pen(new SolidColorBrush(Colors.Blue), 1);
        dc.DrawLine(guide, new Point(0, y), new Point(this.ActualWidth, y));
      }

      foreach (double x in m_verticalGuides)
      {
        Pen guide = new Pen(new SolidColorBrush(Colors.Green), 1);
        dc.DrawLine(guide, new Point(x, 0), new Point(x, this.ActualHeight));
      }

    }

    #endregion
  }
}