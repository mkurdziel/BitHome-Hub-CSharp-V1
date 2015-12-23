using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using SyNet.GuiControls;
using Color=System.Drawing.Color;

namespace SyNet.GuiHelpers.Thumbs
{
  /// <summary>
  ///   Class deriving from thumb that's responsible for dragmoving the object
  ///   in its DataContext
  /// </summary>
  public class MoveThumb : Thumb
  {
    //private IGuiMovable m_movableItem;
    private DesignerCanvas m_designerCanvas;
    private ResizingCanvas m_localCanvas;

    /// <summary>
    ///   Gets an IGuiMoveableItem from the datacontext or returns null
    ///   if not possible
    /// </summary>
    //private IGuiMovable MovableItem
    //{
    //  get
    //  {
    //    if (m_movableItem == null)
    //    {
    //      m_movableItem = DataContext as IGuiMovable;
    //    }
    //    return m_movableItem;
    //  }
    //}

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public MoveThumb()
    {
      Loaded += MoveThumb_Loaded;
    }


    /// <summary>
    ///   Double click mouse handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveThumb_PreviewMouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
    {
      if (m_designerCanvas != null)
      {
        m_designerCanvas.TunnelDoubleClick(e);
      }
    }

    /// <summary>
    ///   Loaded event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveThumb_Loaded( object sender, RoutedEventArgs e )
    {
      //if (MovableItem != null)
      //{
      //  DragDelta += MoveThumb_DragDelta;

      //  DragCompleted += MoveThumb_DragCompleted;

      //  m_designerCanvas = Utilities.FindAncestor(typeof(DesignerCanvas), MovableItem as UIElement) as DesignerCanvas;

      //  m_localCanvas = Utilities.FindAncestor(typeof(ResizingCanvas), MovableItem as UIElement) as ResizingCanvas;

      //  PreviewMouseDoubleClick += MoveThumb_PreviewMouseDoubleClick;
      //}
    }

    /// <summary>
    ///   Drag completed event handler
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void MoveThumb_DragCompleted(object p_sender, DragCompletedEventArgs p_e)
    {
      m_localCanvas.HorizontalGuides.Clear();
      m_localCanvas.VerticalGuides.Clear();
      m_localCanvas.InvalidateVisual();
    }

    /// <summary>
    ///   Drag delta event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveThumb_DragDelta( object sender, DragDeltaEventArgs e )
    {

      //if (this.MovableItem != null &&
      //  m_designerCanvas != null &&
      //  m_localCanvas != null &&
      //  ((SelectableControl)this.m_movableItem).IsSelected)
      //{
      //  double minLeft = double.MaxValue;
      //  double minTop = double.MaxValue;

      //  //
      //  // Get the leftmost position in the selected
      //  //
      //  foreach (IGuiMovable item in this.m_designerCanvas.SelectionService.CurrentSelection)
      //  {
      //    minLeft = Math.Min(item.PositionLeft, minLeft);
      //    minTop = Math.Min(item.PositionTop, minTop);
      //  }

      //  double deltaHorizontal = e.HorizontalChange;
      //  double deltaVertical = e.VerticalChange;

      //  double desiredMinLeft = minLeft + e.HorizontalChange;
      //  double desiredMinTop = minTop + e.VerticalChange;

      //  //
      //  // If either movement produces a result less than what the canvas will
      //  // allow, adjust it to the canvas min
      //  //
      //  if ((desiredMinLeft < 0 && (desiredMinLeft + m_localCanvas.SpaceOnLeft) < 0))
      //  {
      //    deltaHorizontal = Math.Min(-m_localCanvas.SpaceOnLeft, -minLeft);
      //  }

      //  if ((desiredMinTop < 0 && (desiredMinTop + m_localCanvas.SpaceOnTop) < 0))
      //  {
      //    deltaVertical = Math.Min(-m_localCanvas.SpaceOnTop, -minTop);
      //  }

      //  //Debug.WriteLine("DH:" + deltaHorizontal + " PL:"+ this.MovableItem.PositionLeft);

      //  //
      //  // Align along the vertical
      //  //
      //  m_localCanvas.VerticalGuides.Clear();
      //  foreach (UIElement child in m_localCanvas.Children)
      //  {
      //    if (this.MovableItem != child)
      //    {
      //      double tempDelta = Canvas.GetLeft(child) - (this.MovableItem.PositionLeft + deltaHorizontal);
      //      if (Math.Abs(tempDelta) < 8)
      //      {
      //        deltaHorizontal = Canvas.GetLeft(child) - this.MovableItem.PositionLeft;
      //        m_localCanvas.VerticalGuides.Add(Canvas.GetLeft(child));
      //        break;
      //      }
      //    }
      //  }

      //  //
      //  // Align along the horizontal
      //  //
      //  m_localCanvas.HorizontalGuides.Clear();
      //  foreach (UIElement child in m_localCanvas.Children)
      //  {
      //    if (this.MovableItem != child)
      //    {
      //      double tempDelta = Canvas.GetTop(child) - (this.MovableItem.PositionTop + deltaVertical);
      //      if (Math.Abs(tempDelta) < 8)
      //      {
      //        deltaVertical = Canvas.GetTop(child) - this.MovableItem.PositionTop;
      //        m_localCanvas.HorizontalGuides.Add(Canvas.GetTop(child));
      //        break;
      //      }
      //    }
      //  }

      //  //
      //  // Go through and move everything
      //  //
      //  foreach (IGuiMovable item in this.m_designerCanvas.SelectionService.CurrentSelection)
      //  {
      //    item.PositionLeft += deltaHorizontal;
      //    item.PositionTop += deltaVertical;
      //  }

      //  //
      //  // Tell the parent canvas to remeasure itself
      //  //
      //  m_localCanvas.InvalidateMeasure();
      //  m_localCanvas.InvalidateVisual();
      //}

      ////
      //// Prevent the event from tunneling any further
      ////
      //e.Handled = true;
    }
  }
}