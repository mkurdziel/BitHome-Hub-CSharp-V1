using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SyNet.Gui.Thumbs;

namespace SyNet.Gui.Adorners
{
  /// <summary>
  ///   Class for a GUI Dragging Adorner for drag and drop
  /// </summary>
  public class ResizeAdorner : Adorner
  {
    private const int PADDING = 2;
    private const double MIN_SIZE = 18;

    private GuiPanelItem m_item;

    private VisualCollection m_visualChildren;

    private ResizeThumb m_thumbNW = null;
    private ResizeThumb m_thumbN = null;
    private ResizeThumb m_thumbNE = null;
    private ResizeThumb m_thumbE = null;
    private ResizeThumb m_thumbSE = null;
    private ResizeThumb m_thumbS = null;
    private ResizeThumb m_thumbSW = null;
    private ResizeThumb m_thumbW = null;
    private GuiPanelDesigner m_designer = null;
    private List<GuiPanelItem> m_movableItems = new List<GuiPanelItem>();

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
    public ResizeAdorner( UIElement p_adornedElement )
      : base(p_adornedElement)
    {
      m_visualChildren = new VisualCollection(this);

      m_item = p_adornedElement as GuiPanelItem;


      if (m_item != null)
      {
        // Create the W and E thumbs
        if (m_item.CanResizeWidth)
        {
          m_thumbW = CreateThumb(ResizeThumb.EsnThumbLocation.W);
          m_thumbE = CreateThumb(ResizeThumb.EsnThumbLocation.E);

          // Add to the visual children
          m_visualChildren.Add(m_thumbW);
          m_visualChildren.Add(m_thumbE);
        }

        // Create the N and S thumbs
        if (m_item.CanResizeHeight)
        {
          m_thumbN = CreateThumb(ResizeThumb.EsnThumbLocation.N);
          m_thumbS = CreateThumb(ResizeThumb.EsnThumbLocation.S);

          // Add to the visual children
          m_visualChildren.Add(m_thumbN);
          m_visualChildren.Add(m_thumbS);
        }

        // Create corner thumbs
        if (m_item.CanResizeWidth && m_item.CanResizeHeight)
        {
          m_thumbNW = CreateThumb(ResizeThumb.EsnThumbLocation.NW);
          m_thumbNE = CreateThumb(ResizeThumb.EsnThumbLocation.NE);
          m_thumbSW = CreateThumb(ResizeThumb.EsnThumbLocation.SW);
          m_thumbSE = CreateThumb(ResizeThumb.EsnThumbLocation.SE);

          // Add to the visual children
          m_visualChildren.Add(m_thumbNW);
          m_visualChildren.Add(m_thumbNE);
          m_visualChildren.Add(m_thumbSW);
          m_visualChildren.Add(m_thumbSE);
        }
      }
    }

    private ResizeThumb CreateThumb( ResizeThumb.EsnThumbLocation p_location )
    {
      ResizeThumb thumb = new ResizeThumb(m_item, p_location);
      ControlTemplate resizeTemplate = FindResource("ResizeThumbTemplate") as ControlTemplate;
      thumb.Template = resizeTemplate;
      thumb.DragDelta += Thumb_DragDelta;
      thumb.DragStarted += Thumb_DragStarted;
      thumb.DragCompleted += Thumb_DragCompleted;

      switch (p_location)
      {
        case ResizeThumb.EsnThumbLocation.E:
          thumb.Cursor = Cursors.SizeWE;
          break;
        case ResizeThumb.EsnThumbLocation.N:
          thumb.Cursor = Cursors.SizeNS;
          break;
        case ResizeThumb.EsnThumbLocation.NE:
          thumb.Cursor = Cursors.SizeNESW;
          break;
        case ResizeThumb.EsnThumbLocation.NW:
          thumb.Cursor = Cursors.SizeNWSE;
          break;
        case ResizeThumb.EsnThumbLocation.S:
          thumb.Cursor = Cursors.SizeNS;
          break;
        case ResizeThumb.EsnThumbLocation.SE:
          thumb.Cursor = Cursors.SizeNWSE;
          break;
        case ResizeThumb.EsnThumbLocation.SW:
          thumb.Cursor = Cursors.SizeNESW;
          break;
        case ResizeThumb.EsnThumbLocation.W:
          thumb.Cursor = Cursors.SizeWE;
          break;
      }
      return thumb;
    }

