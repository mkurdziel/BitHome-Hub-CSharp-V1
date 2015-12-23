using System;
using System.Drawing;
using SyNet.Gui.Interfaces;

namespace SyNet.Gui.Models
{
  public class GuiPanelControlInfo
  {
    public bool CanInputValue { get; private set; }

    public Type ControlType { get; private set; }

    public string ControlName { get; private set; }

    private string m_controlImage = String.Empty;
    public string ControlImage
    {
      get
      {
        if (String.IsNullOrEmpty(m_controlImage))
        {
           return "pack://application:,,,/Resources/GuiControls/Gear.png"; 
        }
        return m_controlImage;
      }
      private set { m_controlImage = value; }
    }


    public GuiPanelControlInfo(IGuiPanelControl p_control)
    {
      ControlType = p_control.GetType();
      ControlName = p_control.ControlName;
      ControlImage = p_control.ControlImage;
      if (p_control is IGuiPanelValueControl)
      {
        CanInputValue = ((IGuiPanelValueControl) p_control).CanInputValue;
      }
      else
      {
        CanInputValue = false;
      }
    }
  }
}
