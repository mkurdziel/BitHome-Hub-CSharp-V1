using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SyNet.GuiHelpers
{
  /// <summary>
  /// ========================================
  /// .NET Framework 3.0 Custom Control
  /// ========================================
  ///
  /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
  ///
  /// Step 1a) Using this custom control in a XAML file that exists in the current project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:CloseableTabItemDemo"
  ///
  ///
  /// Step 1b) Using this custom control in a XAML file that exists in a different project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:CloseableTabItemDemo;assembly=CloseableTabItemDemo"
  ///
  /// You will also need to add a project reference from the project where the XAML file lives
  /// to this project and Rebuild to avoid compilation errors:
  ///
  ///     Right click on the target project in the Solution Explorer and
  ///     "Add Reference"->"Projects"->[Browse to and select this project]
  ///
  ///
  /// Step 2)
  /// Go ahead and use your control in the XAML file. Note that Intellisense in the
  /// XML editor does not currently work on custom controls and its child elements.
  ///
  /// </summary>
  public class CloseableTabItem : TabItem, INotifyPropertyChanged
  {
    private bool m_isEditing;

    /// <summary>
    /// </summary>
    public static readonly DependencyProperty IsEditingProperty =
      DependencyProperty.Register("IsEditing",
                                  typeof(bool), typeof(CloseableTabItem),
                                  new PropertyMetadata(false, OnEditingChanged)
                                  );

    private static void OnEditingChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    {
      CloseableTabItem item = source as CloseableTabItem;
      if (item != null)
      {
        item.OnEditingPropertyChanged();
      }
      
    }

    public void OnEditingPropertyChanged()
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs("IsEditingVisibility"));
      }
    }

    /// <summary>
    ///   Gets or sets the type of this control
    /// </summary>
    public bool IsEditing
    {
      get
      {
        return (bool)GetValue(IsEditingProperty);
      }
      set
      {
        SetValue(IsEditingProperty, value);
      }
    }

    public Visibility IsEditingVisibility
    {
      get
      {
        if (IsEditing)
        {
          return Visibility.Visible;
        }
        else
        {
          return Visibility.Hidden;
        }
      }
    }



    /// <summary>
    /// 
    /// </summary>
    static CloseableTabItem()
    {
      //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
      //This style is defined in themes\generic.xaml
      DefaultStyleKeyProperty.OverrideMetadata(typeof(CloseableTabItem),
          new FrameworkPropertyMetadata(typeof(CloseableTabItem)));
    }

    /// <summary>
    /// 
    /// </summary>
    public static readonly RoutedEvent CloseTabEvent =
        EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(CloseableTabItem));

    /// <summary>
    /// 
    /// </summary>
    public event RoutedEventHandler CloseTab
    {
      add { AddHandler(CloseTabEvent, value); }
      remove { RemoveHandler(CloseTabEvent, value); }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      Button closeButton = base.GetTemplateChild("PART_Close") as Button;
      if (closeButton != null)
        closeButton.Click += new System.Windows.RoutedEventHandler(closeButton_Click);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      string strMessageBoxText = "Are you sure you want to close this tab?";
      string strCaption = "";
      MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
      MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
      MessageBoxResult rsltMessageBox = MessageBox.Show(
        strMessageBoxText, strCaption, btnMessageBox, icnMessageBox);
      if (rsltMessageBox == MessageBoxResult.Yes)
      {
        this.RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
      }
    }

    #region Implementation of INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
