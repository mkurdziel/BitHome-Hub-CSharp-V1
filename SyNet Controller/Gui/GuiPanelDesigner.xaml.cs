using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using SyNet.Gui.Adorners;
using SyNet.Gui.Controls.SnButton;
using SyNet.Gui.Controls.SnGroupBox;
using SyNet.Gui.Controls.SnTextBox;
using SyNet.Gui.Models;
using SyNet.Gui.Toolbox;
using Action = SyNet.Actions.Action;
using Binding = System.Windows.Data.Binding;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using Trigger = SyNet.Events.Triggers.Trigger;

namespace SyNet.Gui
{
  /// <summary>
  /// Interaction logic for GuiPanelDesigner.xaml
  /// </summary>
  public partial class GuiPanelDesigner : UserControl, INotifyPropertyChanged
  {
    private const int DEFAULT_PADDING = 4;

    public class PanelGuide
    {
      [Flags]
      public enum EsnGuideAlign
      {
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8
      }

      public enum EsnGuideAction
      {
        Move,
        Resize
      }

      public double Distance { get; set; }
      public Line GuideLine { get; set; }
      public EsnGuideAlign GuideAlign { get; set; }
    }

    // Panel guides for moving panel
    private PanelGuide m_guideLeft = null;
    private PanelGuide m_guideRight = null;
    private PanelGuide m_guideTop = null;
    private PanelGuide m_guideBottom = null;

    private List<PanelGuide> m_marginVertical = new List<PanelGuide>();
    private List<PanelGuide> m_marginHorizontal = new List<PanelGuide>();

    private const double SNAP_DISTANCE = 5;
    private const double MARGIN_DISTANCE = 8;

    private SelectionService m_selectionService;
    private ToolboxWindow m_toolBoxWindow;
    private AdornerLayer m_adornerLayer;
    private bool m_isEditing;
    private List<GuideAdorner> m_guideAdorners = new List<GuideAdorner>();

    private GuiPanelItem m_rubberBandStartParent = null;
    private List<GuiPanelItem> m_rubberBandSelection = new List<GuiPanelItem>();

    private Dictionary<GuiPanelItem, GuiPanelItem>
      m_dctItemByParent = new Dictionary<GuiPanelItem, GuiPanelItem>();

    #region Public Properties

    public SelectionService SelectionService
    {
      get { return m_selectionService; }
      set { m_selectionService = value; }
    }

    /// <summary>
    ///   Gets the canvas of this designer
    /// </summary>
    public DesignerCanvas DesignerCanvas
    {
      get
      {
        return x_canvas;
      }
    }

    /// <summary>
    ///   Is editing mode of this panel enabled
    /// </summary>
    public bool IsEditing
    {
      get { return m_isEditing; }
      set
      {
        if (m_isEditing != value)
        {
          m_isEditing = value;
          SetEditing(m_isEditing);
          OnPropertyChanged("IsEditing");
        }
      }
    }

    #endregion

    #region Construction

    /// <summary>
    ///   Gets or sets the GuiPanel asociated with this designer
    /// </summary>
    public GuiPanel GuiPanel { get; set; }

    #endregion

    /// <summary>
    ///   Get the topmost logcal parent
    /// </summary>
    /// <returns></returns>
    private Window GetTopParent()
    {
      DependencyObject dpParent = this.Parent;
      do
      {
        dpParent = LogicalTreeHelper.GetParent(dpParent);
      } while (dpParent.GetType().BaseType != typeof(Window));
      return dpParent as Window;
    }

    /// <summary>
    ///   Set all the necessary editing mechanics
    /// </summary>
    /// <param name="p_editing"></param>
    private void SetEditing(bool p_editing)
    {
      //
      // Trigger the editing of all items
      //
      foreach (GuiPanelItem item in this.GuiPanel.GuiPanelItems)
      {
        item.IsEditing = p_editing;
      }

      if (p_editing)
      {
        x_canvas.Focus();

        if (m_toolBoxWindow == null)
        {
          m_toolBoxWindow = new ToolboxWindow(this.GuiPanel);
          m_toolBoxWindow.Owner = GetTopParent();
          m_toolBoxWindow.Show();
        }
      }
      else
      {
        //
        // Remove all selections
        //
        this.SelectionService.ClearSelection();

        if (m_toolBoxWindow != null)
        {
          m_toolBoxWindow.Close();
          m_toolBoxWindow = null;
        }
      }
    }

    /// <summary>
    ///   Default Constructor
    /// </summary>
    /// <param name="p_panel"></param>
    public GuiPanelDesigner(GuiPanel p_panel)
    {
      this.GuiPanel = p_panel;

      InitializeComponent();

      // Create the selection service
      this.SelectionService = new SelectionService();

      // Listen for selection changes
      this.SelectionService.SelectionChanged += SelectionService_SelectionChanged;

      // Pass the designer into the panel
      x_canvas.GuiPanelDesigner = this;

      // Cache the adorner layer
      m_adornerLayer = AdornerLayer.GetAdornerLayer(x_canvas);

      // Listen to property changes on the panel
      GuiPanel.PropertyChanged += GuiPanel_PropertyChanged;

      // Listen to keydown
      this.PreviewKeyDown += GuiPanelDesigner_PreviewKeyDown;

      // Build the designer
      BuildGuiFromGuiPanel();
    }

    /// <summary>
    ///   If the selection changes, modify the properties in the toolbox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SelectionService_SelectionChanged(object sender, EventArgs e)
    {
      if (m_toolBoxWindow != null)
      {
        if (this.IsEditing &&
            this.SelectionService.CurrentSelection.Count == 1)
        {
          m_toolBoxWindow.ControlProperties =
            new GuiPanelItemConfig(this.SelectionService.CurrentSelection.First());
        }
        else
        {
          m_toolBoxWindow.ControlProperties = null;
        }
      }
    }

    /// <summary>
    ///   Key down event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GuiPanelDesigner_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (this.IsEditing)
      {
        switch (e.Key)
        {
          case Key.Up:
            NudgeUp();
            break;
          case Key.Down:
            NudgeDown();
            break;
          case Key.Left:
            NudgeLeft();
            break;
          case Key.Right:
            NudgeRight();
            break;
          case Key.Delete:
          case Key.Back:
            OnDelete();
            break;
        }
      }
    }

