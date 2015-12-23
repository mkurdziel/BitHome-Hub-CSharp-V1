
namespace SyNet.DataHelpers
{
  /// <summary>
  ///   Interface for an object with an ID property
  /// </summary>
  /// <remarks>
  ///   Created primarily for use with SerializableDictionaryWithID
  /// </remarks>
  /// <typeparam name="TKey"></typeparam>
  public interface IObjectWithID<TKey>
  {
    /// <summary>
    ///   ID to use as a key in a serializable dictionary
    /// </summary>
    TKey ID { get; }
  }
}
