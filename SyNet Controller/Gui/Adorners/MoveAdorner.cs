using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SyNet.Gui.Dialogs;
using SyNet.Gui.Interfaces;
using SyNet.Gui.Thumbs;

namespace SyNet.Gui.Adorners
{
  /// <summary>
  ///   Class for a GUI Dragging Adorner for drag and drop
  /// </summary>
  public class MoveAdorner : Adorner
  {
    private const int PADDING = 2;
    private const double CONTAINER_PADDING = 4;

    private VisualCollection m_visualChildren;
    private MoveThumb m_moveThumb;
    private GuiPanelItem m_item = null;
    private GuiPanelDesigner m_designer = null;
    private List<GuiPanelItem> m_movableItems;

    /// <summary>
    ///   Gets the GuiPanelDesigner for this control
    /// </summary>
    private GuiPanelDesigner GuiPanelDesigner
    {
      get
      {
        if (m_designer == null)
        {
          m_designer = SyNet.GuiHelpers.Utilities.FindAncestor(typeof(GuiPanelDesigner), this)
                       as GuiPanelDesigner;
        }
        return m_designer;
      }
    }

    /// <summary>
    ///   Constructor
    /// </summary>
    /// <param name="p_dragDropData"></param>
    /// <param name="p_dragDropTemplate"></param>
    /// <param name="p_adornedElement"></param>
    /// <param name="p_adornerLayer"></param>
    public MoveAdorner( UIElement p_adornedElement )
      : base(p_adornedElement)
    {
      m_item = p_adornedElement as GuiPanelItem;

      if (m_item != null)
      {
        m_visualChildren = new VisualCollection(this);

        // Create the move thumb
        m_moveThumb = new MoveThumb(p_adornedElement);
        // Listen to changes
        m_moveThumb.DragCompleted += MoveThumb_DragCompleted;
        m_moveThumb.DragDelta += MoveThumb_DragDelta;
        m_moveThumb.MouseDoubleClick += MoveThumb_MouseDoubleClick;

        // Set the default tooltip
        SetToolTip();

        // Set the template
        SetTemplate();

        // Add to the visual children
        m_visualChildren.Add(m_moveThumb);

        this.PreviewMouseDown += MoveAdorner_PreviewMouseDown;
      }
    }

    /// <summary>
    ///   Set a tooltip so we know what we're moving
    /// </summary>
    private void SetToolTip()
    {
      if (m_item.HasAction)
      {
        this.ToolTip = m_item.Action.FormalName;
      }
      else if (m_item.HasParameter)
      {
        if (m_item.Parent != null && m_item.Parent.HasAction)
        {
          this.ToolTip = String.Format("{0}.{1}", m_item.Parent.Action.FormalName, m_item.Parameter.FullName);
        }
        else
        {
          Debug.WriteLine("[ERR] MoveAdorner.SetToolTip - null parent");
        }
      }
      else
      {
        this.ToolTip = null;
      }
    }

    /// <summary>
    ///   Show any quick entry box
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void MoveThumb_MouseDoubleClick( object sender, MouseButtonEventArgs e )
    {
      if (m_item.IsValueControl)
      {
        ValueEntryDialog dlg = new ValueEntryDialog();
        dlg.Value = m_item.Value;
        dlg.Owner = SyNet.GuiHelpers.Utilities.GetTopParent(m_item);
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.ShowDialog();

        if (dlg.DialogResult.HasValue && dlg.DialogResult.Value == true)
        {
          m_item.Value = dlg.Value;
        }
      }
    }

