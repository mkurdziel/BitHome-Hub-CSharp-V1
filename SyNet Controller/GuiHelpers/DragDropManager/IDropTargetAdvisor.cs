using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SyNet.GuiHelpers.DragDropManager
{
  public interface IDropTargetAdvisor
  {
    UIElement TargetUI
    {
      get;
      set;
    }

    bool ApplyMouseOffset
    { 
      get;
    }
    bool IsValidDataObject(IDataObject obj);
    void OnDropCompleted(IDataObject obj, Point dropPoint);
    UIElement GetVisualFeedback(IDataObject obj);
    UIElement GetTopContainer();
  }
}