using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using SyNet.MessageTypes;

namespace SyNet
{

  /// <summary>
  ///   Singleton Message Dispatcher class. Handles all sending and receiving
  ///   of messages in the system
  /// </summary>
  public sealed class MsgDispatcher
  {
    /// <summary>
    ///   Structure of a registered callback event
    /// </summary>
    public struct CallbackStruct
    {
      private readonly UInt64? m_My64BitAddress;
      private readonly UInt16? m_My16BitAddress;
      private readonly byte? m_frameID;
      private readonly AutoResetEvent m_callbackEvent;
      private readonly MsgZigbee.EsnXbeeAPI m_api;
      private bool m_response;
      private readonly List<Msg> m_responseMsgs;

      /// <summary>
      ///   Serial Number to match
      /// </summary>
      public UInt64? SerialNumber
      {
        get { return m_My64BitAddress; }
      }

      /// <summary>
      ///   Network address to match
      /// </summary>
      public UInt16? NetworkAddress
      {
        get { return m_My16BitAddress; }
      }

      /// <summary>
      ///   Frame ID to match
      /// </summary>
      public byte? FrameID
      {
        get { return m_frameID; }
      }

      /// <summary>
      ///   Callback event to wait on
      /// </summary>
      public AutoResetEvent CallbackEvent
      {
        get { return m_callbackEvent; }
      }

      /// <summary>
      ///   XBee API to match
      /// </summary>
      public MsgZigbee.EsnXbeeAPI API
      {
        get { return m_api; }
      }

      /// <summary>
      ///   List of response messages to this callback
      /// </summary>
      public List<Msg> ResponseMsgs
      {
        get { return m_responseMsgs; }
      }

      /// <summary>
      ///   Returns true if a response was received
      /// </summary>
      public bool Response
      {
        get { return m_response; }
        set { m_response = value; }
      }

      /// <summary>
      ///   Initialization constructor
      /// </summary>
      /// <param name="p_my64"></param>
      /// <param name="p_my16"></param>
      /// <param name="p_frameID"></param>
      /// <param name="p_api"></param>
      /// <param name="p_callback"></param>
      public CallbackStruct( UInt64? p_my64,
                             UInt16? p_my16,
                             byte? p_frameID,
                             MsgZigbee.EsnXbeeAPI p_api,
                             AutoResetEvent p_callback )
      {
        m_My64BitAddress = p_my64;
        m_My16BitAddress = p_my16;
        m_frameID = p_frameID;
        m_callbackEvent = p_callback;
        m_api = p_api;
        m_responseMsgs = new List<Msg>();
        m_response = false;
      }

      /// <summary>
      ///   Pop a response message and return it
      /// </summary>
      /// <returns></returns>
      public Msg Pop()
      {
        Msg msg = null;
        if (m_responseMsgs.Count > 0)
        {
          msg = m_responseMsgs[0];
          m_responseMsgs.RemoveAt(0);
        }
        return msg;
      }

      /// <summary>
      ///   Returns the count of response messages
      /// </summary>
      public int MsgCount
      {
        get
        {
          return m_responseMsgs.Count;
        }
      }

      /// <summary>
      ///   Reset the callback to the starting state
      /// </summary>
      public void Reset()
      {
        m_response = false;
        m_responseMsgs.Clear();
      }
    }

    ////////////////////////////////////////////////////////////////////////////
    #region Member Variables

    readonly SyNetSettings m_sySettings = SyNetSettings.Instance;

    private static MsgDispatcher s_mdInstance = null; // null to support/allow deferred inflation
    private static readonly object s_objInstanceLock = new object();

    private readonly NodeManager m_nmManagedNodeList = NodeManager.Instance;

    private readonly ManualResetEvent m_mrevMsgWorkNeededEvent = new ManualResetEvent(false);
    private readonly List<CallbackStruct> m_callbackList = new List<CallbackStruct>();
    private readonly Object m_callbackLock = new object();

