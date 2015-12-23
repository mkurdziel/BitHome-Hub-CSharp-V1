using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SyNet.Gui.Models;

namespace SyNet.Gui.Toolbox
{
  /// <summary>
  /// Interaction logic for GuiPanelItemConfig.xaml
  /// </summary>
  public partial class GuiPanelItemConfig : UserControl
  {
    private GuiPanelItem MyItem { get; set; }

    public GuiPanelItemConfig(GuiPanelItem p_item)
    {
      InitializeComponent();

      MyItem = p_item;

      // If there is a label, show the config
      if (MyItem.HasLabel)
      {
        x_labelPanel.Visibility = Visibility.Visible;

        x_labelPanel.Content = new LabelConfig(MyItem);
      }
      else
      {
        x_labelPanel.Visibility = Visibility.Collapsed;
      }

      x_controlPanel.Content = MyItem.ConigurationPanel;
      x_controlPanel.Visibility = (x_controlPanel.Content != null)
                                       ? Visibility.Visible
                                       : Visibility.Collapsed;

      //
      // Get the list of controls
      //
      List<GuiPanelControlInfo> controlTypes = new List<GuiPanelControlInfo>();

      //
      // Trigger
      //
      if (MyItem.HasTrigger)
      {
        controlTypes.AddRange(GuiManager.Instance.GuiPanelValueControlTypes);
      }

      //
      // Parameter
      //
      if (MyItem.HasParameter)
      {
        controlTypes.AddRange(GuiManager.Instance.GuiPanelInputControlTypes);
      }

      //
      // Container
      //
      if (MyItem.IsContainer)
      {
        controlTypes.AddRange(GuiManager.Instance.GuiPanelControlContainers);
      }

      foreach (GuiPanelControlInfo controlType in controlTypes)
      {
        ToggleButton controlButton = new ToggleButton();
        controlButton.Checked += ControlButton_Checked;
        controlButton.Unchecked += ControlButton_Checked;
        controlButton.Content = controlType.ControlName;
        controlButton.DataContext = controlType;
        x_objectWrapPanel.Children.Add(controlButton);

        //
        // Select it if it's the right control
        //
        if (controlType.ControlType == MyItem.GuiPanelControl.GetType())
        {
          controlButton.IsChecked = true;
        }
      }

      x_objectPanel.Visibility = controlTypes.Count > 0
                                   ? Visibility.Visible
                                   : Visibility.Collapsed;

    }

    void ControlButton_Checked(object sender, RoutedEventArgs e)
    {
      ToggleButton button = sender as ToggleButton;

      if (sender == null) return;

      GuiPanelControlInfo info = button.DataContext as GuiPanelControlInfo;

      if (info == null) return;

      if (button.IsChecked == true && this.MyItem.ControlType != info.ControlType)
      {
        //
        // Set the control to the new button pressed
        //
        MyItem.SetGuiPanelControl(info.ControlType);

        // 
        // Deselect other buttons that are no longer applicable
        //
        foreach (ToggleButton controlButton in x_objectWrapPanel.Children)
        {
          controlButton.IsChecked = (controlButton == button);
        }
      }
      //
      // If this type is already the type set to the control, do nothing
      //
      else if (this.MyItem.ControlType == info.ControlType)
      {
        button.IsChecked = true;
      }
    }
  }
}