    private void Thumb_DragStarted( object p_sender, DragStartedEventArgs p_e )
    {
    }

    /// <summary>
    ///   Drag completed event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Thumb_DragCompleted( object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e )
    {
      this.GuiPanelDesigner.ClearGuides();
    }

    /// <summary>
    ///   Drag handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Thumb_DragDelta( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e )
    {
      ResizeThumb thumb = sender as ResizeThumb;

      //
      // Gather the presetsize
      //
      double preWidth = m_item.ControlWidth;
      double preHeight = m_item.ControlHeight;


      if (thumb != null && m_item != null)
      {
        double horizontalDelta = e.HorizontalChange;
        double verticalDelta = e.VerticalChange;

        //
        // Adjust for boundaries
        //

        // Horizontal plus
        if (thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.E ||
            thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.NE ||
            thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.SE)
        {
          GuiPanelDesigner.PanelGuide.EsnGuideAlign guideType = GuiPanelDesigner.PanelGuide.EsnGuideAlign.Right;

          switch (thumb.ThumbLocation)
          {
            case ResizeThumb.EsnThumbLocation.NE:
              guideType |= GuiPanelDesigner.PanelGuide.EsnGuideAlign.Top;
              break;
            case ResizeThumb.EsnThumbLocation.SE:
              guideType |= GuiPanelDesigner.PanelGuide.EsnGuideAlign.Bottom;
              break;
          }

          double newWidth = m_item.ActualWidth + horizontalDelta;

          if (newWidth > 0)
          {
            //
            // Adjust the point to the canvas scope
            //
            Point adjustedLocation = m_item.TranslatePoint(
              new Point(0, 0),
              this.GuiPanelDesigner.DesignerCanvas);


            //
            // See if we're aligning with anything
            //
            Rect currentPosition = new Rect(
              adjustedLocation.X,
              adjustedLocation.Y,
              m_item.ActualWidth,
              m_item.ActualHeight);

            Rect desiredPosition = new Rect(
              adjustedLocation.X,
              adjustedLocation.Y,
              newWidth,
              m_item.ActualHeight);

            Rect adjustedRect = this.GuiPanelDesigner.ShowGuides(
              m_item,
              currentPosition,
              desiredPosition,
              this.GuiPanelDesigner.SelectionService.GetMovableSelection(m_item), guideType,
              GuiPanelDesigner.PanelGuide.EsnGuideAction.Resize);

            //Debug.WriteLine("Desired:  " + desiredPosition);
            //Debug.WriteLine("Adjusted: " + adjustedRect);

            // Make any adjustments based on the alignment
            horizontalDelta = adjustedRect.Width - m_item.ActualWidth;
          }

          double rightSide = m_item.PositionLeft + m_item.ActualWidth;

          if ((rightSide + horizontalDelta) > this.GuiPanelDesigner.ActualWidth)
          {
            horizontalDelta = (this.GuiPanelDesigner.ActualWidth - rightSide);
          }

        }

        // Horizontal minus
        if (thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.W ||
            thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.NW ||
            thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.SW)
        {
          GuiPanelDesigner.PanelGuide.EsnGuideAlign guideType = GuiPanelDesigner.PanelGuide.EsnGuideAlign.Left;

          switch (thumb.ThumbLocation)
          {
            case ResizeThumb.EsnThumbLocation.NW:
              guideType |= GuiPanelDesigner.PanelGuide.EsnGuideAlign.Top;
              break;
            case ResizeThumb.EsnThumbLocation.SW:
              guideType |= GuiPanelDesigner.PanelGuide.EsnGuideAlign.Bottom;
              break;
          }

          //
          // Adjust the point to the canvas scope
          //
          Point adjustedLocation = m_item.TranslatePoint(
             new Point(0, 0),
             this.GuiPanelDesigner.DesignerCanvas);

          //
          // See if we're aligning with anything
          //
          Rect currentPosition = new Rect(adjustedLocation.X,
                                          adjustedLocation.Y,
                                          m_item.ActualWidth,
                                          m_item.ActualHeight);

          Rect desiredPosition = new Rect(adjustedLocation.X + horizontalDelta,
                                          adjustedLocation.Y,
                                          m_item.ActualWidth,
                                          m_item.ActualHeight);

          Rect adjustedRect = this.GuiPanelDesigner.ShowGuides(m_item, 
            currentPosition,
            desiredPosition,
            this.GuiPanelDesigner.SelectionService.GetMovableSelection(m_item), guideType,
            GuiPanelDesigner.PanelGuide.EsnGuideAction.Resize);

          //Debug.WriteLine("Desired:  " + desiredPosition);
          //Debug.WriteLine("Adjusted: " + adjustedRect);

          // Make any adjustments based on the alignment
          horizontalDelta = adjustedRect.Left - adjustedLocation.X;

          //Debug.WriteLine("Vertical: " + verticalDelta);

          double leftSide = m_item.PositionLeft;

          if ((leftSide + horizontalDelta) < 0)
          {
            horizontalDelta = -leftSide;
          }

          // Make sure we don't shrink to far and push the item down
          if ((m_item.ControlWidth - horizontalDelta) < GuiPanelItem.MIN_WIDTH)
          {
            horizontalDelta = m_item.ControlWidth - GuiPanelItem.MIN_WIDTH;
          }
        }

        // Vertical plus
        if (thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.S ||
         thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.SW ||
         thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.SE)
        {
          GuiPanelDesigner.PanelGuide.EsnGuideAlign guideType = GuiPanelDesigner.PanelGuide.EsnGuideAlign.Bottom;

          switch (thumb.ThumbLocation)
          {
            case ResizeThumb.EsnThumbLocation.SW:
              guideType |= GuiPanelDesigner.PanelGuide.EsnGuideAlign.Left;
              break;
            case ResizeThumb.EsnThumbLocation.SE:
              guideType |= GuiPanelDesigner.PanelGuide.EsnGuideAlign.Right;
              break;
          }

          double newHeight = m_item.ActualHeight + verticalDelta;

          if (newHeight > 0)
          {
            //
            // Adjust the point to the canvas scope
            //
            Point adjustedLocation = m_item.TranslatePoint(
              new Point(0, 0),
              this.GuiPanelDesigner.DesignerCanvas);

            //
            // See if we're aligning with anything
            //
            Rect currentPosition = new Rect(adjustedLocation.X,
                                adjustedLocation.Y,
                                m_item.ActualWidth,
                                m_item.ActualHeight);

            Rect desiredPosition = new Rect(
              adjustedLocation.X,
              adjustedLocation.Y,
              m_item.ActualWidth,
              newHeight);

            Rect adjustedRect = this.GuiPanelDesigner.ShowGuides(
              m_item, 
              currentPosition,
              desiredPosition,
              this.GuiPanelDesigner.SelectionService.GetMovableSelection(m_item), guideType,
              GuiPanelDesigner.PanelGuide.EsnGuideAction.Resize);

            // Make any adjustments based on the alignment
            verticalDelta = adjustedRect.Height - m_item.ActualHeight;
          }

          double bottomSide = m_item.PositionTop + m_item.ActualHeight;

          if ((bottomSide + verticalDelta) > this.GuiPanelDesigner.ActualHeight)
          {
            verticalDelta = (this.GuiPanelDesigner.ActualHeight - bottomSide);
          }
        }

        // Vertical minus
        if (thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.N ||
            thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.NW ||
            thumb.ThumbLocation == ResizeThumb.EsnThumbLocation.NE)
        {

          GuiPanelDesigner.PanelGuide.EsnGuideAlign guideType = GuiPanelDesigner.PanelGuide.EsnGuideAlign.Top;

          switch (thumb.ThumbLocation)
          {
            case ResizeThumb.EsnThumbLocation.NW:
              guideType |= GuiPanelDesigner.PanelGuide.EsnGuideAlign.Left;
              break;
            case ResizeThumb.EsnThumbLocation.NE:
              guideType |= GuiPanelDesigner.PanelGuide.EsnGuideAlign.Right;
              break;
          }
          //
          // Adjust the point to the canvas scope
          //
          Point adjustedLocation = m_item.TranslatePoint(
            new Point(0, 0),
            this.GuiPanelDesigner.DesignerCanvas);


          //
          // See if we're aligning with anything
          //
          Rect currentPosition = new Rect(adjustedLocation.X,
                               adjustedLocation.Y,
                               m_item.ActualWidth,
                               m_item.ActualHeight);

          Rect desiredPosition = new Rect(adjustedLocation.X,
                                          adjustedLocation.Y + verticalDelta,
                                          m_item.ActualWidth,
                                          m_item.ActualHeight);

          Rect adjustedRect = this.GuiPanelDesigner.ShowGuides(m_item, 
            currentPosition,
            desiredPosition,
            this.GuiPanelDesigner.SelectionService.GetMovableSelection(m_item), guideType, GuiPanelDesigner.PanelGuide.EsnGuideAction.Resize);

          // Make any adjustments based on the alignment
          verticalDelta = adjustedRect.Y - adjustedLocation.Y;

          double topSide = m_item.PositionTop;
          if ((topSide + verticalDelta) < 0)
          {
            verticalDelta = - topSide;
          }

          // Make sure we don't shrink to far and push the item down
          if ((m_item.ControlHeight - verticalDelta) < GuiPanelItem.MIN_HEIGHT)
          {
            verticalDelta = m_item.ControlHeight - GuiPanelItem.MIN_HEIGHT; 
          }
        }

        //
        // Make the changes
        //
        switch (thumb.ThumbLocation)
        {
          case ResizeThumb.EsnThumbLocation.E:
            m_item.ControlWidth += horizontalDelta;
            break;
          case ResizeThumb.EsnThumbLocation.W:
            m_item.ControlWidth -= horizontalDelta;
            m_item.PositionLeft += horizontalDelta;
            break;
          case ResizeThumb.EsnThumbLocation.N:
            m_item.ControlHeight -= verticalDelta;
            m_item.PositionTop += verticalDelta;
            break;
          case ResizeThumb.EsnThumbLocation.S:
            m_item.ControlHeight += verticalDelta;
            break;
          case ResizeThumb.EsnThumbLocation.NE:
            m_item.ControlWidth += horizontalDelta;
            m_item.ControlHeight -= verticalDelta;
            m_item.PositionTop += verticalDelta;
            break;
          case ResizeThumb.EsnThumbLocation.SE:
            m_item.ControlWidth += horizontalDelta;
            m_item.ControlHeight += verticalDelta;
            break;
          case ResizeThumb.EsnThumbLocation.NW:
            m_item.ControlWidth -= horizontalDelta;
            m_item.ControlHeight -= verticalDelta;
            m_item.PositionTop += verticalDelta; ;
            m_item.PositionLeft += horizontalDelta;
            break;
          case ResizeThumb.EsnThumbLocation.SW:
            m_item.ControlWidth -= horizontalDelta;
            m_item.ControlHeight += verticalDelta;
            m_item.PositionLeft += horizontalDelta;
            break;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride( Size p_finalSize )
    {
      if (m_thumbNW != null)
      {
        ArrangeThumb(m_thumbNW);
      }
      if (m_thumbN != null)
      {
        ArrangeThumb(m_thumbN);
        if (m_item.ActualWidth >= MIN_SIZE)
        {
          m_thumbN.Visibility = Visibility.Visible;
        }
        else
        {
          m_thumbN.Visibility = Visibility.Hidden;
        }
      }
      if (m_thumbNE != null)
      {
        ArrangeThumb(m_thumbNE);
      }
      if (m_thumbE != null)
      {
        ArrangeThumb(m_thumbE);
        if (m_item.ActualHeight >= MIN_SIZE)
        {
          m_thumbE.Visibility = Visibility.Visible;
        }
        else
        {
          m_thumbE.Visibility = Visibility.Hidden;
        }
      }
      if (m_thumbSE != null)
      {
        ArrangeThumb(m_thumbSE);
      }
      if (m_thumbS != null)
      {
        ArrangeThumb(m_thumbS);
        if (m_item.ActualWidth >= MIN_SIZE)
        {
          m_thumbS.Visibility = Visibility.Visible;
        }
        else
        {
          m_thumbS.Visibility = Visibility.Hidden;
        }
      }
      if (m_thumbSW != null)
      {
        ArrangeThumb(m_thumbSW);
      }
      if (m_thumbW != null)
      {
        ArrangeThumb(m_thumbW);
        if (m_item.ActualHeight >= MIN_SIZE)
        {
          m_thumbW.Visibility = Visibility.Visible;
        }
        else
        {
          m_thumbW.Visibility = Visibility.Hidden;
        }
      }

      return p_finalSize;
    }

    private void ArrangeThumb( ResizeThumb p_thumb )
    {
      p_thumb.Width = 7;
      p_thumb.Height = 7;

      //
      // Arrange it in the proper location
      //
      switch (p_thumb.ThumbLocation)
      {
        case ResizeThumb.EsnThumbLocation.NW:
          p_thumb.Arrange(new Rect(0 - PADDING - p_thumb.Width / 2,
                                   0 - PADDING - p_thumb.Height / 2,
                                   p_thumb.Width,
                                   p_thumb.Height));
          break;
        case ResizeThumb.EsnThumbLocation.N:
          p_thumb.Arrange(new Rect(m_item.ActualWidth / 2 - p_thumb.Width / 2,
                                   0 - PADDING - p_thumb.Height / 2,
                                   p_thumb.Width,
                                   p_thumb.Height));
          break;
        case ResizeThumb.EsnThumbLocation.NE:
          p_thumb.Arrange(new Rect(m_item.ActualWidth + PADDING - p_thumb.Width / 2 - 1,
                                   0 - PADDING - p_thumb.Height / 2,
                                   p_thumb.Width,
                                   p_thumb.Height));
          break;
        case ResizeThumb.EsnThumbLocation.E:
          p_thumb.Arrange(new Rect(m_item.ActualWidth + PADDING - p_thumb.Width / 2 - 1,
                                   m_item.ActualHeight / 2 - p_thumb.Height / 2,
                                   p_thumb.Width,
                                   p_thumb.Height));
          break;
        case ResizeThumb.EsnThumbLocation.SE:
          p_thumb.Arrange(new Rect(m_item.ActualWidth + PADDING - p_thumb.Width / 2 - 1,
                                   m_item.ActualHeight + PADDING - p_thumb.Height / 2,
                                   p_thumb.Width,
                                   p_thumb.Height));
          break;
        case ResizeThumb.EsnThumbLocation.S:
          p_thumb.Arrange(new Rect(m_item.ActualWidth / 2 - p_thumb.Width / 2,
                                   m_item.ActualHeight + PADDING - p_thumb.Height / 2,
                                   p_thumb.Width,
                                   p_thumb.Height));
          break;
        case ResizeThumb.EsnThumbLocation.SW:
          p_thumb.Arrange(new Rect(0 - PADDING - p_thumb.Width / 2,
                                   m_item.ActualHeight + PADDING - p_thumb.Height / 2,
                                   p_thumb.Width,
                                   p_thumb.Height));
          break;
        case ResizeThumb.EsnThumbLocation.W:
          p_thumb.Arrange(new Rect(0 - PADDING - p_thumb.Width / 2,
                                   m_item.ActualHeight / 2 - p_thumb.Height / 2,
                                   p_thumb.Width,
                                   p_thumb.Height));
          break;
      }
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