    private readonly Queue<Msg> m_msgQueueIn = new Queue<Msg>();
    private readonly Queue<Msg> m_msgQueueOut = new Queue<Msg>();

    private readonly object m_lockQueueIn = new object();
    private readonly object m_lockQueueOut = new object();

    private readonly ObservableCollection<Msg> m_msgLog = new ObservableCollection<Msg>();
    private DeviceXBeeSerial m_xbee = null;

    private SyslogSender m_slSyslogger = null;


    private bool m_isRunning = true;

    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Constructors

    /// <summary>
    ///   Default constructor
    /// </summary>
    public MsgDispatcher()
    {
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///   Gets an instance of the message processor
    /// </summary>
    public static MsgDispatcher Instance
    {
      get
      {
        lock (s_objInstanceLock)
        {
          if (s_mdInstance == null)
          {
            s_mdInstance = new MsgDispatcher();
          }
          return s_mdInstance;
        }
      }
    }

    /// <summary>
    ///   Gets or sets the device for the processor
    /// </summary>
    public DeviceXBeeSerial Device
    {
      get { return m_xbee; }
      set { m_xbee = value; }
    }

    /// <summary>
    ///   Gets the message list held by the processor
    /// </summary>
    public ObservableCollection<Msg> MsgList
    {
      get { return m_msgLog; }
    }

    ////////////////////////////////////////////////////////////////////////////

    #region Public Functions

    /// <summary>
    ///   Started as a thread and loops through looking for new messages
    /// </summary>
    public void ProcessMessagesThread()
    {
      Debug.WriteLine("[DBG] SyNetXBeeMsgProcessor.ProcessMessages - Entry");
      while (m_isRunning)
      {
        Msg rxMsg = null;
        bool bDequeuedAnything = false;

        //
        // Incoming Messages
        //
        lock (m_lockQueueIn)
        {
          if (m_msgQueueIn.Count > 0)
          {
            rxMsg = m_msgQueueIn.Dequeue();
            bDequeuedAnything = true;
          }
        }

        if (rxMsg != null)
        {
          // If incoming, process
          if (rxMsg.Direction == Msg.EsnFrameDirection.RCVD)
          {
            if (rxMsg is MsgSyNetRx)
            {
              m_nmManagedNodeList.UpdateNodeWithMsg(rxMsg as MsgSyNetRx);
            }
            else if (rxMsg is MsgZigbeeATCmdResponse)
            {
              MsgZigbeeATCmdResponse rxAtRespMsg = rxMsg as MsgZigbeeATCmdResponse;
              // was ProcessMessageAtCmdResponse( rxMsg as MsgZigbeeATCmdResponse );
              if (rxAtRespMsg.ATCmd == MsgZigbee.EsnXbeeAtCmd.NODE_DISCOVER)
              {
                m_nmManagedNodeList.AddNodeFromATCmdResponse(rxMsg as MsgZigbeeATCmdResponse);
              }
            }
          }
          else
          {
            Debug.WriteLine("[ERR] Dispatcher.ProcessMessages - Message driving in the wrong lane!");
          }

          NotifyCallbacks(rxMsg); // FIXME this surely does need to not be here....
        }

        //
        // Outgoing Messages
        //
        Msg txMsg = null;
        lock (m_lockQueueOut)
        {
          if (m_msgQueueOut.Count > 0)
          {
            txMsg = m_msgQueueOut.Dequeue();
            bDequeuedAnything = true;
          }
        }

        if (txMsg != null)
        {
          // If incoming, process
          if (txMsg.Direction == Msg.EsnFrameDirection.SENT)
          {
            Debug.Assert(m_xbee != null, "Object is not yet initialized!");
            m_xbee.WriteXBeeFrame(txMsg);
          }
          else
          {
            Debug.WriteLine("[ERR] Dispatcher.ProcessMessages - Message driving in the wrong lane!");
          }
        }


        // List is empty, wait
        if (!bDequeuedAnything)
        {
          m_mrevMsgWorkNeededEvent.Reset();
        }

        // Wait if necessary
        if (m_isRunning)
        {
          m_mrevMsgWorkNeededEvent.WaitOne();
        }
      }
      Debug.WriteLine("[DBG] SyNetXBeeMsgProcessor.ProcessMessages - Exit");
    }

    /// <summary>
    ///   Stop Processing
    /// </summary>
    public void Stop()
    {
      m_isRunning = false;
      m_mrevMsgWorkNeededEvent.Set();
    }

    /// <summary>
    ///   Clear the message log
    /// </summary>
    public void Clear()
    {
      m_msgLog.Clear();
    }

    /// <summary>
    ///   Add a new callback
    /// </summary>
    /// <param name="p_my64BitAddress"></param>
    /// <param name="p_my16BitAddress"></param>
    /// <param name="p_frameID"></param>
    /// <param name="p_api"></param>
    public CallbackStruct RegisterCallback(
      UInt64? p_my64BitAddress,
      UInt16? p_my16BitAddress,
      byte? p_frameID,
      MsgZigbee.EsnXbeeAPI p_api )
    {
      CallbackStruct c = new CallbackStruct(p_my64BitAddress,
                                            p_my16BitAddress,
                                            p_frameID,
                                            p_api,
                                            new AutoResetEvent(false));
      lock (m_callbackLock)
      {
        m_callbackList.Add(c);
      }
      return c;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_api"></param>
    /// <returns></returns>
    public CallbackStruct RegisterCallback( MsgZigbee.EsnXbeeAPI p_api )
    {
      return RegisterCallback(null, null, null, p_api);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_64BitAddress"></param>
    /// <returns></returns>
    public CallbackStruct RegisterCallback( UInt64 p_64BitAddress )
    {
      return RegisterCallback(p_64BitAddress, null, null, MsgZigbee.EsnXbeeAPI.INVALID);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_msg"></param>
    private void NotifyCallbacks( Msg p_msg )
    {
      lock (m_callbackLock)
      {
        foreach (CallbackStruct cb in m_callbackList)
        {
          bool retVal = true;
          // API
          if (cb.API != MsgZigbee.EsnXbeeAPI.INVALID)
          {
            retVal = false;
          }

          // Frame ID
          if (cb.FrameID != null &&
              cb.FrameID != p_msg.FrameID)
          {
            retVal = false;
          }

          if (p_msg is MsgZigbeeRx)
          {
            // 64 bit addr
            if (cb.SerialNumber != null &&
                cb.SerialNumber != ((MsgZigbeeRx)p_msg).My64BitAddress)
            {
              retVal = false;
            }
            // 16 bit addr
            if (cb.NetworkAddress != null &&
                cb.NetworkAddress != ((MsgZigbeeRx)p_msg).My16BitAddress)
            {
              retVal = false;
            }
          }
          // If all good, notify the event
          if (retVal)
          {
            cb.ResponseMsgs.Add(p_msg);
            cb.CallbackEvent.Set();
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_callBack"></param>
    public void RemoveCallback( CallbackStruct p_callBack )
    {
      lock (m_callbackLock)
      {
        m_callbackList.Remove(p_callBack);
      }
    }

    /// <summary>
    ///   Add an incoming message to the queue
    /// </summary>
    /// <param name="p_msg"></param>
    public void ReceiveMsg( Msg p_msg )
    {
      // post our incoming message to the log
      m_msgLog.Add(p_msg);
      // FIXME syslogging should be conditional upon user settings (as should the server,type, and severity)
      // write our message to our syslog host as well
      WriteMsgToSyslog("Rx", p_msg);
      lock (m_lockQueueIn)
      {
        m_msgQueueIn.Enqueue(p_msg);
      }
      m_mrevMsgWorkNeededEvent.Set();
    }

    private void WriteMsgToSyslog( string p_strDir, Msg p_msg )
    {
      // if syslogging is requested
      if (m_sySettings.SyslogEnabled)
      {
        // if we don't have a syslog object, yet, create it
        if (m_slSyslogger == null)
        {
          string strHostName = m_sySettings.SyslogTargetHost;
          string strFacility = m_sySettings.SyslogFacility;

          //private SyslogSender m_slSyslogger = new SyslogSender( "192.168.100.34", SyslogSender.EFacility.Local4 );
          //private SyslogSender m_slSyslogger = new SyslogSender( "scm.home", SyslogSender.EFacility.Local4 );
          m_slSyslogger = new SyslogSender(strHostName, SyslogSender.GetFacilityCodeForName(strFacility));
        }
      }
      else
      {
        // logging no longer desired (or never was)

        // if we've been using a loggin object, kill it
        if (m_slSyslogger != null)
        {
          // setting to null will return the object to garbage collection
          m_slSyslogger = null;
        }
      }

      // after all of this... if we are logging then log a message!
      if (m_slSyslogger != null)
      {
        string strLoggedMsg = p_msg.ByteString;
        string strLogText = string.Format("Xbee {0} ({1}) {2}", p_strDir, strLoggedMsg.Length / 3, strLoggedMsg);
        m_slSyslogger.Send(SyslogSender.ESeverityType.Informational, p_msg.Timestamp, strLogText);
      }
    }

    /// <summary>
    ///   Add an outgoing message to the queue
    /// </summary>
    /// <param name="p_msg"></param>
    public void SendMsg( Msg p_msg )
    {
      // Finalize if necessary
      if (!p_msg.IsFinalized)
      {
        p_msg.FinalizeFrame();
      }
      m_msgLog.Add(p_msg);
      // FIXME syslogging should be conditional upon user settings (as should the server,type, and severity)
      // write our message to our syslog host as well
      WriteMsgToSyslog("Tx", p_msg);

      lock (m_lockQueueOut)
      {
        m_msgQueueOut.Enqueue(p_msg);
      }

      m_mrevMsgWorkNeededEvent.Set();
    }

    /// <summary>
    ///   Send a message and wait for a response
    /// </summary>
    /// <param name="p_msg"></param>
    /// <param name="p_waitTime"></param>
    /// <param name="p_cs"></param>
    /// <returns></returns>
    public bool SendMsgWithResponse( Msg p_msg,
                                    TimeSpan p_waitTime,
                                    CallbackStruct p_cs )
    {
      SendMsg(p_msg);
      if (p_cs.CallbackEvent.WaitOne(p_waitTime, true))
      {
        p_cs.Response = true;
      }
      else
      {
        Debug.WriteLine(string.Format("[ERR] No response - {0}", p_msg));
      }
      return p_cs.Response;
    }

    /// <summary>
    ///   Send a message and wait for a response for a particular
    ///   api 
    /// </summary>
    /// <param name="p_msg"></param>
    /// <param name="p_waitTime"></param>
    /// <param name="p_api"></param>
    /// <returns></returns>
    public bool SendMsgWithResponse( Msg p_msg,
                                    TimeSpan p_waitTime,
                                    MsgZigbee.EsnXbeeAPI p_api )
    {
      CallbackStruct cs = RegisterCallback(p_api);
      bool retVal = SendMsgWithResponse(p_msg, p_waitTime, cs);
      RemoveCallback(cs);
      return retVal;
    }

    /// <summary>
    ///   Send a message and wait for a response from a 64bit addr
    /// </summary>
    /// <param name="p_msg"></param>
    /// <param name="p_waitTime"></param>
    /// <param name="p_my64Bitaddr"></param>
    /// <returns></returns>
    public bool SendMsgWithResponse( Msg p_msg,
                                    TimeSpan p_waitTime,
                                    UInt64 p_my64Bitaddr )
    {
      CallbackStruct cs = RegisterCallback(p_my64Bitaddr);
      bool retVal = SendMsgWithResponse(p_msg, p_waitTime, cs);
      RemoveCallback(cs);
      return retVal;
    }

    #endregion

    ////////////////////////////////////////////////////////////////////////////
  }
}