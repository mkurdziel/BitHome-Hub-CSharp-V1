using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using SyNet.Actions;
using SyNet.Events;
using SyNet.Gui.Adorners;
using SyNet.Gui.Interfaces;
using SyNet.Gui.Models;

namespace SyNet.Gui
{
  public class GuiPanelItem : ContentControl, IXmlSerializable,
                                              IGuiPanelValueControl,
                                              INotifyPropertyChanged
  {
    /// <summary>
    ///   Enumeration representing the position of the label with respect to the
    ///   control
    /// </summary>
    public enum EsnLabelPosition
    {
      /// <summary>
      ///   Left
      /// </summary>
      Left,
      /// <summary>
      ///   Top
      /// </summary>
      Top,
      /// <summary>
      ///   Right
      /// </summary>
      Right,
      /// <summary>
      ///   Bottom
      /// </summary>
      Bottom
    }

    ////////////////////////////////////////////////////////////////////////////
    // Constants
    ////////////////////////////////////////////////////////////////////////////
    #region Constants

    public const double MIN_WIDTH = 10;
    public const double MIN_HEIGHT = 10;

    public const string STR_XML_ROOT = "GuiPanelItemSetup";
    public const string STR_XML_POSITIONTOP = "PositionTop";
    public const string STR_XML_POSITIONLEFT = "PositionLeft";
    public const string STR_XML_POSITIONZINDEX = "PositionZIndex";
    public const string STR_XML_GUIITEMCONTROL = "GuiItemControl";
    public const string STR_XML_CLASS = "ClassName";
    public const string STR_XML_GUIPANELCHILDREN = "GuiPanelChildren";
    public const string STR_XML_GUIPANELITEM = "GuiPanelItem";
    public const string STR_XML_WIDTH = "Width";
    public const string STR_XML_HEIGHT = "Height";
    public const string STR_XML_ACTION = "GuiAction";
    public const string STR_XML_TRIGGER = "Gui";
    public const string STR_XML_PARAMETER = "GuiParameter";
    public const string STR_XML_ISCONTROLENABLED = "IsControlEnabled";
    public const string STR_XML_ISLABELVISIBLE = "IsLabelVisible";
    public const string STR_XML_LABELTEXT = "LabelText";
    public const string STR_XML_LABELSPACING = "LabelSpacing";
    public const string STR_XML_LABELPOSITION = "LabelPosition";
    public const string STR_XML_LABELSIZESYNC = "LabelSizeSync";

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // Member Variables
    ////////////////////////////////////////////////////////////////////////////
    #region Member Variables

    private List<GuiPanelItem> m_children = new List<GuiPanelItem>();
    private IGuiPanelControl m_guiPanelControl;
    private GuiPanelDesigner m_guiPanelDesigner = null;
    private GuiPanelItem m_parent = null;

    //
    // Properties
    //
    private double m_positionTop;
    private double m_positionLeft;

    //
    // Flags
    //
    private bool m_isSelected;
    private bool m_isEditing;
    private bool m_isHighlighted;
    private bool m_isMoving;

    //
    // Adorners
    //
    private AdornerLayer m_adornerLayer = null;
    private Adorner m_resizeAdorner = null;
    private Adorner m_selectAdorner = null;
    private Adorner m_moveAdorner = null;
    private Adorner m_highlightAdorner = null;

    //
    //  Action, trigger, parameter
    //
    private GuiAction m_action = null;
    private GuiParameter m_parameter = null;

    //
    // Gui objects for parameters
    //
    private StackPanel m_parameterStackPanel = null;
    private Label m_parameterLabel = null;
    private GuiTriggerItem m_triggerItem;
    private bool m_isLabelVisible = true;
    private string m_labelText = String.Empty;
    private double m_labelSpacing = 0;
    private EsnLabelPosition m_labelPosition = EsnLabelPosition.Left;
    private bool m_labelSizeSync = true;

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // Properties
    ////////////////////////////////////////////////////////////////////////////
    #region Properties

    #region Label


    /// <summary>
    ///   Returns true if the label sizes are synced
    /// </summary>
    public bool LabelSizeSync
    {
      get
      {
        if (this.HasParameter)
        {
          return this.Parent.LabelSizeSync;
        }
        return m_labelSizeSync;
      }
      set
      {

        //
        // If this item has children, save the value and handle the syncing
        // If this item is a child, tell the parent of the change
        //
        if (this.HasParameter)
        {
          m_labelSizeSync = value;

          if (this.Parent.LabelSizeSync != value)
          {
            this.Parent.LabelSizeSync = value;
          }
        }
        else
        {
          if (m_labelSizeSync != value)
          {
            m_labelSizeSync = value;

            foreach (GuiPanelItem child in this.Children)
            {
              child.LabelSizeSync = m_labelSizeSync;
            }

            SyncChildLabelWidths();
          }
        }

        OnPropertyChanged("LabelSizeSync");
      }
    }

    /// <summary>
    ///   Gets or sets the label position relative to the control
    /// </summary>
    public EsnLabelPosition LabelPosition
    {
      get { return m_labelPosition; }
      set
      {
        if (m_labelPosition != value)
        {
          m_labelPosition = value;

          SetupLabel();

          OnPropertyChanged("LabelPosition");
        }
      }
    }

    /// <summary>
    ///   Gets or sets the label spacing
    /// </summary>
    public double LabelSpacing
    {
      get { return m_labelSpacing; }
      set
      {
        if (m_labelSpacing != value)
        {
          m_labelSpacing = value;

          if (m_parameterLabel != null)
          {
            //
            // Update label spacing
            //
            UpdateLabelMargin();
          }

          OnPropertyChanged("LabelSpacing");
        }
      }
    }

    /// <summary>
    ///   Gets or sets a flag that returns true if the label is visible
    /// </summary>
    public bool IsLabelVisible
    {
      get { return m_isLabelVisible; }
      set
      {
        if (m_isLabelVisible != value)
        {
          m_isLabelVisible = value;

          //
          // Update the label with any modifications
          //
          SetupLabel();

          //
          // Sync label widths if necessary
          //
          if (this.Parent != null)
          {
            this.Parent.SyncChildLabelWidths();
          }

          OnPropertyChanged("IsLabelVisible");
        }
      }
    }

    /// <summary>
    ///   Set all child label widths to a value
    /// </summary>
    public double ChildLabelWidth
    {
      set
      {
        foreach (GuiPanelItem child in this.Children)
        {
          child.LabelWidth = value;
        }
      }
    }

    /// <summary>
    ///   Gets or sets the text of the label
    /// </summary>
    public string LabelText
    {
      get { return m_labelText; }
      set
      {
        if (m_labelText == null || !m_labelText.Equals(value))
        {
          m_labelText = value;

          //
          // Update the label if necessary
          //
          if (m_parameterLabel != null)
          {
            m_parameterLabel.Content = m_labelText;
          }

          //
          // Tell the parent to sync if necessary
          //
          if (this.Parent != null)
          {
            this.Parent.SyncChildLabelWidths();
          }

          OnPropertyChanged("LabelText");
        }
      }
    }

    /// <summary>
    ///   Get or set the label with if there is a label
    /// </summary>
    public double LabelWidth
    {
      get
      {
        if (m_parameterLabel != null)
        {
          return m_parameterLabel.Width;
        }
        return 0;
      }
      set
      {
        if (m_parameterLabel != null)
        {
          m_parameterLabel.Width = value;
        }
      }
    }

    #endregion

    /// <summary>
    ///   Gets or sets the enabled state of the control
    /// </summary>
    public bool IsControlEnabled
    {
      get
      {
        if (this.GuiPanelControl != null)
        {
          return this.GuiPanelControl.AsFrameworkElement().IsEnabled;
        }
        return false;
      }
      set
      {
        if (this.GuiPanelControl != null)
        {
          this.GuiPanelControl.AsFrameworkElement().IsEnabled = value;
        }
      }
    }

    /// <summary>
    ///   Return the width of the longest child label
    /// </summary>
    public double LongestChildLabelWidth
    {
      get
      {
        double longest = 0;

        foreach (GuiPanelItem child in this.Children)
        {
          if (child.HasParameter)
          {
            double preWidth = child.LabelWidth;

            child.ResetLabelWidth();

            //
            // Record the longest label
            //
            if (child.LabelWidth > longest)
            {
              longest = child.LabelWidth;
            }

            child.LabelWidth = preWidth;
          }
        }

        return longest;
      }
    }

    /// <summary>
    ///   Returns true if the control is executing
    /// </summary>
    public bool IsExecuting
    {
      get
      {
        return (this.GuiPanelControl is IExecuting);
      }
    }

    /// <summary>
    ///   Returns true if this item can have its value got
    /// </summary>
    public bool IsValueControl
    {
      get
      {
        return (this.GuiPanelControl is IGuiPanelValueControl);
      }
    }

    /// <summary>
    ///   Gets or sets the value of the control if applicable
    /// </summary>
    public String Value
    {
      get
      {
        if (IsValueControl)
        {
          return ((IGuiPanelValueControl)this.GuiPanelControl).Value;
        }
        return String.Empty;
      }
      set
      {
        if (IsValueControl)
        {
          //
          // Publish the value and then update the text boxes accordingly
          //
          this.Dispatcher.Invoke(
                 DispatcherPriority.Normal,
                 new System.Action(
                   delegate()
                   {
                     ((IGuiPanelValueControl)this.GuiPanelControl).Value = value;
                   }
                   )
                   );
        }
      }
    }

    /// <summary>
    ///   Returns true if the control can input a value
    /// </summary>
    public bool CanInputValue
    {
      get
      {
        if (this.GuiPanelControl != null && this.GuiPanelControl is IGuiPanelValueControl )
        {
          return ((IGuiPanelValueControl) this.GuiPanelControl).CanInputValue;
        }
        return false;
      }
    }

    /// <summary>
    ///   Returns true if this item has to live within parental restrictions
    /// </summary>
    public bool IsBoundToParent
    {
      get
      {
        return HasParameter || IsExecuting;
      }
    }

    /// <summary>
    ///   Returns true if this item has a parameter associated with it
    /// </summary>
    public bool HasParameter
    {
      get { return m_parameter != null; }
    }

    /// <summary>
    ///   Gets or sets the parameter associated with this item
    /// </summary>
    public GuiParameter Parameter
    {
      get
      {
        return m_parameter;
      }
      set
      {
        m_parameter = value;

        SetupLabel();

        //
        // If the label has changed, tell the parent to sync if necessary
        //
        if (this.Parent != null)
        {
          this.Parent.SyncChildLabelWidths();
        }
      }
    }

    /// <summary>
    ///   Gets or sets the parameter ID associated with this item
    /// </summary>
    private UInt64 ParamID
    {
      get;
      set;
    }

    /// <summary>
    ///   Returns true if this item has a trigger
    /// </summary>
    public bool HasTrigger
    {
      get { return m_triggerItem != null; }
    }

    /// <summary>
    ///   Gets or sets the trigger associated with this item
    /// </summary>
    public GuiTriggerItem TriggerItem
    {
      get { return m_triggerItem; }
      set
      {
        if (m_triggerItem != null)
        {
          m_triggerItem.PropertyChanged -= TriggerItem_PropertyChanged;
        }

        m_triggerItem = value;

        m_triggerItem.PropertyChanged += TriggerItem_PropertyChanged;

        SetupLabel();
      }
    }

    /// <summary>
    ///   Handle trigger item changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TriggerItem_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      if (this.GuiPanelControl != null)
      {
        this.Value = TriggerItem.LatestValue;
      }
    }