    /// <summary>
    ///   Drag thumb handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void MoveThumb_DragDelta( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e )
    {
     
      if (this.GuiPanelDesigner != null)
      {
        double horizontalChange = e.HorizontalChange;
        double verticalChange = e.VerticalChange;

        //
        // Start the drag when we have actual movement
        //
        if (!m_item.IsDragging && (e.HorizontalChange != 0 || e.VerticalChange != 0))
        {
          DragStarted();
        }
        else if (m_item.IsContainer)
        {
          horizontalChange -= 10;
          verticalChange += 6;
        }

        double maxParentWidth, maxParentHeight;
        //
        // Get the parent bounds
        //
        if (m_item.Parent == null)
        {
          maxParentWidth = GuiPanelDesigner.ActualWidth;
          maxParentHeight = GuiPanelDesigner.ActualHeight;
        }
        else
        {
          Rect containerRect = m_item.Parent.ContainerLocation;
          maxParentWidth = containerRect.Width;
          maxParentHeight = containerRect.Height;
        }

      
        double minLeft = double.MaxValue;
        double minTop = double.MaxValue;
        double maxRight = double.MinValue;
        double maxBottom = double.MinValue;

        //
        // Loop through and find the boundaries
        //
        foreach (GuiPanelItem item in m_movableItems)
        {
          //
          // If an item has its parent selected, it'll get moved with the parent
          //
          if (!this.GuiPanelDesigner.IsParentSelected(item))
          {
            minLeft = double.IsNaN(item.PositionLeft) ? 0 : Math.Min(item.PositionLeft, minLeft);
            minTop = double.IsNaN(item.PositionTop) ? 0 : Math.Min(item.PositionTop, minTop);

            double right = item.PositionLeft + item.ActualWidth;
            double bottom = item.PositionTop + item.ActualHeight;

            maxRight = double.IsNaN(right) ? maxParentWidth : Math.Max(right, maxRight);
            maxBottom = double.IsNaN(bottom) ? maxParentHeight : Math.Max(bottom, maxBottom);
          }
        }

        double deltaHorizontal = Math.Max(-minLeft, horizontalChange);
        double deltaVertical = Math.Max(-minTop, verticalChange);

        //
        // Make sure we're not going off the right hand boundary
        //
        if ((maxRight + deltaHorizontal) > maxParentWidth)
        {
          deltaHorizontal = maxParentWidth - maxRight;
        }

        //
        // Make sure we're not going off the bottom boundary
        //
        if ((maxBottom + deltaVertical) > maxParentHeight)
        {
          deltaVertical = maxParentHeight - maxBottom;
        }

        //
        // See if we're aligning with anything
        //

        //
        // If the item has no parent, it is being dragged in the global scope.
        // Otherwise, we need to convert it into the global scope and then
        // convert it back at the end
        //

        Rect currentPosition;
        Rect desiredPosition;

        if (m_item.Parent == null)
        {
          currentPosition =
          new Rect(
            m_item.PositionLeft,
            m_item.PositionTop,
            m_item.ActualWidth,
            m_item.ActualHeight);
        }
        else
        {
          Point adjustedPoint = m_item.TranslatePoint(new Point(0, 0), this.GuiPanelDesigner.DesignerCanvas);

          currentPosition =
          new Rect(
            adjustedPoint.X,
            adjustedPoint.Y,
            m_item.ActualWidth,
            m_item.ActualHeight);
        }

        desiredPosition = currentPosition;
        desiredPosition.X += deltaHorizontal;
        desiredPosition.Y += deltaVertical;


        GuiPanelDesigner.PanelGuide.EsnGuideAlign guideType =
          GuiPanelDesigner.PanelGuide.EsnGuideAlign.Top |
          GuiPanelDesigner.PanelGuide.EsnGuideAlign.Bottom |
          GuiPanelDesigner.PanelGuide.EsnGuideAlign.Left |
          GuiPanelDesigner.PanelGuide.EsnGuideAlign.Right;

        Rect adjustedRect = this.GuiPanelDesigner.ShowGuides(m_item,
          currentPosition,
          desiredPosition, m_movableItems, guideType, GuiPanelDesigner.PanelGuide.EsnGuideAction.Move);

        // Find the deltas
        deltaHorizontal = adjustedRect.X - currentPosition.X;
        deltaVertical = adjustedRect.Y - currentPosition.Y;

        //
        // Loop through and move it
        //
        foreach (GuiPanelItem item in m_movableItems)
        {
          double left = item.PositionLeft;
          double top = item.PositionTop;

          if (double.IsNaN(left)) left = 0;
          if (double.IsNaN(top)) top = 0;

          item.PositionLeft = left + deltaHorizontal;
          item.PositionTop = top + deltaVertical;
        }

        //
        //  Highlight any container below the mouse
        //
        this.GuiPanelDesigner.HighlightContainers(m_item);


        e.Handled = true;
      }
    }

