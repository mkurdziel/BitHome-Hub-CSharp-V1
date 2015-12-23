using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SyNet.GuiHelpers
{
  /// <summary>
  ///   Button that displays only an image
  /// </summary>
  public class ImageButton : Button
  {
    /// <summary>
    ///   Source of the image to display on the button
    /// </summary>
    public ImageSource Source
    {
      get { return base.GetValue(SourceProperty) as ImageSource; }
      set { base.SetValue(SourceProperty, value); }
    }

    /// <summary>
    ///   Dependency property to set the image source
    /// </summary>
    public static readonly DependencyProperty SourceProperty =
      DependencyProperty.Register("Source", typeof(ImageSource),
                                  typeof(ImageButton));
  }
}