    /// <summary>
    ///   Returns true if this item should have a label
    /// </summary>
    public bool HasLabel
    {
      get
      {
        return HasParameter || HasTrigger;
      }
    }

    /// <summary>
    ///   Returns true if this item has an action associated with it
    /// </summary>
    public bool HasAction
    {
      get { return m_action != null; }
    }

    /// <summary>
    ///   Gets or sets the action associated with this item (if any)
    /// </summary>
    public GuiAction Action
    {
      get
      {
        return m_action;
      }
      set
      {
        m_action = value;
      }
    }

    /// <summary>
    ///   Gets or sets the parent of this item
    /// </summary>
    public GuiPanelItem Parent
    {
      get { return m_parent; }
      set { m_parent = value; }
    }

    /// <summary>
    ///   Gets or sets the highlighted state of this
    /// </summary>
    public bool IsHighlighted
    {
      get { return m_isHighlighted; }
      set
      {
        if (m_isHighlighted != value)
        {
          m_isHighlighted = value;


          if (m_isHighlighted)
          {
            SetHighlightVisibility(true);
          }
          else
          {
            SetHighlightVisibility(false);

            foreach (GuiPanelItem child in this.Children)
            {
              child.IsHighlighted = false;
            }
          }
        }
      }
    }

    /// <summary>
    ///   Gets or sets the width of the control
    /// </summary>
    public double ControlWidth
    {
      get
      {
        if (this.GuiPanelControl != null)
        {
          return this.GuiPanelControl.AsFrameworkElement().Width;
        }
        return 0;
      }
      set
      {
        if (this.GuiPanelControl != null)
        {
          if (value >= MIN_WIDTH)
          {
            this.GuiPanelControl.AsFrameworkElement().Width = value;
          }
          else
          {
            this.GuiPanelControl.AsFrameworkElement().Width = MIN_WIDTH;
          }
        }
      }
    }

