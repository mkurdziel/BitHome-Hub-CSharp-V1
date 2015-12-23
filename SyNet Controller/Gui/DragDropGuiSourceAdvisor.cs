using System;
using System.Windows;
using System.Windows.Controls;
using SyNet.GuiControls;
using SyNet.GuiHelpers.DragDropManager;

namespace SyNet.Gui
{
  public class DragDropGuiSourceAdvisor : IDragSourceAdvisor
  {
    ////////////////////////////////////////////////////////////////////////////
    // Source Advisor
    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of IDragSourceAdvisor

    private UIElement m_sourceAndTargetElement;

    public UIElement SourceUI
    {
      get { return m_sourceAndTargetElement; }
      set { m_sourceAndTargetElement = value; }
    }

    public DragDropEffects SupportedEffects
    {
      get
      {
        return DragDropEffects.Copy;
      }
    }

    public DataObject GetDataObject( UIElement draggedElt )
    {
      DataObject obj = null;

      //ContentControl cctrl = draggedElt as ContentControl;
      //if (cctrl != null)
      //{
      //  if (cctrl.DataContext != null)
      //  {
      //    obj = new DataObject(cctrl.DataContext);
      //  }
      //}

      TreeViewItem cctrl = draggedElt as TreeViewItem;
      if (cctrl != null)
      {
        if (cctrl.Header != null)
        {
          obj = new DataObject(cctrl.Header);
        }
      }


      return obj;
    }

    public void FinishDrag( UIElement draggedElt, DragDropEffects finalEffects )
    {
      //if ((finalEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
      //{
      //  XmlReader reader = XmlReader.Create(new StringReader(_serializedElt));
      //  UIElement elt = XamlReader.Load(reader) as UIElement;

      //  (_sourceAndTargetElt as Canvas).Children.Add(elt);
      //}
    }

    public bool IsDraggable(UIElement dragElt)
    {
      TreeViewItem cctrl = dragElt as TreeViewItem;
      if (cctrl != null)
      {
        if (cctrl.Header != null)
        {
          if (cctrl.Header is SyNet.Actions.Action)
          {
            return true;
          }

          if (cctrl.Header is SyNet.Events.Triggers.Trigger)
          {
            return true;
          }
        }
      }
      return false;
    }

    //public bool IsDraggable( UIElement dragElt )
    //{
    //  ContentControl cctrl = dragElt as ContentControl;
    //  if (cctrl != null)
    //  {
    //    if (cctrl.DataContext != null)
    //    {
    //      if (cctrl.DataContext is SyNet.Actions.Action)
    //      {
    //        return true;
    //      }

    //      if (cctrl.DataContext is SyNet.Events.Triggers.Trigger)
    //      {
    //        return true;
    //      }
    //    }
    //  }
    //  return false;
    //}

    public UIElement GetTopContainer()
    {
      return Application.Current.MainWindow;
    }

    #endregion
  }
}
