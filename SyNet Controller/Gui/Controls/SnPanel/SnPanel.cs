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

namespace SyNet.Gui.Controls.SnPanel
{
  public class SnPanel : Grid, IControlContainer, IGuiPanelControl
  {
    public const string STR_XML_ROOT = "SnPanel";
    public const string STR_XML_HEADER = "Header";

    private string m_name = "Panel";

    private Canvas m_canvas = new Canvas();

    public string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public SnPanel()
    {
      this.Width = 100;
      this.Height = 100;
      ((Grid)this).Children.Add(m_canvas);
      this.Background = new SolidColorBrush(Colors.White);
      this.ClipToBounds = true;
    }

    #region Implementation of IGuiPanelControl

    /// <summary>
    ///   Returns a path to the control image
    /// </summary>
    public string ControlImage
    {
      get { return String.Empty; }
    }

    /// <summary>
    ///   Returns the control name
    /// </summary>
    public string ControlName
    {
      get { return "Panel"; }
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

      Debug.WriteLine("[DBG] SnPanel.AddChildControl - adding duplicate control");
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

      Debug.WriteLine("[DBG] SnPanel.RemoveChildControl - control not found");
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
      return true;
    }

    /// <summary>
    ///   Save the control settings to an XElement
    /// </summary>
    /// <returns></returns>
    public XElement Save()
    {
      XElement element = new XElement(STR_XML_ROOT);
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

    /// <summary>
    ///   Gets the configuration panel
    /// </summary>
    public UserControl ConigurationPanel
    {
      get { return null; }
    }
  }
}
