using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using SyNet.DataHelpers;

namespace SyNet
{
  /// <summary>
  ///   Enumerator class for enumerating over nodes in the node list
  /// </summary>
  public class NodeEnumerator : IEnumerator<Device>
  {
    private Device[] m_nodes;

    // Enumerators are positioned before the first element
    // until the first MoveNext() call.
    private int position = -1;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    public NodeEnumerator(Device[] list)
    {
      m_nodes = list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
      position++;
      return (position < m_nodes.Length);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
      position = -1;
    }

    /// <summary>
    /// 
    /// </summary>
    Device IEnumerator<Device>.Current
    {
      get
      {
        try
        {
          return m_nodes[position];
        }
        catch (IndexOutOfRangeException)
        {
          throw new InvalidOperationException();
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public object Current
    {
      get
      {
        try
        {
          return m_nodes[position];
        }
        catch (IndexOutOfRangeException)
        {
          throw new InvalidOperationException();
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
    }
  }

  /// <summary>
  ///   Class that holds a list of nodes
  /// </summary>
  [XmlRoot("NodeList")]
  [XmlInclude(typeof (Device))]
  [XmlInclude(typeof (DeviceXBee))]
  public class SyNetNodeList : INotifyCollectionChanged,
                               ICollection<Device>,
                               ICollection
  {
    #region Member Variables

    private object m_syncRoot = new object();

    #endregion

    #region Constructors

    /// <summary>
    ///   Default constructor
    /// </summary>
    public SyNetNodeList()
    {
      NodeDictionary = new SerializableDictionaryWithId<ulong, Device>();
    }

    #endregion

    #region Properties

    /// <summary>
    ///   Indexor
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public Device this[int i]
    {
      get
      {
        return NodeList[i];
      }
    }

    /// <summary>
    ///   Node Dictionary
    /// </summary>
    public SerializableDictionaryWithId<UInt64, Device> NodeDictionary
    { get; set; }

    /// <summary>
    ///   Returns a list of nodes
    /// </summary>
    public List<Device> NodeList
    {
      get
      {
        List<Device> list = new List<Device>(NodeDictionary.Count);
        foreach (KeyValuePair<ulong, Device> pair in NodeDictionary)
        {
          list.Add(pair.Value);
        }
        return list;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    ///   Returns true if this serial number exists in our lookup
    /// </summary>
    /// <returns></returns>
    public bool ContainsSerial(UInt64 p_dvcSerialNumber64Bit)
    {
      return NodeDictionary.ContainsKey(p_dvcSerialNumber64Bit);
    }

    /// <summary>
    ///   Get a node from its serial number
    /// </summary>
    /// <returns></returns>
    public Device GetNodeFromMy64Addr(UInt64 p_dvcSerialNumber64Bit)
    {
      Device dxTheDevice = null;
      if (NodeDictionary.ContainsKey(p_dvcSerialNumber64Bit))
      {
        dxTheDevice = NodeDictionary[p_dvcSerialNumber64Bit];
      }
      return dxTheDevice;
    }

    /// <summary>
    ///   Override the insert item to prevent duplications
    /// </summary>
    public void Add(Device p_dxNode)
    {
      if (NodeDictionary.ContainsKey(p_dxNode.ID))
      {
        // Update
        p_dxNode.LastSeen = DateTime.Now;
      }
      else
      {
        // Add thhe item and the lookup
        lock (m_syncRoot)
        {
          NodeDictionary.Add(p_dxNode.ID, p_dxNode);
        }
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, p_dxNode));
      }
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs p_chevArgs)
    {
      if (CollectionChanged != null)
      {
        CollectionChanged(this, p_chevArgs);
      }
    }

    /// <summary>
    ///   Clear all nodes from the list
    /// </summary>
    public void Clear()
    {
      lock (m_syncRoot)
      {
        NodeDictionary.Clear();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }
    }

    /// <summary>
    ///   Returns true if the specific node is contained in the list
    /// </summary>
    /// <param name="p_dxNode"></param>
    /// <returns></returns>
    public bool Contains(Device p_dxNode)
    {
      return NodeDictionary.ContainsKey(p_dxNode.ID);
    }

    /// <summary>
    ///   Removes a specific node from the list
    /// </summary>
    /// <param name="p_dxNode"></param>
    /// <returns></returns>
    public bool Remove(Device p_dxNode)
    {
      int index = 0;
      bool retVal = false;
      lock (m_syncRoot)
      {
        if (Contains(p_dxNode))
        {
          Device[] nodes = new Device[NodeDictionary.Count];
          CopyTo(nodes, 0);

          foreach (Device node in nodes)
          {
            if (node == p_dxNode)
            {
              break;
            }
            index++;
          }
          retVal = NodeDictionary.Remove(p_dxNode.ID);
          OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, p_dxNode, index));
        }
      }
      return retVal;
    }

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetEnumerator()
    {
      Device[] nodes = new Device[NodeDictionary.Count];
      NodeDictionary.Values.CopyTo(nodes, 0);
      return new NodeEnumerator(nodes);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator<Device> IEnumerable<Device>.GetEnumerator()
    {
      Device[] nodes = new Device[NodeDictionary.Count];
      NodeDictionary.Values.CopyTo(nodes, 0);
      return new NodeEnumerator(nodes);
    }

    #endregion

    #region ICollection Memebers

    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(Device[] array, int arrayIndex)
    {
      foreach (KeyValuePair<ulong, Device> pair in NodeDictionary)
      {
        array.SetValue(pair.Value, arrayIndex++);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    public void CopyTo(Array array, int index)
    {
      lock (m_syncRoot)
      {
        foreach (KeyValuePair<ulong, Device> pair in NodeDictionary)
        {
          array.SetValue(pair.Value, index++);
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int Count
    {
      get { return NodeDictionary.Count; }
    }

    /// <summary>
    /// 
    /// </summary>
    public object SyncRoot
    {
      get { return m_syncRoot; }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsSynchronized
    {
      get { return true; }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsReadOnly
    {
      get { return false; }
    }

    #endregion

    #region INotifyCollectionChanged Members

    /// <summary>
    /// 
    /// </summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion
  }
}