using MWComLibCS.ExternalControl;
using PCAB_Debugger_GUI.Properties;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PCAB_Debugger_GUI.cntConfig;
using static PCAB_Debugger_GUI.cntConfigSettings;
using static PCAB_Debugger_GUI.PCAB_SerialInterface;
using static PCAB_Debugger_GUI.PCAB_TASK;
using static PCAB_Debugger_GUI.ShowSerialPortName;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class winMain : Window
    {
        SerialPortTable[] ports;
        public clsSerialIO _io;
        private winMonitor monitor;
        private int visa32Resource;

        public winMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title += " Ver," + System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
            BOARD_GRID.IsEnabled = false;
#if DEBUG
            Settings.Default.Reset();
            this.Title += "_DEBUG MODE";
            BOARD_GRID.IsEnabled = true;
#endif
            visa32Resource = VisaControlNI.NewResourceManager();
            SERIAL_PORTS_COMBOBOX_RELOAD();
            if (SERIAL_PORTS_COMBOBOX.Items.Count > 0) { SERIAL_PORTS_COMBOBOX.SelectedIndex = 0; CONNECT_BUTTON.IsEnabled = true; }
            if (ports != null) { for (int i = 0; i < ports.Length; i++) { if (Settings.Default.spCaption == ports[i].Caption) { SERIAL_PORTS_COMBOBOX.SelectedIndex = i; break; } } }
            WAITE_TIME_TEXTBOX.Text = Settings.Default.mli.ToString("0");
            SERIAL_NUMBERS_TEXTBOX.Text = Settings.Default.sn;
            if (Settings.Default.winMainTop >= 0 &&
                (Settings.Default.winMainTop + Settings.Default.winMainHeight) <
                SystemParameters.VirtualScreenHeight)
            {
                this.Top = Settings.Default.winMainTop;
            }
            if (Settings.Default.winMainLeft >= 0 &&
                (Settings.Default.winMainLeft + Settings.Default.winMainWidth) <
                SystemParameters.VirtualScreenHeight)
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
        }

        #region Serial EVENT

        private void SERIAL_PORTS_COMBOBOX_RELOAD()
        {
            SERIAL_PORTS_COMBOBOX.Items.Clear();
            ports = GetDeviceNames();
            if (ports != null)
            {
                foreach (SerialPortTable port in ports)
                {
                    SERIAL_PORTS_COMBOBOX.Items.Add(port.Caption);
                }
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
            if (_io?.isOpen == true)
            {
                OnError(null, null);
            }
            else
            {
                string[] sn = SERIAL_NUMBERS_TEXTBOX.Text.Replace(" ", "").Split(',');
                _io = new clsSerialIO(ports[SERIAL_PORTS_COMBOBOX.SelectedIndex].Name, UInt32.Parse(BAUD_RATE_COMBOBOX.Text.Trim().Replace(",","")));
                _io.OnError += OnError;
                try
                {
                    _io.Open(sn, uint.Parse(WAITE_TIME_TEXTBOX.Text));
                }
                catch
                {
                    _io.Close();
                    SERIAL_PORTS_COMBOBOX_RELOAD();
                    SERIAL_PORTS_COMBOBOX_DropDownClosed(null, null);
                    return;
                }
                if (_io?.isOpen == true)
                {
                    monitor = new winMonitor();
                    monitor.MONITOR_GRID.Children.Clear();
                    monitor.MONITOR_GRID.RowDefinitions.Clear();
                    monitor.MONITOR_GRID.ColumnDefinitions.Clear();
                    for (int i = 0; i < _io.PCAB_Monitors.Count; i++)
                    {
                        monitor.MONITOR_GRID.ColumnDefinitions.Add(new ColumnDefinition());
                        _io.PCAB_Monitors[i].SetValue(Grid.ColumnProperty, i);
                        monitor.MONITOR_GRID.Children.Add(_io.PCAB_Monitors[i]);
                    }
                    if(_io.PCAB_Monitors.Count > 1) { monitor.MONITOR_GRID.ShowGridLines = true; }
                    monitor.Show();
                    BOARD_GRID.Children.Clear();
                    if (_io.PCAB_Boards.Count == 1)
                    {
                        BOARD_GRID.Children.Add(_io.PCAB_Boards[0]);
                        ((cntBOARD)BOARD_GRID.Children[0]).AUTO.setResourceManager = visa32Resource;
                        ((cntBOARD)BOARD_GRID.Children[0]).AUTO.ButtonClickEvent += AUTO_ButtonClickEvent;
                        ((cntBOARD)BOARD_GRID.Children[0]).CONFIG.ButtonClickEvent += CONFIG_ButtonClickEvent;
                        ((cntBOARD)BOARD_GRID.Children[0]).CONFIG.CONFIG_SETTINGS.CheckboxClickEvent += CONFIG_CONFIG_SETTINGS_CheckboxClickEventHandler;
                        if (!readConfig(((cntBOARD)BOARD_GRID.Children[0]).CONFIG.CONFIG_SETTINGS))
                        {
                            MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        BOARD_GRID.Children.Add(new TabControl());
                        for (int i = 0; i < _io.PCAB_Boards.Count; i++)
                        {
                            ((TabControl)BOARD_GRID.Children[0]).FontSize = 24;
                            ((TabControl)BOARD_GRID.Children[0]).Margin = new Thickness(5);
                            ((TabControl)BOARD_GRID.Children[0]).Items.Add(new TabItem());
                            ((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Header = "S/N, " + _io.PCAB_Boards[i].SerialNumber;
                            ((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content = _io.PCAB_Boards[i];
                            ((cntBOARD)((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content).AUTO.setResourceManager = visa32Resource;
                            ((cntBOARD)((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content).AUTO.ButtonClickEvent += AUTO_ButtonClickEvent;
                            ((cntBOARD)((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content).CONFIG.ButtonClickEvent += CONFIG_ButtonClickEvent;
                            ((cntBOARD)((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content).CONFIG.CONFIG_SETTINGS.CheckboxClickEvent += CONFIG_CONFIG_SETTINGS_CheckboxClickEventHandler;
                            if (!readConfig(((cntBOARD)((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content).CONFIG.CONFIG_SETTINGS))
                            {
                                MessageBox.Show("Config read error.\nS/N, " + _io.PCAB_Boards[i].SerialNumber, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    SERIAL_PORTS_COMBOBOX.IsEnabled = false;
                    SERIAL_CONFIG_GRID.IsEnabled = false;
                    BAUD_RATE_COMBOBOX.IsEnabled = false;
                    BOARD_GRID.IsEnabled = true;
                    CONNECT_BUTTON_CONTENT.Text = "Disconnect";

                    if(_io.PCAB_Boards.Count == 1 && _io.PCAB_Boards[0].SerialNumber == "*" &&
                        (_io.serial.PCAB_GetMode(new PCAB_UnitInterface(_io.PCAB_Boards[0].SerialNumber)) == 0x0F ||
                        _io.serial.PCAB_GetMode(new PCAB_UnitInterface(_io.PCAB_Boards[0].SerialNumber)) == 0x0A))
                    {
                        if (MessageBox.Show("Do you want to launch a binary editor?", "Binary editor", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            this.Hide();
                            Window win = new winEditor(_io.serial, new PCAB_UnitInterface(_io.PCAB_Boards[0].SerialNumber));
                            win.ShowDialog();
                            this.Show();
                        }
                    }
                }
                else
                {
                    _io.Close();
                    MessageBox.Show("No valid PCAB found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Other EVENT

        private void DEC_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9のみ
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
                MessageBox.Show("Enter in the range 0 to 65535");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("Enter in the range 0 to 65535");
                e.Handled = true;
            }
        }

        private void SN_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9およびa-f/A-Fのみ
            e.Handled = !new Regex("[0-9|a-z|A-Z|,| |*]").IsMatch(e.Text);
        }

        private void SN_TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼付け場合
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9|a-z|A-Z|,]|[ ]").IsMatch(strTXT[cnt].ToString()))
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
            try
            {
                string strBF = ((TextBox)sender).Text.Replace(" ", "");
                if(0 <= strBF.IndexOf("*") && strBF.Length != 1)
                {
                    MessageBox.Show("Multiple \"*\" specifications cannot be specified.");
                    e.Handled = true;
                    return;
                }
                string[] arrBF = strBF.Split(',');
                if (arrBF.Length > 0)
                {
                    foreach (string str in arrBF)
                    {
                        if (str.Length < 0 || 15 < str.Length) { throw new Exception(); }
                    }
                }
                else { throw new Exception(); }
                return;
            }
            catch
            {
                MessageBox.Show("Enter the serial number between 1 and 15 characters, without spaces, separated by commas.");
                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_io?.isOpen == true)
            {
                Settings.Default.visaAddr = _io.PCAB_Boards[0].AUTO.VNALOOP_VISAADDR_TEXTBOX.Text;
                Settings.Default.visaTO = long.Parse(_io.PCAB_Boards[0].AUTO.VNALOOP_TIMEOUT_TEXTBOX.Text);
                Settings.Default.fnHeader = _io.PCAB_Boards[0].AUTO.VNALOOP_FILEHEADER_TEXTBOX.Text;
                if (MessageBox.Show("Communication with PCAB\nDo you want to disconnect and exit?", "Worning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    OnError(null, null);
                }
                else { e.Cancel = true; }
            }
            Settings.Default.spCaption = SERIAL_PORTS_COMBOBOX.Text;
            Settings.Default.mli = long.Parse(WAITE_TIME_TEXTBOX.Text);
            Settings.Default.sn = SERIAL_NUMBERS_TEXTBOX.Text;
            Settings.Default.winMainTop = this.Top;
            Settings.Default.winMainLeft = this.Left;
            Settings.Default.winMainHeight = this.Height;
            Settings.Default.winMainWidth = this.Width;
            Settings.Default.Save();
        }
        #endregion

        #region Sub Function EVENT
        private void AUTO_ButtonClickEvent(object sender, RoutedEventArgs e, string dirPath)
        {
            string strSN = ((cntAUTO)sender).SerialNumber;
            bool detFLG = false;
            cntConfig conf = new cntConfig();
            foreach (cntBOARD board in _io.PCAB_Boards)
            {
                if (board.SerialNumber == strSN)
                { conf = board.CONFIG; detFLG = true; break; }
            }
            if (!detFLG) { return; }
            readConfig(conf.CONFIG_SETTINGS);
            winLoop win = new winLoop((cntAUTO)sender, _io.serial, new PCAB_UnitInterface(strSN), dirPath);
            win.ShowDialog();
            CONFIG_ButtonClickEvent(conf, null, ButtonCategory.WRITE);
        }
        private void CONFIG_ButtonClickEvent(object sender, RoutedEventArgs e, ButtonCategory category)
        {
            string strSN = ((cntConfig)sender).SerialNumber;
            uint sectorPage = 0xE0;
            uint stateNum = 0;
            string strBF = ((cntConfig)sender).SAVEADDRESS_COMBOBOX.Text;
            string[] strARR = strBF.Split('-');
            try
            {
                switch (category)
                {
                    case ButtonCategory.WRITEDSA:
                    case ButtonCategory.WRITEDPS:
                    case ButtonCategory.WRITE:
                        int dsaIN = -1;
                        List<uint> dsa = new List<uint>();
                        List<uint> dps = new List<uint>();
                        if (category == ButtonCategory.WRITE || category == ButtonCategory.WRITEDSA)
                        {
                            for (uint i = 0; i < 15; i++)
                            {
                                int d = ((cntConfig)sender).CONFIG_SETTINGS.GetDSA(i + 1);
                                if (0 <= d && d < 64) { dsa.Add((uint)d); }
                            }
                            dsaIN = ((cntConfig)sender).CONFIG_SETTINGS.GetDSA(0);
                            if (dsa.Count != 15 || dsaIN < 0 || dsaIN > 31)
                            {
                                MessageBox.Show("ATT config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        if (category == ButtonCategory.WRITE || category == ButtonCategory.WRITEDPS)
                        {
                            for (uint i = 0; i < 15; i++)
                            {
                                int d = ((cntConfig)sender).CONFIG_SETTINGS.GetDPS(i + 1);
                                if (0 <= d && d < 64) { dps.Add((uint)d); }
                            }
                            if (dps.Count != 15)
                            {
                                MessageBox.Show("Phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        if (category == ButtonCategory.WRITE || category == ButtonCategory.WRITEDSA)
                        {
                            if (!_io.serial.PCAB_WriteDSAin(new PCAB_UnitInterface(strSN), (uint)dsaIN))
                            {
                                MessageBox.Show("J1 in ATT config write error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (!_io.serial.PCAB_WriteDSA(new PCAB_UnitInterface(strSN), dsa))
                            {
                                MessageBox.Show("ATT config write error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        if (category == ButtonCategory.WRITE || category == ButtonCategory.WRITEDPS)
                        {
                            if (!_io.serial.PCAB_WriteDPS(new PCAB_UnitInterface(strSN), dps))
                            {
                                MessageBox.Show("Phase config write error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        if (category == ButtonCategory.WRITEDSA)
                        {
                            MessageBox.Show("ATT config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (category == ButtonCategory.WRITEDPS)
                        {
                            MessageBox.Show("Phase config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (category == ButtonCategory.WRITE)
                        {
                            MessageBox.Show("Config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                    case ButtonCategory.READ:
                        if (readConfig(((cntConfig)sender).CONFIG_SETTINGS))
                        {
                            MessageBox.Show("Config read done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                    case ButtonCategory.LOADMEM:
                        if (MessageBox.Show("Load configuration from memory."
                            , "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
                        if (strARR.Length == 1)
                        {
                            if (!uint.TryParse(strARR[0].Trim(), out stateNum))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        }
                        else if (strARR.Length == 2)
                        {
                            if (!uint.TryParse(strARR[0].Trim().StartsWith("0x") ? strARR[0].Trim().Substring(2) : strARR[0], System.Globalization.NumberStyles.HexNumber, null, out sectorPage))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                            if (!uint.TryParse(strARR[1].Trim(), out stateNum))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        }
                        if (_io.serial.PCAB_LoadState(new PCAB_UnitInterface(strSN), (byte)sectorPage, stateNum) == false)
                        { MessageBox.Show("Load memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        if (readConfig(((cntConfig)sender).CONFIG_SETTINGS))
                        { MessageBox.Show("Load memory done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                        else
                        { MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case ButtonCategory.SAVEMEM:
                        if (MessageBox.Show("Save configuration from memory."
                            , "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
                        if (strARR.Length == 1)
                        {
                            if (!uint.TryParse(strARR[0].Trim(), out stateNum))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        }
                        else if (strARR.Length == 2)
                        {
                            if (!uint.TryParse(strARR[0].Trim().StartsWith("0x") ? strARR[0].Trim().Substring(2) : strARR[0], System.Globalization.NumberStyles.HexNumber, null, out sectorPage))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                            if (!uint.TryParse(strARR[1].Trim(), out stateNum))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        }
                        if (_io.serial.PCAB_SaveState(new PCAB_UnitInterface(strSN), (byte)sectorPage, stateNum) == false)
                        { MessageBox.Show("Save memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        if (readConfig(((cntConfig)sender).CONFIG_SETTINGS))
                        { MessageBox.Show("Save memory done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                        else
                        { MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case ButtonCategory.RESET:
                        if (MessageBox.Show("Restore default settins."
                            , "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
                        if (_io.serial.PCAB_PRESET(new PCAB_UnitInterface(strSN)))
                        {
                            MessageBox.Show("Preset done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            if (!readConfig(((cntConfig)sender).CONFIG_SETTINGS))
                            {
                                MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Reset error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CONFIG_CONFIG_SETTINGS_CheckboxClickEventHandler(object sender, RoutedEventArgs e, CheckBoxCategory category, bool? isChecked)
        {
            string strSN = ((cntConfigSettings)sender).SerialNumber;
            bool ch;
            bool? result;
            if (isChecked == false) { ch = false; }
            else { ch = true; }
            try
            {
                switch (category)
                {
                    case CheckBoxCategory.StandbyAMP:
                        result = _io.serial.PCAB_SetSTB_AMP(new PCAB_UnitInterface(strSN), ch);
                        if (result != true) { ((cntConfigSettings)sender).CHECKBOX_Indeterminate("STBAMP", null); }
                        break;
                    case CheckBoxCategory.StandbyDRA:
                        result = _io.serial.PCAB_SetSTB_DRA(new PCAB_UnitInterface(strSN), ch);
                        if (result != true) { ((cntConfigSettings)sender).CHECKBOX_Indeterminate("STBDRA", null); }
                        break;
                    case CheckBoxCategory.StandbyLNA:
                        result = _io.serial.PCAB_SetSTB_LNA(new PCAB_UnitInterface(strSN), ch);
                        if (result != true) { ((cntConfigSettings)sender).CHECKBOX_Indeterminate("STBLNA", null); }
                        break;
                    case CheckBoxCategory.LowPowerMode:
                        result = _io.serial.PCAB_SetLowPowerMode(new PCAB_UnitInterface(strSN), ch);
                        if (result != true) { ((cntConfigSettings)sender).CHECKBOX_Indeterminate("LPM", null); }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnError(object sender, PCABEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                monitor.WindowClose();
                _io.Close();
                BOARD_GRID.Children.Clear();
                BOARD_GRID.Children.Add(new cntBOARD());
                BOARD_GRID.IsEnabled = false;
                SERIAL_PORTS_COMBOBOX.IsEnabled = true;
                SERIAL_CONFIG_GRID.IsEnabled = true;
                BAUD_RATE_COMBOBOX.IsEnabled = true;
                CONNECT_BUTTON_CONTENT.Text = "Connect";
            }));
        }
        #endregion

        private bool readConfig(cntConfigSettings conf)
        {
            try
            {
                List<uint> dsa = _io.serial.PCAB_GetDSA(new PCAB_UnitInterface(conf.SerialNumber));
                int dsaIN = _io.serial.PCAB_GetDSAin(new PCAB_UnitInterface(conf.SerialNumber));
                List<uint> dps = _io.serial.PCAB_GetDPS(new PCAB_UnitInterface(conf.SerialNumber));
                if (dsa.Count != dps.Count || dsa.Count != 15) { return false; }
                bool? result;
                result = _io.serial.PCAB_GetSTB_AMP(new PCAB_UnitInterface(conf.SerialNumber));
                switch (result)
                {
                    case true:
                        conf.CHECKBOX_Checked("STBAMP", null);
                        break;
                    case false:
                        conf.CHECKBOX_Unchecked("STBAMP", null);
                        break;
                    default:
                        conf.CHECKBOX_Indeterminate("STBAMP", null);
                        break;
                }
                result = _io.serial.PCAB_GetSTB_DRA(new PCAB_UnitInterface(conf.SerialNumber));
                switch (result)
                {
                    case true:
                        conf.CHECKBOX_Checked("STBDRA", null);
                        break;
                    case false:
                        conf.CHECKBOX_Unchecked("STBDRA", null);
                        break;
                    default:
                        conf.CHECKBOX_Indeterminate("STBDRA", null);
                        break;
                }
                result = _io.serial.PCAB_GetLowPowerMode(new PCAB_UnitInterface(conf.SerialNumber));
                switch (result)
                {
                    case true:
                        conf.CHECKBOX_Checked("LPM", null);
                        break;
                    case false:
                        conf.CHECKBOX_Unchecked("LPM", null);
                        break;
                    default:
                        conf.CHECKBOX_Indeterminate("LPM", null);
                        break;
                }
                result = _io.serial.PCAB_GetSTB_LNA(new PCAB_UnitInterface(conf.SerialNumber));
                switch (result)
                {
                    case true:
                        conf.CHECKBOX_Checked("STBLNA", null);
                        break;
                    case false:
                        conf.CHECKBOX_Unchecked("STBLNA", null);
                        break;
                    default:
                        conf.CHECKBOX_Indeterminate("STBLNA", null);
                        break;
                }
                conf.ALL_DPS_CHECKBOX.IsChecked = false;
                conf.ALL_DSA_CHECKBOX.IsChecked = false;
                conf.SetDSA(0, dsaIN);
                for(int i = 0; i < dsa.Count; i++)
                {
                    conf.SetDSA((uint)(i + 1), (int)dsa[i]);
                    conf.SetDPS((uint)(i + 1), (int)dps[i]);
                }
                return true;
            }
            catch { return false; }
        }

    }
}
