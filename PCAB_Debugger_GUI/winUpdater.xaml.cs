using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows;
using static PCAB_Debugger_GUI.PCAB;
using static PCAB_Debugger_GUI.ShowSerialPortName;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// winUpdater.xaml の相互作用ロジック
    /// </summary>
    public partial class winUpdater : Window
    {
        SerialPortTable[] ports;
        private SerialPort _mod;
        const uint PICO_FLASH_SIZE_BYTES = 2 * 1024 * 1024;
        const uint FLASH_BLOCK_SIZE = 1u << 16;
        const uint FLASH_SECTOR_SIZE = 1u << 12;
        const uint FLASH_PAGE_SIZE = 1u << 8;

        public winUpdater()
        {
            InitializeComponent();
        }

        private void UPDATE_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            string sn = "0000";


            _mod = new SerialPort(ports[SERIAL_PORTS_COMBOBOX.SelectedIndex].Name);
            //_mod.BaudRate = 9600;
            _mod.BaudRate = 115200;
            _mod.DataBits = 8;
            _mod.Parity = Parity.None;
            _mod.StopBits = StopBits.One;
            _mod.Handshake = Handshake.None;
            _mod.DtrEnable = true;
            _mod.Encoding = Encoding.ASCII;
            _mod.NewLine = "\r\n";
            _mod.ReadBufferSize = 2048;
            _mod.WriteTimeout = 5000;
            _mod.ReadTimeout = 5000;
            try
            {
                _mod.Open();
            }
            catch (UnauthorizedAccessException) { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            catch (Exception) { MessageBox.Show("Serial port open Error.\n{e.ToString()}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            try
            {
                _mod.WriteLine("");
                _mod.WriteLine("#" + sn + " EROM 6-0");
                _mod.WriteLine("#" + sn + " CUI 0");
                Thread.Sleep(500);
                _mod.DiscardInBuffer();
                _mod.WriteLine("#" + sn + " GetIDN");
                string[] arrBf = _mod.ReadLine().Split(',');
                if (arrBf.Length != 4) { MessageBox.Show("Not the target device.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); _mod?.Close(); return; }
                if (arrBf[0] != "Orient Microwave Corp." || arrBf[1] != "LX00-0004-00" || arrBf[2] != sn || arrBf[3].Substring(0, 4) != "1.1.")
                { MessageBox.Show("Not the target device.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); _mod?.Close(); return; }

                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.FileName = "PCAB_Debugger_FW.bin";
                ofd.Filter = "Firmware Binary File(*.bin)|*.bin|すべてのファイル(*.*)|*.*";
                ofd.Title = "Select Firmware File.";
                if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) { _mod?.Close(); return; }
                string filePath = ofd.FileName;
                ofd.Dispose();
                using (Stream stream = new FileStream(filePath,FileMode.Open))
                using(BinaryReader br = new BinaryReader(stream))
                {
                    uint blockNum = (uint)(stream.Length / FLASH_BLOCK_SIZE) + 1;
                    do
                    {
                        _mod.WriteLine("#" + sn + " RROM " + blockNum.ToString("X0") + "-00");
                        if(_mod.ReadLine() == "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")
                        { break; }
                        blockNum++;
                    } while (true);

                    byte[] datBF = new byte[FLASH_PAGE_SIZE];
                    datBF = br.ReadBytes(datBF.Length);
                    uint count = 0;
                    while (datBF.Length != 0)
                    {
                        string strBF = "";
                        strBF = BitConverter.ToString(datBF).Replace("-","");
                        for(uint i = (uint)strBF.Length; i < 2 * FLASH_PAGE_SIZE; i+=2) { strBF += "FF"; }
                        _mod.WriteLine("#" + sn + " WROM " + 
                            ((blockNum * FLASH_BLOCK_SIZE + count * FLASH_PAGE_SIZE) / FLASH_BLOCK_SIZE).ToString("X0") + "-" + 
                            (((blockNum * FLASH_BLOCK_SIZE + count * FLASH_PAGE_SIZE) % FLASH_BLOCK_SIZE) / FLASH_PAGE_SIZE).ToString("X0") + " " +
                            strBF);
                        if(_mod.ReadLine().Substring(0,4) != "DONE") 
                        { MessageBox.Show("Write ROM error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); _mod?.Close(); return; }
                        count++;
                        datBF = br.ReadBytes(datBF.Length);
                    }
                    _mod.WriteLine("#" + sn + " UROM 0 " + blockNum.ToString("X0") + " " + ((blockNum - 1) * FLASH_BLOCK_SIZE).ToString("X0"));
                    try
                    {
                        if (_mod.ReadLine().Substring(0, 3) == "ERR")
                        { MessageBox.Show("Firmware update error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); _mod?.Close(); return; }
                    }
                    catch
                    {
                        _mod?.Close();
                        MessageBox.Show("Firmware update completed.", "Sucess", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch { MessageBox.Show("Serial Connect Error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); _mod?.Close(); return; }
        }

        private void SERIAL_PORTS_COMBOBOX_RELOAD()
        {
            SERIAL_PORTS_COMBOBOX.Items.Clear();
            ports = GetDeviceNames();
            foreach (SerialPortTable port in ports)
            {
                SERIAL_PORTS_COMBOBOX.Items.Add(port.Caption);
            }
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownOpened(object sender, EventArgs e)
        {
            SERIAL_PORTS_COMBOBOX_RELOAD();
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SERIAL_PORTS_COMBOBOX.SelectedIndex < 0) { UPDATE_BUTTON.IsEnabled = false; }
            else { UPDATE_BUTTON.IsEnabled = true; }
        }
    }
}
