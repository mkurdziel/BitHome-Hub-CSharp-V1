using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SyNet
{
  internal class SyslogSender
  {
    #region Public Enumerations

    public enum ESeverityType
    {
      Emergency = 0,
      Alert = 1,
      Critical = 2,
      Error = 3,
      Warning = 4,
      Notice = 5,
      Informational = 6,
      Debug = 7
    }


    public enum EFacility : int
    {
      /*
       Notes from RFC: 
          ftp://ftp.rfc-editor.org/in-notes/rfc3164.txt
	
       Note 1 - Various operating systems have been found to utilize
           Facilities 4, 10, 13 and 14 for security/authorization,
           audit, and alert messages which seem to be similar.
       Note 2 - Various operating systems have been found to utilize
           both Facilities 9 and 15 for clock (cron/at) messages.
      */
      Kern = 0, // 0 kernel messages
      User = 1, // 1 generic user-level messages
      Mail = 2, // 2 mail subsystem
      Daemon = 3, // 3 other system daemons
      Auth = 4, // 4 security/authorization messages (DEPRECATED Use LOG_AUTHPRIV instead)
      Syslog = 5, // 5 messages generated internally by syslogd
      LPR = 6, // 6 line printer subsystem
      News = 7, // 7 USENET news subsystem
      UUCP = 8, // 8 UUCP subsystem
      Cron = 9, // 9 clock daemon (cron and at) 9+15 do the same depending on OS
      AuthPriv = 10, // 10 security/authorization messages (private)
      FTP = 11, // 11 FTP
      NTP = 12, // 12 Network Time Protocol
      Audit = 13, // 13 log audit		
      Audit2 = 14, // 14 log audit		
      CRON2 = 15, // 15 clock daemon (cron and at) 9+15 do the same depending on OS
      Local0 = 16, // 16 reserved for local use
      Local1 = 17, // 17 reserved for local use
      Local2 = 18, // 18 reserved for local use
      Local3 = 19, // 19 reserved for local use
      Local4 = 20, // 20 reserved for local use
      Local5 = 21, // 21 reserved for local use
      Local6 = 22, // 22 reserved for local use
      Local7 = 23, // 23 reserved for local use
      Unknown
    }

    #endregion

    #region Class Static Data (common to all instances of this class)

    private static UdpClient s_ucUdp;
    private static ASCIIEncoding s_aeAscii = null;

    #endregion

    #region Private Member Data (unique to each class instance)

    private readonly string m_strMachineName;

    private readonly int m_nFacilitiyId;
    private string m_strSyslogLocalHostIpAddress;
    private string m_strSyslogRemoteHostIpAddress;

    #endregion

    #region Construction

    public SyslogSender(string p_strSyslogServer, EFacility p_eFacility)
    {
      // if this is the first instance, create our static data
      if (s_aeAscii == null)
      {
        s_aeAscii = new ASCIIEncoding();
      }

      m_nFacilitiyId = (int) p_eFacility;
      m_strMachineName = string.Format("{0} ", Dns.GetHostName());
      m_strSyslogLocalHostIpAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
      m_strSyslogRemoteHostIpAddress = Dns.GetHostEntry(p_strSyslogServer).AddressList[0].ToString();
    }

    #endregion

    #region Log-Message Interface

    /// <summary>
    /// Write a syslog message to remote daemon (time stamped with time of this method call)
    /// </summary>
    /// <param name="p_eSeverity">on of our set of known severities</param>
    /// <param name="p_strMsgBody">the message to be logged</param>
    public void Send(ESeverityType p_eSeverity, string p_strMsgBody)
    {
      Send(p_eSeverity, DateTime.Now, p_strMsgBody);
    }


    public static EFacility GetFacilityCodeForName(string p_strFacilityName)
    {
      Dictionary<string, EFacility> dctFacilityValueByName = new Dictionary<string, EFacility>();
      dctFacilityValueByName["local0"] = EFacility.Local0;
      dctFacilityValueByName["local1"] = EFacility.Local1;
      dctFacilityValueByName["local2"] = EFacility.Local2;
      dctFacilityValueByName["local3"] = EFacility.Local3;
      dctFacilityValueByName["local4"] = EFacility.Local4;
      dctFacilityValueByName["local5"] = EFacility.Local5;
      dctFacilityValueByName["local6"] = EFacility.Local6;
      dctFacilityValueByName["local7"] = EFacility.Local7;

      EFacility eFacility = EFacility.Unknown;
      string strFacilityName = p_strFacilityName.ToLower();
      if (dctFacilityValueByName.ContainsKey(strFacilityName))
      {
        eFacility = dctFacilityValueByName[strFacilityName];
      }
      return eFacility;
    }

    /// <summary>
    /// Write a syslog message to remote daemon
    /// </summary>
    /// <param name="p_eSeverity">one of our set of known severities</param>
    /// <param name="p_dtMessageTimeStamp">the log time-stamp</param>
    /// <param name="p_strMsgBody">the message to be logged</param>
    public void Send(ESeverityType p_eSeverity, DateTime p_dtMessageTimeStamp, string p_strMsgBody)
    {
      try
      {
        s_ucUdp = new UdpClient(m_strSyslogRemoteHostIpAddress, 514);
        // facility *8 
        // + Severity
        // == Priority Type as a number
        int nPriorityNumber = CalculateSyslogPriority(m_nFacilitiyId, p_eSeverity);

        string[] strParams = {
                               string.Format("<{0}>", nPriorityNumber),
                               p_dtMessageTimeStamp.ToString("MMM dd HH:mm:ss "),
                               m_strMachineName, p_strMsgBody
                             };

        byte[] byRawMsgAr = s_aeAscii.GetBytes(string.Concat(strParams));
        s_ucUdp.Send(byRawMsgAr, byRawMsgAr.Length);
      }
      catch (Exception e)
      {
        if (e != null)
        {
          // kill compiler warnings...
        }
      }
      finally
      {
        if (s_ucUdp != null)
        {
          s_ucUdp.Close();
          s_ucUdp = null;
        }
      }
    }

    #endregion

    #region Private Methods

    private static int CalculateSyslogPriority(int p_nFacilityID, ESeverityType p_eSeverity)
    {
      // facility *8 
      // + Severity
      // == Priority Type as a number
      int prioritynumber = (p_nFacilityID*8) + (int) p_eSeverity;

      return prioritynumber;
    }

    /// <summary>
    /// Unused call for now...
    /// </summary>
    /// <param name="p_strIpAddress"></param>
    /// <param name="p_strMsgBody"></param>
    private void Send(string p_strIpAddress, string p_strMsgBody)
    {
      if (p_strIpAddress == null ||
          (p_strIpAddress.Length < 5))
      {
        m_strSyslogRemoteHostIpAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
      }
      else
      {
        m_strSyslogRemoteHostIpAddress = p_strIpAddress;
      }
      Send(ESeverityType.Warning, DateTime.Now, p_strMsgBody);
    }

    #endregion
  }
}