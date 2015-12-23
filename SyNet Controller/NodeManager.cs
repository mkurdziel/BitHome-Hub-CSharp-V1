using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Xml.Serialization;
using SyNet.EventArguments;
using SyNet.MessageTypes;
using SyNet.Protocol;
using Timer = System.Timers.Timer;

namespace SyNet
{
  /// <summary>
  ///   Singleton NodeManager class to manage all nodes in the system
  /// </summary>
  [XmlRoot("NodeInformation")]
  [XmlInclude(typeof(DeviceXBee))]
  [XmlInclude(typeof(SyNetNodeList))]
  public sealed class NodeManager
  {

    ////////////////////////////////////////////////////////////////////////////
    #region Member Variables

    private static NodeManager s_instance;
    private static readonly object s_objInstanceLock = new object();

    private SyNetNodeList m_nodeList;
    private readonly List<Device> m_lstNodesToInvestigate;

    private readonly object m_lockNewNodeList = new object();

    private bool m_isRunning = true;

    private readonly ManualResetEvent m_mrevMsgWorkNeededEvent;

    private readonly Timer m_timerNodeRefresh;
    private bool m_bIsTimerElapsed = false;
    private bool m_bIsPeriodicCheckEnabled = true;

    // Used for generating randon device IDs
    private readonly Random m_rand = new Random();

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Constructors

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public NodeManager()
    {
      lock(s_objInstanceLock)
      {
        if (s_instance == null)
        {
          s_instance = this;
        }
      }

      m_nodeList = new SyNetNodeList();
      m_mrevMsgWorkNeededEvent = new ManualResetEvent(false);
      m_lstNodesToInvestigate = new List<Device>();
      m_timerNodeRefresh = new Timer();
      m_timerNodeRefresh.Elapsed += TimerEvent;
      m_timerNodeRefresh.Interval = 120000; // 2 minutes
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Properties

    /// <summary>
    ///   Gets an instance of the message processor
    /// </summary>
    [XmlIgnore]
    public static NodeManager Instance
    {
      get
      {
        lock (s_objInstanceLock)
        {
          if (s_instance == null)
          {
            s_instance = new NodeManager();
          }
          return s_instance;
        }
      }
    }

    /// <summary>
    ///   Gets or sets whether the periodic node check occurs
    /// </summary>
    [XmlAttribute]
    public bool PeriodicCheck
    {
      get { return m_bIsPeriodicCheckEnabled; }
      set { m_bIsPeriodicCheckEnabled = value; }
    }

    /// <summary>
    ///   Gets a list of nodes
    /// </summary>
    [XmlArray("NodeList")]
    [XmlArrayItem("Device")]
    public SyNetNodeList SyNetNodeList
    {
      get { return m_nodeList; }
      set { m_nodeList = value; }
    }

    /// <summary>
    ///   List of devices
    /// </summary>
    [XmlIgnore]
    public List<Device> NodeList
    {
      get { return m_nodeList.NodeList; }
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Methods

    /// <summary>
    ///   Generate a random 64-bit ID
    /// </summary>
    /// <returns></returns>
    public UInt64 GenerateRandomID()
    {
      byte[] bytes = new byte[8];
      m_rand.NextBytes(bytes);
      return BitConverter.ToUInt64(bytes, 0);
    }

    /// <summary>
    ///   Refresh the available nodes
    /// </summary>
    public void RefreshNodes()
    {
      foreach (Device node in m_nodeList)
      {
        node.Refresh();
      }

      MsgDispatcher.Instance.SendMsg(new MsgSyNetDeviceStatusRequest(
                                            DeviceXBee.BroadcastDevice(),
                                            EsnAPIDeviceStatusRequest.STATUS_REQUEST));
      //MsgProc.SendMsg(new MsgZigbeeATCmd(MsgZigbee.EsnXbeeAtCmd.NODE_DISCOVER));
      Debug.WriteLine(string.Format("[DBG] Refreshing Nodes {0}", DateTime.Now));
    }

    /// <summary>
    ///   Refresh the available nodes
    /// </summary>
    public static void RefreshNodeInfos()
    {
      MsgDispatcher.Instance.SendMsg(new MsgSyNetDeviceStatusRequest(
                                            DeviceXBee.BroadcastDevice(),
                                            EsnAPIDeviceStatusRequest.INFO_REQUEST));
      //MsgProc.SendMsg(new MsgZigbeeATCmd(MsgZigbee.EsnXbeeAtCmd.NODE_DISCOVER));
      Debug.WriteLine(string.Format("[DBG] Refreshing Node Info {0}", DateTime.Now));
    }


    /// <summary>
    ///   Remove a node from the node list
    ///   This is not perminant if the node is active, it will be discovered
    ///   again
    /// </summary>
    /// <param name="p_node"></param>
    public void RemoveNode(Device p_node)
    {
      m_nodeList.Remove(p_node);
      OnNodeRemoved(p_node.ID);
    }

    /// <summary>
    ///   Add a node from a zigbee RX message
    /// </summary>
    /// <param name="p_msg"></param>
    /// <returns></returns>
    public DeviceXBee AddNode(MsgZigbeeRx p_msg)
    {
      return AddNode(p_msg.My64BitAddress, p_msg.My16BitAddress);
    }

    internal void AddNodeFromATCmdResponse(MsgZigbeeATCmdResponse p_msgATCmdResponse)
    {
      const int OFFSET = MsgZigbeeATCmdResponse.PAYLOAD_OFFSET;
      UInt64 my64 = EBitConverter.ToUInt64(p_msgATCmdResponse.MsgData, OFFSET + 2);
      UInt16 my16 = EBitConverter.ToUInt16(p_msgATCmdResponse.MsgData, OFFSET);
      AddNode(my64, my16);
    }

    /// <summary>
    ///   Add a node from a serialnumber and network address
    /// </summary>
    /// <param name="p_addr"></param>
    /// <param name="p_addr16"></param>
    /// <returns></returns>
    public DeviceXBee AddNode(UInt64 p_addr, UInt16 p_addr16)
    {
      DeviceXBee node = m_nodeList.GetNodeFromMy64Addr(p_addr) as DeviceXBee;

      // If we have not seen this node, create it and mark it for further investigation
      if (node == null)
      {
        node = new DeviceXBee
                 {
                   SerialNumber = p_addr,
                   NetworkAddress = p_addr16
                 };
        m_nodeList.Add(node);
        m_mrevMsgWorkNeededEvent.Set();
        OnNodeDiscovered(node.ID, false);
      }

      return node;
    }

    /// <summary>
    ///   Add a node to the new node list so it is investigated
    /// </summary>
    /// <param name="p_node"></param>
    public void AddNodeForInvestigation(Device p_node)
    {
      lock (m_lockNewNodeList)
      {
        m_lstNodesToInvestigate.Add(p_node);
      }
      m_mrevMsgWorkNeededEvent.Set();
    }

    /// <summary>
    ///   Get a node from the list or add it if not there
    /// </summary>
    /// <param name="p_nNodeID"></param>
    /// <returns></returns>
    public Device GetNode(UInt64 p_nNodeID)
    {
      return m_nodeList.GetNodeFromMy64Addr(p_nNodeID);
    }

    /// <summary>
    ///   Update the node with message
    /// </summary>
    /// <param name="p_msg"></param>
    public void UpdateNodeWithMsg(MsgSyNetRx p_msg)
    {
      Device node = GetNode(p_msg.My64BitAddress) ?? AddNode(p_msg);

      node.HandleMessage(p_msg);

      if (p_msg is MsgSyNetDeviceStatus)
      {
        MsgSyNetDeviceStatus rxStatusMsg = p_msg as MsgSyNetDeviceStatus;
        if (rxStatusMsg.DeviceStatus == EsnAPIDeviceStatusValues.HW_RESET)
        {
          // let's schedule read of detailed info so we can see if code changed which caused reboot
          node.NeedLightInvestigation = true;
        }
        else if ((rxStatusMsg.DeviceStatus == EsnAPIDeviceStatusValues.ACTIVE) ||
                 (rxStatusMsg.DeviceStatus == EsnAPIDeviceStatusValues.INFO))
        {
          if ((!node.HaveFullCatalog || node.NeedLightInvestigation) && !node.BeingInvestigated)
          {
            AddNodeForInvestigation(node);
          }
        }
      }
    }

    #endregion

    #region Thread Control Methods

    /// <summary>
    ///   Main thread function
    /// </summary>
    public void ManageNodesThread()
    {
      // First time, we want to refresh the info to make sure
      // that the version numbers match up
      RefreshNodeInfos();

      // Start the timer
      m_timerNodeRefresh.Start();

      while (m_isRunning)
      {
        bool bDidAnythingThisPass = false;

        // Task (2of3) - See if there are new nodes to be processed
        if (m_lstNodesToInvestigate.Count > 0)
        {
          InvestigateNewNode(m_lstNodesToInvestigate[0]);
          lock (m_lockNewNodeList)
          {
            m_lstNodesToInvestigate.RemoveAt(0);
          }
          bDidAnythingThisPass = true;
        }

        // Task (3of3) - poll our network on timer expiration
        // See if the timer has run out
        if (m_bIsTimerElapsed)
        {
          // is GUI allowing us to poll?
          if (m_bIsPeriodicCheckEnabled)
          {
            bDidAnythingThisPass = true;
            RefreshNodes();
          }
          // reset our expired indicator
          m_bIsTimerElapsed = false;
          // Wait two minutes and then fire again
          m_timerNodeRefresh.Start();
        }

        if (!bDidAnythingThisPass)
        {
          m_mrevMsgWorkNeededEvent.Reset();
        }

        if (m_isRunning)
        {
          // Wait if necessary
          m_mrevMsgWorkNeededEvent.WaitOne();
        }
      }
    }

    /// <summary>
    ///   Stop the router from routing
    /// </summary>
    public void Stop()
    {
      m_isRunning = false;
      m_mrevMsgWorkNeededEvent.Set();
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Thread Utility Methods

    /// <summary>
    ///   Event for when timer is triggered
    /// </summary>
    /// <param name="p_objSource"></param>
    /// <param name="p_evElapsedEventArgs"></param>
    private void TimerEvent(object p_objSource, ElapsedEventArgs p_evElapsedEventArgs)
    {
      m_bIsTimerElapsed = true;
      m_mrevMsgWorkNeededEvent.Set();
    }

    /// <summary>
    ///   Investigate information about a new node
    /// </summary>
    /// <param name="p_node"></param>
    private void InvestigateNewNode(Device p_node)
    {
      int nRetryCount;
      const int MAX_RETRIES = 5;
      const int MAX_WAIT_IN_SECONDS = 2;
      int nNumFunctions = 0;
      List<byte> nNumParamsLst = new List<byte>();
      MsgDispatcher mdDispatcher = MsgDispatcher.Instance;
      bool bFailedDataRead = false;

      p_node.BeingInvestigated = true;

      // Register a new callback so we get notified
      MsgDispatcher.CallbackStruct cs =
        mdDispatcher.RegisterCallback(p_node.ID);

      // Investigate device info
      bool bReceivedInfo = false;
      cs.Reset();
      for (nRetryCount = 0; nRetryCount < MAX_RETRIES; nRetryCount++)
      {
        Debug.WriteLine("[InvestigateNewNode] Sending info request");
        mdDispatcher.SendMsg(new MsgSyNetDeviceStatusRequest(p_node as DeviceXBee,
                                                             EsnAPIDeviceStatusRequest.INFO_REQUEST));
        if (cs.CallbackEvent.WaitOne(TimeSpan.FromSeconds(MAX_WAIT_IN_SECONDS), true))
        {
          while (cs.MsgCount > 0)
          {
            Msg msg = cs.Pop();
            if (msg is MsgSyNetDeviceStatus)
            {
              Debug.WriteLine("[InvestigateNewNode] Status info received");
              bReceivedInfo = true;
              break;
            }
          }
          if (bReceivedInfo)
          {
            break;
          }
        }
        else
        {
          Debug.WriteLine("[ERR] Device Info timeout");
        }
      }
      if (!bReceivedInfo)
      {
        bFailedDataRead = true;
      }

      if (p_node.NeedLightInvestigation)
      {
        bFailedDataRead = true; // single that we are done investigating
      }

      if (!bFailedDataRead)
      {
        // Investigate device catalog
        bReceivedInfo = false;
        cs.Reset();
        for (nRetryCount = 0; nRetryCount < MAX_RETRIES; nRetryCount++)
        {
          Debug.WriteLine("[InvestigateNewNode] Sending catalog request");
          mdDispatcher.SendMsg(new MsgSyNetCatalogRequest(p_node as DeviceXBee));
          if (cs.CallbackEvent.WaitOne(TimeSpan.FromSeconds(MAX_WAIT_IN_SECONDS), true))
          {
            while (cs.MsgCount > 0)
            {
              Msg msg = cs.Pop();
              if (msg is MsgSyNetCatalogResponse &&
                  ((MsgSyNetCatalogResponse)msg).EntryID == 0x00)
              {
                nNumFunctions = ((MsgSyNetCatalogResponse)msg).TotalEntries;
                Debug.WriteLine("[InvestigateNewNode] Catalog info received");
                bReceivedInfo = true;
                break;
              }
            }
            if (bReceivedInfo)
            {
              break;
            }
          }
          else
          {
            Debug.WriteLine("[ERR] Catalog Entry Count timeout");
          }
        }
        if (!bReceivedInfo)
        {
          bFailedDataRead = true;
        }

        if (!bFailedDataRead)
        {
          // Investigate each catalog entry
          int nFuntionsNeeded = nNumFunctions;
          for (byte nFunctionID = 1; nFunctionID <= nNumFunctions; nFunctionID++)
          {
            cs.Reset();
            bool bFunctionReceived = false;
            for (nRetryCount = 0; nRetryCount < MAX_RETRIES; nRetryCount++)
            {
              Debug.WriteLine(string.Format("[InvestigateNewNode] Sending catalog request for {0}", nFunctionID));
              mdDispatcher.SendMsg(new MsgSyNetCatalogRequest(p_node as DeviceXBee, nFunctionID));
              if (cs.CallbackEvent.WaitOne(TimeSpan.FromSeconds(MAX_WAIT_IN_SECONDS), true))
              {
                while (cs.MsgCount > 0)
                {
                  Msg msg = cs.Pop();
                  if (msg is MsgSyNetCatalogResponse &&
                    ((MsgSyNetCatalogResponse)msg).EntryID == nFunctionID)
                  {
                    nNumParamsLst.Add(((MsgSyNetCatalogResponse)msg).NumParams);
                    Debug.WriteLine(string.Format("[InvestigateNewNode] Catalog info received - {0}", nFunctionID));
                    nFuntionsNeeded--;
                    bFunctionReceived = true;
                    break;
                  }
                  Debug.WriteLine(string.Format("[ERR] Mis-matched catalog function response - Func ID expected={0}", nFunctionID));
                  Debug.WriteLine(string.Format("    {0}", msg.GetType()));
                  Debug.WriteLine(string.Format("    {0}", ((MsgSyNetCatalogResponse)msg).EntryID) );
                }
                if (bFunctionReceived)
                {
                  break;
                }
              }
              else
              {
                Debug.WriteLine(string.Format("[ERR] Catalog entry timeout - {0}", nFunctionID));
              }
            }
          }
          if (nFuntionsNeeded > 0)
          {
            bFailedDataRead = true;
          }

          if (!bFailedDataRead)
          {
            // Investigate function parameters
            for (byte nFunctionID = 1; nFunctionID <= nNumFunctions; nFunctionID++)
            {
              int nParamsNeeded = nNumParamsLst[nFunctionID - 1];
              for (byte nParamID = 0; nParamID <= nNumParamsLst[nFunctionID - 1]; nParamID++)
              {
                bool bParamFound = false;

                // The zero param is the return type and it's not needed if
                // the function returns void
                if (nParamID == 0 &&
                    p_node.GetFunction(nFunctionID).ReturnType == EsnDataTypes.VOID)
                {
                  nParamsNeeded--;
                  continue;
                }

                cs.Reset();
                for (nRetryCount = 0; nRetryCount < MAX_RETRIES; nRetryCount++)
                {
                  Debug.WriteLine(string.Format("[InvestigateNewNode] Sending param info for {0} - {1}", nFunctionID, nParamID));
                  mdDispatcher.SendMsg(new MsgSyNetParameterRequest(p_node as DeviceXBee, nFunctionID, nParamID));
                  if (cs.CallbackEvent.WaitOne(TimeSpan.FromSeconds(MAX_WAIT_IN_SECONDS), true))
                  {
                    while (cs.MsgCount > 0)
                    {
                      Msg msg = cs.Pop();
                      if (msg is MsgSyNetParameterResponse &&
                          ((MsgSyNetParameterResponse)msg).FunctionID == nFunctionID &&
                          ((MsgSyNetParameterResponse)msg).ParamID == nParamID)
                      {
                        nParamsNeeded--;
                        bParamFound = true;
                        break;
                      }
                    }

                    if (bParamFound)
                    {
                      break;
                    }
                  }
                  else
                  {
                    Debug.WriteLine(string.Format("[ERR] Param info timeout - {0}-{1}", nFunctionID, nParamID));
                  }
                }
              }
              if (nParamsNeeded > 0)
              {
                bFailedDataRead = true;
              }
            }
          }
        }
      }

      // Remove the callback
      mdDispatcher.RemoveCallback(cs);

      if (p_node.NeedLightInvestigation)
      {
        // re-read INFO for node on hardware reset (to see if code changed)
        p_node.NeedLightInvestigation = false;
        Debug.WriteLine(string.Format("{0}: Completed Light-Investigate for Node hash=0x{1}", p_node.DeviceName,
                                      p_node.GetHashCode().ToString("X")));
      }
      else if (!bFailedDataRead)
      {
        Debug.WriteLine(string.Format("{0}: Have Catalog for Node hash=0x{1}", p_node.DeviceName,
                                      p_node.GetHashCode().ToString("X")));
        p_node.HaveFullCatalog = true;
        p_node.HaveFullParameters = true;
        OnNodeDiscovered(p_node.ID, true);
      }

      p_node.BeingInvestigated = false;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    #region Node-list Notification Events and assoc. methods
    private void OnNodeDiscovered(UInt64 p_deviceID, bool p_fullParameters)
    {
      if (NodeDiscovered != null)
      {
        NodeDiscovered(this, new NodeDiscoveredEventArgs(p_deviceID, p_fullParameters));
      }
    }

    internal delegate void NodeDiscoveredEventHandler(object p_sender,
                                                       NodeDiscoveredEventArgs p_args);

    internal event NodeDiscoveredEventHandler NodeDiscovered;


    private void OnNodeRemoved(UInt64 p_deviceID)
    {
      if (NodeRemoved != null)
      {
        NodeRemoved(this, new NodeDiscoveredEventArgs(p_deviceID, false));
      }
    }

    internal delegate void NodeRemovedEventHandler(object p_sender,
                                                       NodeDiscoveredEventArgs p_args);

    internal event NodeRemovedEventHandler NodeRemoved;

  }

    #endregion
}