    /// <summary>
    ///   Delete event handler
    /// </summary>
    private void OnDelete()
    {
      List<GuiPanelItem> selectedItems = this.SelectionService.CurrentSelection;

      //
      // Iterate through the selected items and delete appropriatly
      //

      //
      // Check to see if any parameters are selected without their parents,
      // if so, warn the user that the action will be deleted too
      //
      bool bOnlyParamSelected = false;
      foreach (GuiPanelItem item in selectedItems)
      {
        if (item.IsBoundToParent)
        {
          if (item.Parent != null && item.Parent.HasAction)
          {
            bOnlyParamSelected = true;
            break;
          }
          else
          {
            Debug.WriteLine("[ERR] GuiPanelDesigner.OnDelete - Some sort of error");
          }
        }
      }

      if (bOnlyParamSelected)
      {
        MessageBoxResult result = MessageBox.Show(
          "Deleting this item will also delete the action it belongs to.", "Warning", MessageBoxButton.OKCancel,
          MessageBoxImage.Exclamation);

        if (result == MessageBoxResult.Cancel)
        {
          return;
        }
      }

      //
      // Go through and delete each item
      //
      foreach (GuiPanelItem item in selectedItems)
      {
        GuiPanelItem itemToDelete = item;

        //
        // If the item is a parameter then delete the action
        //
        if (item.IsBoundToParent)
        {
          if (item.Parent != null && item.Parent.HasAction)
          {
            itemToDelete = item.Parent;
            itemToDelete.RemoveChild(item);
          }
          else
          {
            Debug.WriteLine("[ERR] GuiPanelDesigner.OnDelete - Parameter parental error");
          }
        }

        if (itemToDelete.Parent != null)
        {
          itemToDelete.Parent.RemoveChild(itemToDelete);
        }
        else
        {
          this.GuiPanel.RemoveControl(itemToDelete);
        }
      }
    }

