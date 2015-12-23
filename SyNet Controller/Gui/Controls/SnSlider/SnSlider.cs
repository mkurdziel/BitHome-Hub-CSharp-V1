using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using SyNet.Gui.Interfaces;
using Action = SyNet.Actions.Action;
using Trigger = SyNet.Events.Triggers.Trigger;

namespace SyNet.Gui.Controls.SnSlider
{
  /// <summary>
  ///  Textbox control
  /// </summary>
  public class SnSlider : Slider,
                           IGuiPanelValueControl
  {

    public const string STR_XML_ROOT = "SnSlider";
    public const string STR_XML_VALUE = "Value";

    /// <summary>
    ///   Default constructor
    /// </summary>
    public SnSlider()
    {
      this.ValueChanged += SnSlider_ValueChanged;
    }

    /// <summary>
    ///   Slider changed event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      OnExecute();
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
    // IGuiPanelControl
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IGuiPanelControl

    /// <summary>
    ///   Returns a path to the control image
    /// </summary>
    public string ControlImage
    {
      get { return "pack://application:,,,/Gui/Controls/SnSlider/SnSlider.png"; }
    }

    /// <summary>
    ///   Returns the control name
    /// </summary>
    public string ControlName
    {
      get { return "Slider"; }
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
    ///   Load the settings from an XElement
    /// </summary>
    /// <param name="p_element"></param>
    public bool Load(XElement p_element)
    {
      XAttribute value = p_element.Attribute(STR_XML_VALUE);
      if (value != null)
      {
        ((Slider) this).Value = double.Parse(value.Value);
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

      element.Add(new XAttribute(STR_XML_VALUE, ((Slider)this).Value));

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
        return ((Slider)this).Value.ToString();
      }
      set
      {
        double sliderValue = 0;
        double.TryParse(value, out sliderValue);
        ((Slider) this).Value = sliderValue;
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
