using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;
using SyNet.MessageTypes;
using SyNet.Protocol;
using UserControl = System.Windows.Controls.UserControl;
using System.Globalization;
using System.Threading;

namespace SyNet.GuiControls {
  /// <summary>
  /// Interaction logic for DeviceUpdater.xaml
  /// </summary>
  public partial class DeviceUpdater : UserControl, INotifyPropertyChanged {

    ////////////////////////////////////////////////////////////////////////////
    #region Member variables

    private MsgDispatcher.CallbackStruct m_cs;

    /// <summary>
    ///   Structure of all necessary thread worker references
    /// </summary>
    protected struct WorkerArgs {
      /// <summary>
      ///   Target xbee device
      /// </summary>
      public DeviceXBee TargetDevice;
      /// <summary>
      ///   Reference to the message dispatcher
      /// </summary>
      public MsgDispatcher MessageDispatcher;
      /// <summary>
      ///   Update filename
      /// </summary>
      public String Filename;
    }

    /// <summary>
    ///   Structure of progress arguments
    /// </summary>
    protected struct ProgressArgs {
      // State machine of current progress
      /// <summary>
      /// 
      /// </summary>
      public String Status;
      /// <summary>
      /// 
      /// </summary>
      public String Download;
      /// <summary>
      /// 
      /// </summary>
      public int Retries;
      /// <summary>
      /// 
      /// </summary>
      public Color ColorConnect;
      /// <summary>
      /// 
      /// </summary>
      public Color ColorDownload;
      /// <summary>
      /// 
      /// </summary>
      public Color ColorComplete;
      /// <summary>
      /// 
      /// </summary>
      /// <param name="p_status"></param>
      /// <param name="p_downloadP"></param>
      /// <param name="p_retriesP"></param>
      /// <param name="p_cconP"></param>
      /// <param name="p_cdP"></param>
      /// <param name="p_ccP"></param>
      public ProgressArgs(String p_status, String p_downloadP,
        int p_retriesP, Color p_cconP, Color p_cdP, Color p_ccP)
      {
        Status = p_status;
        Download = p_downloadP;
        Retries = p_retriesP;
        ColorConnect = p_cconP;
        ColorDownload = p_cdP;
        ColorComplete = p_ccP;
      }
    }

    private const string FILENAME_DEFAULT = "[No file selected]";

    private byte[] fileBuffer = new byte[16384];
    private string m_filename = FILENAME_DEFAULT;

