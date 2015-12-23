namespace SyNet.GuiControls
{
  using System;
  using System.Windows;
  using System.Windows.Forms;

  /// <summary>
  /// Interaction logic for EditPreferencesDlg.xaml
  /// </summary>
  public partial class EditPreferencesDlg : Window
  {
    #region Private Member Data

    private int m_nSelectedBaudRate;
    private string m_strSelectedSerialPort;
    private string[] m_strAvailableSerialPortsAr;
    private string m_strLogDirSpec;
    private DialogResult m_drEndResult;
    private bool m_bSyslogEnabled;
    private string m_strSyslogFacility;
    private string m_strSyslogTargetHostname;
    private string[] m_strSupportedBaudRatesAr;
    private string[] m_strSupportedSyslogFacilitiesAr;

    #endregion

    #region Constructor

    /// <summary>
    ///   Default constructor
    /// </summary>
    public EditPreferencesDlg()
    {
      InitializeComponent();
    }

    #endregion

    /// <summary>
    ///   Load settings from the propeties into the GUI
    /// </summary>
    public void LoadSettings()
    {
      m_drEndResult = System.Windows.Forms.DialogResult.Cancel;
      //
      // setup serial baud rate
      //
      cbxBaudRates.ItemsSource = m_strSupportedBaudRatesAr;
      // Load Baud Rate
      bool bFoundBaudRate = false;
      foreach (string strBaudRate in cbxBaudRates.Items)
      {
        if (strBaudRate == m_nSelectedBaudRate.ToString())
        {
          cbxBaudRates.SelectedItem = strBaudRate;
          bFoundBaudRate = true;
          break;
        }
      }
      if (!bFoundBaudRate)
      {
        cbxBaudRates.SelectedIndex = 0; // select our first one, then...
      }

      //
      // Load up the Serial Port combo box with available ports
      //
      cbxAvailablePorts.ItemsSource = m_strAvailableSerialPortsAr;
      //
      // select the com port
      //
      bool bFoundPort = false;
      foreach (String strCommPort in cbxAvailablePorts.Items)
      {
        if (strCommPort == m_strSelectedSerialPort)
        {
          cbxAvailablePorts.SelectedItem = strCommPort;
          bFoundPort = true;
          break;
        }
      }
      if (!bFoundPort)
      {
        cbxAvailablePorts.SelectedIndex = 0; // select our first one, then...
      }

      //
      // Load log directory
      //
      m_LabelLogDir.Content = m_strLogDirSpec;

      //
      //  Setup syslog controls
      //

      // set our syslog enablement
      cbxEnableSyslog.IsChecked = m_bSyslogEnabled;

      // set out syslog target host
      tbxTargetHost.Text = m_strSyslogTargetHostname;

      cbxLogFacility.ItemsSource = m_strSupportedSyslogFacilitiesAr;
      //
      // select the Syslog Facility under which we are to log
      //
      bool bFoundFacility = false;
      foreach (String strFacility in cbxLogFacility.Items)
      {
        if (strFacility == m_strSyslogFacility)
        {
          cbxLogFacility.SelectedItem = strFacility;
          bFoundFacility = true;
          break;
        }
      }
      if (!bFoundFacility)
      {
        cbxLogFacility.SelectedIndex = 0; // select our first one, then...
      }
    }

    /// <summary>
    ///   Selected Baud Rate
    /// </summary>
    public int SelectedBaudRate
    {
      get
      {
        m_nSelectedBaudRate = Int32.Parse(cbxBaudRates.SelectedItem.ToString());
        return m_nSelectedBaudRate;
      }
      set { m_nSelectedBaudRate = value; }
    }

    /// <summary>
    ///   Selected serial port
    /// </summary>
    public string SelectedSerialPort
    {
      get { return (String) cbxAvailablePorts.SelectedItem; }
      set { m_strSelectedSerialPort = value; }
    }


    /// <summary>
    ///   List of supported baud rates
    /// </summary>
    public string[] SupportedBaudRates
    {
      get { return m_strSupportedBaudRatesAr; }
      set { m_strSupportedBaudRatesAr = value; }
    }


    /// <summary>
    /// 
    /// </summary>
    public string[] SupportedSyslogFacilities
    {
      get { return m_strSupportedSyslogFacilitiesAr; }
      set { m_strSupportedSyslogFacilitiesAr = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public string[] AvailableSerialPorts
    {
      set { m_strAvailableSerialPortsAr = value; }
    }


    /// <summary>
    /// 
    /// </summary>
    public string LogDirectory
    {
      get { return (String) m_LabelLogDir.Content; }
      set { m_strLogDirSpec = value; }
    }

    /// <summary>
    /// R/W PROPERTY: get/set syslog enabled value
    /// </summary>
    public bool SyslogEnabled
    {
      get
      {
        // make sure our fields are filled in if we are to be enabled...
        return (cbxEnableSyslog.IsChecked == true && SyslogTargetHostname.Length > 0 && SyslogFacility.Length > 0);
      }
      set { m_bSyslogEnabled = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public string SyslogTargetHostname
    {
      get { return tbxTargetHost.Text; }
      set { m_strSyslogTargetHostname = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public string SyslogFacility
    {
      get { return cbxLogFacility.SelectedItem.ToString(); }
      set { m_strSyslogFacility = value; }
    }

    /// <summary>
    /// R/O PROPERTY: return the ending action of this dialog just before close
    /// </summary>
    public DialogResult EndResult
    {
      get { return m_drEndResult; }
    }

    /// <summary>
    ///   Button click even handler for saving settings
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
      m_drEndResult = System.Windows.Forms.DialogResult.OK;
      Close();
    }

    /// <summary>
    ///   Button click event handler for selecting a log directory
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonLogDirSelect_Click(object sender, RoutedEventArgs e)
    {
      FolderBrowserDialog fbd = new FolderBrowserDialog();

      if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        m_LabelLogDir.Content = fbd.SelectedPath;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cbxEnableSyslog_Checked(object sender, RoutedEventArgs e)
    {
      // CRAP!  this doesn't seem to be disabling the fields like i'd like...
      bool bIsEnabled = cbxEnableSyslog.IsChecked == true ? true : false;
      cbxLogFacility.IsEnabled = bIsEnabled;
      tbxTargetHost.IsEnabled = bIsEnabled;
    }
  }
}