using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SyNet.GuiControls;

namespace SyNet.GuiHelpers.Thumbs
{
  public class ResizeThumb : Thumb
  {
    //private IGuiResizable resizableItem;
    private DesignerCanvas designerCanvas;

    public ResizeThumb()
    {
      DragDelta += this.ResizeThumb_DragDelta;
      this.Loaded += ResizeThumb_Loaded;
    }

    void ResizeThumb_Loaded( object sender, RoutedEventArgs e )
    {
      //this.resizableItem = DataContext as IGuiResizable;

      //if (this.resizableItem != null)
      //{
      //  this.designerCanvas = Utilities.FindAncestor(typeof (DesignerCanvas), 
      //                                               resizableItem as UIElement) as DesignerCanvas;
      //}
      //else
      //{
      //  this.Visibility = Visibility.Collapsed;
      //}
    }

    private void ResizeThumb_DragDelta( object sender, DragDeltaEventArgs e )
    {
      //if (this.resizableItem != null && this.designerCanvas != null && 
      //    this.resizableItem is IGuiSelectable && ((IGuiSelectable)this.resizableItem).IsSelected)
      //{
      //  double minLeft = double.MaxValue;
      //  double minTop = double.MaxValue;
      //  double minDeltaHorizontal = double.MaxValue;
      //  double minDeltaVertical = double.MaxValue;
      //  double dragDeltaVertical, dragDeltaHorizontal;

      //  foreach (IGuiResizable item in this.designerCanvas.SelectionService.CurrentSelection)
      //  {
      //    FrameworkElement element = item as FrameworkElement;
      //    if (element != null)
      //    {
      //      minLeft = Math.Min(Canvas.GetLeft(element), minLeft);
      //      minTop = Math.Min(Canvas.GetTop(element), minTop);

      //      minDeltaVertical = Math.Min(minDeltaVertical, element.ActualHeight - element.MinHeight);
      //      minDeltaHorizontal = Math.Min(minDeltaHorizontal, element.ActualWidth - element.MinWidth);
      //    }
      //  }

      //  foreach (IGuiResizable item in this.designerCanvas.SelectionService.CurrentSelection)
      //  {
      //    FrameworkElement element = item as FrameworkElement;
      //    if (element != null)
      //    {
      //      switch (VerticalAlignment)
      //      {
      //        case VerticalAlignment.Bottom:
      //          dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
      //          element.Height = element.ActualHeight - dragDeltaVertical;
      //          break;
      //        case VerticalAlignment.Top:
      //          dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
      //          Canvas.SetTop(element, Canvas.GetTop(element) + dragDeltaVertical);
      //          element.Height = element.ActualHeight - dragDeltaVertical;
      //          break;
      //      }

      //      switch (HorizontalAlignment)
      //      {
      //        case HorizontalAlignment.Left:
      //          dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
      //          Canvas.SetLeft(element, Canvas.GetLeft(element) + dragDeltaHorizontal);
      //          element.Width = element.ActualWidth - dragDeltaHorizontal;
      //          break;
      //        case HorizontalAlignment.Right:
      //          dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
      //          element.Width = element.ActualWidth - dragDeltaHorizontal;
      //          break;
      //      }
      //    }
      //  }

      //  e.Handled = true;
      //}
    }
  }
}