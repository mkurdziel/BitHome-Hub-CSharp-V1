using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SyNet.Gui.Adorners;
using SyNet.Gui.Interfaces;

namespace SyNet.Gui
{
  /// <summary>
  ///   Derives from Canvas and provides designing functionality
  /// </summary>
  public class DesignerCanvas : Canvas
  {

    private List<object> m_HitTestResults = new List<object>();
    private List<GuiPanelItem> m_selectedObjects = new List<GuiPanelItem>();
    private Point? m_rubberbandSelectionStartPoint = null;

    public GuiPanelDesigner GuiPanelDesigner { get; set; }

    /// <summary>
    ///   PreviewMouseDown Handler
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseDown(e);

      HandleMouseDown(e, false);

      e.Handled = false;
    }

    /// <summary>
    ///   Mouse move override
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);

      // if mouse button is not pressed we have no drag operation, ...
      if (e.LeftButton != MouseButtonState.Pressed)
        m_rubberbandSelectionStartPoint = null;

      // ... but if mouse button is pressed and start
      // point value is set we do have one
      if (m_rubberbandSelectionStartPoint.HasValue)
      {
        // create rubberband adorner
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer != null)
        {
          RubberbandAdorner adorner = new RubberbandAdorner(this, m_rubberbandSelectionStartPoint);
          if (adorner != null)
          {
            adornerLayer.Add(adorner);
          }
        }
      }
      e.Handled = true;
    }

    /// <summary>
    ///   Look under the mouse point and see if the item is a container
    /// </summary>
    /// <param name="p_point"></param>
    /// <returns></returns>
    public GuiPanelItem GetContainerUnderMouse(Point p_point, GuiPanelItem p_droppedItem, bool p_getFirst)
    {
      GuiPanelItem item = null;

      m_HitTestResults.Clear();
      m_selectedObjects.Clear();

      //
      // Get the hit test result from the canvas which wlil return all elements
      // that are between the mouse click and the canvas
      //
      VisualTreeHelper.HitTest(this,
          new HitTestFilterCallback(OnHitTestFilter),
          new HitTestResultCallback(OnHitTestBehavior),
          new PointHitTestParameters(p_point));

      //
      // Iterate through all hittest results and find those that are configurable
      //
      foreach (object hit in m_HitTestResults)
      {
        UIElement element = hit as UIElement;
        if (element != null)
        {
          FindSelectable(element);
        }
      }

      //
      // If there are configurable objects that are hit, investigate
      // Otherwise, we want to select the panel
      //
      if (m_selectedObjects.Count > 0)
      {
        if (p_getFirst)
        {
          //
          // If the top item is not the dropped item, see if its a container
          //
          if (m_selectedObjects[0] != p_droppedItem &&
              m_selectedObjects[0].IsContainer)
          {
            item = m_selectedObjects[0];
          }
          else if (m_selectedObjects[0] == p_droppedItem && m_selectedObjects.Count > 1 &&
                   m_selectedObjects[1].IsContainer)
          {
            item = m_selectedObjects[1];
          }
        }
        else
        {
          foreach (GuiPanelItem panelItem in m_selectedObjects)
          {
            if (panelItem.IsContainer && panelItem != p_droppedItem && p_droppedItem.IsUnaffiliated)
            {
              item = panelItem;
              break;
            }
          }
        }
      }
      return item;
    }

    /// <summary>
    ///   Handle mouse down events
    /// </summary>
    /// <param name="e"></param>
    /// <param name="p_isDoubleClick"></param>
    private void HandleMouseDown(MouseButtonEventArgs e, bool p_isDoubleClick)
    {
      // Hack here, why is the event raised on the parent as well???
      //if (e.OriginalSource == this)
      //{
      //  return;
      //}

      // Make sure we're in editing mode
      if (this.GuiPanelDesigner.IsEditing == false)
      {
        return;
      }

      //
      // Get the position from the canvas
      //
      Point pt = e.GetPosition(this);
      m_HitTestResults.Clear();
      m_selectedObjects.Clear();

      //
      // Get the hit test result from the canvas which wlil return all elements
      // that are between the mouse click and the canvas
      //
      VisualTreeHelper.HitTest(this,
          new HitTestFilterCallback(OnHitTestFilter),
          new HitTestResultCallback(OnHitTestBehavior),
          new PointHitTestParameters(pt));

      //
      // Iterate through all hittest results and find those that are configurable
      //
      foreach (object hit in m_HitTestResults)
      {
        UIElement element = hit as UIElement;
        if (element != null)
        {
          FindSelectable(element);
        }
      }

      //
      // If there are configurable objects that are hit, investigate
      // Otherwise, we want to select the panel
      //
      if (m_selectedObjects.Count > 0)
      {
        this.GuiPanelDesigner.SelectionService.RequestSelection(m_selectedObjects[0]);
      }
      else
      {
        // if you click directly on the canvas all 
        // selected items are 'de-selected'
        this.GuiPanelDesigner.SelectionService.ClearSelection();
      }
      //else
      //{
      // in case that this click is the start of a 
      // drag operation we cache the start point
      this.m_rubberbandSelectionStartPoint = new Point?(e.GetPosition(this));
      e.Handled = true;
      //}

      Focus();
    }

    private void FindSelectable(DependencyObject p_object)
    {
      if (p_object == null) return;
      if (p_object is GuiPanelItem)
      {
        GuiPanelItem item = p_object as GuiPanelItem;

        if (!m_selectedObjects.Contains(item))
        {
          m_selectedObjects.Add(item);
        }
        return;
      }
      FindSelectable(VisualTreeHelper.GetParent(p_object));
    }

    /// <summary>
    ///   Hittest filter
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    private HitTestFilterBehavior OnHitTestFilter(DependencyObject o)
    {
      UIElement element = o as UIElement;
      if (element == this)
      {
        return HitTestFilterBehavior.ContinueSkipSelf;
      }

      //
      // Filter the element and make sure it's selectable
      //
      if (element != null && element.IsHitTestVisible && element != this)
      {

        return HitTestFilterBehavior.Continue;
      }

      return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
    }

    /// <summary>
    ///   Hittest behavior
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private HitTestResultBehavior OnHitTestBehavior(HitTestResult result)
    {
      // Add the hit test result to the list that will be processed after the enumeration.
      m_HitTestResults.Add(result.VisualHit);
      // Set the behavior to return visuals at all z-order levels.
      return HitTestResultBehavior.Continue;
    }

  }
}