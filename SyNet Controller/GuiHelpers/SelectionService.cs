using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SyNet.GuiControls;

namespace SyNet.GuiHelpers
{
  internal class SelectionService
  {
    private DesignerCanvas designerCanvas;
    //private ObservableCollection<IGuiSelectable> m_currentSelection;

    //internal ObservableCollection<IGuiSelectable> CurrentSelection
    //{
    //  get
    //  {
    //    return m_currentSelection;
    //  }
    //}

    public SelectionService(DesignerCanvas canvas)
    {
      this.designerCanvas = canvas;
      //m_currentSelection = new ObservableCollection<IGuiSelectable>();
    }

    //internal void SelectItem(IGuiSelectable item)
    //{
    //  this.ClearSelection();
    //  this.AddToSelection(item);
    //}

    //internal void AddToSelection(IGuiSelectable item)
    //{
    //  if (item is IGuiGroupable)
    //  {
    //    List<IGuiGroupable> groupItems = GetGroupMembers(item as IGuiGroupable);

    //    foreach (IGuiSelectable groupItem in groupItems)
    //    {
    //      groupItem.IsSelected = true;
    //      CurrentSelection.Add(groupItem);
    //    }
    //  }
    //  else
    //  {
    //    item.IsSelected = true;
    //    CurrentSelection.Add(item);
    //  }
    //}

    //internal void RemoveFromSelection(IGuiSelectable item)
    //{
    //  if (item is IGuiGroupable)
    //  {
    //    List<IGuiGroupable> groupItems = GetGroupMembers(item as IGuiGroupable);

    //    foreach (IGuiSelectable groupItem in groupItems)
    //    {
    //      groupItem.IsSelected = false;
    //      CurrentSelection.Remove(groupItem);
    //    }
    //  }
    //  else
    //  {
    //    item.IsSelected = false;
    //    CurrentSelection.Remove(item);
    //  }
    //}

    //internal void ClearSelection()
    //{
    //  foreach (IGuiSelectable selection in CurrentSelection)
    //  {
    //    selection.IsSelected = false;
    //  }
    //  CurrentSelection.Clear();
    //}

    //internal void SelectAll()
    //{
    //  ClearSelection();
    //  foreach (IGuiSelectable selection in designerCanvas.Children.OfType<IGuiSelectable>())
    //  {
    //    CurrentSelection.Add(selection);
    //    selection.IsSelected = true;
    //  }
    //}

    //internal List<IGuiGroupable> GetGroupMembers(IGuiGroupable item)
    //{
    //  IEnumerable<IGuiGroupable> list = designerCanvas.Children.OfType<IGuiGroupable>();
    //  IGuiGroupable rootItem = GetRoot(list, item);
    //  return GetGroupMembers(list, rootItem);
    //}

    //internal IGuiGroupable GetGroupRoot(IGuiGroupable item)
    //{
    //  IEnumerable<IGuiGroupable> list = designerCanvas.Children.OfType<IGuiGroupable>();
    //  return GetRoot(list, item);
    //}

    //private IGuiGroupable GetRoot(IEnumerable<IGuiGroupable> list, IGuiGroupable node)
    //{
    //  if (node == null || node.ParentID == Guid.Empty)
    //  {
    //    return node;
    //  }
    //  else
    //  {
    //    foreach (IGuiGroupable item in list)
    //    {
    //      if (item.ID == node.ParentID)
    //      {
    //        return GetRoot(list, item);
    //      }
    //    }
    //    return null;
    //  }
    //}

    //private List<IGuiGroupable> GetGroupMembers(IEnumerable<IGuiGroupable> list, IGuiGroupable parent)
    //{
    //  List<IGuiGroupable> groupMembers = new List<IGuiGroupable>();
    //  groupMembers.Add(parent);

    //  var children = list.Where(node => node.ParentID == parent.ID);

    //  foreach (IGuiGroupable child in children)
    //  {
    //    groupMembers.AddRange(GetGroupMembers(list, child));
    //  }

    //  return groupMembers;
    //}

    ///// <summary>
    /////   Request an item to be selected.  Handles keyboard multi-selection
    /////   logic as well as a check for Editing mode
    ///// </summary>
    ///// <param name="items"></param>
    ///// <param name="p_isDoubleClick"></param>
    //public void RequestSelection(List<IGuiSelectable> items, bool p_isDoubleClick)
    //{
    //  //
    //  // We'll only select if we're editing
    //  //
    //  if (designerCanvas.IsEditing)
    //  {

    //    if (items.Count == 0)
    //    {
    //      return;
    //    }

    //    IGuiSelectable item = items[0];

    //    if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
    //    {
    //      if (item.IsSelected)
    //      {
    //        RemoveFromSelection(item);
    //      }
    //      else
    //      {
    //        //
    //        // Make sure we're not mixing types
    //        //
    //        if (CurrentSelection.Count > 0)
    //        {
    //          if (item.GetType().IsAssignableFrom(CurrentSelection[0].GetType()))
    //          {
    //            AddToSelection(item);
    //          }
    //        }
    //        else
    //        {
    //          AddToSelection(item);
    //        }
    //      }
    //    }
    //    else if (!item.IsSelected)
    //    {
    //      //
    //      // If item has no selectable parent, then just select it
    //      //
    //      if (items.Count == 1)
    //      {
    //        SelectItem(item);
    //      }
    //      else 
    //      {
    //        //
    //        // If an item is selected and its parent is not yet selected
    //        // then select its parent
    //        //
    //        if (item.SelectableParent != null && !CurrentSelection.Contains(item.SelectableParent))
    //        {
    //          SelectItem(item.SelectableParent);
    //        }
    //          //
    //          // If the second item is a child item of the first item
    //          // and the child item is selected, don't change selection
    //          //
    //        else if (items[1].SelectableParent == items[0] && CurrentSelection.Contains(items[1]))
    //        {
    //          return;
    //        }
    //        //
    //        // If a child is hit and its parent is selected, select the child
    //        //
    //        else
    //        {
    //          SelectItem(item);
    //        }
    //      }
    //    }
    //    else
    //    {
    //      //
    //      // Item is already selected. See if there is a secondary item to 
    //      // select
    //      //
    //      if (items.Count > 1)
    //      {
    //        if (items[1].SelectableParent == items[0] && p_isDoubleClick)
    //        {
    //          SelectItem(items[1]);
    //        }
    //      }
    //    }
    //  }
    //}
  }
}