    /// <summary>
    ///   Gets or sets the height of the control
    /// </summary>
    public double ControlHeight
    {
      get
      {
        if (this.GuiPanelControl != null)
        {
          return this.GuiPanelControl.AsFrameworkElement().Height;
        }
        return 0;
      }
      set
      {
        if (this.GuiPanelControl != null)
        {
          if (value > MIN_HEIGHT)
          {
            this.GuiPanelControl.AsFrameworkElement().Height = value;
          }
          else
          {
            this.GuiPanelControl.AsFrameworkElement().Height = MIN_HEIGHT;
          }
        }
      }
    }

    /// <summary>
    ///   Get or set a flag stating that the item is currently moving
    /// </summary>
    public bool IsDragging
    {
      get { return m_isMoving; }
      set
      {
        if (m_isMoving != value)
        {
          m_isMoving = value;

          if (m_isMoving)
          {
            EnableDraggingVisuals();
          }
          else
          {
            DisableDraggingVisuals();
          }
        }
      }
    }

    /// <summary>
    ///   Returns true if the containing control is a container
    /// </summary>
    public bool IsContainer
    {
      get
      {
        if (this.GuiPanelControl != null && this.GuiPanelControl is IControlContainer)
        {
          return true;
        }
        return false;
      }
    }

    /// <summary>
    ///   Gets or sets the designer for this item
    /// </summary>
    private GuiPanelDesigner GuiPanelDesigner
    {
      get
      {
        if (m_guiPanelDesigner == null)
        {
          m_guiPanelDesigner =
            SyNet.GuiHelpers.Utilities.FindAncestor(typeof(GuiPanelDesigner), this)
            as GuiPanelDesigner;
        }
        return m_guiPanelDesigner;
      }
      set
      {
        m_guiPanelDesigner = value;
      }
    }

    /// <summary>
    ///   Gets or sets a flag determining if the item is being edited
    /// </summary>
    public bool IsEditing
    {
      get { return m_isEditing; }
      set
      {
        if (m_isEditing != value)
        {
          m_isEditing = value;

          if (this.IsEditing)
          {
            EnableEditing();
          }
          else
          {
            DisableEditing();
          }
        }

        //
        // Trickle the changes down to all children
        //
        foreach (GuiPanelItem child in Children)
        {
          child.IsEditing = this.IsEditing;
        }
      }
    }

    /// <summary>
    ///   Gets the AdornerLayer for this object
    /// </summary>
    public AdornerLayer AdornerLayer
    {
      get
      {
        if (m_adornerLayer == null)
        {
          m_adornerLayer = AdornerLayer.GetAdornerLayer(this);
        }
        return m_adornerLayer;
      }
      set
      {
        m_adornerLayer = value;

        //
        // Trickle this down to the children
        //
        foreach (GuiPanelItem child in this.Children)
        {
          child.AdornerLayer = value;
        }
      }
    }

    /// <summary>
    ///   Get or sets a flag selecting this item
    /// </summary>
    public bool IsSelected
    {
      get { return m_isSelected; }
      set
      {
        if (m_isSelected != value)
        {
          m_isSelected = value;
          if (m_isSelected)
          {
            SelectItem();
          }
          else
          {
            DeselectItem();
          }
        }
      }
    }

    /// <summary>
    ///   Gets or sets the top position
    /// </summary>
    public double PositionTop
    {
      get { return m_positionTop; }
      set
      {
        if (m_positionTop != value)
        {
          m_positionTop = value;
          OnPropertyChanged("PositionTop");
        }
      }
    }

    /// <summary>
    ///   Gets or sets the left position
    /// </summary>
    public double PositionLeft
    {
      get { return m_positionLeft; }
      set
      {
        if (m_positionLeft != value)
        {
          m_positionLeft = value;
          OnPropertyChanged("PositionLeft");
        }
      }
    }

    /// <summary>
    ///   Gets or sets the Z Index of the control
    /// </summary>
    public int PositionZIndex { get; set; }

    /// <summary>
    ///   Gets or sets the control associated with this item
    /// </summary>
    public IGuiPanelControl GuiPanelControl
    {
      get { return m_guiPanelControl; }
      set
      {
        if (m_guiPanelControl != value)
        {
          double controlWidth = GuiPanelControl != null ? ControlWidth : value.AsFrameworkElement().Width;
          double controlHeight = GuiPanelControl != null ? ControlHeight : value.AsFrameworkElement().Height;
          string preValue = this.Value;
          IGuiPanelControl preControl = m_guiPanelControl;

          // Remove any children from the old control
          foreach (GuiPanelItem guiPanelItem in Children)
          {
            ((IControlContainer)m_guiPanelControl).RemoveChildControl(guiPanelItem);
          }

        
          m_guiPanelControl = value;

          // Add children to new control
          foreach (GuiPanelItem guiPanelItem in Children)
          {
            ((IControlContainer)m_guiPanelControl).AddChildControl(guiPanelItem);
          }

          m_guiPanelControl.AsFrameworkElement().Width = controlWidth;
          m_guiPanelControl.AsFrameworkElement().Height = controlHeight;


          //
          // If there was an old control, use its value
          //
          if (preControl != null)
          {
            this.Value = preValue;
          }

          // Since we are the facade for this control, set it as our content
          if (this.HasLabel)
          {
            SetupLabel();

            if (this.Parent != null)
              this.Parent.SyncChildLabelWidths();
          }
          else
          {
            this.Content = m_guiPanelControl;
          }
        }
      }
    }

