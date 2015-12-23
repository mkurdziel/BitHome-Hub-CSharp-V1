using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SyNet.Gui.Models;
using SyNet.GuiHelpers.DragDropManager;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace SyNet.Gui
{
  public class DragDropGuiTargetAdvisor : IDropTargetAdvisor
  {
    #region Implementation of IDropTargetAdvisor

    public UIElement TargetUI { get; set; }

    public bool ApplyMouseOffset
    {
      get
      {
        return false;
      }
    }

    public bool IsValidDataObject( IDataObject obj )
    {
      return GetExtractedObject(obj) != null;
    }

    public void OnDropCompleted( IDataObject obj, Point dropPoint )
    {
      GuiPanelDesigner designer =
        GuiHelpers.Utilities.FindAncestor(typeof (GuiPanelDesigner), TargetUI) as GuiPanelDesigner;

      if (designer != null)
      {
        GuiPanel panel = designer.GuiPanel;

        if (panel != null)
        {
          object dropObject = GetExtractedObject(obj);

          designer.AddObject(dropObject, dropPoint);
        }
      }
    }

    public UIElement GetVisualFeedback( IDataObject obj )
    {
      Object dataObj = GetExtractedObject(obj);

      Rectangle rect = new Rectangle();
     

      if (dataObj != null)
      {
        if (dataObj is Actions.Action)
        {
          ImageBrush brush = new ImageBrush();
          brush.ImageSource = new BitmapImage(
            new Uri("pack://application:,,,/Resources/Actions/Action.png")) ;

          rect.Width = 32;
          rect.Height = 32;
          rect.Fill = brush;
          rect.Opacity = 0.5;
          rect.IsHitTestVisible = false;
        }
        else if (dataObj is Events.Triggers.Trigger)
        {
          ImageBrush brush = new ImageBrush();
          brush.ImageSource = new BitmapImage(
            new Uri("pack://application:,,,/Resources/Events/Trigger.png"));

          rect.Width = 32;
          rect.Height = 32;
          rect.Fill = brush;
          rect.Opacity = 0.5;
          rect.IsHitTestVisible = false;
        }
        else
        {
          rect.Width = 10;
          rect.Height = 10;
          rect.Fill = new SolidColorBrush(Colors.Red);
          rect.Opacity = 0.5;
          rect.IsHitTestVisible = false;
        }
      }


      return rect;
    }

    public UIElement GetTopContainer()
    {
      return GuiHelpers.Utilities.FindAncestor(typeof(DesignerCanvas), TargetUI);
    }

    private object GetExtractedObject( IDataObject p_obj )
    {
      DataObject dataObject = p_obj as DataObject;

      if (dataObject == null) return null;

      foreach (string format in p_obj.GetFormats())
      {
        Type formatType = Type.GetType(format);
        if ( formatType != null && 
             (formatType.IsSubclassOf(typeof(Actions.Action)) ||
              formatType.IsSubclassOf(typeof(Events.Triggers.Trigger)))
          )
        {
          return p_obj.GetData(format);
        }
      }

      return null;
    }

    #endregion
  }
}
