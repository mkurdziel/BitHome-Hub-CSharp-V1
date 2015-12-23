using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using SyNet.Actions;
using SyNet.Events;
using SyNet.Events.Triggers;
using SyNet.Gui;
using SyNet.Gui.Interfaces;
using SyNet.XamlPanels.GuiPanels;

namespace SyNet.Gui.Models
{
  /// <summary>
  ///   Class representing a single Gui Panel
  /// </summary>
  public class GuiPanel : INotifyPropertyChanged
  {
    ////////////////////////////////////////////////////////////////////////////
    #region Member Variables

    private string m_title;
    private Color m_backgroundColor;
    private Color m_foregroundColor;
    private bool m_isEditing;

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Public Properties

    /// <summary>
    ///   Gets or sets a list of GuiPanelItems in this gui panel
    /// </summary>
    [XmlArray]
    public List<GuiPanelItem> GuiPanelItems
    {
      get; set;
    }

    /// <summary>
    ///   Gets or sets the title color of the panel
    /// </summary>
    [XmlIgnore]
    public Color ForegroundColor
    {
      get { return m_foregroundColor; }
      set
      {
        m_foregroundColor = value;
        OnPropertyChanged("ForegroundColor");
      }
    }

    /// <summary>
    ///   Gets or sets the title color via hex string
    /// </summary>
    [XmlAttribute(AttributeName = "ForegroundColor")]
    public string TitleColorString
    {
      get
      {
        return ForegroundColor.ToString();
      }
      set
      {
        ForegroundColor = (Color)ColorConverter.ConvertFromString(value);
      }
    }

    /// <summary>
    ///   Gets or sets the background color of the panel
    /// </summary>
    [XmlIgnore]
    public Color BackgroundColor
    {
      get { return m_backgroundColor; }
      set
      {
        m_backgroundColor = value;
        OnPropertyChanged("BackgroundColor");
      }
    }

    /// <summary>
    ///   Gets or sets the background color via hex string
    /// </summary>
    [XmlAttribute(AttributeName="BackgroundColor")]
    public string BackgroundColorString
    {
      get
      {
        return BackgroundColor.ToString();
      }
      set
      {
        BackgroundColor = (Color)ColorConverter.ConvertFromString(value);
      }
    }

    ///// <summary>
    /////   Returns true if this panel is being edited
    ///// </summary>
    //[XmlIgnore]
    //public bool IsEditing
    //{
    //  get { return m_isEditing; }
    //  set
    //  {
    //    if (m_isEditing != value)
    //    {
    //      m_isEditing = value;

    //      //
    //      // Send the changes to all items in the panel
    //      //
    //      foreach (GuiPanelItem item in GuiPanelItems)
    //      {
    //        item.IsEditing = this.IsEditing;
    //      }

    //      //
    //      // Notify any listeners
    //      //
    //      OnPropertyChanged("IsEditing");
    //    }
    //  }
    //}

    /// <summary>
    ///   Panel Title
    /// </summary>
    [XmlAttribute]
    public string Title
    {
      get { return m_title; }
      set
      {
        m_title = value;
        OnPropertyChanged("Title");
      }
    }

    /// <summary>
    ///   Panel description, set by user.
    /// </summary>
    [XmlElement]
    public string Description { get; set; }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Constructors

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public GuiPanel()
    {
      BackgroundColor = Colors.White;
      ForegroundColor = Colors.Black;
      GuiPanelItems = new List<GuiPanelItem>();
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Public Properties

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Events

    /// <summary>
    ///   Property changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Fires events for property changed
    /// </summary>
    /// <param name="p_strPropertyName"></param>
    protected void OnPropertyChanged(string p_strPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(
          this,
          new PropertyChangedEventArgs(p_strPropertyName));
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IGuiConfigurable

    /// <summary>
    ///   Returns the configuration user control for this item
    /// </summary>
    /// <returns></returns>
    public UserControl GetConfigUserControl()
    {
      //return new GuiPanelConfigDialog(this);
      return null;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////
     
    /// <summary>
    ///   Add a guipanel item to the model
    /// </summary>
    /// <param name="p_control"></param>
    public void AddControl(GuiPanelItem p_control)
    {
      GuiPanelItems.Add(p_control);

      OnPropertyChanged("GuiPanelItems");
    }

    public void RemoveControl(GuiPanelItem p_item)
    {
      GuiPanelItems.Remove(p_item);

      OnPropertyChanged("GuiPanelItems");
    }
  }
}