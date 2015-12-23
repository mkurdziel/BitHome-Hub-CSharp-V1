using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SyNet.Gui.Controls.SnButton;
using SyNet.Gui.Controls.SnGroupBox;
using SyNet.Gui.Controls.SnLabel;
using SyNet.Gui.Controls.SnPanel;
using SyNet.Gui.Controls.SnSlider;
using SyNet.Gui.Controls.SnTextBox;
using SyNet.Gui.Interfaces;

namespace SyNet.Gui.Models
{
  /// <summary>
  ///   Singleton manager for GUI objects in the system
  /// </summary>
  public class GuiManager
  {
    private static GuiManager s_instance;
    private static readonly object s_objInstanceLock = new object();
    private List<GuiPanelControlInfo> m_guiControlTypes = new List<GuiPanelControlInfo>();

    /// <summary>
    ///   Returns a list of gui control types
    /// </summary>
    [XmlIgnore]
    public List<GuiPanelControlInfo> GuiPanelControlTypes
    {
      get { return m_guiControlTypes; }
    }

    /// <summary>
    ///  Returns a list of gui panel value control types
    /// </summary>
    [XmlIgnore]
    public List<GuiPanelControlInfo> GuiPanelValueControlTypes
    {
      get
      {
        List<GuiPanelControlInfo> retList = new List<GuiPanelControlInfo>();

        foreach (GuiPanelControlInfo typeInfo in GuiPanelControlTypes)
        {
          if (typeof(IGuiPanelValueControl).IsAssignableFrom(typeInfo.ControlType))
          {
            retList.Add(typeInfo);
          }
        }

        return retList;
      }
    }

    /// <summary>
    ///  Returns a list of gui panel value control types
    /// </summary>
    [XmlIgnore]
    public List<GuiPanelControlInfo> GuiPanelControlContainers
    {
      get
      {
        List<GuiPanelControlInfo> retList = new List<GuiPanelControlInfo>();

        foreach (GuiPanelControlInfo typeInfo in GuiPanelControlTypes)
        {
          if (typeof(IControlContainer).IsAssignableFrom(typeInfo.ControlType))
          {
            retList.Add(typeInfo);
          }
        }

        return retList;
      }
    }

    /// <summary>
    ///  Returns a list of input controls
    /// </summary>
    [XmlIgnore]
    public List<GuiPanelControlInfo> GuiPanelInputControlTypes
    {
      get
      {
        List<GuiPanelControlInfo> retList = new List<GuiPanelControlInfo>();

        foreach (GuiPanelControlInfo typeInfo in GuiPanelControlTypes)
        {
          if (typeInfo.CanInputValue)
          {
            retList.Add(typeInfo);
          }
        }

        return retList;
      }
    }


    /// <summary>
    ///   Singleton instance of the GuiManager
    /// </summary>
    [XmlIgnore]
    public static GuiManager Instance
    {
      get
      {
        lock (s_objInstanceLock)
        {
          if (s_instance == null)
          {
            s_instance = new GuiManager();
          }
          return s_instance;
        }
      }
    }

    /// <summary>
    ///   List of GUI panels manged by the GuiManager
    /// </summary>
    public List<GuiPanel> GuiPanels { get; set; }

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public GuiManager()
    {
      lock(s_objInstanceLock)
      {
        if (s_instance == null)
        {
          s_instance = this;
        }
      }

      GuiPanels = new List<GuiPanel>();
      AddGuiControlObjects();
    }

    private void AddGuiControlObjects()
    {
      m_guiControlTypes.Add(new GuiPanelControlInfo(new SnButton()));
      m_guiControlTypes.Add(new GuiPanelControlInfo(new SnLabel()));
      m_guiControlTypes.Add(new GuiPanelControlInfo(new SnGroupBox()));
      m_guiControlTypes.Add(new GuiPanelControlInfo(new SnTextBox()));
      m_guiControlTypes.Add(new GuiPanelControlInfo(new SnSlider()));
      m_guiControlTypes.Add(new GuiPanelControlInfo(new SnPanel()));

    }

    /// <summary>
    ///   Add a new panel to the gui panel collection
    /// </summary>
    /// <param name="p_panel"></param>
    public void AddGuiPanel(GuiPanel p_panel)
    {
      if(!GuiPanels.Contains(p_panel))
      {
        GuiPanels.Add(p_panel);
      }
    }

    /// <summary>
    ///   Remove a panel from the gui panel collection
    /// </summary>
    /// <param name="p_panel"></param>
    /// <returns></returns>
    public bool RemoveGuiPanel(GuiPanel p_panel)
    {
      bool retVal = false;
      if (GuiPanels.Contains(p_panel))
      {
        GuiPanels.Remove(p_panel);
        retVal = true;
      }
      return retVal;
    }

    /// <summary>
    ///   Disables the editing of all panels
    /// </summary>
    public void DisableAllEditing()
    {
      //foreach (GuiPanel panel in GuiPanels)
      //{
      //  panel.IsEditing = false;
      //}
    }
  }
}