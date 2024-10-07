using PCAB_Debugger_ACS.Properties;
using PCAB_Debugger_ComLib;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using static PCAB_Debugger_ComLib.cntConfigPorts;
using static PCAB_Debugger_ComLib.ShowSerialPortName;
using static PCAB_Debugger_ComLib.PCAB_TASK;
using static PCAB_Debugger_ComLib.PCAB_SerialInterface;
using System.Collections;
using System.Threading;
using static PCAB_Debugger_ComLib.POS;
using System.Windows.Controls.Primitives;

namespace PCAB_Debugger_ACS
{
    /// <summary>
    /// winMain.xaml の相互作用ロジック
    /// </summary>
    public partial class winMain : Window
    {
        private POS _pos;
        private PCAB _pcab1x;
        private PCAB _pcab2x;
        private PCAB _pcab3x;
        private SerialPortTable[] ports;
        NormalizedColorChart cc;
        private winPOS winPOSmonitor;
        private winPCAB_SensorMonitor winPCABsensor;
        private bool isControl = false;

        public winMain()
        {
#if DEBUG
            Settings.Default.Reset();
#endif
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
            CONFIG_EXPANDER.IsExpanded = true;
            BOARD_CONFIG_EXPANDER.IsExpanded = false;
            CONTROL_GRID.IsEnabled = false;

#if DEBUG
            Settings.Default.Reset();
            this.Title += "_DEBUG MODE";
            CONTROL_GRID.IsEnabled = true;
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
            VIEW_COMBOBOX.SelectedIndex = Settings.Default.view;
            SERIAL_PORTS_COMBOBOX_DropDownClosed(null, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isControl)
            {
                if (MessageBox.Show("Communication with PCAB\nDo you want to disconnect and exit?", "Worning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    DISCONNECT();
                }
            }
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
            Settings.Default.view = VIEW_COMBOBOX.SelectedIndex;
            Settings.Default.winMainTop = this.Top;
            Settings.Default.winMainLeft = this.Left;
            Settings.Default.winMainHeight = this.Height;
            Settings.Default.winMainWidth = this.Width;
            Settings.Default.winMainMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            Settings.Default.Save();
        }

        private void DISCONNECT()
        {
            _pos?.POS_AutoTaskStop();
            _pos?.Close();
            winPOSmonitor?.WindowClose();
            PTU11.Children.Clear();
            PTU12.Children.Clear();
            PTU13.Children.Clear();
            PTU21.Children.Clear();
            PTU22.Children.Clear();
            PTU23.Children.Clear();
            PTU31.Children.Clear();
            PTU32.Children.Clear();
            PTU33.Children.Clear();
            _pcab1x?.Close();
            _pcab2x?.Close();
            _pcab3x?.Close();
            winPCABsensor?.WindowClose();
            _pos = null;
            _pcab1x = null;
            _pcab2x = null;
            _pcab3x = null;
            CONFIG_EXPANDER.IsExpanded = true;
            CONFIG_GRID.IsEnabled = true;
            CONTROL_GRID.IsEnabled = false;
            BOARD_CONFIG_EXPANDER.IsExpanded = false;
            CONNECT_BUTTON_CONTENT.Text = "Connect";
            isControl = false;
        }

        private void CONNECT_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (isControl) { DISCONNECT();}
            else
            {
                foreach (SerialPortTable port in GetDeviceNames())
                {
                    if (port.Caption == SERIAL_PORTS_COMBOBOX0.Text)
                    {
                        _pos = new POS(port.Name, int.Parse(BAUD_RATE_COMBOBOX0.Text.Trim().Replace(",", "")));
                    }
                    if (port.Caption == SERIAL_PORTS_COMBOBOX1.Text)
                    {
                        _pcab1x = new PCABs(port.Name, UInt32.Parse(BAUD_RATE_COMBOBOX1.Text.Trim().Replace(",", "")));
                    }
                    if (port.Caption == SERIAL_PORTS_COMBOBOX2.Text)
                    {
                        _pcab2x = new PCABs(port.Name, UInt32.Parse(BAUD_RATE_COMBOBOX2.Text.Trim().Replace(",", "")));
                    }
                    if (port.Caption == SERIAL_PORTS_COMBOBOX3.Text)
                    {
                        _pcab3x = new PCABs(port.Name, UInt32.Parse(BAUD_RATE_COMBOBOX3.Text.Trim().Replace(",", "")));
                    }
                }
                if(_pos != null && _pcab1x != null && _pcab2x != null && _pcab3x != null)
                {
                    ROTATE ang1;
                    ROTATE ang2;
                    if (VIEW_COMBOBOX.SelectedIndex == 0) { ang1 = ROTATE.RIGHT_TURN; ang2 = ROTATE.ZERO; }
                    else if (VIEW_COMBOBOX.SelectedIndex == 1) { ang1 = ROTATE.MIRROR_RIGHT_TURN; ang2 = ROTATE.MIRROR_ZERO; }
                    else{ ang1 = ROTATE.MATRIX; ang2 = ROTATE.MATRIX; }
                    List<PCABs.SN_POSI> sn1x = new List<PCABs.SN_POSI>();
                    sn1x.Add(new PCABs.SN_POSI(SERIAL_NUMBERS_TEXTBOX11.Text, ang1));
                    sn1x.Add(new PCABs.SN_POSI(SERIAL_NUMBERS_TEXTBOX12.Text, ang1));
                    sn1x.Add(new PCABs.SN_POSI(SERIAL_NUMBERS_TEXTBOX13.Text, ang1));
                    _pcab1x.OnError += PCAB_OnError;
                    _pcab1x.Open(sn1x, 100);
                    if (_pcab1x.isOpen != true || _pcab1x.serial.UNITs.Count != 3)
                    {
                        MessageBox.Show("Seriao Number detection error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _pcab1x.Close();
                        _pos = null;
                        _pcab1x = null;
                        _pcab2x = null;
                        _pcab3x = null;
                    }
                    List<PCABs.SN_POSI> sn2x = new List<PCABs.SN_POSI>();
                    sn2x.Add(new PCABs.SN_POSI(SERIAL_NUMBERS_TEXTBOX21.Text, ang2));
                    sn2x.Add(new PCABs.SN_POSI(SERIAL_NUMBERS_TEXTBOX22.Text, ang2));
                    sn2x.Add(new PCABs.SN_POSI(SERIAL_NUMBERS_TEXTBOX23.Text, ang2));
                    _pcab2x.OnError += PCAB_OnError;
                    _pcab2x.Open(sn2x, 100);
                    if (_pcab2x.isOpen != true || _pcab2x.serial.UNITs.Count != 3)
                    {
                        MessageBox.Show("Seriao Number detection error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _pcab1x.Close();
                        _pcab2x.Close();
                        _pos = null;
                        _pcab1x = null;
                        _pcab2x = null;
                        _pcab3x = null;
                    }
                    List<PCABs.SN_POSI> sn3x = new List<PCABs.SN_POSI>();
                    sn3x.Add(new PCABs.SN_POSI(SERIAL_NUMBERS_TEXTBOX31.Text, ang2));
                    sn3x.Add(new PCABs.SN_POSI(SERIAL_NUMBERS_TEXTBOX32.Text, ang2));
                    sn3x.Add(new PCABs.SN_POSI(SERIAL_NUMBERS_TEXTBOX33.Text, ang2));
                    _pcab3x.OnError += PCAB_OnError;
                    _pcab3x.Open(sn3x, 100);
                    if (_pcab3x.isOpen != true || _pcab3x.serial.UNITs.Count != 3)
                    {
                        MessageBox.Show("Seriao Number detection error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _pcab1x.Close();
                        _pcab2x.Close();
                        _pcab3x.Close();
                        _pos = null;
                        _pcab1x = null;
                        _pcab2x = null;
                        _pcab3x = null;
                    }
                    _pcab1x.PCAB_Boards[0].STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                    _pcab1x.PCAB_Boards[1].STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                    _pcab1x.PCAB_Boards[2].STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                    _pcab2x.PCAB_Boards[0].STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                    _pcab2x.PCAB_Boards[1].STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                    _pcab2x.PCAB_Boards[2].STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                    _pcab3x.PCAB_Boards[0].STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                    _pcab3x.PCAB_Boards[1].STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                    _pcab3x.PCAB_Boards[2].STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                    _pcab1x.PCAB_Monitors[0].TITLE = "PTU11";
                    _pcab1x.PCAB_Monitors[1].TITLE = "PTU12";
                    _pcab1x.PCAB_Monitors[2].TITLE = "PTU13";
                    _pcab2x.PCAB_Monitors[0].TITLE = "PTU21";
                    _pcab2x.PCAB_Monitors[1].TITLE = "PTU22";
                    _pcab2x.PCAB_Monitors[2].TITLE = "PTU23";
                    _pcab3x.PCAB_Monitors[0].TITLE = "PTU31";
                    _pcab3x.PCAB_Monitors[1].TITLE = "PTU32";
                    _pcab3x.PCAB_Monitors[2].TITLE = "PTU33";

                    PTU11.Children.Clear();
                    PTU12.Children.Clear();
                    PTU13.Children.Clear();
                    PTU21.Children.Clear();
                    PTU22.Children.Clear();
                    PTU23.Children.Clear();
                    PTU31.Children.Clear();
                    PTU32.Children.Clear();
                    PTU33.Children.Clear();
                    PTU11.Children.Add(_pcab1x.PCAB_Boards[0]);
                    PTU12.Children.Add(_pcab1x.PCAB_Boards[1]);
                    PTU13.Children.Add(_pcab1x.PCAB_Boards[2]);
                    PTU21.Children.Add(_pcab2x.PCAB_Boards[0]);
                    PTU22.Children.Add(_pcab2x.PCAB_Boards[1]);
                    PTU23.Children.Add(_pcab2x.PCAB_Boards[2]);
                    PTU31.Children.Add(_pcab3x.PCAB_Boards[0]);
                    PTU32.Children.Add(_pcab3x.PCAB_Boards[1]);
                    PTU33.Children.Add(_pcab3x.PCAB_Boards[2]);

                    winPCABsensor = new winPCAB_SensorMonitor();
                    winPCABsensor.PTU11.Children.Add(_pcab1x.PCAB_Monitors[0]);
                    winPCABsensor.PTU12.Children.Add(_pcab1x.PCAB_Monitors[1]);
                    winPCABsensor.PTU13.Children.Add(_pcab1x.PCAB_Monitors[2]);
                    winPCABsensor.PTU21.Children.Add(_pcab2x.PCAB_Monitors[0]);
                    winPCABsensor.PTU22.Children.Add(_pcab2x.PCAB_Monitors[1]);
                    winPCABsensor.PTU23.Children.Add(_pcab2x.PCAB_Monitors[2]);
                    winPCABsensor.PTU31.Children.Add(_pcab3x.PCAB_Monitors[0]);
                    winPCABsensor.PTU32.Children.Add(_pcab3x.PCAB_Monitors[1]);
                    winPCABsensor.PTU33.Children.Add(_pcab3x.PCAB_Monitors[2]);
                    winPCABsensor.Show();

                    winPOSmonitor = new winPOS();
                    _pos.OnReadDAT += winPOSmonitor.OnReadDAT;
                    _pos.OnTaskError += POS_OnError;
                    _pos.Open();
                    _pos.POS_AutoTaskStart();
                    winPOSmonitor.Show();

                    CONFIG_EXPANDER.IsExpanded = false;
                    CONFIG_GRID.IsEnabled = false;
                    CONTROL_GRID.IsEnabled = true;
                    BOARD_CONFIG_EXPANDER.IsExpanded = true;
                    CONNECT_BUTTON_CONTENT.Text = "Disconnect";
                    isControl = true;
                }
                else
                {
                    _pos = null;
                    _pcab1x = null;
                    _pcab2x = null;
                    _pcab3x = null;
                    MessageBox.Show("Serial port detection error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SERIAL_PORTS_COMBOBOX_RELOAD(null, null);
                }
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
            double thisHeight = this.Height;
            if(this.WindowState == WindowState.Maximized) { thisHeight = System.Windows.SystemParameters.WorkArea.Height - 50; }
            if (sender == CONFIG_EXPANDER) { CONFIG_GRID.Height = thisHeight * 0.7; }
            if (sender == BOARD_CONFIG_EXPANDER){ BOARD_CONFIG_GRID.Height = thisHeight * 0.7;}
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double thisHeight = this.Height;
            if (this.WindowState == WindowState.Maximized) { thisHeight = System.Windows.SystemParameters.WorkArea.Height - 50; }
            if (CONFIG_EXPANDER.IsExpanded) { CONFIG_GRID.Height = thisHeight * 0.7; }
            if (BOARD_CONFIG_EXPANDER.IsExpanded) { BOARD_CONFIG_GRID.Height = thisHeight * 0.7; }
        }

        private void STBLNA_CheckboxClick(object sender, RoutedEventArgs e, bool? isChecked)
        {
            bool ic;
            if (isChecked == true) { ic = true; }
            else if (isChecked == false) { ic = false; }
            else { return; }
            try
            {
                switch (((Grid)((cntConfigPorts)sender).Parent).Name)
                {
                    case "PTU11":
                        _pcab1x.serial.PCAB_SetSTB_LNA(_pcab1x.serial.UNITs[0], ic);
                        break;
                    case "PTU12":
                        _pcab1x.serial.PCAB_SetSTB_LNA(_pcab1x.serial.UNITs[1], ic);
                        break;
                    case "PTU13":
                        _pcab1x.serial.PCAB_SetSTB_LNA(_pcab1x.serial.UNITs[2], ic);
                        break;
                    case "PTU21":
                        _pcab2x.serial.PCAB_SetSTB_LNA(_pcab2x.serial.UNITs[0], ic);
                        break;
                    case "PTU22":
                        _pcab2x.serial.PCAB_SetSTB_LNA(_pcab2x.serial.UNITs[1], ic);
                        break;
                    case "PTU23":
                        _pcab2x.serial.PCAB_SetSTB_LNA(_pcab2x.serial.UNITs[2], ic);
                        break;
                    case "PTU31":
                        _pcab3x.serial.PCAB_SetSTB_LNA(_pcab3x.serial.UNITs[0], ic);
                        break;
                    case "PTU32":
                        _pcab3x.serial.PCAB_SetSTB_LNA(_pcab3x.serial.UNITs[1], ic);
                        break;
                    case "PTU33":
                        _pcab3x.serial.PCAB_SetSTB_LNA(_pcab3x.serial.UNITs[2], ic);
                        break;
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
        }

        private void STB_LPM_CheckboxClick(object sender, RoutedEventArgs e)
        {
            bool ic;
            if(e.RoutedEvent.Name == "Checked") { ic= true; }
            else if(e.RoutedEvent.Name == "Unchecked") { ic= false; }
            else { return; }
            if (sender == STBAMP_CHECKBOX)
            {
                foreach (PCAB_UnitInterface unit in _pcab1x.serial.UNITs) { _pcab1x.serial.PCAB_SetSTB_AMP(unit, ic); }
                foreach (PCAB_UnitInterface unit in _pcab2x.serial.UNITs) { _pcab2x.serial.PCAB_SetSTB_AMP(unit, ic); }
                foreach (PCAB_UnitInterface unit in _pcab3x.serial.UNITs) { _pcab3x.serial.PCAB_SetSTB_AMP(unit, ic); }
            }
            if (sender == STBDRA_CHECKBOX)
            {
                foreach (PCAB_UnitInterface unit in _pcab1x.serial.UNITs) { _pcab1x.serial.PCAB_SetSTB_DRA(unit, ic); }
                foreach (PCAB_UnitInterface unit in _pcab2x.serial.UNITs) { _pcab2x.serial.PCAB_SetSTB_DRA(unit, ic); }
                foreach (PCAB_UnitInterface unit in _pcab3x.serial.UNITs) { _pcab3x.serial.PCAB_SetSTB_DRA(unit, ic); }
            }
            if (sender == SETLPM_CHECKBOX)
            {
                foreach (PCAB_UnitInterface unit in _pcab1x.serial.UNITs) { _pcab1x.serial.PCAB_SetLowPowerMode(unit, ic); }
                foreach (PCAB_UnitInterface unit in _pcab2x.serial.UNITs) { _pcab2x.serial.PCAB_SetLowPowerMode(unit, ic); }
                foreach (PCAB_UnitInterface unit in _pcab3x.serial.UNITs) { _pcab3x.serial.PCAB_SetLowPowerMode(unit, ic); }
            }
        }

        private void WRITEDSA_Click(object sender, RoutedEventArgs e)
        {
            List<uint> dsa = new List<uint>();
            dsa.Clear();
            for (uint i = 0; i < 15; i++)
            { dsa.Add((uint)((cntConfigPorts)PTU11.Children[0]).GetDSA(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDSAin(_pcab1x.serial.UNITs[0], (uint)((cntConfigPorts)PTU11.Children[0]).GetDSA(0)))
            { MessageBox.Show("PTU11 DSA in write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (_pcab1x.serial.PCAB_WriteDSA(_pcab1x.serial.UNITs[0], dsa))
            { MessageBox.Show("PTU11 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dsa.Clear();
            for (uint i = 0; i < 15; i++)
            { dsa.Add((uint)((cntConfigPorts)PTU12.Children[0]).GetDSA(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDSAin(_pcab1x.serial.UNITs[0], (uint)((cntConfigPorts)PTU12.Children[0]).GetDSA(0)))
            { MessageBox.Show("PTU12 DSA in write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (_pcab1x.serial.PCAB_WriteDSA(_pcab1x.serial.UNITs[0], dsa))
            { MessageBox.Show("PTU12 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dsa.Clear();
            for (uint i = 0; i < 15; i++)
            { dsa.Add((uint)((cntConfigPorts)PTU13.Children[0]).GetDSA(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDSAin(_pcab1x.serial.UNITs[0], (uint)((cntConfigPorts)PTU13.Children[0]).GetDSA(0)))
            { MessageBox.Show("PTU13 DSA in write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (_pcab1x.serial.PCAB_WriteDSA(_pcab1x.serial.UNITs[0], dsa))
            { MessageBox.Show("PTU13 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dsa.Clear();
            for (uint i = 0; i < 15; i++)
            { dsa.Add((uint)((cntConfigPorts)PTU21.Children[0]).GetDSA(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDSAin(_pcab2x.serial.UNITs[0], (uint)((cntConfigPorts)PTU21.Children[0]).GetDSA(0)))
            { MessageBox.Show("PTU21 DSA in write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (_pcab1x.serial.PCAB_WriteDSA(_pcab2x.serial.UNITs[0], dsa))
            { MessageBox.Show("PTU21 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dsa.Clear();
            for (uint i = 0; i < 15; i++)
            { dsa.Add((uint)((cntConfigPorts)PTU22.Children[0]).GetDSA(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDSAin(_pcab2x.serial.UNITs[0], (uint)((cntConfigPorts)PTU22.Children[0]).GetDSA(0)))
            { MessageBox.Show("PTU22 DSA in write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (_pcab1x.serial.PCAB_WriteDSA(_pcab2x.serial.UNITs[0], dsa))
            { MessageBox.Show("PTU22 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dsa.Clear();
            for (uint i = 0; i < 15; i++)
            { dsa.Add((uint)((cntConfigPorts)PTU23.Children[0]).GetDSA(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDSAin(_pcab2x.serial.UNITs[0], (uint)((cntConfigPorts)PTU23.Children[0]).GetDSA(0)))
            { MessageBox.Show("PTU23 DSA in write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (_pcab1x.serial.PCAB_WriteDSA(_pcab2x.serial.UNITs[0], dsa))
            { MessageBox.Show("PTU23 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dsa.Clear();
            for (uint i = 0; i < 15; i++)
            { dsa.Add((uint)((cntConfigPorts)PTU31.Children[0]).GetDSA(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDSAin(_pcab3x.serial.UNITs[0], (uint)((cntConfigPorts)PTU31.Children[0]).GetDSA(0)))
            { MessageBox.Show("PTU31 DSA in write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (_pcab1x.serial.PCAB_WriteDSA(_pcab3x.serial.UNITs[0], dsa))
            { MessageBox.Show("PTU31 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dsa.Clear();
            for (uint i = 0; i < 15; i++)
            { dsa.Add((uint)((cntConfigPorts)PTU32.Children[0]).GetDSA(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDSAin(_pcab3x.serial.UNITs[0], (uint)((cntConfigPorts)PTU32.Children[0]).GetDSA(0)))
            { MessageBox.Show("PTU32 DSA in write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (_pcab1x.serial.PCAB_WriteDSA(_pcab3x.serial.UNITs[0], dsa))
            { MessageBox.Show("PTU32 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dsa.Clear();
            for (uint i = 0; i < 15; i++)
            { dsa.Add((uint)((cntConfigPorts)PTU33.Children[0]).GetDSA(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDSAin(_pcab3x.serial.UNITs[0], (uint)((cntConfigPorts)PTU33.Children[0]).GetDSA(0)))
            { MessageBox.Show("PTU33 DSA in write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (_pcab1x.serial.PCAB_WriteDSA(_pcab3x.serial.UNITs[0], dsa))
            { MessageBox.Show("PTU33 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if(sender == WRITEDSA)
            { MessageBox.Show("ATT config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
        }

        private void WRITEDPS_Click(object sender, RoutedEventArgs e)
        {
            List<uint> dps = new List<uint>();
            dps.Clear();
            for (uint i = 0; i < 15; i++)
            { dps.Add((uint)((cntConfigPorts)PTU11.Children[0]).GetDPS(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDPS(_pcab1x.serial.UNITs[0], dps))
            { MessageBox.Show("PTU11 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dps.Clear();
            for (uint i = 0; i < 15; i++)
            { dps.Add((uint)((cntConfigPorts)PTU12.Children[0]).GetDPS(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDPS(_pcab1x.serial.UNITs[0], dps))
            { MessageBox.Show("PTU12 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dps.Clear();
            for (uint i = 0; i < 15; i++)
            { dps.Add((uint)((cntConfigPorts)PTU13.Children[0]).GetDPS(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDPS(_pcab1x.serial.UNITs[0], dps))
            { MessageBox.Show("PTU13 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dps.Clear();
            for (uint i = 0; i < 15; i++)
            { dps.Add((uint)((cntConfigPorts)PTU21.Children[0]).GetDPS(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDPS(_pcab2x.serial.UNITs[0], dps))
            { MessageBox.Show("PTU21 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dps.Clear();
            for (uint i = 0; i < 15; i++)
            { dps.Add((uint)((cntConfigPorts)PTU22.Children[0]).GetDPS(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDPS(_pcab2x.serial.UNITs[0], dps))
            { MessageBox.Show("PTU22 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dps.Clear();
            for (uint i = 0; i < 15; i++)
            { dps.Add((uint)((cntConfigPorts)PTU23.Children[0]).GetDPS(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDPS(_pcab2x.serial.UNITs[0], dps))
            { MessageBox.Show("PTU23 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dps.Clear();
            for (uint i = 0; i < 15; i++)
            { dps.Add((uint)((cntConfigPorts)PTU31.Children[0]).GetDPS(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDPS(_pcab3x.serial.UNITs[0], dps))
            { MessageBox.Show("PTU31 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dps.Clear();
            for (uint i = 0; i < 15; i++)
            { dps.Add((uint)((cntConfigPorts)PTU32.Children[0]).GetDPS(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDPS(_pcab3x.serial.UNITs[0], dps))
            { MessageBox.Show("PTU32 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            dps.Clear();
            for (uint i = 0; i < 15; i++)
            { dps.Add((uint)((cntConfigPorts)PTU33.Children[0]).GetDPS(i + 1)); }
            if (_pcab1x.serial.PCAB_WriteDPS(_pcab3x.serial.UNITs[0], dps))
            { MessageBox.Show("PTU33 DSA write Failure.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (sender == WRITEDPS)
            { MessageBox.Show("Phase config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
        }

        private void WRITE_Click(object sender, RoutedEventArgs e)
        {
            WRITEDSA_Click(sender, e);
            WRITEDPS_Click(sender, e);
            MessageBox.Show("Config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void READ_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LOADDMEM_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SAVEMEM_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PCAB_OnError(object sender, PCABEventArgs e)
        {

        }

        private void POS_OnError(object sender, POSEventArgs e)
        {

        }

        private void readConfig()
        {

        }

        private void writeConfig()
        {

        }
    }
}
