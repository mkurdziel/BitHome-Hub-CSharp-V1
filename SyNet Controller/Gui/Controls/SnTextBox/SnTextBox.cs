using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using SyNet.Gui.Interfaces;
using Action = SyNet.Actions.Action;
using Trigger = SyNet.Events.Triggers.Trigger;

namespace SyNet.Gui.Controls.SnTextBox
{
  /// <summary>
  ///  Textbox control
  /// </summary>
  public class SnTextBox : TextBox,
                           IGuiPanelValueControl
  {

    public const string STR_XML_ROOT = "SnTextBox";
    public const string STR_XML_TEXT = "Text";

    /// <summary>
    ///   Enumeration descripting the exection action of this control
    /// </summary>
    public enum EsnExecutionType
    {
      None,
      TextChanged,
      Return
    }

    private EsnExecutionType m_executionType = EsnExecutionType.None;

    /// <summary>
    ///   Gets or sets the execution action
    /// </summary>
    public EsnExecutionType ExecutionType
    {
      get { return m_executionType; }
      set { m_executionType = value; }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public SnTextBox()
    {
      this.TextChanged += SnTextBox_TextChanged;
      this.KeyDown += SnTextBox_KeyDown;
    }

    /// <summary>
    ///   Key down event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SnTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (ExecutionType == EsnExecutionType.Return && (e.Key == Key.Return ||
        e.Key == Key.Enter))
      {
        OnExecute();
      }
    }

    /// <summary>
    ///   Text changed handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SnTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (ExecutionType == EsnExecutionType.TextChanged)
      {
        OnExecute();
      }
    }

    /// <summary>
    ///   Fire execute event
    /// </summary>
    private void OnExecute()
    {
      if (Execute != null)
      {
        Execute(this, new RoutedEventArgs());
      }
    }

    ////////////////////////////////////////////////////////////////////////////
    // IExecuting
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IExecuting

    public event RoutedEventHandler Execute;

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // IActionable
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IActionable

    /// <summary>
    ///   Gets or sets the action associated with this control
    /// </summary>
    public Action Action { get; set; }

    /// <summary>
    ///   Gets or sets a flag connecting this control to its parent's action
    /// </summary>
    public bool ConnectToParent { get; set; }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // ITriggerable
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of ITriggerable

    /// <summary>
    ///   Gets or sets the trigger associated with this control
    /// </summary>
    public Trigger Trigger { get; set; }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // IGuiPanelControl
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IGuiPanelControl

    /// <summary>
    ///   Returns the control name
    /// </summary>
    public string ControlName
    {
      get { return "Text Box"; }
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
      get { return true; }
    }

    /// <summary>
    ///   Returns true if the height of the container can be resized
    /// </summary>
    public bool CanResizeHeight
    {
      get { return false; }
    }

    /// <summary>
    ///   Gets the configuration panel
    /// </summary>
    public UserControl ConigurationPanel
    {
      get { return null; }
    }

    /// <summary>
    ///   Returns a path to the control image
    /// </summary>
    public string ControlImage
    {
      get { return "pack://application:,,,/Gui/Controls/SnTextBox/SnTextBox.png"; }
    }

    /// <summary>
    ///   Load the settings from an XElement
    /// </summary>
    /// <param name="p_element"></param>
    public bool Load(XElement p_element)
    {
      XAttribute text = p_element.Attribute(STR_XML_TEXT);
      if (text != null)
      {
        this.Text = text.Value;
      }

      return false;
    }

    /// <summary>
    ///   Save the control settings to an XElement
    /// </summary>
    /// <returns></returns>
    public XElement Save()
    {
      XElement element = new XElement(STR_XML_ROOT);

      element.Add(new XAttribute(STR_XML_TEXT, this.Text));

      return element;
    }

    /// <summary>
    ///   Returns an array of rectangles to snap to
    /// </summary>
    public Rect[] AlignmentRegtangles
    {
      get
      {
        Rect[] alignRects = new Rect[1];
        alignRects[0] = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
        return alignRects;
      }
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // IGuiPanelValueControl
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IGuiPanelValueControl

    /// <summary>
    ///   Returns true if a value can be got
    /// </summary>
    public bool CanGetValue
    {
      get { return true; }
    }

    /// <summary>
    ///   Returns true if a value can get set
    /// </summary>
    public bool CanSetValue
    {
      get { return true; }
    }

    /// <summary>
    ///   Gets or sets the value of this control
    /// </summary>
    public string Value
    {
      get
      {
        return this.Text;
      }
      set
      {
        this.Text = value;
      }
    }

    /// <summary>
    ///   Returns true
    /// </summary>
    public bool CanInputValue
    {
      get { return true; }
    }

    #endregion
  }
}
