using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using SyNet.Properties;

namespace SyNet
{
  /// <summary>
  ///   Singleton for access to settings in the system
  /// </summary>
  public sealed class SyNetSettings
  {
    #region Memeber Variables

    XmlDocument xmlDocument = new XmlDocument();
    string documentPath = Application.StartupPath + "\\..\\..\\settings.xml";
    private static SyNetSettings s_snSettingsInstance;
    private static readonly object s_objIinstanceLock = new object();

    #endregion

    #region Public Properites

    private bool m_isDeserializing;
    private bool m_isSerializing;

    private double m_xmlVersion = 0.0;
    
    /// <summary>
    ///   Flag stating whether or not the application is in the process of 
    ///   serializing
    /// </summary>
    public bool IsSerializing
    {
      get
      {
        return m_isSerializing;
      }
      set
      {
        m_isSerializing = true;
      }
    }

    /// <summary>
    ///   Flag stating whether or not the application is in the process
    ///   of deserializing
    /// </summary>
    public bool IsDeserializing
    {
      get { return m_isDeserializing; }
      set
      {
        bool bFinished = false;
        if (m_isDeserializing && !value)
        {
          bFinished = true;
        }
        m_isDeserializing = value;

        if (bFinished && DeserializingFinished != null)
        {
          DeserializingFinished();
        }
      }
    }

    /// <summary>
    ///   Serial Baud Rate
    /// </summary>
    public int SerialBaud
    {
      get
      {
        return Settings.Default.SerialBaud;
      }
      set
      {
        Settings.Default.SerialBaud = value;
        SaveSettings();
      }
    }

    /// <summary>
    ///   Serial Port
    /// </summary>
    public String SerialComPort
    {
      get
      {
        return Settings.Default.SerialPort;
      }
      set
      {
        Settings.Default.SerialPort = value;
        SaveSettings();
      }

    }

    /// <summary>
    ///   Log directory
    /// </summary>
    public String LogDirectory
    {
      get
      {
        return Settings.Default.LogDir;
      }
      set
      {
        Settings.Default.LogDir = value;
        SaveSettings();
      }

    }

    /// <summary>
    /// R/W PROPERTY: get/set syslog-enabled value
    /// </summary>
    /// <remarks>on set the settings will be persisted</remarks>
    public bool SyslogEnabled
    {
      get
      {
        return Settings.Default.EnableSyslog;
      }
      set
      {
        Settings.Default.EnableSyslog = value;
        SaveSettings();
      }
    }

    /// <summary>
    /// R/W PROPERTY: get/set the selected syslog target host
    /// </summary>
    /// <remarks>on set the settings will be persisted</remarks>
    public string SyslogTargetHost
    {
      get
      {
        return Settings.Default.TargetHost;
      }
      set
      {
        Settings.Default.TargetHost = value;
        SaveSettings();
      }
    }

    /// <summary>
    /// R/W PROPERTY: get/set the selected syslog facility
    /// </summary>
    /// <remarks>on set the settings will be persisted</remarks>
    public string SyslogFacility
    {
      get
      {
        return Settings.Default.LogFacility;
      }
      set
      {
        Settings.Default.LogFacility = value;
        SaveSettings();
      }
    }

    /// <summary>
    /// R/O PROPERTY: get the list of supported facility values for syslog
    /// </summary>
    public string[] SupportedFacilities
    {
      get
      {
        string[] strFacilityAr = new string[Settings.Default.SupportedFacilities.Count];
        Settings.Default.SupportedFacilities.CopyTo(strFacilityAr, 0);
        return strFacilityAr;
      }
    }

    /// <summary>
    /// R/O PROPERTY: get the list of supported baud rates
    /// </summary>
    public string[] SupportedBaudRates
    {
      get
      {
        string[] strBaudRatesAr = new string[Settings.Default.SupportedBaudRates.Count];
        Settings.Default.SupportedBaudRates.CopyTo(strBaudRatesAr, 0);
        return strBaudRatesAr;
      }
    }

    /// <summary>
    ///   Gets an instance of this class
    /// </summary>
    public static SyNetSettings Instance
    {
      get
      {
        lock (s_objIinstanceLock)
        {
          if (s_snSettingsInstance == null)
          {
            s_snSettingsInstance = new SyNetSettings();
          }
          return s_snSettingsInstance;
        }
      }
    }

    /// <summary>
    /// Version of this XML
    /// </summary>
    public double XMLVersion
    {
      get { return m_xmlVersion; }
      set { m_xmlVersion = value; }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   Default Constructor
    /// </summary>
    public SyNetSettings()
    {
      try
      {
        xmlDocument.Load(documentPath);
      }
      catch
      {
        xmlDocument.LoadXml("<settings></settings>");
      }
    }

    #endregion

    /// <summary>
    ///   Save the settings
    /// </summary>
    public void SaveSettings()
    {
      Settings.Default.Save();
    }

    ///<summary>
    ///   Delegate for the deserialization finished method
    ///</summary>
    public delegate void DeserializingFinishedDelegate();

    /// <summary>
    ///   Event for deserialization finished
    /// </summary>
    public event DeserializingFinishedDelegate DeserializingFinished;


    /// <summary>
    ///   Initialize the system settings and upgrade if necessary
    /// </summary>
    public void InitializeSystemSettings()
    {
      // upgrade our settings if needed for this release
      // (happens on first run of new version install)
      try
      {
        if (Settings.Default.UpgradeRequired)
        {
          Settings.Default.Upgrade();
          Settings.Default.Reload();

          // prevent further upgrades
          Settings.Default.UpgradeRequired = false;
          Settings.Default.Save();
        }
      }
      catch (Exception exception)
      {
        string strErrMsg = String.Format("{0}\n\nTry again at next run?", exception.Message);
        DialogResult drResult = MessageBox.Show(strErrMsg,
                                                Application.ProductName,
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Warning);
        if (drResult == DialogResult.No)
        {
          // prevent further upgrade attempts
          Settings.Default.UpgradeRequired = false;
          Settings.Default.Save();
        }
      }
    }
  }
}