    private BackgroundWorker m_backgroundWorker1;

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///   Filename of update file
    /// </summary>
    public string FileName
    {
      get { return m_filename; }
      set
      {
        m_filename = value;
        OnPropertyChanged("FileName");
      }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public DeviceUpdater()
    {
      InitializeComponent();
      InitializeBackgroundWorker();

    }

    /// <summary>
    ///   Initialize the background worker for the download
    /// </summary>
    private void InitializeBackgroundWorker()
    {
      m_backgroundWorker1 = new BackgroundWorker();
      m_backgroundWorker1.DoWork += DownloadFile;
      m_backgroundWorker1.WorkerReportsProgress = true;
      m_backgroundWorker1.WorkerSupportsCancellation = true;
      m_backgroundWorker1.ProgressChanged += ProgressChanged;
      m_backgroundWorker1.RunWorkerCompleted += RunWorkerCompleted;
    }

    private void DownloadFile(object s, DoWorkEventArgs args)
    {
      BackgroundWorker worker = s as BackgroundWorker;
      ProgressArgs progArgs = new ProgressArgs("Idle.",
                                                "Idle.",
                                                0,
                                                Colors.Yellow,
                                                Colors.Blue,
                                                Colors.Blue);

      int pageSize = 64;		// For ATmega168, we have to load 128 bytes at a time (program full page). Document: AVR095
      int memoryAddressHigh;
      int memoryAddressLow;
      int lastAddress;
      int currentAddress;
      int progress = 0;
      bool msgResponse;
      string currentFile = string.Empty;
      byte[] buffer = new byte[pageSize + 4];
      string filename = string.Empty;
      WorkerArgs wArgs = (WorkerArgs)args.Argument;
      MsgDispatcher msgProc = wArgs.MessageDispatcher;
      DeviceXBee xbeeTarget = wArgs.TargetDevice;
      String hexFileName = wArgs.Filename;
      Debug.WriteLine("[DBG] Starting download file worker");


      // Try and do this
      //try {

        worker.ReportProgress(progress, progArgs);

        ////////////////////////////////////////////////////////////////////////
        #region -- Take hex file and store it sequentially into large array

        filename = Path.GetFileName(hexFileName);
        if (!ParseFile(FileName, out currentAddress, out lastAddress)) {
          MessageBox.Show("Could not parse file");
          return;
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////

        if (worker.CancellationPending) {
          args.Cancel = true;
          return;
        }


        ////////////////////////////////////////////////////////////////////////
        #region -- Send reset message

        progArgs.ColorConnect = Colors.Yellow;
        progArgs.Status = ("Sending Reset Message.");
        worker.ReportProgress(progress, progArgs);

        msgResponse = false;
        m_cs.Reset();

        // Send reset message
        msgProc.SendMsg(
          new MsgSyNetBootloadTransmit(xbeeTarget,
                                       EsnAPIBootloadTransmit.REBOOT_DEVICE)
        );
        
        // Wait for a response
        if (m_cs.CallbackEvent.WaitOne(TimeSpan.FromSeconds(30), true))
        {
          // Make sure it's the correct response
          MsgSyNetDeviceStatus msg = m_cs.ResponseMsgs[0] as MsgSyNetDeviceStatus;
          if (msg != null && msg.DeviceStatus == EsnAPIDeviceStatusValues.HW_RESET)
          {
            msgResponse = true;
          }
        }

        // If the response is bad, bail
        if (!msgResponse) {
          progArgs.ColorConnect = Colors.Red;
          progArgs.Status = ("The target IC did not broadcast reset");
          worker.ReportProgress(progress, progArgs);
          return;
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////

        if (worker.CancellationPending) {
          args.Cancel = true;
          return;
        }

        ////////////////////////////////////////////////////////////////////////
        #region -- send bootloading message


        progArgs.Status = ("Sending Bootload Request.");
        worker.ReportProgress(progress, progArgs);

        // Send a bootload message to the device
        msgResponse = false;
        // Reset the callback
        m_cs.Reset();
        int tmpCount = 0;
        while (!msgResponse && (tmpCount++) < 5) {
          msgResponse = msgProc.SendMsgWithResponse(
          new MsgSyNetBootloadTransmit(xbeeTarget,
                                       EsnAPIBootloadTransmit.BOOTLOAD_REQUEST),
          TimeSpan.FromSeconds(20),
          m_cs
          );
        }

        // Make sure it's the right message
        if (msgResponse) {
          msgResponse = false;
          MsgSyNetBootloadResponse msg = m_cs.ResponseMsgs[0] as
                                         MsgSyNetBootloadResponse;
          if (msg != null) {
            if (msg.Response ==
                EsnAPIBootloadResponse.BOOTLOAD_READY) {
              msgResponse = true;
            }
          }
        }

        // If we didn't make it, there was an error
        if (!msgResponse) {
          progArgs.ColorConnect = Colors.Red;
          progArgs.Status = ("The target IC did not broadcast bootload");
          worker.ReportProgress(progress, progArgs);
          return;
        }

        progArgs.ColorConnect = Colors.Green;
        worker.ReportProgress(progress, progArgs);

        #endregion
        ////////////////////////////////////////////////////////////////////////

        if (worker.CancellationPending) {
          args.Cancel = true;
          return;
        }


        int block_size = 0;
        int checkSum;
        bool datablockSuccess = false;
        int startAddress = currentAddress;

        // Begin download loop
        while (currentAddress <= lastAddress) {

          if (worker.CancellationPending) {
            args.Cancel = true;
            return;
          }

          block_size = Math.Min(pageSize, lastAddress - currentAddress + 1);

          // Convert 16-bit current_memory_address into two 8-bit characters
          memoryAddressHigh = currentAddress / 256;
          memoryAddressLow = currentAddress % 256;

          // Calculate current check_sum
          checkSum = 0;
          checkSum = checkSum + block_size;
          checkSum = checkSum + memoryAddressHigh;
          checkSum = checkSum + memoryAddressLow;

          for (int j = 0; j < block_size; j++)
            checkSum = checkSum + fileBuffer[currentAddress + j];

          // Now reduce check_sum to 8 bits
          while (checkSum > 256)
            checkSum -= 256;

          // Now take 2's compliment
          checkSum = 256 - checkSum;

          // Send the record header
          buffer[0] = (byte)block_size;
          buffer[1] = (byte)memoryAddressHigh;
          buffer[2] = (byte)memoryAddressLow;
          buffer[3] = (byte)checkSum;

          Array.Copy(fileBuffer, currentAddress, buffer, 4, block_size);

          // Send the record data
          m_cs.Reset();
          msgProc.SendMsg(
            new MsgSyNetBootloadTransmit(
              xbeeTarget,
              EsnAPIBootloadTransmit.DATA_TRANSMIT,
              buffer)
            );

          datablockSuccess = false;
          if (m_cs.CallbackEvent.WaitOne(TimeSpan.FromSeconds(10), false)) {
            //Debug.WriteLine("[DBG] SyNetBootload - Response ");
            MsgSyNetBootloadResponse msg = m_cs.ResponseMsgs[0] as MsgSyNetBootloadResponse;

            // Check that the essage was cast successfully
            // and it's a data success response
            if (msg != null &&
                msg.Response == EsnAPIBootloadResponse.DATA_SUCCESS) {
              // Make sure its for the right data
              if (msg.MemoryAddress == (UInt16)currentAddress) {
                datablockSuccess = true;
              } else {
                Debug.WriteLine(String.Format("[ERR] Return for {0} expected {1}",
                                              msg.MemoryAddress,
                                              currentAddress));
                if (msg.MemoryAddress < currentAddress)
                {
                  currentAddress = msg.MemoryAddress + block_size;
                }
                progArgs.Retries++;
              }
            } 
          } else {
            progArgs.Retries++;
            Debug.WriteLine("[ERR] SyNetBootload - No response");
          }

          if (datablockSuccess) {
            progress = (int)(((float)currentAddress - (float)startAddress) /
              ((float)lastAddress - (float)startAddress + 1.0) * 100.0);

            currentAddress = currentAddress + block_size;

            progArgs.Status = String.Format("Loaded {0} data words.", currentAddress);
            progArgs.Download = "Running...";
            progArgs.ColorDownload = Colors.Green;
            progArgs.ColorComplete = Colors.Yellow;
          } else {
            progArgs.ColorDownload = Colors.Yellow;
          }

          worker.ReportProgress(progress, progArgs);
          //  #endregion
        }

        worker.ReportProgress(100, progArgs);

        // We're done
        m_cs.Reset();
        bool response = msgProc.SendMsgWithResponse(
          new MsgSyNetBootloadTransmit(
            xbeeTarget, EsnAPIBootloadTransmit.DATA_COMPLETE),
          TimeSpan.FromSeconds(5),
          m_cs);


        progArgs.ColorConnect = Colors.Blue;
        progArgs.ColorDownload = Colors.Blue;
        progArgs.ColorComplete = Colors.Green;
        progArgs.Status = ("Idle.");
        progArgs.Download = "Download Complete.";
        worker.ReportProgress(progress, progArgs);

      //} catch (System.Exception ex) {

      //  if (string.IsNullOrEmpty(currentFile))
      //    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      //  else
      //    MessageBox.Show(string.Format("Error in file {0}\n\n{1}", filename, ex.Message), "W00ter", MessageBoxButtons.OK, MessageBoxIcon.Error);
      //}

    }

    /// <summary>
    /// Take in an Intel hex file and load it into a large array.
    /// This allows us to read sequential data from the large array without having to
    /// worry about weird character line-feed breaks in the text/HEX file.
    /// </summary>
    /// <returns>success</returns>
    /// <remarks>For ATmega168, we have to load 128 bytes at a time (program full page). Document: AVR095</remarks>
    private bool ParseFile(string path, out int minAddress, out int maxAddress)
    {
      int byteCount;
      int memoryAddressHigh;
      int memoryAddressLow;
      int memoryAddress;
      int lineNumber = 0;
      string filename = Path.GetFileName(path);

      maxAddress = int.MinValue;
      minAddress = int.MaxValue;

      // Pre-fill hex_array with 0xFF
      for (int i = 0; i < fileBuffer.Length; i++)
        fileBuffer[i] = 0xFF;

      // Read in the HEX file - clean it up
      try {
        if (!File.Exists(path)) {
          System.Windows.Forms.MessageBox.Show(string.Format("File not found {0}", path));
          return false;
        }
        using (StreamReader f = new StreamReader(path, Encoding.ASCII)) {
          string line;
          while ((line = f.ReadLine()) != null) {
            line = line.Trim();
            lineNumber++;
            if (line.Length == 0)
              continue;

            #region -- Intel file format
            if (line.StartsWith(":")) {
              switch (line.Substring(7, 2)) {						// Ignore:
                case "02":			//	02 - extended segment address record
                case "04":			//	04 - extended linear address record
                  if (MessageBox.Show(
                    string.Format(
                      "Warning in line {0} of {1}\n\nUnsupported record type\n\nContinue download ?",
                      lineNumber, filename
                      ),
                    string.Format("Title"),
                    MessageBoxButtons.YesNo
                    ) == DialogResult.No) {
                    f.Close();
                    return false;
                  }
                  continue;
                case "03":
                  continue;
                case "01":			// 01 - end-of-file record
                  f.Close();
                  return true;
                case "00":
                  break;
                default:
                  if (MessageBox.Show(
                    string.Format(
                      "Warning in line {0} of {1}\n\nUnknown record type {2}\n\nContinue download ?",
                      lineNumber, filename, line.Substring(7, 2)
                      ),
                    "Caption",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1) == DialogResult.No) {
                    f.Close();
                    return false;
                  }
                  continue;
              }

              byteCount = int.Parse(line.Substring(1, 2), NumberStyles.HexNumber);
              if (byteCount == 0)
                continue;

              // Get the memory address of this line
              memoryAddressHigh = int.Parse(line.Substring(3, 2), NumberStyles.HexNumber);
              memoryAddressLow = int.Parse(line.Substring(5, 2), NumberStyles.HexNumber);
              memoryAddress = (memoryAddressHigh * 256) + memoryAddressLow;

              for (int idx = 0; idx < byteCount; idx++) {
                int address = memoryAddress + idx;
                if (address >= fileBuffer.Length) {
                  MessageBox.Show(
                    string.Format("Error in line {0} of {1}\n\nAddress {2:X} out of buffer (max. {3:X})", lineNumber, filename, address, fileBuffer.Length),
                    "Caption",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                  f.Close();
                  return false;
                }
                if (maxAddress < address)
                  maxAddress = address;
                if (minAddress > address)
                  minAddress = address;
                fileBuffer[address] = byte.Parse(line.Substring(9 + idx * 2, 2), NumberStyles.HexNumber);
              }
              continue;
            }
            #endregion

            #region -- Motorola S format
            if (line.StartsWith("S")) {
              MessageBox.Show(
                string.Format("Error in line {0} of {1}\n\nMotorols S format not supported.", lineNumber, filename),
                "Caption",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
                );
              f.Close();
              return false;
            }
            #endregion
          }
          f.Close();
        }
      } catch (System.Exception ex) {
        // see http://en.wikipedia.org/wiki/S-record for detail
        MessageBox.Show(
          string.Format("Error in line {0} of {1}\n\n{2}", lineNumber, filename, ex.Message),
          "Caption",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
          );
        return false;
      }
      return true;
    }

    /// <summary>
    /// This sub resets all the GUI stuff and displays the given error to the user
    /// </summary>
    /// <param name="error_msg"></param>
    private void ErrorOut(string error_msg)
    {
      m_LabelStatus.Content = "Idle";
      m_LabelDownload.Content = "Idle";
      MessageBox.Show(error_msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }


    /// <summary>
    ///   Property changed event handler
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    protected void OnPropertyChanged(string propertyName)
    {
      if (this.PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdateProgressArgs(ProgressArgs progArgs)
    {
      m_LabelStatus.Content = progArgs.Status;
      m_LabelDownload.Content = progArgs.Download;
      m_LabelRetries.Content = progArgs.Retries;

      m_RectDownload.Fill = new SolidColorBrush(progArgs.ColorDownload);
      m_RectConnect.Fill = new SolidColorBrush(progArgs.ColorConnect);
      m_RectComplete.Fill = new SolidColorBrush(progArgs.ColorComplete);
    }

    /// <summary>
    ///   Event for when the progress needs to be updated
    /// </summary>
    /// <param name="s"></param>
    /// <param name="args"></param>
    private void ProgressChanged(object s, ProgressChangedEventArgs args)
    {
      ProgressArgs progArgs = (ProgressArgs)args.UserState;
      UpdateProgressArgs(progArgs);

      m_ProgressBarDownload.Value = args.ProgressPercentage;
    }

    /// <summary>
    ///   Event when the download worker has completed
    /// </summary>
    /// <param name="s"></param>
    /// <param name="args"></param>
    private void RunWorkerCompleted(object s, RunWorkerCompletedEventArgs args)
    {
      m_ButtonDownload.IsEnabled = true;
      m_ButtonCancel.IsEnabled = false;
      m_ProgressBarDownload.Value = 0;

      // Remove the callback
      MsgDispatcher msgProc = DataContext as MsgDispatcher;
      if (msgProc != null) {
        msgProc.RemoveCallback(m_cs);
      }

      // Renable the node manager poking
      NodeManager.Instance.PeriodicCheck = true;
    }

    /// <summary>
    ///   Hex file selection button click handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonFileSelect_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
      ofd.Multiselect = false;

      ofd.Filter = "Data Sources (*.hex)|*.hex*";

      if (ofd.ShowDialog() == DialogResult.OK) {
        FileName = ofd.FileName;
      }
    }

    /// <summary>
    ///   Start the download
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonDownload_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      // Check to make sure a file is selected
      if (FileName.Equals(FILENAME_DEFAULT)) {
        MessageBox.Show("Please select a file");
        return;
      }

      // Check to make sure a node is selected
      if (m_ComboBoxNodeList.SelectedIndex == -1) {
        MessageBox.Show("Select a node");
        return;
      }

      DeviceXBee xbeeTarget = m_ComboBoxNodeList.SelectionBoxItem as DeviceXBee;
      if (xbeeTarget == null) {
        MessageBox.Show("Could not cast target device");
        return;
      }


      WorkerArgs args = new WorkerArgs();
      args.TargetDevice = xbeeTarget;
      args.MessageDispatcher = MsgDispatcher.Instance;
      args.Filename = FileName;
      m_cs = MsgDispatcher.Instance.RegisterCallback(xbeeTarget.SerialNumber);

      m_ButtonDownload.IsEnabled = false;
      m_ButtonCancel.IsEnabled = true;
      // Disable the node manager from poking around
      NodeManager.Instance.PeriodicCheck = false;

      // Start the downloader thread
      m_backgroundWorker1.RunWorkerAsync(args);
    }

    /// <summary>
    ///   Cancel the download
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonStop_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      m_backgroundWorker1.CancelAsync();
    }
  }
}