    /// <summary>
    ///   Get or set the list of children
    /// </summary>
    /// <remarks>
    ///   Do not use this list for adding children 
    /// </remarks>
    public List<GuiPanelItem> Children
    {
      get { return m_children; }
      set
      {
        m_children = value;

        SetupLabel();

        // clear from control
        ClearControlChildren();

        foreach (GuiPanelItem item in m_children)
        {
          AddChildControl(item);
        }

      }
    }

    /// <summary>
    ///   Returns the location of the container
    /// </summary>
    public Rect ContainerLocation
    {
      get
      {
        if (this.IsContainer)
        {
          return ((IControlContainer)this.GuiPanelControl).ContainerLocation;
        }
        return new Rect();
      }
    }

    /// <summary>
    ///   Returns any additional inside top margin
    /// </summary>
    public double MarginInsideTop
    {
      get
      {
        if (this.IsContainer)
        {
          return ((IControlContainer)this.GuiPanelControl).MarginInsideTop;
        }
        return 0;
      }
    }

    /// <summary>
    ///   Returns any additional inside bottom margin
    /// </summary>
    public double MarginInsideBottom
    {
      get
      {
        if (this.IsContainer)
        {
          return ((IControlContainer)this.GuiPanelControl).MarginInsideBottom;
        }
        return 0;
      }
    }

    /// <summary>
    ///   Returns any additional inside left margin
    /// </summary>
    public double MarginInsideLeft
    {
      get
      {
        if (this.IsContainer)
        {
          return ((IControlContainer)this.GuiPanelControl).MarginInsideLeft;
        }
        return 0;
      }
    }

    /// <summary>
    ///   Returns any addition inside right margin
    /// </summary>
    public double MarginInsideRight
    {
      get
      {
        if (this.IsContainer)
        {
          return ((IControlContainer)this.GuiPanelControl).MarginInsideRight;
        }
        return 0;
      }
    }

    #endregion

    /// <summary>
    ///   Return the control type
    /// </summary>
    public Type ControlType
    {
      get
      {
        if (this.GuiPanelControl != null)
        {
          return this.GuiPanelControl.GetType();
        }
        return null;
      }
    }

    /// <summary>
    ///   Handle label width syncing
    /// </summary>
    public void SyncChildLabelWidths()
    {
      if (LabelSizeSync)
      {
        double longestLabelWidth = LongestChildLabelWidth;
        ChildLabelWidth = longestLabelWidth;
      }
      else
      {
        foreach (GuiPanelItem child in this.Children)
        {
          child.ResetLabelWidth();
        }
      }
    }

    /// <summary>
    ///   Clear the label width and reset it to its measured value
    /// </summary>
    private void ResetLabelWidth()
    {
      if (m_parameterLabel != null)
      {
        m_parameterLabel.Width = Double.NaN;
        m_parameterLabel.Measure(new Size(double.MaxValue, double.MaxValue));
        m_parameterLabel.Width = m_parameterLabel.DesiredSize.Width;
      }
    }

    /// <summary>
    ///   Update the label spacing based on the spacing value and the location
    /// </summary>
    private void UpdateLabelMargin()
    {
      if (m_parameterLabel != null)
      {
        switch (LabelPosition)
        {
          case EsnLabelPosition.Top:
            m_parameterLabel.Margin = new Thickness(0, 0, 0, LabelSpacing);
            break;
          case EsnLabelPosition.Bottom:
            m_parameterLabel.Margin = new Thickness(0, LabelSpacing, 0, 0);
            break;
          case EsnLabelPosition.Left:
            m_parameterLabel.Margin = new Thickness(0, 0, LabelSpacing, 0);
            break;
          case EsnLabelPosition.Right:
            m_parameterLabel.Margin = new Thickness(LabelSpacing, 0, 0, 0);
            break;
        }
      }
    }

    /// <summary>
    ///   Sets up the visual contents of this to have a label if there is a
    ///   parameter associated with the item
    /// </summary>
    private void SetupLabel()
    {
      BuildLabelText();

      //
      // Make sure we have a control and a parameter
      //
      if (this.GuiPanelControl != null)
      {
        // Remove from possible parent
        RemoveFromOldParent(this.GuiPanelControl);

        //
        // If we have a parameter or a trigger, create the stackpanel and label
        //
        if (this.HasLabel && IsLabelVisible)
        {
          this.Content = null;

          // Create the stackpanel
          m_parameterStackPanel = new StackPanel();

          // Create the label
          m_parameterLabel = new Label();

          m_parameterLabel.HorizontalAlignment = HorizontalAlignment.Left;

          // Set the label text
          m_parameterLabel.Content = LabelText;

          // Set the label spacing
          UpdateLabelMargin();

          // Add them
          switch (LabelPosition)
          {
            case EsnLabelPosition.Top:
              m_parameterStackPanel.Children.Add(m_parameterLabel);
              m_parameterStackPanel.Children.Add(this.GuiPanelControl.AsUIElement());

              m_parameterStackPanel.Orientation = Orientation.Vertical;
              break;
            case EsnLabelPosition.Bottom:
              m_parameterStackPanel.Children.Add(this.GuiPanelControl.AsUIElement());
              m_parameterStackPanel.Children.Add(m_parameterLabel);

              m_parameterStackPanel.Orientation = Orientation.Vertical;
              break;
            case EsnLabelPosition.Left:
              m_parameterStackPanel.Children.Add(m_parameterLabel);
              m_parameterStackPanel.Children.Add(this.GuiPanelControl.AsUIElement());

              m_parameterStackPanel.Orientation = Orientation.Horizontal;
              break;
            case EsnLabelPosition.Right:
              m_parameterStackPanel.Children.Add(this.GuiPanelControl.AsUIElement());
              m_parameterStackPanel.Children.Add(m_parameterLabel);

              m_parameterStackPanel.Orientation = Orientation.Horizontal;
              break;
          }

          this.Content = m_parameterStackPanel;
        }
        else
        {
          this.Content = this.GuiPanelControl;

          // Nullify the unused components
          m_parameterStackPanel = null;
          m_parameterLabel = null;
        }
      }
    }

