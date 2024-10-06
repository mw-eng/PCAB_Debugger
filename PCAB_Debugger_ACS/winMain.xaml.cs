using PCAB_Debugger_ACS.Properties;
using PCAB_Debugger_ComLib;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PCAB_Debugger_ComLib.ShowSerialPortName;

namespace PCAB_Debugger_ACS
{
    /// <summary>
    /// winMain.xaml の相互作用ロジック
    /// </summary>
    public partial class winMain : Window
    {
        private POS _pos;
        private SerialPortTable[] ports;
        NormalizedColorChart cc;
        private winPOS winPOSmonitor;

        public winMain()
        {
            InitializeComponent();
            if (Settings.Default.winMainTop >= SystemParameters.VirtualScreenTop &&
                (Settings.Default.winMainTop + Settings.Default.winMainHeight) <
                SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
            {
                this.Top = Settings.Default.winMainTop;
            }
            if (Settings.Default.winMainLeft >= SystemParameters.VirtualScreenLeft &&
                (Settings.Default.winMainLeft + Settings.Default.winMainWidth) <
                SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
            {
                this.Left = Settings.Default.winMainLeft;
            }
            if (Settings.Default.winMainWidth > 0 &&
                Settings.Default.winMainWidth < SystemParameters.WorkArea.Width)
            {
                this.Width = Settings.Default.winMainWidth;
            }
            if (Settings.Default.winMainHeight > 0 &&
                Settings.Default.winMainHeight < SystemParameters.WorkArea.Height)
            {
                this.Height = Settings.Default.winMainHeight;
            }
            if (Settings.Default.winMainMaximized)
            { Loaded += (o, e) => WindowState = WindowState.Maximized; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title += " Ver," + System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
#if DEBUG
            Settings.Default.Reset();
            this.Title += "_DEBUG MODE";
#endif
            SERIAL_PORTS_COMBOBOX_RELOAD(sender, e);
            SERIAL_PORTS_COMBOBOX0.SelectedIndex = Settings.Default.spBaudRate0;
            SERIAL_PORTS_COMBOBOX1.SelectedIndex = Settings.Default.spBaudRate1;
            SERIAL_PORTS_COMBOBOX2.SelectedIndex = Settings.Default.spBaudRate2;
            SERIAL_PORTS_COMBOBOX3.SelectedIndex = Settings.Default.spBaudRate3;
            if (SERIAL_PORTS_COMBOBOX0.Items.Count > 0) { SERIAL_PORTS_COMBOBOX0.SelectedIndex = 0; }
            if (SERIAL_PORTS_COMBOBOX1.Items.Count > 1) { SERIAL_PORTS_COMBOBOX1.SelectedIndex = 1; }
            if (SERIAL_PORTS_COMBOBOX2.Items.Count > 2) { SERIAL_PORTS_COMBOBOX2.SelectedIndex = 2; }
            if (SERIAL_PORTS_COMBOBOX3.Items.Count > 2) { SERIAL_PORTS_COMBOBOX3.SelectedIndex = 2; }
            if (ports != null)
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    if (Settings.Default.spCaption0 == ports[i].Caption)
                    { SERIAL_PORTS_COMBOBOX0.SelectedIndex = i; }
                    if (Settings.Default.spCaption1 == ports[i].Caption)
                    { SERIAL_PORTS_COMBOBOX1.SelectedIndex = i; }
                    if (Settings.Default.spCaption2 == ports[i].Caption)
                    { SERIAL_PORTS_COMBOBOX2.SelectedIndex = i; }
                    if (Settings.Default.spCaption3 == ports[i].Caption)
                    { SERIAL_PORTS_COMBOBOX3.SelectedIndex = i; }
                }
            }
            SERIAL_NUMBERS_TEXTBOX11.Text = Settings.Default.sn11;
            SERIAL_NUMBERS_TEXTBOX12.Text = Settings.Default.sn12;
            SERIAL_NUMBERS_TEXTBOX13.Text = Settings.Default.sn13;
            SERIAL_NUMBERS_TEXTBOX21.Text = Settings.Default.sn21;
            SERIAL_NUMBERS_TEXTBOX22.Text = Settings.Default.sn22;
            SERIAL_NUMBERS_TEXTBOX23.Text = Settings.Default.sn23;
            SERIAL_NUMBERS_TEXTBOX31.Text = Settings.Default.sn31;
            SERIAL_NUMBERS_TEXTBOX32.Text = Settings.Default.sn32;
            SERIAL_NUMBERS_TEXTBOX33.Text = Settings.Default.sn33;
            SERIAL_PORTS_COMBOBOX_DropDownClosed(null, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (_ioList.Count > 0)
            //{
            //    foreach (clsSerialIO _io in _ioList)
            //    {
            //        if (_io?.isOpen == true)
            //        {
            //            if (MessageBox.Show("Communication with PCAB\nDo you want to disconnect and exit?", "Worning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            //            {
            //                OnError(null, null);
            //            }
            //            else { e.Cancel = true; return; }
            //        }
            //    }
            //}
            Settings.Default.spCaption0 = SERIAL_PORTS_COMBOBOX1.Text;
            Settings.Default.spCaption1 = SERIAL_PORTS_COMBOBOX1.Text;
            Settings.Default.spCaption2 = SERIAL_PORTS_COMBOBOX2.Text;
            Settings.Default.spCaption3 = SERIAL_PORTS_COMBOBOX3.Text;
            Settings.Default.spBaudRate0 = SERIAL_PORTS_COMBOBOX0.SelectedIndex;
            Settings.Default.spBaudRate1 = SERIAL_PORTS_COMBOBOX1.SelectedIndex;
            Settings.Default.spBaudRate2 = SERIAL_PORTS_COMBOBOX2.SelectedIndex;
            Settings.Default.spBaudRate3 = SERIAL_PORTS_COMBOBOX3.SelectedIndex;
            Settings.Default.sn11 = SERIAL_NUMBERS_TEXTBOX11.Text;
            Settings.Default.sn12 = SERIAL_NUMBERS_TEXTBOX12.Text;
            Settings.Default.sn13 = SERIAL_NUMBERS_TEXTBOX13.Text;
            Settings.Default.sn21 = SERIAL_NUMBERS_TEXTBOX21.Text;
            Settings.Default.sn22 = SERIAL_NUMBERS_TEXTBOX22.Text;
            Settings.Default.sn23 = SERIAL_NUMBERS_TEXTBOX23.Text;
            Settings.Default.sn31 = SERIAL_NUMBERS_TEXTBOX31.Text;
            Settings.Default.sn32 = SERIAL_NUMBERS_TEXTBOX32.Text;
            Settings.Default.sn33 = SERIAL_NUMBERS_TEXTBOX33.Text;
            Settings.Default.winMainTop = this.Top;
            Settings.Default.winMainLeft = this.Left;
            Settings.Default.winMainHeight = this.Height;
            Settings.Default.winMainWidth = this.Width;
            Settings.Default.winMainMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            Settings.Default.Save();
        }

        private void CONNECT_BUTTON_Click(object sender, RoutedEventArgs e)
        {

        }

        /*
        private void RUN_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (_pos != null)
            {
                _pos.OnReadDAT -= winPOSmonitor.OnReadDAT;
                _pos.OnTimeoutError -= winPOSmonitor.OnTimeoutError;
                winPOSmonitor.WindowClose();
                _pos.OnTaskError -= OnTaskError;
                _pos.POS_AutoTaskStop();
                _pos.Close();
                _pos = null;
                SERIAL_PORTS_COMBOBOX.IsEnabled = true;
                RUN_BUTTON.Content = "RUN";
            }
            else
            {
                SerialPort _serialPort = new SerialPort();
                _serialPort.PortName = ports[SERIAL_PORTS_COMBOBOX.SelectedIndex].Name;
                _serialPort.BaudRate = 115200;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.DtrEnable = true;
                _serialPort.Encoding = Encoding.ASCII;
                _serialPort.NewLine = "\r\n";
                _pos = new POS(_serialPort);
                winPOSmonitor = new winPOS();
                _pos.OnTaskError += OnTaskError;
                _pos.OnReadDAT += winPOSmonitor.OnReadDAT;
                _pos.OnTimeoutError += winPOSmonitor.OnTimeoutError;
                try
                {
                    _pos.Open();
                    _pos.POS_AutoTaskStart();
                    winPOSmonitor.Show();
                    SERIAL_PORTS_COMBOBOX.IsEnabled = false;
                    LOG_TEXTBOX.Text = "";
                    RUN_BUTTON.Content = "STOP";
                }
                catch
                {
                    _pos.Close();
                    _pos = null;
                    SERIAL_PORTS_COMBOBOX_RELOAD(this, null);
                }
            }
        }

        private void OnTaskError(object sender, POSEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                winPOSmonitor.WindowClose();
                _pos?.POS_AutoTaskStop();
                _pos?.Close();
                _pos = null;
                SERIAL_PORTS_COMBOBOX.IsEnabled = true;
                RUN_BUTTON.Content = "RUN";
            }));
        }
        */
        private void SERIAL_PORTS_COMBOBOX_RELOAD(object sender, EventArgs e)
        {
            ports = GetDeviceNames();
            if (sender is ComboBox)
            {
                ((ComboBox)sender).Items.Clear();
                if (ports != null)
                {
                    foreach (SerialPortTable port in ports)
                    {
                        if (port.Caption != SERIAL_PORTS_COMBOBOX0.Text &&
                            port.Caption != SERIAL_PORTS_COMBOBOX1.Text &&
                            port.Caption != SERIAL_PORTS_COMBOBOX2.Text &&
                            port.Caption != SERIAL_PORTS_COMBOBOX3.Text)
                            ((ComboBox)sender).Items.Add(port.Caption);
                    }
                }
            }
            else
            {
                SERIAL_PORTS_COMBOBOX0.Items.Clear();
                SERIAL_PORTS_COMBOBOX1.Items.Clear();
                SERIAL_PORTS_COMBOBOX2.Items.Clear();
                SERIAL_PORTS_COMBOBOX3.Items.Clear();
                if (ports != null)
                {
                    foreach (SerialPortTable port in ports)
                    {
                        SERIAL_PORTS_COMBOBOX0.Items.Add(port.Caption);
                        SERIAL_PORTS_COMBOBOX1.Items.Add(port.Caption);
                        SERIAL_PORTS_COMBOBOX2.Items.Add(port.Caption);
                        SERIAL_PORTS_COMBOBOX3.Items.Add(port.Caption);
                    }
                }
            }
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownOpened(object sender, EventArgs e)
        {
            SERIAL_PORTS_COMBOBOX_RELOAD(sender, e);
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SERIAL_PORTS_COMBOBOX0.SelectedIndex >= 0 &&
                SERIAL_PORTS_COMBOBOX1.SelectedIndex >= 0 &&
                SERIAL_PORTS_COMBOBOX2.SelectedIndex >= 0 &&
                SERIAL_PORTS_COMBOBOX3.SelectedIndex >= 0 &&
                SERIAL_NUMBERS_TEXTBOX11.Text.Replace(" ", "").Length > 0 &&
                SERIAL_NUMBERS_TEXTBOX12.Text.Replace(" ", "").Length > 0 &&
                SERIAL_NUMBERS_TEXTBOX13.Text.Replace(" ", "").Length > 0 &&
                SERIAL_NUMBERS_TEXTBOX21.Text.Replace(" ", "").Length > 0 &&
                SERIAL_NUMBERS_TEXTBOX22.Text.Replace(" ", "").Length > 0 &&
                SERIAL_NUMBERS_TEXTBOX23.Text.Replace(" ", "").Length > 0 &&
                SERIAL_NUMBERS_TEXTBOX31.Text.Replace(" ", "").Length > 0 &&
                SERIAL_NUMBERS_TEXTBOX32.Text.Replace(" ", "").Length > 0 &&
                SERIAL_NUMBERS_TEXTBOX33.Text.Replace(" ", "").Length > 0 
                )
            { CONNECT_BUTTON.IsEnabled = true; }
            else { CONNECT_BUTTON.IsEnabled = false; }
        }

        private void SN_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9およびa-f/A-Fのみ
            e.Handled = !new Regex("[0-9|a-z|A-Z]").IsMatch(e.Text);
        }

        private void SN_TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼付け場合
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9|a-z|A-Z]|[ ]").IsMatch(strTXT[cnt].ToString()))
                    {
                        // 処理済み
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        private void SN_TextBox_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string strBF = ((TextBox)sender).Text.Replace(" ", "");
            if(strBF.Length > 15)
            {
                MessageBox.Show("Enter the serial number between 1 and 15 characters, without spaces, separated by commas.");
                e.Handled = true;
            }
            SERIAL_PORTS_COMBOBOX_DropDownClosed(null, null);
        }

        private void SAVEADDRESS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SAVEADDRESS_COMBOBOX.SelectedIndex < 0) { SAVEADDRESS_COMBOBOX.SelectedIndex = 0; }
        }

        private void EXPANDER_Expanded(object sender, RoutedEventArgs e)
        {

        }
    }
}
