
namespace SyNet.EventArguments
{
  using System;

  /// <summary>
  /// CLASS Custom EventArgs identifying which device/node was just discovered
  /// </summary>
  public class NodeDiscoveredEventArgs : EventArgs
  {
    private readonly UInt64 m_dvcDeviceID;
    private readonly bool m_bFullParameters;

    /// <summary>
    /// Constructor with initial data
    /// </summary>
    /// <param name="p_dvcDeviceID"></param>
    /// <param name="p_bFullParameters"></param>
    public NodeDiscoveredEventArgs(UInt64 p_dvcDeviceID, bool p_bFullParameters)
    {
      m_dvcDeviceID = p_dvcDeviceID;
      m_bFullParameters = p_bFullParameters;
    }

    /// <summary>
    /// R/O PROPERTY: Returns the newly discovered Device ID
    /// </summary>
    public UInt64 DeviceID
    {
      get { return m_dvcDeviceID; }
    }

    /// <summary>
    /// R/O PROPERTY: Returns the state of known parameters
    /// </summary>
    public bool FullParameters
    {
      get { return m_bFullParameters; }
    }

  }
}
