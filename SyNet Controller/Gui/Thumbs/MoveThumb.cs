using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SyNet.Gui.Thumbs
{
  public class MoveThumb : Thumb
  {
    private GuiPanelItem m_item;

    public MoveThumb(UIElement p_element)
    {
      if (p_element is GuiPanelItem)
      {
        m_item = p_element as GuiPanelItem;
      }
    }

    /// <summary>
    ///   Set the background to the control
    /// </summary>
    public void SetBackgroundToControl()
    {
      Grid backgroundGrid = this.Template.FindName("templateBackground", this) as Grid;

      if (backgroundGrid != null)
      {
        Rect bounds = VisualTreeHelper.GetDescendantBounds(m_item);

        RenderTargetBitmap bitmap = new RenderTargetBitmap(
          (int)bounds.Width,
          (int)bounds.Height,
          96,
          96,
          PixelFormats.Default);

        DrawingVisual dv = new DrawingVisual();
        using (DrawingContext ctx = dv.RenderOpen())
        {
          VisualBrush vb = new VisualBrush(m_item);
          ctx.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
        }
        bitmap.Render(dv);

        ImageBrush imgBrush = new ImageBrush(bitmap);
        imgBrush.Opacity = 0.6;
        imgBrush.Stretch = Stretch.None;
        backgroundGrid.Background = imgBrush;
      }
    }
  }
}