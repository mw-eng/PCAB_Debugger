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
using static PCAB_Debugger_GUI.PCAB;
using System.Text.RegularExpressions;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class winMain : Window
    {
        ShowSerialPortName.SerialPortTable[] ports;
        PCAB _mod;
        bool _state;

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
            _state = false;
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

        private void SERIAL_PORTS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if(SERIAL_PORTS_COMBOBOX.SelectedIndex < 0) { CONNECT_BUTTON.IsEnabled = false; }
            else { CONNECT_BUTTON.IsEnabled = true; }
        }

        private void CONNECT_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (_state)
            {
                _mod.PCAB_AutoTaskStop();
                _mod = null;
                _state = false;
                SERIAL_PORTS_COMBOBOX.IsEnabled = true;
                WAITE_TIME_TEXTBOX.IsEnabled = true;
                INIT_CHECKBOX.IsEnabled = true;
                CONTL_GRID.IsEnabled = false;
                ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Connect";
            }
            else
            {
                _mod = new PCAB(ports[SERIAL_PORTS_COMBOBOX.SelectedIndex].Name);
                _mod.OnUpdateDAT += OnUpdateDAT;
                _mod.OnError += OnError;
                if (_mod.PCAB_AutoTaskStart(uint.Parse(WAITE_TIME_TEXTBOX.Text),INIT_CHECKBOX.IsChecked))
                {
                    SERIAL_PORTS_COMBOBOX.IsEnabled = false;
                    WAITE_TIME_TEXTBOX.IsEnabled = false;
                    INIT_CHECKBOX.IsEnabled = false;
                    CONTL_GRID.IsEnabled = true;
                    _state = true;
                    ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Disconnect";
                    string stb;
                    stb = _mod.PCAB_CMD("GetSTB.AMP", 1);
                    if (stb == "STB") { CHECKBOX_Checked("STBAMP", null); } else { CHECKBOX_Unchecked("STBAMP", null); }
                    stb = _mod.PCAB_CMD("GetSTB.DRA", 1);
                    if (stb == "STB") { CHECKBOX_Checked("STBDRA", null); } else { CHECKBOX_Unchecked("STBDRA", null); }
                    stb = _mod.PCAB_CMD("GetSTB.LNA", 1);
                    if (stb == "STB") { CHECKBOX_Checked("STBLNA", null); } else { CHECKBOX_Unchecked("STBLNA", null); }
                    stb = _mod.PCAB_CMD("GetLPM", 1);
                    if (stb == "LOW") { CHECKBOX_Checked("LPM", null); } else { CHECKBOX_Unchecked("LPM", null); }
                    stb = _mod.PCAB_CMD("GetALD", 1);
                    if (stb == "ENB") { CHECKBOX_Checked("ALD", null); } else { CHECKBOX_Unchecked("ALD", null); }
                }
            }
        }


        private void READMEM_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SAVEMEM_Click(object sender, RoutedEventArgs e)
        {

        }

        private void WRITEPS_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            if(typeof(CheckBox) == sender.GetType())
            {

            }
            else
            {
                switch ((string)sender)
                {
                    case "STBAMP":
                        STBAMP_CHECKBOX.IsChecked = true;
                        break;
                    case "STBDRA":
                        STBDRA_CHECKBOX.IsChecked = true;
                        break;
                    case "STBLNA":
                        STBLNA_CHECKBOX.IsChecked = true;
                        break;
                    case "LPM":
                        SETLPM_CHECKBOX.IsChecked = true;
                        break;
                    case "ALD":
                        SETALD_CHECKBOX.IsChecked = true;
                        break;
                    default: break;
                }
            }
        }

        private void CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            if (typeof(CheckBox) == sender.GetType())
            {

            }
            else
            {
                switch ((string)sender)
                {
                    case "STBAMP":
                        STBAMP_CHECKBOX.IsChecked = false;
                        break;
                    case "STBDRA":
                        STBDRA_CHECKBOX.IsChecked = false;
                        break;
                    case "STBLNA":
                        STBLNA_CHECKBOX.IsChecked = false;
                        break;
                    case "LPM":
                        SETLPM_CHECKBOX.IsChecked = false;
                        break;
                    case "ALD":
                        SETALD_CHECKBOX.IsChecked = false;
                        break;
                    default: break;
                }
            }
        }
        private void OnError(object sender, PCABEventArgs e)
        {
            _mod.PCAB_AutoTaskStop();
            MessageBox.Show(e.Message.ToString(), "Error",MessageBoxButton.OK,MessageBoxImage.Error);
            _state = false;
            _mod = null;
            SERIAL_PORTS_COMBOBOX.IsEnabled = true;
            CONTL_GRID.IsEnabled = false;
            ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Connect";
        }

        private void OnUpdateDAT(object sender, PCABEventArgs e)
        {
            if(e.ReceiveDAT.Length != 3) { return; }
            if (_mod != null)
            {
                _mod.Id_now = e.ReceiveDAT[0];
                _mod.Vd_now = e.ReceiveDAT[1];
                _mod.TEMP_now = e.ReceiveDAT[2];
            }
            string[] arrBf = e.ReceiveDAT[2].Trim(',').Split(',');
            List<string> list = new List<string>();
            int cnt = 0;
            foreach(string strBf in arrBf)
            {
                string[] bf = strBf.Split(':');
                list.Add(bf[1]);
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SNS_ID_LABEL.Content = ((double.Parse(e.ReceiveDAT[0]) - 1.65) / 0.09).ToString("0.00");
                SNS_VD_LABEL.Content = (double.Parse(e.ReceiveDAT[1]) * 0.099).ToString("0.00");
                TEMP01.Content = (double.Parse(list[0])).ToString("0.00");
                TEMP02.Content = (double.Parse(list[1])).ToString("0.00");
                TEMP03.Content = (double.Parse(list[2])).ToString("0.00");
                TEMP04.Content = (double.Parse(list[3])).ToString("0.00");
                TEMP05.Content = (double.Parse(list[4])).ToString("0.00");
                TEMP06.Content = (double.Parse(list[5])).ToString("0.00");
                TEMP07.Content = (double.Parse(list[6])).ToString("0.00");
                TEMP08.Content = (double.Parse(list[7])).ToString("0.00");
                TEMP09.Content = (double.Parse(list[8])).ToString("0.00");
                TEMP10.Content = (double.Parse(list[9])).ToString("0.00");
                TEMP11.Content = (double.Parse(list[10])).ToString("0.00");
                TEMP12.Content = (double.Parse(list[11])).ToString("0.00");
                TEMP13.Content = (double.Parse(list[12])).ToString("0.00");
                TEMP14.Content = (double.Parse(list[13])).ToString("0.00");
                TEMP15.Content = (double.Parse(list[14])).ToString("0.00");
            }));
        }

        private void DEC_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9およびa-f/A-Fのみ
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void DEC_TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼付け場合
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9]|[ ]").IsMatch(strTXT[cnt].ToString()))
                    {
                        // 処理済み
                        e.Handled = true;
                        break;
                    }
                }
            }
        }
        private void DEC_TextBox_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                uint uintVal = Convert.ToUInt32(((TextBox)sender).Text);
                if (0 <= uintVal && uintVal <= 65535) { return; }
                MessageBox.Show("0-65535の範囲で入力してください");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("0-65535の範囲で入力してください");
                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_state)
            {
                if (MessageBox.Show("Communication with PCAB\nDo you want to disconnect and exit?", "Worning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    _mod.PCAB_AutoTaskStop(); _mod = null; _state = false;
                }
                else { e.Cancel = true; }
            }
        }
    }
}