    /// <summary>
    ///   Set the template for the thumb
    /// </summary>
    private void SetTemplate()
    {
      //
      // Display the appropriate adorner if its a container or not
      //
      if (m_item.IsContainer)
      {
        m_moveThumb.Template = FindResource("MoveThumbContainerTemplate") as ControlTemplate;
      }
      else
      {
        m_moveThumb.Template = FindResource("MoveThumbTemplate") as ControlTemplate;
      }
    }

    /// <summary>
    ///   Drag completed event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void MoveThumb_DragCompleted( object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e )
    {
      // If we're not actually dragging we don't need to drop
      if (!m_item.IsDragging) return;

      this.GuiPanelDesigner.OnDropCompleted(m_item, m_movableItems);

      SetTemplate();

      // Show the original item now that the drag is done
      this.m_item.Opacity = 1;

      GuiPanelDesigner.ClearContainerHighlights();

      GuiPanelDesigner.ClearGuides();

      this.InvalidateArrange();
    }

    /// <summary>
    ///   Start the drag
    /// </summary>
    private void DragStarted()
    {
      //
      // Make sure this item is selected
      //
      if (!m_item.IsSelected)
      {
        this.GuiPanelDesigner.SelectionService.AddToSelection(m_item);
      }

      m_movableItems = this.GuiPanelDesigner.SelectionService.GetMovableSelection(m_item);

      this.GuiPanelDesigner.OnDragStarted(m_item, m_movableItems);

      m_moveThumb.Template = FindResource("MoveThumbDragTemplate") as ControlTemplate;

      m_moveThumb.ApplyTemplate();

      m_moveThumb.SetBackgroundToControl();

      m_item.Opacity = 0;

      // Start the container highlighting
      this.GuiPanelDesigner.HighlightContainers(m_item);

      this.InvalidateArrange();
    }

    void MoveAdorner_PreviewMouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
    {
      if (this.GuiPanelDesigner != null)
      {
        this.GuiPanelDesigner.SelectionService.RequestSelection(m_item);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride( Size p_finalSize )
    {
      if (m_item.IsDragging)
      {
        m_moveThumb.Width = this.AdornedElement.DesiredSize.Width + PADDING + PADDING;
        m_moveThumb.Height = this.AdornedElement.DesiredSize.Height + PADDING + PADDING;

        m_moveThumb.Arrange(new Rect(-PADDING, -PADDING, m_moveThumb.Width, m_moveThumb.Height));

      }
      else if (m_item.IsContainer)
      {
        m_moveThumb.Width = 15;
        m_moveThumb.Height = 15;

        //
        // Place the move adorner directly over the control
        //
        m_moveThumb.Arrange(new Rect(8,
                                     -7.5,
                                     m_moveThumb.Width,
                                     m_moveThumb.Height));
      }
      else
      {
        m_moveThumb.Width = this.AdornedElement.DesiredSize.Width;
        m_moveThumb.Height = this.AdornedElement.DesiredSize.Height;

        //
        // Place the move adorner directly over the control
        //
        m_moveThumb.Arrange(new Rect(0, 0, m_moveThumb.Width, m_moveThumb.Height));
      }

      return p_finalSize;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_index"></param>
    /// <returns></returns>
    protected override Visual GetVisualChild( int p_index )
    {
      return m_visualChildren[p_index];
    }

    /// <summary>
    /// 
    /// </summary>
    protected override int VisualChildrenCount
    {
      get { return m_visualChildren.Count; }
    }
  }
}