using PCAB_Debugger_GUI.Properties;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Collections;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class winMain : Window
    {
        ShowSerialPortName.SerialPortTable[] ports;

        public winMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title += " Ver," + System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
#if DEBUG
            Settings.Default.Reset();
            this.Title += "_DEBUG MODE";
#endif
            SERIAL_PORTS_COMBOBOX_RELOAD();
        }

        private void SERIAL_PORTS_COMBOBOX_RELOAD()
        {
            SERIAL_PORTS_COMBOBOX.Items.Clear();
            ports = ShowSerialPortName.GetDeviceNames();
            foreach (ShowSerialPortName.SerialPortTable port in ports)
            {
                SERIAL_PORTS_COMBOBOX.Items.Add(port.Caption);
            }
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownOpened(object sender, EventArgs e)
        {
            SERIAL_PORTS_COMBOBOX_RELOAD();
        }

        private void CONNECT_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (mod?.IsOpen == true)
            {
                if (MessageBox.Show("Disconnect communication with PCAB.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning)
                    == MessageBoxResult.OK)
                {
                    mod.Close();
                    mod = null;
                    SERIAL_PORTS_COMBOBOX.IsEnabled = true;
                    CONTL_GRID.IsEnabled = false;
                    ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Connect";
                }
                else { return; }
            }
            else
            {
                mod = new SerialPort();
                mod.PortName = ports[SERIAL_PORTS_COMBOBOX.SelectedIndex].Name;
                mod.BaudRate = 9600;
                mod.DataBits = 8;
                mod.Parity = Parity.None;
                mod.StopBits = StopBits.One;
                mod.Handshake = Handshake.None;
                mod.DtrEnable = false;
                mod.Encoding = Encoding.ASCII;
                mod.NewLine = "\n";
                mod.ReadBufferSize = 2048;
                mod.ErrorReceived += SerialErrorReceivedEventHandler;
                try { mod.Open(); }
                catch (UnauthorizedAccessException) { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); mod = null; return; }
                catch (Exception) { MessageBox.Show("Serial port open Error.\n{e.ToString()}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); mod = null; return; }
                SERIAL_PORTS_COMBOBOX.IsEnabled = false;
                CONTL_GRID.IsEnabled = true;
                ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Disconnect";
            }
        }

        private void SerialErrorReceivedEventHandler(object sender, SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show("" + sender.ToString() + e.EventType.ToString(), "Error",MessageBoxButton.OK,MessageBoxImage.Error);
            try { mod.Close(); }
            catch { }
            mod = null;
            SERIAL_PORTS_COMBOBOX.IsEnabled = true;
            CONTL_GRID.IsEnabled = false;
            ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Connect";
        }
    }
}
