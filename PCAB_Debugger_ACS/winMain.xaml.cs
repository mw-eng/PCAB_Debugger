using PCAB_Debugger_ACS.Properties;
using PCAB_Debugger_ComLib;
using System;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static PCAB_Debugger_ComLib.POS;
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
        private DateTime startTIME = new DateTime();
        NormalizedColorChart cc;
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
            startTIME = DateTime.Now;

            colorSlider.Minimum = -180;
            colorSlider.Maximum = 180;
            cc = new NormalizedColorChart(colorSlider.Minimum, colorSlider.Maximum);
            colorSlider.Minimum = -360;
            colorSlider.Maximum = 360;


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.winMainTop = this.Top;
            Settings.Default.winMainLeft = this.Left;
            Settings.Default.winMainHeight = this.Height;
            Settings.Default.winMainWidth = this.Width;
            Settings.Default.winMainMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            Settings.Default.Save();
        }

        private void RUN_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (_pos != null)
            {
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
                _pos.OnTaskError += OnTaskError;
                _pos.OnReadDAT += OnReadDAT;
                try
                {
                    _pos.Open();
                    SERIAL_PORTS_COMBOBOX.IsEnabled = false;
                    LOG_TEXTBOX.Text = "";
                    RUN_BUTTON.Content = "STOP";
                    _pos.POS_AutoTaskStart();
                }
                catch
                {
                    _pos = null;
                    SERIAL_PORTS_COMBOBOX_RELOAD(this, null);
                }
            }
        }

        private void OnTaskError(object sender, POSEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                _pos?.POS_AutoTaskStop();
                _pos?.Close();
                _pos = null;
                SERIAL_PORTS_COMBOBOX.IsEditable = true;
                RUN_BUTTON.Content = "RUN";
            }));
        }

        private void OnReadDAT(object sender, POSEventArgs e)
        {
            try
            {
                POS.PAST2 dat = new PAST2(e.ReceiveDAT);
                if (DateTime.Now - startTIME > new TimeSpan(100 * 1000))
                {
                    startTIME = DateTime.Now;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        POS_VIEWER.DATA = dat;
                    }));
                }
            }
            catch(Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LOG_TEXTBOX.Text += ex.Message + " > " + BitConverter.ToString(e.ReceiveDAT.ToArray()) + "\n";
                    LOG_TEXTBOX.ScrollToEnd();
                }));
            }
        }

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
                        ((ComboBox)sender).Items.Add(port.Caption);
                    }
                }
            }
            else
            {
                SERIAL_PORTS_COMBOBOX.Items.Clear();
                if (ports != null)
                {
                    foreach (SerialPortTable port in ports)
                    {
                        SERIAL_PORTS_COMBOBOX.Items.Add(port.Caption);
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
            if (SERIAL_PORTS_COMBOBOX.SelectedIndex >= 0) { RUN_BUTTON.IsEnabled = true; }
            else { RUN_BUTTON.IsEnabled = false; }
        }


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            colorVal.Content = colorSlider.Value;
            colorGRID.Background = new SolidColorBrush(cc.getColor(colorSlider.Value));
        }
    }
}
