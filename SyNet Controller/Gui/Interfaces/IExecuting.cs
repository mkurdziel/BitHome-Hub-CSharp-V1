using System.Windows;

namespace SyNet.Gui.Interfaces
{
  /// <summary>
  ///   Interface for a control that can execute an action
  /// </summary>
  public interface IExecuting
  {
    /// <summary>
    ///   Trigged when a an action is attempted to be executed
    /// </summary>
    event RoutedEventHandler Execute;
  }
}