    /// <summary>
    ///   Gui Panel Property Changed Event Handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GuiPanel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "GuiPanelItems")
      {
        BuildGuiFromGuiPanel();
      }
    }

    /// <summary>
    ///   Iterate through the GuiPanel's items and build the visuals
    /// </summary>
    private void BuildGuiFromGuiPanel()
    {
      if (x_canvas != null || this.GuiPanel != null)
      {
        x_canvas.Children.Clear();

        foreach (GuiPanelItem item in this.GuiPanel.GuiPanelItems)
        {
          Binding b;

          b = new Binding("PositionLeft");
          b.Source = item;
          b.Mode = BindingMode.TwoWay;
          item.SetBinding(Canvas.LeftProperty, b);

          b = new Binding("PositionTop");
          b.Source = item;
          b.Mode = BindingMode.TwoWay;
          item.SetBinding(Canvas.TopProperty, b);

          b = new Binding("PositionZIndex");
          b.Source = item;
          b.Mode = BindingMode.TwoWay;
          item.SetBinding(Canvas.ZIndexProperty, b);

          //
          // Set the adorner layer
          //
          item.AdornerLayer = AdornerLayer.GetAdornerLayer(x_canvas);

          x_canvas.Children.Add(item);
        }
      }
    }

    /// <summary>
    ///   Returns true if the item has a selected parent
    /// </summary>
    /// <param name="p_item"></param>
    /// <returns></returns>
    public bool IsParentSelected(GuiPanelItem p_item)
    {
      if (p_item.Parent != null)
      {
        if (p_item.Parent.IsSelected)
        {
          return true;
        }
        return IsParentSelected(p_item.Parent);
      }
      return false;
    }

    ////////////////////////////////////////////////////////////////////////////
    // Add Methods
    ////////////////////////////////////////////////////////////////////////////
    #region Add Methods

    /// <summary>
    ///   Add an object to the panel. Determines if its valid before adding.
    /// </summary>
    /// <param name="p_o"></param>
    /// <param name="p_point"></param>
    public void AddObject(object p_o, Point p_point)
    {
      if (p_o is Actions.Action)
      {
        AddAction(p_o as Actions.Action, p_point);
      }

      if (p_o is Events.Triggers.Trigger)
      {
        AddTrigger(p_o as Events.Triggers.Trigger, p_point);
      }
    }

    /// <summary>
    ///   Add an action to the panel. 
    /// </summary>
    /// <param name="p_action"></param>
    /// <param name="p_point"></param>
    private void AddAction(Action p_action, Point p_point)
    {
      if (this.GuiPanel == null)
      {
        Debug.WriteLine("[ERR] GuiPanelDesigner.Add Action - null guipanel");
        return;
      }

      //
      // Create the GUI Action that we're going to use
      //
      GuiAction guiAction = new GuiAction(p_action);

      //
      // Add the groupbox to surround the action
      //
      SnGroupBox groupBox = new SnGroupBox();
      groupBox.Header = p_action.Name;
      GuiPanelItem groupBoxItem = new GuiPanelItem(groupBox, p_point);
      // Set the action to the container
      groupBoxItem.Action = guiAction;
      this.GuiPanel.AddControl(groupBoxItem);

      double groupBoxY = 0;
      double maxWidth = 0;

      //
      // Add each parameter to the box
      //
      foreach (GuiParameter inputParameter in guiAction.InputParameters)
      {
        SnTextBox paramTextbox = new SnTextBox();
        GuiPanelItem itemTextbox = new GuiPanelItem(paramTextbox, new Point(0, groupBoxY));

        // Give the text box the parameter
        itemTextbox.Parameter = inputParameter;

        // Set a default textbox width
        itemTextbox.ControlWidth = 100;
        itemTextbox.Measure(new Size(x_canvas.ActualWidth, x_canvas.ActualHeight));

        //
        // Track the max width
        //
        if (itemTextbox.DesiredSize.Width > maxWidth)
        {
          maxWidth = itemTextbox.DesiredSize.Width;
        }

        //
        // Increment the Y order by the height of the label
        //
        groupBoxY += itemTextbox.DesiredSize.Height + 4;

        groupBoxItem.AddChild(itemTextbox);
      }

      //
      // Adjust label widths
      //
      groupBoxItem.SyncChildLabelWidths();

      //
      // Add the execute button
      //
      SnButton executeButton = new SnButton();
      executeButton.Content = "Execute";
      executeButton.Width = 100;
      executeButton.Measure(new Size(x_canvas.ActualWidth, x_canvas.ActualHeight));

      //
      // Track the max width
      //
      if (executeButton.DesiredSize.Width > maxWidth)
      {
        maxWidth = executeButton.DesiredSize.Width;
      }

      GuiPanelItem buttonItem = new GuiPanelItem(executeButton, new Point(0, groupBoxY));
      buttonItem.PositionLeft = groupBoxItem.LongestChildLabelWidth;

      groupBoxY += executeButton.DesiredSize.Height + DEFAULT_PADDING;

      groupBoxItem.AddChild(buttonItem);

      //
      // Resize the groupbox to fit its contents
      //
      groupBoxItem.ControlWidth = maxWidth + 20;
      groupBoxItem.ControlHeight = groupBoxY + 20;
    }

    /// <summary>
    ///   Add a trigger to the panel
    /// </summary>
    /// <param name="p_trigger"></param>
    /// <param name="p_point"></param>
    /// 
    private void AddTrigger(Trigger p_trigger, Point p_point)
    {
      if (this.GuiPanel == null)
      {
        Debug.WriteLine("[ERR] GuiPanelDesigner.AddTrigger - null guipanel");
        return;
      }

      //
      // Add the groupbox to surround the trigger
      //
      SnTextBox tb = new SnTextBox();

      GuiPanelItem triggerItem = new GuiPanelItem(tb, p_point);
      triggerItem.IsControlEnabled = false;

      triggerItem.ControlWidth = 100;
      triggerItem.TriggerItem = new GuiTriggerItem(p_trigger.ID);

      this.GuiPanel.AddControl(triggerItem);

    }

    #endregion

    #region Implementation of INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// </summary>
    /// <param name="p_strPropertyName"></param>
    protected void OnPropertyChanged(string p_strPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(p_strPropertyName));
    }


    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // Nudging
    ////////////////////////////////////////////////////////////////////////////
    #region Nude Methods

    /// <summary>
    ///   Nude selected objects left
    /// </summary>
    private void NudgeLeft()
    {
      foreach (GuiPanelItem item in this.SelectionService.CurrentSelection)
      {
        if (item.PositionLeft > 0)
        {
          item.PositionLeft -= 1;
        }
      }
    }

    /// <summary>
    ///   Nude selected objects up
    /// </summary>
    private void NudgeUp()
    {
      foreach (GuiPanelItem item in this.SelectionService.CurrentSelection)
      {
        if (item.PositionTop > 0)
        {
          item.PositionTop -= 1;
        }
      }
    }

    /// <summary>
    ///   Nude selection objects down
    /// </summary>
    private void NudgeDown()
    {
      foreach (GuiPanelItem item in this.SelectionService.CurrentSelection)
      {
        double height;
        if (item.Parent == null)
        {
          height = x_canvas.ActualHeight;
        }
        else
        {
          height = item.Parent.ActualHeight;
        }

        if ((item.PositionTop + item.ActualHeight) < height)
        {
          item.PositionTop += 1;
        }
      }
    }

    /// <summary>
    ///   Nude selection objects down
    /// </summary>
    private void NudgeRight()
    {
      foreach (GuiPanelItem item in this.SelectionService.CurrentSelection)
      {
        double width;
        if (item.Parent == null)
        {
          width = x_canvas.ActualWidth;
        }
        else
        {
          width = item.Parent.ActualWidth;
        }

        if ((item.PositionLeft + item.ActualWidth) < width)
        {
          item.PositionLeft += 1;
        }
      }
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // Highlighting
    ////////////////////////////////////////////////////////////////////////////
    #region Highlighting

    /// <summary>
    ///   Handle the dropping of a moved object
    /// </summary>
    /// <param name="p_movingItem"></param>
    /// <param name="p_dropPoint"></param>
    public void HighlightContainers(GuiPanelItem p_movingItem)
    {
      // clear any existing highlights
      ClearContainerHighlights();

      //
      // If the item has no parent, highlight based on mouse location.
      // If the item has a parent, highlight the parent
      //

      if (p_movingItem.Parent == null)
      {
        // Get the canas position and the item position
        Point canvasPoint = Mouse.GetPosition(x_canvas);

        GuiPanelItem parentItem = x_canvas.GetContainerUnderMouse(canvasPoint, p_movingItem, false);
        if (parentItem != null &&
            !parentItem.IsSelected)
        {
          parentItem.IsHighlighted = true;
        }
      }
      else
      {
        p_movingItem.Parent.IsHighlighted = true;
      }
    }

    /// <summary>
    ///   Loop through items and clear any highlights
    /// </summary>
    public void ClearContainerHighlights()
    {
      foreach (GuiPanelItem item in this.GuiPanel.GuiPanelItems)
      {
        item.IsHighlighted = false;
      }
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // Drag & Drop
    ////////////////////////////////////////////////////////////////////////////
    #region Drag & Drop

    /// <summary>
    ///   Handle the dropping of a moved object
    /// </summary>
    /// <param name="p_movingItem"></param>
    /// <param name="p_dropPoint"></param>
    public void OnDropCompleted(GuiPanelItem p_movingItem, List<GuiPanelItem> p_movableItems)
    {
      //
      // Tell each item that dragging is completed so they can adjust their visuals
      //
      foreach (GuiPanelItem selection in this.SelectionService.CurrentSelection)
      {
        selection.IsDragging = false;
      }

      // Get the canas position and the item position
      Point canvasPoint = Mouse.GetPosition(x_canvas);

      GuiPanelItem parentItem = x_canvas.GetContainerUnderMouse(canvasPoint, p_movingItem, false);
      if (parentItem != null)
      {
        Point parentPoint = Mouse.GetPosition(parentItem);

        //
        // Work with every selected item
        //
        foreach (GuiPanelItem selection in p_movableItems)
        {

          if (selection != parentItem && 
              !IsParentSelected(selection) &&
              !selection.IsBoundToParent)
          {
            Point controlPoint = Mouse.GetPosition(selection);

            // Remove it from the panel and add it to the new parent
            this.GuiPanel.RemoveControl(selection);

            // Add it to the new parent
            parentItem.AddChild(selection);

            Point offset = parentItem.ContainerLocation.TopLeft;

            // Adjust the coordinates
            selection.PositionLeft = parentPoint.X + offset.X - controlPoint.X;
            selection.PositionTop = parentPoint.Y + offset.Y - controlPoint.Y;
          }
        }
      }
    }

    /// <summary>
    ///   Handle the dropping of a moved object
    /// </summary>
    /// <param name="p_movingItem"></param>
    /// <param name="p_dropPoint"></param>
    public void OnDragStarted(GuiPanelItem p_movingItem, List<GuiPanelItem> p_movableItems)
    {
      //
      // Tell each item that dragging is so they can adjust their visuals
      //
      foreach (GuiPanelItem selection in this.SelectionService.CurrentSelection)
      {
        selection.IsDragging = true;
      }

      //
      // If this item has a parent, remove it from the parent and put it
      // into the scope of the panel
      // Also adjust the coordinates
      //

      // Get the canas position and the item position
      Point canvasPoint = Mouse.GetPosition(x_canvas);

      foreach (GuiPanelItem selection in p_movableItems)
      {
        Point controlPoint = Mouse.GetPosition(selection);

        if (p_movingItem.Parent != null &&
            !selection.IsBoundToParent)
        {
          //
          // Remove it from its parent
          //
          p_movingItem.Parent.RemoveChild(selection);

          //
          // Add it to the panel
          //
          this.GuiPanel.AddControl(selection);

          //
          // Adjust the coordinates
          //
          selection.PositionLeft = canvasPoint.X - controlPoint.X;
          selection.PositionTop = canvasPoint.Y - controlPoint.Y;
        }
      }
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // Margins and Guides
    ////////////////////////////////////////////////////////////////////////////

    #region Margin and Guides

    /// <summary>
    ///   Show any possible guides and return an adjusted location
    /// </summary>
    /// <param name="p_item"></param>
    /// <param name="p_currentLocation"></param>
    /// <param name="p_desiredLocation"></param>
    /// <param name="p_movableItems"></param>
    /// <param name="p_guideType"></param>
    /// <param name="p_guideAction"></param>
    /// <returns></returns>
    public Rect ShowGuides(GuiPanelItem p_item,
                           Rect p_currentLocation,
                           Rect p_desiredLocation,
                           List<GuiPanelItem> p_movableItems,
                           PanelGuide.EsnGuideAlign p_guideType,
                           PanelGuide.EsnGuideAction p_guideAction)
    {
      Rect retRect = p_desiredLocation;

      ClearGuides();

      m_guideTop = null;
      m_guideBottom = null;
      m_guideLeft = null;
      m_guideRight = null;

      //
      // Clear the margins
      //
      m_marginVertical.Clear();
      m_marginHorizontal.Clear();

      //
      // Recurse through and find the closest guides
      //
      foreach (GuiPanelItem item in this.GuiPanel.GuiPanelItems)
      {
        FindClosestGuides(item, ref p_currentLocation, ref p_desiredLocation, p_movableItems, p_guideType);
      }

      //
      // Recurse through and find the closest margins
      //
      foreach (GuiPanelItem item in this.GuiPanel.GuiPanelItems)
      {
        FindClosestMargins(p_item, item, ref p_currentLocation, ref p_desiredLocation, p_movableItems, p_guideType);
      }

      //
      // Find margins from panel
      //
      FindPanelMargins(ref p_currentLocation, ref p_desiredLocation, p_guideType);


      // Find the closer guide in vertical && horizontal directions
      if (m_guideLeft != null && m_guideRight != null)
      {
        // If the distance is the same, keep them both
        if (m_guideLeft.Distance != m_guideRight.Distance)
        {
          if (m_guideLeft.Distance < m_guideRight.Distance)
          {
            m_guideRight = null;
          }
          else
          {
            m_guideLeft = null;
          }
        }
      }
      if (m_guideTop != null && m_guideBottom != null)
      {
        if (m_guideTop.Distance != m_guideBottom.Distance)
        {
          if (m_guideTop.Distance < m_guideBottom.Distance)
          {
            m_guideBottom = null;
          }
          else
          {
            m_guideTop = null;
          }
        }
      }

      // Compare margins to guides
      if (m_marginVertical.Count > 0)
      {
        double marginDistance = m_marginVertical.First().Distance;

        if (m_guideTop != null)
        {
          // If the top is less, clear the margins
          if (m_guideTop.Distance < marginDistance)
          {
            m_marginVertical.Clear();
          }
          else if (m_guideTop.Distance > marginDistance)
          {
            m_guideTop = null;
          }
        }

        if (m_guideBottom != null)
        {
          // If the top is less, clear the margins
          if (m_guideBottom.Distance < marginDistance)
          {
            m_marginVertical.Clear();
          }
          else if (m_guideBottom.Distance > marginDistance)
          {
            m_guideBottom = null;
          }
        }
      }
      // Compare horizontal margins to guides
      if (m_marginHorizontal.Count > 0)
      {
        double marginDistance = m_marginHorizontal.First().Distance;

        if (m_guideLeft != null)
        {
          // If the top is less, clear the margins
          if (m_guideLeft.Distance < marginDistance)
          {
            m_marginHorizontal.Clear();
          }
          else if (m_guideLeft.Distance > marginDistance)
          {
            m_guideLeft = null;
          }
        }

        if (m_guideRight != null)
        {
          // If the top is less, clear the margins
          if (m_guideRight.Distance < marginDistance)
          {
            m_marginHorizontal.Clear();
          }
          else if (m_guideRight.Distance > marginDistance)
          {
            m_guideRight = null;
          }
        }
      }

      //
      // Draw the guides and make the adjustments
      //
      if (m_guideLeft != null)
      {
        retRect.X = m_guideLeft.GuideLine.X1;

        DrawGuide(m_guideLeft.GuideLine);
      }

      if (m_guideRight != null)
      {
        // Only adjust on the left or the right
        if (m_guideLeft == null)
        {
          if (p_guideAction == GuiPanelDesigner.PanelGuide.EsnGuideAction.Move)
          {
            retRect.X = m_guideRight.GuideLine.X1 - p_item.ActualWidth;
          }
          else
          {
            if (m_guideRight.GuideLine.X1 - retRect.X > GuiPanelItem.MIN_WIDTH)
            {
              retRect.Width = m_guideRight.GuideLine.X1 - retRect.X;
            }
            else
            {
              retRect.Width = GuiPanelItem.MIN_WIDTH;
            }
          }
        }
        DrawGuide(m_guideRight.GuideLine);
      }

      if (m_guideTop != null)
      {
        retRect.Y = m_guideTop.GuideLine.Y1;

        DrawGuide(m_guideTop.GuideLine);
      }

      if (m_guideBottom != null)
      {
        if (m_guideTop == null)
        {
          if (p_guideAction == GuiPanelDesigner.PanelGuide.EsnGuideAction.Move)
          {
            retRect.Y = m_guideBottom.GuideLine.Y1 - p_item.ActualHeight;
          }
          else
          {
            if (m_guideBottom.GuideLine.Y1 - retRect.Y > GuiPanelItem.MIN_HEIGHT)
            {
              retRect.Height = m_guideBottom.GuideLine.Y1 - retRect.Y;
            }
            else
            {
              retRect.Height = GuiPanelItem.MIN_HEIGHT;
            }
          }
        }
        DrawGuide(m_guideBottom.GuideLine);
      }

      //
      // Draw margin guides
      //
      // Horiztonal
      foreach (PanelGuide guide in this.m_marginHorizontal)
      {
        if (guide.GuideAlign == PanelGuide.EsnGuideAlign.Right)
        {
          retRect.X = guide.GuideLine.X2;
        }
        else
        {
          if (p_guideAction == GuiPanelDesigner.PanelGuide.EsnGuideAction.Move)
          {
            retRect.X = guide.GuideLine.X2 - p_desiredLocation.Width;
          }
          else
          {
            retRect.Width = guide.GuideLine.X2 - p_desiredLocation.X;
          }
        }
        DrawMargin(guide.GuideLine);
      }
      // Vertical
      foreach (PanelGuide guide in this.m_marginVertical)
      {
        if (guide.GuideAlign == PanelGuide.EsnGuideAlign.Bottom)
        {
          retRect.Y = guide.GuideLine.Y2;
        }
        else
        {
          if (p_guideAction == GuiPanelDesigner.PanelGuide.EsnGuideAction.Move)
          {
            retRect.Y = guide.GuideLine.Y2 - p_desiredLocation.Height;
          }
          else
          {
            retRect.Height = guide.GuideLine.Y2 - p_desiredLocation.Y;
          }
        }
        DrawMargin(guide.GuideLine);
      }

      // Return the possibly adjusted point
      return retRect;
    }

    /// <summary>
    ///   Find any margins within the canvas
    /// </summary>
    /// <param name="p_movingRect"></param>
    private void FindPanelMargins(ref Rect p_currentLocation,
                                   ref Rect p_movingRect,
                                   PanelGuide.EsnGuideAlign p_align)
    {
      Point leftPoint = new Point(0, 0);
      Rect compareItemRect = new Rect(leftPoint.X, leftPoint.Y, x_canvas.ActualWidth, x_canvas.ActualHeight);

      // Left inside margin - Left side of moving, left side of compare
      if ((p_align & PanelGuide.EsnGuideAlign.Left) == PanelGuide.EsnGuideAlign.Left)
      {
        FindHorizontalMargin(
          ref p_currentLocation,
          ref compareItemRect, ref p_movingRect,
          compareItemRect.X + 1,
          compareItemRect.X + MARGIN_DISTANCE + 1,
          p_movingRect.X);
      }

      // Right inside margin - Right side of moving, right side of compare
      if ((p_align & PanelGuide.EsnGuideAlign.Right) == PanelGuide.EsnGuideAlign.Right)
      {
        FindHorizontalMargin(
          ref p_currentLocation,
          ref compareItemRect, ref p_movingRect,
          compareItemRect.X + compareItemRect.Width - 1,
          compareItemRect.X + compareItemRect.Width - MARGIN_DISTANCE - 1,
          p_movingRect.X + p_movingRect.Width);
      }

      // Top inside margin - Top side of moving, Top side of compare
      if ((p_align & PanelGuide.EsnGuideAlign.Top) == PanelGuide.EsnGuideAlign.Top)
      {
        FindVerticalMargin(
          ref p_currentLocation,
          ref compareItemRect, ref p_movingRect,
          compareItemRect.Y + 1,
          compareItemRect.Y + MARGIN_DISTANCE + 1,
          p_movingRect.Y);
      }

      // Bottom inside margin - Bottom side of moving, bottom side of compare
      if ((p_align & PanelGuide.EsnGuideAlign.Bottom) == PanelGuide.EsnGuideAlign.Bottom)
      {
        FindVerticalMargin(
          ref p_currentLocation,
          ref compareItemRect, ref p_movingRect,
          compareItemRect.Y + compareItemRect.Height - 1,
          compareItemRect.Y + compareItemRect.Height - MARGIN_DISTANCE - 1,
          p_movingRect.Y + p_movingRect.Height);
      }
    }

    /// <summary>
    ///   Go through the items and find the closest margins
    /// </summary>
    /// <param name="p_item"></param>
    /// <param name="p_rect"></param>
    private void FindClosestMargins(GuiPanelItem p_movingItem,
      GuiPanelItem p_item,
      ref Rect p_currentLocation,
      ref Rect p_movingRect,
      List<GuiPanelItem> p_movableItems,
      PanelGuide.EsnGuideAlign p_align)
    {
      if (p_movableItems.Contains(p_item))
      {
        return;
      }

      Point leftPoint = p_item.TranslatePoint(new Point(0, 0), x_canvas);
      Rect compareItemRect = new Rect(leftPoint.X, leftPoint.Y, p_item.ActualWidth, p_item.ActualHeight);

      // Left outside margin - Left side of moving, right side of compare
      if ((p_align & PanelGuide.EsnGuideAlign.Left) == PanelGuide.EsnGuideAlign.Left)
      {
        FindHorizontalMargin(
          ref p_currentLocation,
          ref compareItemRect, ref p_movingRect,
          compareItemRect.X + compareItemRect.Width - 1,
          compareItemRect.X + compareItemRect.Width + MARGIN_DISTANCE - 1,
          p_movingRect.X);
      }

      // Right outside margin - Right side of moving, left side of compare
      if ((p_align & PanelGuide.EsnGuideAlign.Right) == PanelGuide.EsnGuideAlign.Right)
      {
        FindHorizontalMargin(
          ref p_currentLocation,
          ref compareItemRect, ref p_movingRect,
          compareItemRect.X + 1,
          compareItemRect.X - MARGIN_DISTANCE + 1,
          p_movingRect.X + p_movingRect.Width);
      }

      // Top outside margin - Top side of moving, bottom side of compare
      if ((p_align & PanelGuide.EsnGuideAlign.Top) == PanelGuide.EsnGuideAlign.Top)
      {
        FindVerticalMargin(
          ref p_currentLocation,
          ref compareItemRect, ref p_movingRect,
          compareItemRect.Y + compareItemRect.Height - 1,
          compareItemRect.Y + compareItemRect.Height + MARGIN_DISTANCE - 1,
          p_movingRect.Y);
      }

      // Bottom outside margin - Bottom side of moving, Top side of compare
      if ((p_align & PanelGuide.EsnGuideAlign.Bottom) == PanelGuide.EsnGuideAlign.Bottom)
      {
        FindVerticalMargin(
          ref p_currentLocation,
          ref compareItemRect, ref p_movingRect,
          compareItemRect.Y + 1,
          compareItemRect.Y - MARGIN_DISTANCE + 1,
          p_movingRect.Y + p_movingRect.Height);
      }


      if (p_item.IsContainer)
      {
        // Left inside margin - Left side of moving, left side of compare
        if ((p_align & PanelGuide.EsnGuideAlign.Left) == PanelGuide.EsnGuideAlign.Left)
        {
          FindHorizontalMargin(
            ref p_currentLocation,
            ref compareItemRect, ref p_movingRect,
            compareItemRect.X + 1,
            compareItemRect.X + MARGIN_DISTANCE + p_item.MarginInsideLeft + 1,
            p_movingRect.X);
        }

        // Right inside margin - Right side of moving, right side of compare
        if ((p_align & PanelGuide.EsnGuideAlign.Right) == PanelGuide.EsnGuideAlign.Right)
        {
          FindHorizontalMargin(
            ref p_currentLocation,
            ref compareItemRect, ref p_movingRect,
            compareItemRect.X + compareItemRect.Width - 1,
            compareItemRect.X + compareItemRect.Width - MARGIN_DISTANCE - p_item.MarginInsideRight - 1,
            p_movingRect.X + p_movingRect.Width);
        }

        // Top inside margin - Top side of moving, Top side of compare
        if ((p_align & PanelGuide.EsnGuideAlign.Top) == PanelGuide.EsnGuideAlign.Top)
        {
          FindVerticalMargin(
            ref p_currentLocation,
            ref compareItemRect, ref p_movingRect,
            compareItemRect.Y + 1,
            compareItemRect.Y + MARGIN_DISTANCE + p_item.MarginInsideTop + 1,
            p_movingRect.Y);
        }

        // Bottom inside margin - Bottom side of moving, bottom side of compare
        if ((p_align & PanelGuide.EsnGuideAlign.Bottom) == PanelGuide.EsnGuideAlign.Bottom)
        {
          FindVerticalMargin(
            ref p_currentLocation,
            ref compareItemRect, ref p_movingRect,
            compareItemRect.Y + compareItemRect.Height - 1,
            compareItemRect.Y + compareItemRect.Height - MARGIN_DISTANCE - p_item.MarginInsideBottom - 1,
            p_movingRect.Y + p_movingRect.Height);
        }
      }

      // Cycle through any chilren and check them
      if (p_item.IsContainer)
      {
        foreach (GuiPanelItem child in p_item.Children)
        {
          FindClosestMargins(p_movingItem, child, ref p_currentLocation, ref p_movingRect, p_movableItems, p_align);
        }
      }
    }

    /// <summary>
    ///   Find any margins in the vertical direction
    /// </summary>
    /// <param name="p_compareRect"></param>
    /// <param name="p_movingRect"></param>
    /// <param name="p_compareY"></param>
    /// <param name="p_compareYWithMargin"></param>
    /// <param name="p_movingY"></param>
    private void FindVerticalMargin(ref Rect p_currentLocation,
                                     ref Rect p_compareRect,
                                     ref Rect p_movingRect,
                                     double p_compareY,
                                     double p_compareYWithMargin,
                                     double p_movingY)
    {
      double center;
      // First see if we're within horizontal boundaries
      if (WithinHorizontalBoundaries(ref p_currentLocation, ref p_compareRect, ref p_movingRect, out center))
      {
        // Next see if we're within snapping distance
        double verticalMarginDistance = Math.Abs(p_compareYWithMargin - p_movingY);
        if (verticalMarginDistance <= SNAP_DISTANCE)
        {
          // Now check to see if we're closer than any other left margins
          bool add = false;

          if (m_marginVertical.Count == 0)
          {
            add = true;
          }
          else
          {
            // If we're greater than the distance in there, don't add
            // if we're the same, add
            // if we're smaller, clear and add
            double currentDistance = m_marginVertical.First().Distance;
            if (currentDistance > verticalMarginDistance)
            {
              m_marginVertical.Clear();
              add = true;
            }
            else if (currentDistance == verticalMarginDistance)
            {
              add = true;
            }
          }

          // If we made it here, add it
          if (add)
          {
            Line marginLine = new Line();
            marginLine.X1 = center;
            marginLine.X2 = center;
            marginLine.Y1 = p_compareY;
            marginLine.Y2 = p_compareYWithMargin;
            PanelGuide margin = new PanelGuide();
            margin.Distance = verticalMarginDistance;
            margin.GuideLine = marginLine;

            // Tell the panel which side we're on
            if (p_movingY < p_compareY)
            {
              margin.GuideAlign = PanelGuide.EsnGuideAlign.Top;
            }
            else
            {
              margin.GuideAlign = PanelGuide.EsnGuideAlign.Bottom;
            }

            m_marginVertical.Add(margin);
          }
        }
      }
    }

    /// <summary>
    ///   Find any margins in the horizontal direction
    /// </summary>
    /// <param name="p_compareRect"></param>
    /// <param name="p_movingRect"></param>
    /// <param name="p_compareX"></param>
    /// <param name="p_compareXWithMargin"></param>
    /// <param name="p_movingX"></param>
    private void FindHorizontalMargin(ref Rect p_currentLocation,
                                       ref Rect p_compareRect,
                                       ref Rect p_movingRect,
                                       double p_compareX,
                                       double p_compareXWithMargin,
                                       double p_movingX)
    {
      // Comparing the left side of the moving rectangle to the right side of the item
      double centerLeft;
      // First see if we're within vertical boundaries
      if (WithinVerticalBoundaries(ref p_currentLocation, ref p_compareRect, ref p_movingRect, out centerLeft))
      {
        // Next see if we're within snapping distance
        double horizontalMarginDistance = Math.Abs(p_compareXWithMargin - p_movingX);
        if (horizontalMarginDistance <= SNAP_DISTANCE)
        {
          // Now check to see if we're closer than any other left margins
          bool add = false;

          if (m_marginHorizontal.Count == 0)
          {
            add = true;
          }
          else
          {
            // If we're greater than the distance in there, don't add
            // if we're the same, add
            // if we're smaller, clear and add
            double currentDistance = m_marginHorizontal.First().Distance;
            if (currentDistance > horizontalMarginDistance)
            {
              m_marginHorizontal.Clear();
              add = true;
            }
            else if (currentDistance == horizontalMarginDistance)
            {
              add = true;
            }
          }

          // If we made it here, add it
          if (add)
          {
            Line marginLine = new Line();
            marginLine.Y1 = centerLeft;
            marginLine.Y2 = centerLeft;
            marginLine.X1 = p_compareX;
            marginLine.X2 = p_compareXWithMargin;
            PanelGuide margin = new PanelGuide();
            margin.Distance = horizontalMarginDistance;
            margin.GuideLine = marginLine;

            // Tell the panel which side we're on
            if (p_movingX < p_compareX)
            {
              margin.GuideAlign = PanelGuide.EsnGuideAlign.Left;
            }
            else
            {
              margin.GuideAlign = PanelGuide.EsnGuideAlign.Right;
            }

            m_marginHorizontal.Add(margin);
          }
        }
      }
    }

    /// <summary>
    ///   See if the item is within the vertical boundaries of another item
    /// </summary>
    /// <param name="p_compareItem"></param>
    /// <param name="p_movingRect"></param>
    /// <param name="p_leftCenter"></param>
    /// <returns></returns>
    private bool WithinVerticalBoundaries(ref Rect p_currentLocation,
                                           ref Rect p_compareItem,
                                           ref Rect p_movingRect,
                                           out double p_leftCenter)
    {
      double y1top = p_compareItem.Y;
      double y1bot = p_compareItem.Y + p_compareItem.Height;
      double y2top = p_movingRect.Y;
      double y2bot = p_movingRect.Y + p_movingRect.Height;
      double y3top = p_currentLocation.Y;
      double y3bot = p_currentLocation.Y + p_currentLocation.Height;

      // Compare is almost above moving
      if (y1top <= y2top && y1bot <= y2bot && y1bot >= y2top)
      {
        p_leftCenter = y1bot - (y1bot - y3top) / 2;
        return true;
      }
      // Compare is within the moving
      if (y1bot <= y2bot && y1top >= y2top)
      {
        p_leftCenter = p_compareItem.Height / 2 + y1top;
        return true;
      }
      // Moving is within compare
      if (y2top >= y1top && y2bot <= y1bot)
      {
        p_leftCenter = p_movingRect.Height / 2 + y3top;
        return true;
      }
      // Compare is almost below moving
      if (y2top <= y1top && y2bot <= y1bot && y2bot >= y1top)
      {
        p_leftCenter = (y3bot - y1top) / 2 + y1top;
        return true;
      }
      p_leftCenter = 0;
      return false;
    }

    /// <summary>
    ///   See if the item is within the horizontal boundaries of another item
    /// </summary>
    /// <param name="p_compareItem"></param>
    /// <param name="p_movingRect"></param>
    /// <param name="p_center"></param>
    /// <returns></returns>
    private bool WithinHorizontalBoundaries( ref Rect p_currentLocation,
                                             ref Rect p_compareItem,
                                             ref Rect p_movingRect,
                                             out double p_center)
    {
      double x1left = p_compareItem.X;
      double x1right = p_compareItem.X + p_compareItem.Width;
      double x2left = p_movingRect.X;
      double x2right = p_movingRect.X + p_movingRect.Width;
      double x3left = p_currentLocation.X;
      double x3right = p_currentLocation.X + p_currentLocation.Width;

      // Compare is almost left of moving
      if (x1left <= x2left && x1right <= x2right && x1right >= x2left)
      {
        p_center = x1right - (x1right - x3left) / 2;
        return true;
      }
      // Compare is within the moving
      if (x1right <= x2right && x1left >= x2left)
      {
        p_center = p_compareItem.Width / 2 + x1left;
        return true;
      }
      // Moving is within compare
      if (x2left >= x1left && x2right <= x1right)
      {
        p_center = p_movingRect.Width / 2 + x3left;
        return true;
      }
      // Compare is almost right of moving
      if (x2left <= x1left && x2right <= x1right && x2right >= x1left)
      {
        p_center = (x3right - x1left) / 2 + x1left;
        return true;
      }

      p_center = 0;
      return false;
    }

    /// <summary>
    ///   Go through the items and find the closest guides
    /// </summary>
    /// <param name="p_item"></param>
    /// <param name="p_movingRect"></param>
    private void FindClosestGuides(GuiPanelItem p_item,
      ref Rect p_currentLocation,
      ref Rect p_movingRect,
      List<GuiPanelItem> p_movableItems,
      PanelGuide.EsnGuideAlign p_align)
    {
      if (p_movableItems.Contains(p_item))
      {
        return;
      }

      //
      // Left guide
      //
      if ((p_align & PanelGuide.EsnGuideAlign.Left) == PanelGuide.EsnGuideAlign.Left)
      {
        Point leftPoint = p_item.TranslatePoint(new Point(0, 0), x_canvas);
        double leftDistance = Math.Abs(leftPoint.X - p_movingRect.X);
        // First check to see if we're within snapping distance
        if (leftDistance <= SNAP_DISTANCE)
        {
          // If there is no left guide, immediatly use this one
          if (m_guideLeft == null ||
              (m_guideLeft != null && leftDistance < m_guideLeft.Distance))
          {
            m_guideLeft = new PanelGuide();
            m_guideLeft.Distance = leftDistance;
            m_guideLeft.GuideLine = new Line();

            m_guideLeft.GuideLine.X1 = leftPoint.X;
            m_guideLeft.GuideLine.X2 = leftPoint.X;

            // Find the highest point and the lowest point
            m_guideLeft.GuideLine.Y1 = Math.Min(p_currentLocation.Y, leftPoint.Y);
            m_guideLeft.GuideLine.Y2 = Math.Max(p_currentLocation.Bottom,
              leftPoint.Y + p_item.ActualHeight);
          }
          // If the same, see if it's further in the perpendicular direction
          else if (m_guideLeft != null &&
                   leftDistance == m_guideLeft.Distance)
          {
            m_guideLeft.GuideLine.Y1 = Math.Min(leftPoint.Y, m_guideLeft.GuideLine.Y1);
            m_guideLeft.GuideLine.Y2 = Math.Max(
              m_guideLeft.GuideLine.Y2,
              leftPoint.Y + p_item.ActualHeight);
          }
        }
      }

      //
      // Right guide
      //
      if ((p_align & PanelGuide.EsnGuideAlign.Right) == PanelGuide.EsnGuideAlign.Right)
      {
        Point rightPoint = p_item.TranslatePoint(new Point(0, 0), x_canvas);
        rightPoint.X += p_item.ActualWidth;
        double rightDistance = Math.Abs(rightPoint.X - (p_movingRect.X + p_movingRect.Width));
        // First check to see if we're within snapping distance
        if (rightDistance <= SNAP_DISTANCE)
        {
          // If there is no right guide, immediatly use this one
          if (m_guideRight == null ||
              (m_guideRight != null && rightDistance < m_guideRight.Distance))
          {
            m_guideRight = new PanelGuide();
            m_guideRight.Distance = rightDistance;
            m_guideRight.GuideLine = new Line();

            m_guideRight.GuideLine.X1 = rightPoint.X;
            m_guideRight.GuideLine.X2 = rightPoint.X;

            // Find the highest point and the lowest point
            m_guideRight.GuideLine.Y1 = Math.Min(p_currentLocation.Y, rightPoint.Y);
            m_guideRight.GuideLine.Y2 = Math.Max(p_currentLocation.Bottom,
              rightPoint.Y + p_item.ActualHeight);
          }
          // If the same, see if it's further in the perpendicular direction
          else if (m_guideRight != null &&
                   rightDistance == m_guideRight.Distance)
          {
            m_guideRight.GuideLine.Y1 = Math.Min(rightPoint.Y, m_guideRight.GuideLine.Y1);
            m_guideRight.GuideLine.Y2 = Math.Max(
              m_guideRight.GuideLine.Y2,
              rightPoint.Y + p_item.ActualHeight);
          }
        }
      }

      //
      // Top guide
      //
      if ((p_align & PanelGuide.EsnGuideAlign.Top) == PanelGuide.EsnGuideAlign.Top)
      {
        Point topPoint = p_item.TranslatePoint(new Point(0, 0), x_canvas);
        double topDistance = Math.Abs(topPoint.Y - p_movingRect.Y);
        // First check to see if we're within snapping distance
        if (topDistance <= SNAP_DISTANCE)
        {
          // If there is no top guide, immediatly use this one
          if (m_guideTop == null ||
              (m_guideTop != null && topDistance < m_guideTop.Distance))
          {
            m_guideTop = new PanelGuide();
            m_guideTop.Distance = topDistance;
            m_guideTop.GuideLine = new Line();

            m_guideTop.GuideLine.Y1 = topPoint.Y;
            m_guideTop.GuideLine.Y2 = topPoint.Y;

            // Find the highest point and the lowest point
            m_guideTop.GuideLine.X1 = Math.Min(p_currentLocation.X, topPoint.X);
            m_guideTop.GuideLine.X2 = Math.Max(p_currentLocation.Right,
              topPoint.X + p_item.ActualWidth);
          }
          // If the same, see if it's further in the perpendicular direction
          else if (m_guideTop != null &&
                   topDistance == m_guideTop.Distance)
          {
            m_guideTop.GuideLine.X1 = Math.Min(topPoint.X, m_guideTop.GuideLine.X1);
            m_guideTop.GuideLine.X2 = Math.Max(
              m_guideTop.GuideLine.X2,
              topPoint.X + p_item.ActualWidth);
          }
        }
      }

      //
      // Bottom guide
      //
      if ((p_align & PanelGuide.EsnGuideAlign.Bottom) == PanelGuide.EsnGuideAlign.Bottom)
      {
        Point bottomPoint = p_item.TranslatePoint(new Point(0, 0), x_canvas);
        bottomPoint.Y += p_item.ActualHeight;
        double bottomDistance = Math.Abs(bottomPoint.Y - (p_movingRect.Y + p_movingRect.Height));
        // First check to see if we're within snapping distance
        if (bottomDistance <= SNAP_DISTANCE)
        {
          // If there is no bottom guide, immediatly use this one
          if (m_guideBottom == null ||
              (m_guideBottom != null && bottomDistance < m_guideBottom.Distance))
          {
            m_guideBottom = new PanelGuide();
            m_guideBottom.Distance = bottomDistance;
            m_guideBottom.GuideLine = new Line();

            m_guideBottom.GuideLine.Y1 = bottomPoint.Y;
            m_guideBottom.GuideLine.Y2 = bottomPoint.Y;

            // Find the highest point and the lowest point
            m_guideBottom.GuideLine.X1 = Math.Min(p_currentLocation.X, bottomPoint.X);
            m_guideBottom.GuideLine.X2 = Math.Max(p_currentLocation.Right,
              bottomPoint.X + p_item.ActualWidth);
          }
          // If the same, see if it's further in the perpendicular direction
          else if (m_guideBottom != null &&
                   bottomDistance == m_guideBottom.Distance)
          {
            m_guideBottom.GuideLine.X1 = Math.Min(bottomPoint.X, m_guideBottom.GuideLine.X1);
            m_guideBottom.GuideLine.X2 = Math.Max(
              m_guideBottom.GuideLine.X2,
              bottomPoint.X + p_item.ActualWidth);
          }
        }
      }


      // Cycle through any chilren and check them
      if (p_item.IsContainer)
      {
        foreach (GuiPanelItem child in p_item.Children)
        {
          FindClosestGuides(child, ref p_currentLocation, ref p_movingRect, p_movableItems, p_align);
        }
      }
    }

    /// <summary>
    ///   Clear any guide adorners
    /// </summary>
    public void ClearGuides()
    {
      if (m_adornerLayer != null)
      {
        foreach (GuideAdorner adorner in m_guideAdorners)
        {
          m_adornerLayer.Remove(adorner);
        }
      }
    }

    /// <summary>
    ///   Draw a margin line
    /// </summary>
    /// <param name="p_guide"></param>
    private void DrawMargin(Line p_guide)
    {
      DrawGuideAdorner(p_guide, GuideAdorner.EsnGuideType.Margin);
    }

    /// <summary>
    ///   Draw a guide line
    /// </summary>
    /// <param name="p_guide"></param>
    private void DrawGuide(Line p_guide)
    {
      DrawGuideAdorner(p_guide, GuideAdorner.EsnGuideType.Guide);
    }

    /// <summary>
    ///   Draw the actual adorner
    /// </summary>
    /// <param name="p_guide"></param>
    /// <param name="p_type"></param>
    private void DrawGuideAdorner(Line p_guide, GuideAdorner.EsnGuideType p_type)
    {
      if (m_adornerLayer != null)
      {
        GuideAdorner guide = new GuideAdorner(x_canvas, p_guide, p_type);
        m_adornerLayer.Add(guide);
        m_guideAdorners.Add(guide);
      }
    }

    /// <summary>
    ///   
    /// </summary>
    /// <param name="p_startPoint"></param>
    /// <param name="p_selectionRectangle"></param>
    public void SelectWithinRectangle(Point p_startPoint, Rect p_selectionRectangle)
    {
      // Set to defaults
      m_rubberBandStartParent = null;
      m_rubberBandSelection.Clear();

      // Gather any intersected objects
      foreach (GuiPanelItem item in this.GuiPanel.GuiPanelItems)
      {
        SelectIfWithinRectangle(item, ref p_startPoint, ref p_selectionRectangle);
      }
      int selected = 0;

      // Make sure we're selecting on the same parentage
      foreach (GuiPanelItem selection in m_rubberBandSelection)
      {
        if (selection.Parent == m_rubberBandStartParent)
        {
          this.SelectionService.AddToSelection(selection);
          selected++;
        }
      }

      // If there were no selected, go back and retry
      if (selected == 0 && m_rubberBandStartParent != null)
      {
        foreach (GuiPanelItem selection in m_rubberBandSelection)
        {
          if (selection.Parent == m_rubberBandStartParent.Parent)
          {
            this.SelectionService.AddToSelection(selection);
          }
        }
      }
    }

    /// <summary>
    ///   Check if the item is within the selection rectangle
    /// </summary>
    /// <param name="p_item"></param>
    /// <param name="p_rectangle"></param>
    private void SelectIfWithinRectangle(GuiPanelItem p_item, ref Point p_startPoint, ref Rect p_rectangle)
    {
      Point canvasPoint = p_item.TranslatePoint(new Point(0, 0), x_canvas);
      Rect itemRect = new Rect(canvasPoint.X, canvasPoint.Y, p_item.ActualWidth, p_item.ActualHeight);

      // Capture the starting parent
      if (itemRect.Contains(p_startPoint))
      {
        m_rubberBandStartParent = p_item;
      }

      // Check to see if we intersect
      if (p_rectangle.IntersectsWith(itemRect))
      {
        m_rubberBandSelection.Add(p_item);
      }

      //
      // If this item has children, recurse through
      //
      if (p_item.IsContainer)
      {
        foreach (GuiPanelItem child in p_item.Children)
        {
          SelectIfWithinRectangle(child, ref p_startPoint, ref p_rectangle);
        }
      }
    }

    #endregion
  }
}
