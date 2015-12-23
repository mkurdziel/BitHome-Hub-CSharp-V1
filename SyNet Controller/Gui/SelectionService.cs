using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace SyNet.Gui
{
  /// <summary>
  ///   Responsible for handling selection logics
  /// </summary>
  public class SelectionService 
  {
    private List<GuiPanelItem> m_currentSelection = new List<GuiPanelItem>();

    /// <summary>
    ///   List holding the currently selected objects
    /// </summary>
    internal List<GuiPanelItem> CurrentSelection
    {
      get
      {
        return m_currentSelection;
      }
    }

    internal void SelectItem(GuiPanelItem item)
    {
      this.ClearSelection();
      this.AddToSelection(item);
    }

    internal void AddToSelection(GuiPanelItem item)
    {
      item.IsSelected = true;
      CurrentSelection.Add(item);
      OnSelectionChanged();
    }

    internal void RemoveFromSelection(GuiPanelItem item)
    {
      item.IsSelected = false;
      CurrentSelection.Remove(item);
      OnSelectionChanged();
    }

    internal void ClearSelection()
    {
      foreach (GuiPanelItem selection in CurrentSelection)
      {
        selection.IsSelected = false;
      }
      CurrentSelection.Clear();
      OnSelectionChanged();
    }

    /// <summary>
    ///   Request an item to be selected.  Handles keyboard multi-selection
    ///   logic as well as a check for Editing mode
    /// </summary>
    /// <param name="items"></param>
    /// <param name="p_isDoubleClick"></param>
    public void RequestSelection(GuiPanelItem p_item)
    {
      if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
      {
        if (p_item.IsSelected)
        {
          RemoveFromSelection(p_item);
        }
        else
        {
          AddToSelection(p_item);
        }
      }
      else if (!p_item.IsSelected)
      {
        SelectItem(p_item);
      }
    }

    /// <summary>
    ///   Return a list of selected items that are movable along with the passed in item
    /// </summary>
    /// <param name="p_item"></param>
    /// <returns></returns>
    public List<GuiPanelItem> GetMovableSelection(GuiPanelItem p_item)
    {
      List<GuiPanelItem> movableItems = new List<GuiPanelItem>();

      foreach (GuiPanelItem otherSelectedItem in CurrentSelection)
      {
        if (otherSelectedItem.Parent == p_item.Parent)
        {
          movableItems.Add(otherSelectedItem);
        }
      }

      return movableItems;
    }

    public event EventHandler SelectionChanged;

    /// <summary>
    /// </summary>
    /// <param name="p_strPropertyName"></param>
    protected void OnSelectionChanged()
    {
      if (SelectionChanged != null)
        SelectionChanged(this, new EventArgs());
    }

  }
}