using System.Windows;
using System.Windows.Controls.Primitives;
using SyNet.GuiControls;

namespace SyNet.Gui.Thumbs
{
  public class SelectThumb : Thumb
  {
    public SelectThumb()
    {
    }

    /// <summary>
    ///   Double click mouse handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    //private void MoveThumb_PreviewMouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
    //{
    //  if (m_designerCanvas != null)
    //  {
    //    m_designerCanvas.TunnelDoubleClick(e);
    //  }
    //}

    /// <summary>
    ///   Loaded event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SelectThumb_Loaded(object sender, RoutedEventArgs e)
    {
      //if (SelectableItem != null)
      //{
      //  m_designerCanvas = Utilities.FindAncestor(typeof(DesignerCanvas), SelectableItem as UIElement) as DesignerCanvas;
      //}
    }
  }
}