    /// <summary>
    ///   Get the correct label text
    /// </summary>
    /// <returns></returns>
    private void BuildLabelText()
    {
      if (String.IsNullOrEmpty(LabelText))
      {
        if (this.HasParameter)
          LabelText = this.Parameter.FullName;
        if (this.HasTrigger)
          LabelText = this.TriggerItem.Name;
      }
    }

    private void RemoveFromOldParent(IGuiPanelControl p_control)
    {
      DependencyObject parent = LogicalTreeHelper.GetParent(p_control.AsUIElement());
      if (parent != null && parent is StackPanel)
      {
        ((StackPanel)parent).Children.Remove(this.GuiPanelControl.AsUIElement());
      }
    }

    /// <summary>
    ///   Set visuals present when the item is being moved
    /// </summary>
    private void EnableDraggingVisuals()
    {
      SetSelectVisibility(false);
      SetResizeVisibility(false);
    }

    /// <summary>
    ///   Add the highlight adorner
    /// </summary>
    private void AddHighlightAdorner()
    {
      //
      // Add the highlight adorner
      //
      m_highlightAdorner = new HighlightAdorner(this);
      SetHighlightVisibility(false);
      this.AdornerLayer.Add(m_highlightAdorner);
    }

    /// <summary>
    ///   Remove the highlight adorner
    /// </summary>
    private void RemoveHighlightAdorner()
    {
      this.AdornerLayer.Remove(m_highlightAdorner);
      m_highlightAdorner = null;
    }

    /// <summary>
    ///   Remove visuals present when item is being moved
    /// </summary>
    private void DisableDraggingVisuals()
    {
      SetSelectVisibility(true);
      SetResizeVisibility(true);
    }

    /// <summary>
    ///   Take the item out of editing mode
    /// </summary>
    private void DisableEditing()
    {
      RemoveResizeAdorner();
      RemoveMoveAdorner();
      RemoveSelectAdorner();
      RemoveHighlightAdorner();
    }

    /// <summary>
    ///   Set the item in editing mode
    /// </summary>
    private void EnableEditing()
    {
      AddSelectAdorner();
      AddMoveAdorner();
      AddResizeAdorner();
      AddHighlightAdorner();

      //
      // If it's not a container, add the move adorner
      //
      if (!IsContainer)
      {
        SetMoveVisibility(true);
      }
    }

    /// <summary>
    ///   Set the visilibty of the select adorner
    /// </summary>
    /// <param name="p_visibility"></param>
    private void SetSelectVisibility( bool p_visibility )
    {
      if (m_selectAdorner != null)
      {
        m_selectAdorner.Visibility = p_visibility ? Visibility.Visible : Visibility.Hidden;
      }
    }

    /// <summary>
    ///   Set the visilibty of the resize adorner
    /// </summary>
    /// <param name="p_visibility"></param>
    private void SetResizeVisibility( bool p_visibility )
    {
      if (m_resizeAdorner != null)
      {
        m_resizeAdorner.Visibility = p_visibility ? Visibility.Visible : Visibility.Hidden;
      }
    }

    /// <summary>
    ///   Set the visilibty of the move adorner
    /// </summary>
    /// <param name="p_visibility"></param>
    private void SetMoveVisibility( bool p_visibility )
    {
      if (m_moveAdorner != null)
      {
        m_moveAdorner.Visibility = p_visibility ? Visibility.Visible : Visibility.Hidden;
      }
    }

    /// <summary>
    ///   Set the visilibty of the highlight adorner
    /// </summary>
    /// <param name="p_visibility"></param>
    private void SetHighlightVisibility( bool p_visibility )
    {
      if (m_highlightAdorner != null)
      {
        m_highlightAdorner.Visibility = p_visibility ? Visibility.Visible : Visibility.Hidden;
      }
    }

    /// <summary>
    ///   Add the move adorner
    /// </summary>
    private void AddMoveAdorner()
    {
      //
      // Add the move adorner in editing mode
      //
      m_moveAdorner = new MoveAdorner(this);
      SetMoveVisibility(false);
      this.AdornerLayer.Add(m_moveAdorner);
    }

    private void RemoveMoveAdorner()
    {
      this.AdornerLayer.Remove(m_moveAdorner);
      m_moveAdorner = null;
    }

    /// <summary>
    ///   Add the resize adorner
    /// </summary>
    private void AddResizeAdorner()
    {
      if (this.AdornerLayer != null)
      {
        m_resizeAdorner = new ResizeAdorner(this);
        SetResizeVisibility(false);
        this.AdornerLayer.Add(m_resizeAdorner);
      }
    }

    /// <summary>
    ///   Remove the resize adorner
    /// </summary>
    private void RemoveResizeAdorner()
    {
      if (this.AdornerLayer != null)
      {
        this.AdornerLayer.Remove(m_resizeAdorner);
        m_resizeAdorner = null;
      }
    }

    /// <summary>
    ///   Add the select adorner
    /// </summary>
    private void AddSelectAdorner()
    {
      if (this.AdornerLayer != null)
      {
        m_selectAdorner = new SelectAdorner(this);
        SetSelectVisibility(false);
        this.AdornerLayer.Add(m_selectAdorner);
      }
    }

    /// <summary>
    ///   Remove the select adorner
    /// </summary>
    private void RemoveSelectAdorner()
    {
      if (this.AdornerLayer != null)
      {
        this.AdornerLayer.Remove(m_selectAdorner);
        m_selectAdorner = null;
      }
    }

    /// <summary>
    ///   Remove the selected adorner from the item
    /// </summary>
    private void DeselectItem()
    {
      SetSelectVisibility(false);
      SetResizeVisibility(false);

      //
      // If it's a container, remove the move adorner
      //
      if (this.IsContainer)
      {
        SetMoveVisibility(false);
      }
    }

