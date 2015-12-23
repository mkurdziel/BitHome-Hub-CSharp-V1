using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using SyNet.Gui.Interfaces;

namespace SyNet.Gui.Controls.SnButton
{
  /// <summary>
  ///   GUI Button class
  /// </summary>
  public class SnButton : Button, IGuiPanelValueControl, IExecuting
  {
    public const string STR_XML_ROOT = "SnButton";
    public const string STR_XML_CONTENT = "Content";

    /// <summary>
    ///   Constructor
    /// </summary>
    public SnButton()
    {
      this.Click += SnButton_Click;
    }

    /// <summary>
    ///   Click event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SnButton_Click(object sender, RoutedEventArgs e)
    {
      if (Execute != null)
      {
        Execute(this, new RoutedEventArgs());
      }
    }

    ////////////////////////////////////////////////////////////////////////////
    // IGuiPanelControl
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IGuiPanelControl

    /// <summary>
    ///   Returns a path to the control image
    /// </summary>
    public string ControlImage
    {
      get { return "pack://application:,,,/Gui/Controls/SnButton/SnButton.png"; }
    }


    /// <summary>
    ///   Returns the control name
    /// </summary>
    public string ControlName
    {
      get { return "Button"; }
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
      get { return true; }
    }

    /// <summary>
    ///   Load the settings from an XElement
    /// </summary>
    /// <param name="p_element"></param>
    public bool Load(XElement p_element)
    {
      XAttribute xContent = p_element.Attribute(STR_XML_CONTENT);
        if (xContent != null)
        {
          this.Content = xContent.Value;
        }
        return true;
    }

    /// <summary>
    ///   Save the control settings to an XElement
    /// </summary>
    /// <returns></returns>
    public XElement Save()
    {
      XElement element = new XElement(STR_XML_ROOT);

      element.Add(new XAttribute(STR_XML_CONTENT, this.Content.ToString()));

      return element;
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
    // IExecuting
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IExecuting

    public event RoutedEventHandler Execute;

    #endregion

    #region Implementation of IGuiPanelValueControl

    /// <summary>
    ///   Gets or sets the value of this control
    /// </summary>
    public string Value
    {
      get { return this.Content.ToString(); }
      set { this.Content = value;}
    }

    /// <summary>
    ///   Returns false
    /// </summary>
    public bool CanInputValue
    {
      get { return false;}
    }

    #endregion

    /// <summary>
    ///   Gets the configuration panel
    /// </summary>
    public UserControl ConigurationPanel
    {
      get
      {
        return new SnButtonConfig(this);
      }
    }
  }
}
