using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace SyNet.BindingHelpers {
  /// <summary>
  ///   Binding class to bind between two properties
  /// </summary>
  public class BindingProxy : FrameworkElement {
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty InProperty;
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty OutProperty;

    /// <summary>
    ///   Default constructor
    /// </summary>
    public BindingProxy()
    {
      Visibility = Visibility.Collapsed;
    }

    /// <summary>
    ///   Static constructor
    /// </summary>
    static BindingProxy()
    {
      FrameworkPropertyMetadata inMetadata = new FrameworkPropertyMetadata(
          delegate(DependencyObject p, DependencyPropertyChangedEventArgs args) {
            if (null != BindingOperations.GetBinding(p, OutProperty))
              (p as BindingProxy).Out = args.NewValue;
          });

      inMetadata.BindsTwoWayByDefault = false;
      inMetadata.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

      InProperty = DependencyProperty.Register("In",
          typeof(object),
          typeof(BindingProxy),
          inMetadata);

      FrameworkPropertyMetadata outMetadata = new FrameworkPropertyMetadata(
          delegate(DependencyObject p, DependencyPropertyChangedEventArgs args) {
            ValueSource source = DependencyPropertyHelper.GetValueSource(p, args.Property);

            if (source.BaseValueSource != BaseValueSource.Local) {
              BindingProxy proxy = p as BindingProxy;
              object expected = proxy.In;
              if (!object.ReferenceEquals(args.NewValue, expected)) {
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    DispatcherPriority.DataBind, new Operation(delegate {
                          proxy.Out = proxy.In;
                        }));
              }
            }
          });

      outMetadata.BindsTwoWayByDefault = true;
      outMetadata.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

      OutProperty = DependencyProperty.Register("Out",
          typeof(object),
          typeof(BindingProxy),
          outMetadata);
    }

    /// <summary>
    ///   Object coming in
    /// </summary>
    public object In
    {
      get { return this.GetValue(InProperty); }
      set { this.SetValue(InProperty, value); }
    }

    /// <summary>
    ///   Object going out
    /// </summary>
    public object Out
    {
      get { return this.GetValue(OutProperty); }
      set { this.SetValue(OutProperty, value); }
    }

    /// <summary>
    /// 
    /// </summary>
    public delegate void Operation();
  }
}
