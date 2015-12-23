using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SyNet.Gui.Adorners
{
  /// <summary>
  ///   Addorner for the rubber band lasso
  /// </summary>
  public class RubberbandAdorner : Adorner
  {
    private Point? startPoint;
    private Point? endPoint;
    private Pen rubberbandPen;
    private DesignerCanvas designerCanvas;

    /// <summary>
    ///   Default constructor
    /// </summary>
    /// <param name="designerCanvas"></param>
    /// <param name="dragStartPoint"></param>
    public RubberbandAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
      : base(designerCanvas)
    {
      this.SnapsToDevicePixels = true;
      this.designerCanvas = designerCanvas;
      this.startPoint = dragStartPoint;
      rubberbandPen = new Pen(Brushes.Black, 1);
      //rubberbandPen.DashStyle = new DashStyle(new double[] { 1, 2 }, 1);
    }

    /// <summary>
    ///   Mouse move handler
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        if (!this.IsMouseCaptured)
          this.CaptureMouse();

        endPoint = e.GetPosition(this);
        this.InvalidateVisual();
      }
      else
      {
        if (this.IsMouseCaptured) this.ReleaseMouseCapture();
      }

      e.Handled = true;
    }

    /// <summary>
    ///   Mouse up event handler
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
    {
      // release mouse capture
      if (this.IsMouseCaptured) this.ReleaseMouseCapture();

      UpdateSelection();

      // remove this adorner from adorner layer
      AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.designerCanvas);
      if (adornerLayer != null)
        adornerLayer.Remove(this);

      e.Handled = true;
    }

    /// <summary>
    ///   OnRender override
    /// </summary>
    /// <param name="dc"></param>
    protected override void OnRender(DrawingContext dc)
    {
      base.OnRender(dc);

      // without a background the OnMouseMove event would not be fired!
      // Alternative: implement a Canvas as a child of this adorner, like
      // the ConnectionAdorner does.
      dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

      if (this.startPoint.HasValue && this.endPoint.HasValue)
        dc.DrawRectangle(
          Brushes.Transparent, rubberbandPen, new Rect(
                                                new Point((int) this.startPoint.Value.X, (int) this.startPoint.Value.Y),
                                                new Point((int) this.endPoint.Value.X, (int) this.endPoint.Value.Y)));
    }

    private void UpdateSelection()
    {
      if (startPoint.HasValue && endPoint.HasValue)
      {
        designerCanvas.GuiPanelDesigner.SelectionService.ClearSelection();

        Rect rubberBand = new Rect(startPoint.Value, endPoint.Value);

        designerCanvas.GuiPanelDesigner.SelectWithinRectangle(startPoint.Value, rubberBand);
      }
    }
  }
}