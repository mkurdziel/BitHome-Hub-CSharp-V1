using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SyNet.Gui;
using SyNet.GuiHelpers;
using SyNet.GuiHelpers.Adorners;
using SelectionService=SyNet.GuiHelpers.SelectionService;

namespace SyNet.GuiControls
{
  /// <summary>
  ///   Derives from Canvas and provides designing functionality
  /// </summary>
  public partial class DesignerCanvas : ResizingCanvas
  {
    private Point? m_rubberbandSelectionStartPoint = null;
    private bool m_isEditing;
    private SelectionService m_selectionService;
    private AdornerLayer m_adornerLayer;

    private List<object> m_HitTestResults = new List<object>();
    //private List<IGuiSelectable> m_selectedObjects = new List<IGuiSelectable>();


    #region Properties

    /// <summary>
    ///   The SelectionService for the canvas
    /// </summary>
    internal SelectionService SelectionService
    {
      get
      {
        return m_selectionService;
      }
    }

    /// <summary>
    ///   Returns true if the canvas is being edited
    /// </summary>
    internal bool IsEditing
    {
      get
      {
        return m_isEditing;
      }
      set
      {
        m_isEditing = value;

        //
        // Clear selection if there is no editing
        //
        if (!m_isEditing)
        {
          //SelectionService.ClearSelection();
        }
      }
    }

    #endregion

    /// <summary>
    ///   Default constructor
    /// </summary>
    public DesignerCanvas()
    {
      m_selectionService = new SelectionService(this);

      SetupCommands();

      this.Loaded += DesignerCanvas_Loaded;

      this.PreviewMouseDown += DesignerCanvas_PreviewMouseDown;

      this.DragEnter += DesignerCanvas_DragEnter;
    }

    void DesignerCanvas_DragEnter( object sender, DragEventArgs e )
    {
      MessageBox.Show("Got a drag enter");
    }

    private void DesignerCanvas_PreviewMouseDown( object sender, MouseButtonEventArgs e )
    {
      //if (m_selectionService.CurrentSelection.Count > 0)
      //{
      //  this.ContextMenu = m_selectionService.CurrentSelection[0].SelectedContextMenu;
      //}
    }

    private void DesignerCanvas_Loaded( object p_sender, RoutedEventArgs p_e )
    {
      m_adornerLayer = AdornerLayer.GetAdornerLayer(this);
    }

    public void OnKeyPressed(Key p_key)
    {
      Debug.WriteLine(p_key); 
    }


    #region Event Handlers

    /// <summary>
    ///   PreviewMouseDown Handler
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPreviewMouseDown( MouseButtonEventArgs e )
    {
      base.OnPreviewMouseDown(e);

      HandleMouseDown(e, false);

      e.Handled = false;
    }


    private void HandleMouseDown(MouseButtonEventArgs e, bool p_isDoubleClick)
    {
      // Hack here, why is the event raised on the parent as well???
      if (e.OriginalSource == this)
      {
        return;
      }

      //
      // Get the position from the canvas
      //
      Point pt = e.GetPosition(this);
      m_HitTestResults.Clear();
      //m_selectedObjects.Clear();

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
      //if (m_selectedObjects.Count > 0)
      //{
      //  SelectionService.RequestSelection(m_selectedObjects, p_isDoubleClick);

      //}
      //else
      //{
      //  //
      //  // If nothing was selected, deselect everything and select the 
      //  // whole panel
      //  //
      //  SelectionService.ClearSelection();
      //} 
    }

    private void FindSelectable( DependencyObject p_object )
    {
      //if (p_object == null) return;
      //if (p_object is IGuiSelectable)
      //{
      //  if (!m_selectedObjects.Contains(p_object as IGuiSelectable))
      //  {
      //    m_selectedObjects.Add(p_object as IGuiSelectable);
      //  }
      //  return;
      //}
      //FindSelectable(VisualTreeHelper.GetParent(p_object));
    }

    /// <summary>
    ///   Hittest filter
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    private HitTestFilterBehavior OnHitTestFilter( DependencyObject o )
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
    private HitTestResultBehavior OnHitTestBehavior( HitTestResult result )
    {
      // Add the hit test result to the list that will be processed after the enumeration.
      m_HitTestResults.Add(result.VisualHit);
      // Set the behavior to return visuals at all z-order levels.
      return HitTestResultBehavior.Continue;
    }

    /// <summary>
    ///   Mouse Down Event Handler
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDown( MouseButtonEventArgs e )
    {
      base.OnMouseDown(e);
      if (e.Source == this && IsEditing)
      {
        // in case that this click is the start of a 
        // drag operation we cache the start point
        //this.m_rubberbandSelectionStartPoint = new Point?(e.GetPosition(this));

        // if you click directly on the canvas all 
        // selected items are 'de-selected'
        //SelectionService.ClearSelection();
        Focus();
      }
    }

    /// <summary>
    ///   Mouse Move Event Handler
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseMove( MouseEventArgs e )
    {
      base.OnMouseMove(e);

      if (IsEditing)
      {
        // if mouse button is not pressed we have no drag operation, ...
        if (e.LeftButton != MouseButtonState.Pressed)
          this.m_rubberbandSelectionStartPoint = null;

        // ... but if mouse button is pressed and start
        // point value is set we do have one
        if (this.m_rubberbandSelectionStartPoint.HasValue)
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
    }

    #endregion

    protected override void OnDrop( DragEventArgs e )
    {
      base.OnDrop(e);
      //DragDataObject dragDataObject = e.Data.GetData(typeof(DragDataObject)) as DragDataObject;
      //if (dragDataObject != null && dragDataObject.ObjectID != 0)
      //{
        //DesignerItem newItem = null;
        //Object content = XamlReader.Load(XmlReader.Create(new StringReader(dragObject.Xaml)));

        //if (content != null)
        //{
        //  newItem = new DesignerItem();
        //  newItem.Content = content;

        //  Point position = e.GetPosition(this);

        //  if (dragObject.DesiredSize.HasValue)
        //  {
        //    Size desiredSize = dragObject.DesiredSize.Value;
        //    newItem.Width = desiredSize.Width;
        //    newItem.Height = desiredSize.Height;

        //    DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
        //    DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
        //  }
        //  else
        //  {
        //    DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X));
        //    DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y));
        //  }

        //  Canvas.SetZIndex(newItem, this.Children.Count);
        //  this.Children.Add(newItem);
        //  SetConnectorDecoratorTemplate(newItem);

        //  //update selection
        //  this.SelectionService.SelectItem(newItem);
        //  newItem.Focus();
        //}

      //  e.Handled = true;
      //}
    }

    //private void SetConnectorDecoratorTemplate(DesignerItem item)
    //{
    //  if (item.ApplyTemplate() && item.Content is UIElement)
    //  {
    //    ControlTemplate template = DesignerItem.GetConnectorDecoratorTemplate(item.Content as UIElement);
    //    Control decorator = item.Template.FindName("PART_ConnectorDecorator", item) as Control;
    //    if (decorator != null && template != null)
    //      decorator.Template = template;
    //  }
    //}
    public void TunnelDoubleClick(MouseButtonEventArgs p_args)
    {
      HandleMouseDown(p_args, true);
    }
  }
}