    /// <summary>
    ///   Add the selected adorner to the item
    /// </summary>
    private void SelectItem()
    {
      SetSelectVisibility(true);
      SetResizeVisibility(true);

      //
      // If it's a container, add the move adorner
      //
      if (this.IsContainer)
      {
        SetMoveVisibility(true);
      }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public GuiPanelItem()
    {
      BindPosition();
      this.Loaded += new RoutedEventHandler(GuiPanelItem_Loaded);
    }

    /// <summary>
    ///   Bind any properties to the frameworkelement properties
    /// </summary>
    private void BindPosition()
    {
      Binding binding;

      // Position Top
      binding = new Binding();
      binding.Source = this;
      binding.Path = new PropertyPath("PositionTop");
      binding.Mode = BindingMode.TwoWay;
      this.SetBinding(Canvas.TopProperty, binding);

      // Position Left
      binding = new Binding();
      binding.Source = this;
      binding.Path = new PropertyPath("PositionLeft");
      binding.Mode = BindingMode.TwoWay;
      this.SetBinding(Canvas.LeftProperty, binding);
    }

    /// <summary>
    ///   Initialization constructor
    /// </summary>
    /// <param name="p_designer"></param>
    /// <param name="p_control"></param>
    public GuiPanelItem( IGuiPanelControl p_control, Point p_point )
      : this()
    {
      GuiPanelControl = p_control;
      PositionTop = p_point.Y;
      PositionLeft = p_point.X;
      this.Loaded += new RoutedEventHandler(GuiPanelItem_Loaded);

    }

    /// <summary>
    ///   Loaded event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void GuiPanelItem_Loaded( object sender, RoutedEventArgs e )
    {
      if (this.GuiPanelDesigner != null)
      {
        this.IsEditing = this.GuiPanelDesigner.IsEditing;
      }
    }

    /// <summary>
    ///   Initialization constructor
    /// </summary>
    /// <param name="p_designer"></param>
    /// <param name="p_control"></param>
    public GuiPanelItem( IGuiPanelControl p_control )
    {
      GuiPanelControl = p_control;
      PositionZIndex = 0;
      this.Loaded += GuiPanelItem_Loaded;

    }

    /// <summary>
    ///   Clear the child controls from the GuiPanelControl
    /// </summary>
    private void ClearControlChildren()
    {
      if (this.GuiPanelControl != null && this.GuiPanelControl is IControlContainer)
      {
        foreach (IGuiPanelControl child in ((IControlContainer)this.GuiPanelControl).Children)
        {
          ((IControlContainer)this.GuiPanelControl).RemoveChildControl(child);

        }
      }
    }

    /// <summary>
    ///   Add a child item
    /// </summary>
    /// <param name="p_item"></param>
    /// <returns></returns>
    public bool AddChild( GuiPanelItem p_item )
    {
      bool bRetVal = false;

      m_children.Add(p_item);

      p_item.Parent = this;

      bRetVal = AddChildControl(p_item);

      //
      // If it is an executing function, listen to it
      //
      if (bRetVal)
      {
        if (p_item.IsExecuting)
        {
          ((IExecuting)p_item.GuiPanelControl).Execute += GuiPanelItem_Execute;
        }
      }

      return bRetVal;
    }

    /// <summary>
    ///   Event handler for an execution
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GuiPanelItem_Execute( object sender, RoutedEventArgs e )
    {
      //
      // We'll start by assuming that there is an item for each parameter
      //
      foreach (GuiPanelItem child in this.Children)
      {
        //
        // Only act on items with parameters
        //
        if (child.HasParameter)
        {
          //
          // Take the value out of the control and put it in the parameter
          //
          child.Parameter.StringValue = child.Value;
        }
      }

      //
      // Execute the action
      //
      ActionManager.Instance.ExecuteThreadedAction(this.Action.ActionID);
    }

    /// <summary>
    ///   Add a child item to the control
    /// </summary>
    /// <param name="p_item"></param>
    /// <returns></returns>
    private bool AddChildControl( GuiPanelItem p_item )
    {
      bool bRetVal = false;
      if (this.GuiPanelControl != null && this.GuiPanelControl is IControlContainer)
      {
        bRetVal = ((IControlContainer)this.GuiPanelControl).AddChildControl(p_item);
      }
      return bRetVal;
    }

    /// <summary>
    ///   Remove a child item
    /// </summary>
    /// <param name="p_item"></param>
    /// <returns></returns>
    public bool RemoveChild( GuiPanelItem p_item )
    {
      if (this.Children.Contains(p_item))
      {
        m_children.Remove(p_item);
        p_item.Parent = null;
      }
      else
      {
        return false;
      }

      if (this.GuiPanelControl != null && this.GuiPanelControl is IControlContainer)
      {
        return ((IControlContainer)this.GuiPanelControl).RemoveChildControl(p_item);
      }
      return false;
    }

    /// <summary>
    ///   Get the gui panel control to a certain type
    /// </summary>
    /// <param name="p_type"></param>
    public void SetGuiPanelControl(Type p_type)
    {
      //
      // Create the object
      //
      object controlObj = Activator.CreateInstance(p_type);

      //
      // Make sure this object is an IGuiPanelControl
      //
      if (controlObj is IGuiPanelControl)
      {
        GuiPanelControl = controlObj as IGuiPanelControl;
      }
    }

    ////////////////////////////////////////////////////////////////////////////
    // IXmlSerialzable
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IXmlSerializable

    /// <summary>
    /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
    /// </returns>
    public XmlSchema GetSchema()
    {
      return null;
    }

    /// <summary>
    /// Generates an object from its XML representation.
    /// </summary>
    /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. 
    ///                 </param>
    public void ReadXml( XmlReader p_reader )
    {
      double? width = null;
      double? height = null;

      XElement xElement = XElement.ReadFrom(p_reader) as XElement;

      if (xElement != null)
      {
        Load(xElement);
      }
    }

    /// <summary>
    ///   Handle any deserialization cleanup
    /// </summary>
    private void Instance_DeserializingFinished()
    {
      //
      // Grab the parameter if we have an ID
      //
      if (ParamID != 0)
      {
        this.Parameter = ActionManager.Instance.GetParameter(ParamID) as GuiParameter;
      }

      //
      // Refresh the label if we have a trigger
      //
      if (HasTrigger)
      {
        SetupLabel();

        //
        // Tell the trigger item to start listening to the trigger
        //
        TriggerItem.RegisterWithTrigger();
      }

      SyncChildLabelWidths();
    }


    /// <summary>
    ///   Returns the controls configuration panel
    /// </summary>
    public UserControl ConigurationPanel
    {
      get
      {
        if (this.GuiPanelControl != null)
        {
          return this.GuiPanelControl.ConigurationPanel;
        }
        return null;
      }
    }

    /// <summary>
    ///   Load the element from an XElement
    /// </summary>
    /// <param name="p_element"></param>
    public bool Load( XElement p_element )
    {


      double? width = null;
      double? height = null;

      XElement rootElement = p_element;

      if (rootElement.Name == STR_XML_GUIPANELITEM)
      {
        rootElement = p_element.Element(STR_XML_ROOT);
      }

      if (rootElement == null) return false;

      //
      // Read in any attributes
      //
      if (rootElement.HasAttributes)
      {
        foreach (XAttribute attribute in rootElement.Attributes())
        {
          switch (attribute.Name.ToString())
          {
            case STR_XML_POSITIONLEFT:
              PositionLeft = Double.Parse(attribute.Value);
              break;
            case STR_XML_POSITIONTOP:
              PositionTop = Double.Parse(attribute.Value);
              break;
            case STR_XML_POSITIONZINDEX:
              PositionZIndex = Int32.Parse(attribute.Value);
              break;
            case STR_XML_WIDTH:
              width = Double.Parse(attribute.Value);
              break;
            case STR_XML_HEIGHT:
              height = Double.Parse(attribute.Value);
              break;
            case STR_XML_PARAMETER:
              ParamID = UInt64.Parse(attribute.Value);
              break;
            case STR_XML_ISLABELVISIBLE:
              IsLabelVisible = bool.Parse(attribute.Value);
              break;
            case STR_XML_LABELTEXT:
              LabelText = attribute.Value;
              break;
            case STR_XML_LABELSPACING:
              LabelSpacing = double.Parse(attribute.Value);
              break;
            case STR_XML_LABELPOSITION:
              LabelPosition = (EsnLabelPosition)Enum.Parse(typeof(EsnLabelPosition), attribute.Value);
              break;
            case STR_XML_LABELSIZESYNC:
              LabelSizeSync = bool.Parse(attribute.Value);
              break;
          }
        }
      }

      //
      // Read in any elements
      //
      if (rootElement.HasElements)
      {
        foreach (XElement element in rootElement.Elements())
        {
          switch (element.Name.ToString())
          {
            case STR_XML_GUIITEMCONTROL:
              {
                bool validControl = ReadControl(element);
                //
                // If the control was created, set the dimensions
                //
                if (validControl)
                {
                  if (width != null)
                  {
                    ControlWidth = (double)width;
                  }

                  if (height != null)
                  {
                    ControlHeight = (double)height;
                  }
                }
              }
              break;
            case STR_XML_GUIPANELCHILDREN:
              {
                ReadChildren(element);
              }
              break;

            case STR_XML_ACTION:
              ReadAction(element);
              break;
            case GuiTriggerItem.STR_XML_ROOT:
              ReadTrigger(element);
              break;
          }
        }
      }

      //
      // Handle cleanup
      //
      SyNetSettings.Instance.DeserializingFinished += Instance_DeserializingFinished;

      return true;
    }

    /// <summary>
    ///   Read the trigger from an xElement
    /// </summary>
    /// <param name="p_element"></param>
    private void ReadTrigger( XElement p_element )
    {
      TriggerItem = new GuiTriggerItem();
      TriggerItem.Load(p_element);
    }


    /// <summary>
    ///   Read an action from an XElement
    /// </summary>
    /// <param name="p_element"></param>
    private void ReadAction( XElement p_element )
    {
      XmlSerializer serializer = new XmlSerializer(typeof(GuiAction));
      String str = p_element.ToString();
      StringReader stringReader = new StringReader(p_element.ToString());
      XmlReader xmlReader = XmlReader.Create(stringReader);
      this.Action = serializer.Deserialize(xmlReader) as GuiAction;
    }

    /// <summary>
    ///   Save the control settings to an XElement
    /// </summary>
    /// <returns></returns>
    public XElement Save()
    {
      XElement xElement = new XElement(STR_XML_ROOT);

      //
      // Position
      //
      xElement.Add(new XAttribute(STR_XML_POSITIONTOP, PositionTop.ToString()));
      xElement.Add(new XAttribute(STR_XML_POSITIONLEFT, PositionLeft.ToString()));
      xElement.Add(new XAttribute(STR_XML_POSITIONZINDEX, PositionZIndex.ToString()));

      //
      // Size
      //
      xElement.Add(new XAttribute(STR_XML_WIDTH, this.GuiPanelControl.AsFrameworkElement().ActualWidth.ToString()));
      xElement.Add(new XAttribute(STR_XML_HEIGHT, this.GuiPanelControl.AsFrameworkElement().ActualHeight.ToString()));

      //
      // Label
      //
      if (HasLabel)
      {
        xElement.Add(new XAttribute(STR_XML_ISLABELVISIBLE, this.IsLabelVisible.ToString()));
        xElement.Add(new XAttribute(STR_XML_LABELTEXT, this.LabelText));
        xElement.Add(new XAttribute(STR_XML_LABELSPACING, this.LabelSpacing.ToString()));
        xElement.Add(new XAttribute(STR_XML_LABELPOSITION, this.LabelPosition.ToString()));
      }
      if (HasAction)
      {
        xElement.Add(new XAttribute(STR_XML_LABELSIZESYNC, this.LabelSizeSync.ToString()));
      }

      //
      // Control
      //
      XElement xControl = new XElement(STR_XML_GUIITEMCONTROL);
      xControl.Add(new XAttribute(STR_XML_CLASS, this.GuiPanelControl.GetType().ToString()));
      xControl.Add(new XAttribute(STR_XML_ISCONTROLENABLED, this.IsControlEnabled));
      xControl.Add(this.GuiPanelControl.Save());
      xElement.Add(xControl);

      //
      // Action
      //
      if (m_action != null)
      {
        XmlSerializer serializer = new XmlSerializer(m_action.GetType());
        StringBuilder actionStringBuilder = new StringBuilder();
        XmlWriter writer = XmlWriter.Create(actionStringBuilder);
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        serializer.Serialize(writer, m_action, ns);
        XElement xAction = XElement.Parse(actionStringBuilder.ToString());

        xElement.Add(xAction);
      }

      //
      // Parameter
      //
      if (m_parameter != null)
      {
        xElement.Add(new XAttribute(STR_XML_PARAMETER, this.Parameter.ParamID));
      }

      //
      // Trigger
      //
      if (m_triggerItem != null)
      {
        xElement.Add(m_triggerItem.Save());
      }

      //
      // Children
      //
      if (Children.Count > 0)
      {
        XElement xChildren = new XElement(STR_XML_GUIPANELCHILDREN);

        foreach (GuiPanelItem child in this.Children)
        {
          xChildren.Add(child.Save());
        }
        xElement.Add(xChildren);
      }

      return xElement;
    }

    /// <summary>
    ///   Read in the children
    /// </summary>
    /// <param name="p_element"></param>
    private void ReadChildren( XElement p_element )
    {
      if (p_element.HasElements)
      {
        foreach (XElement element in p_element.Elements())
        {
          if (element.Name == STR_XML_GUIPANELITEM || element.Name == STR_XML_ROOT)
          {
            GuiPanelItem item = new GuiPanelItem();
            item.Load(element);

            if (item != null)
            {
              AddChild(item);
            }
          }
        }
      }
    }

    /// <summary>
    ///   Read the control from an XElement
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private bool ReadControl( XElement element )
    {
      XAttribute xClass = element.Attribute(STR_XML_CLASS);
      if (xClass != null)
      {
        Type controlClassType = Type.GetType(xClass.Value.ToString());

        //
        // Now create the control class based on its type
        //
        if (controlClassType != null)
        {
          //
          // Create the object
          //
          object controlObj = Activator.CreateInstance(controlClassType);

          //
          // Make sure this object is an IGuiPanelControl
          //
          if (controlObj is IGuiPanelControl)
          {
            GuiPanelControl = controlObj as IGuiPanelControl;
            if (element.HasElements)
            {
              GuiPanelControl.Load(element.Elements().First());
            }

            //
            // Load control enabled
            //
            XAttribute xEnabled = element.Attribute(STR_XML_ISCONTROLENABLED);
            if (xEnabled != null)
            {
              this.IsControlEnabled = Boolean.Parse(xEnabled.Value);
            }

            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Converts an object into its XML representation.
    /// </summary>
    /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. 
    ///                 </param>
    public void WriteXml( XmlWriter writer )
    {
      XElement element = this.Save();

      element.WriteTo(writer);
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // IGuiPanelControl
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IGuiPanelControl

    /// <summary>
    ///   Return the control name
    /// </summary>
    public string ControlImage
    {
      get
      {
        if (this.GuiPanelControl != null)
        {
          return this.GuiPanelControl.ControlImage;
        }
        return string.Empty;
      }
    }

    /// <summary>
    ///   Return the control name
    /// </summary>
    public string ControlName
    {
      get
      {
        if (this.GuiPanelControl != null)
        {
          return this.GuiPanelControl.ControlName;
        }
        return string.Empty;
      }
    }

    /// <summary>
    ///   Return the GuiPanelItem as a UIElement
    /// </summary>
    /// <returns></returns>
    public UIElement AsUIElement()
    {
      return this as UIElement;
    }

    /// <summary>
    ///   Return the GuiPanelControl as a FrameworkElement
    /// </summary>
    /// <returns></returns>
    public FrameworkElement AsFrameworkElement()
    {
      return this as FrameworkElement;
    }


    /// <summary>
    ///   Returns true if the width of the container can be resized
    /// </summary>
    public bool CanResizeWidth
    {
      get
      {
        return this.GuiPanelControl.CanResizeWidth;
      }
    }

    /// <summary>
    ///   Returns true if the height of the container can be resized
    /// </summary>
    public bool CanResizeHeight
    {
      get
      {
        return this.GuiPanelControl.CanResizeHeight;
      }
    }

    /// <summary>
    ///   Returns true if this item isn't affiliated with any logical function
    /// </summary>
    public bool IsUnaffiliated
    {
      get
      {
        return !HasAction && !HasParameter;
      }
    }

    /// <summary>
    ///   Returns true if this is a container and allows controls to be dropped on it
    /// </summary>
    public bool AllowControlDrop
    {
      get
      {
        return this.IsContainer && !(HasAction);
      }
    }

    #endregion

    /// <summary>
    ///   Gets the context menu
    /// </summary>
    //public override ContextMenu SelectedContextMenu
    //{
    //  get
    //  {
    //    ContextMenu menu = new ContextMenu();
    //    MenuItem item;

    //    AddMenuItem(menu, "Bring To Front", GuiControls.DesignerCanvas.BringToFront, "MoveToFrontSmall");
    //    AddMenuItem(menu, "Bring Forward", GuiControls.DesignerCanvas.BringForward, "MoveForwardSmall");
    //    AddMenuItem(menu, "Send To Back", GuiControls.DesignerCanvas.SendToBack, "MoveToBackSmall");
    //    AddMenuItem(menu, "Send Backward", GuiControls.DesignerCanvas.SendBackward, "MoveBackwardSmall");

    //    return menu;
    //  }
    //}

    private void AddMenuItem( ContextMenu p_parentMenu, string p_header,
      ICommand p_command, string p_iconPath )
    {
      MenuItem item = new MenuItem();
      item.Header = p_header;
      item.Command = p_command;
      //item.CommandTarget = this.DesignerCanvas;
      //Image icon = new Image();
      //icon.Source = new BitmapImage(
      //  new Uri(String.Format("pack://application:,,,/Resources/Designer/{0}.png", p_iconPath)));
      //item.Icon = icon;

      p_parentMenu.Items.Add(item);
    }

    ////////////////////////////////////////////////////////////////////////////
    // INotifyPropertyChanged
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Fires events for property changed
    /// </summary>
    /// <param name="p_strPropertyName"></param>
    protected void OnPropertyChanged( string p_strPropertyName )
    {
      if (PropertyChanged != null)
        PropertyChanged(
          this,
          new PropertyChangedEventArgs(p_strPropertyName));
    }

    #endregion
  }
}
