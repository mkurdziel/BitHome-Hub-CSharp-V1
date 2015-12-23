using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using SyNet.Gui.Interfaces;

namespace SyNet.Gui.Controls.SnGroupBox
{
  public class SnGroupBox : GroupBox, IControlContainer, IGuiPanelValueControl
  {
    public const string STR_XML_ROOT = "SnGroupBox";
    public const string STR_XML_HEADER = "Header";

    private string m_name = "GroupBox";

    private Canvas m_canvas = new Canvas();

    public string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public SnGroupBox()
    {
      this.Width = 100;
      this.Height = 100;
      this.Header = "GroupBox";
      this.Content = m_canvas;
      this.Background = new SolidColorBrush(Colors.White);
      this.ClipToBounds = true;
    }

    #region Implementation of IGuiPanelControl

    /// <summary>
    ///   Returns a path to the control image
    /// </summary>
    public string ControlImage
    {
      get { return "pack://application:,,,/Gui/Controls/SnGroupBox/SnGroupBox.png"; }
    }

    /// <summary>
    ///   Returns the control name
    /// </summary>
    public string ControlName
    {
      get { return "Group Box"; }
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

    #endregion

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
    public void ReadXml(XmlReader reader)
    {
      String header = reader.GetAttribute(STR_XML_HEADER);
      if (header != null)
      {
        this.Header = header;
      }
    }

    /// <summary>
    /// Converts an object into its XML representation.
    /// </summary>
    /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. 
    ///                 </param>
    public void WriteXml(XmlWriter writer)
    {
      if (this.Header != null)
      {
        writer.WriteAttributeString(STR_XML_HEADER, this.Header.ToString());
      }
    }

    #endregion

    #region Implementation of IControlContainer

    #endregion

    #region Implementation of IControlContainer

    #endregion

    #region Implementation of IControlContainer

    public ReadOnlyCollection<IGuiPanelControl> Children
    {
      get
      {
        List<IGuiPanelControl> controls = new List<IGuiPanelControl>();
        foreach (var child in m_canvas.Children)
        {
          if (child is IGuiPanelControl)
          {
            controls.Add(child as IGuiPanelControl);
          }
          else
          {
            Debug.WriteLine("[ERR] SnGroupBox.Children - child of incorrect type");
          }
        }
        return controls.AsReadOnly();
      }
    }

    /// <summary>
    ///   Add a child control
    /// </summary>
    /// <param name="p_control"></param>
    public bool AddChildControl(IGuiPanelControl p_control)
    {
      if (!m_canvas.Children.Contains(p_control.AsUIElement()))
      {
        //
        // bug here where logical tree depth gets too deep
        //
        m_canvas.Children.Add(p_control.AsUIElement());
        return true;
      }

      Debug.WriteLine("[DBG] SnGroupBox.AddChildControl - adding duplicate control");
      return false;
    }

    /// <summary>
    ///   Remove a child control
    /// </summary>
    /// <param name="p_control"></param>
    /// <returns></returns>
    public bool RemoveChildControl(IGuiPanelControl p_control)
    {
      if (m_canvas.Children.Contains(p_control.AsUIElement()))
      {
        m_canvas.Children.Remove(p_control.AsUIElement());
        return true;
      }

      Debug.WriteLine("[DBG] SnGroupBox.RemoveChildControl - control not found");
      return false;
    }

    /// <summary>
    ///   Get the location of the actual container within the control
    /// </summary>
    public Rect ContainerLocation
    {
      get
      {
        Point p = TranslatePoint(new Point(0, 0), m_canvas);
        return new Rect(p.X, p.Y, m_canvas.ActualWidth, m_canvas.ActualHeight);
      }
    }

    /// <summary>
    ///   Returns any additional inside top margin
    /// </summary>
    public double MarginInsideTop
    {
      get
      {
        return 10;
      }
    }

    /// <summary>
    ///   Returns any additional inside bottom margin
    /// </summary>
    public double MarginInsideBottom
    {
      get
      {
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
        return 0;
      }
    }

    /// <summary>
    ///   Get the control configuration panel
    /// </summary>
    /// <returns></returns>
    public UserControl GetConfigPanel()
    {
      return null;
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
      XAttribute xHeader = p_element.Attribute(STR_XML_HEADER);
      if (xHeader != null)
      {
        this.Header = xHeader.Value;
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
      element.Add(new XAttribute(STR_XML_HEADER, this.Header));
      return element;
    }

    ///// <summary>
    /////   Returns an array of rectangles to snap to
    ///// </summary>
    //public Rect[] AlignmentRegtangles
    //{
    //  get
    //  {
    //    Rect[] alignRects = new Rect[1];
    //    alignRects[0] = new Rect(0,0,this.ActualWidth, this.ActualHeight);
    //    return alignRects;
    //  }
    //}

    #endregion

    #region Implementation of IGuiPanelValueControl

    /// <summary>
    ///   Gets or sets the value of this control
    /// </summary>
    public string Value
    {
      get { return this.Header.ToString(); }
      set { this.Header = value; }
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
      get { return new SnGroupBoxConfig(this); }
    }
  }
}
