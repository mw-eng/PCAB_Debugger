using PCAB_Debugger_ACS.Properties;
using PCAB_Debugger_ComLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using static PCAB_Debugger_ComLib.cntConfigPorts;
using static PCAB_Debugger_ComLib.PCAB_SerialInterface;
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
        private PANEL _ptp;
        private SerialPortTable[] ports;
        NormalizedColorChart cc;
        private winPOS winPOSmonitor;
        private winPCAB_SensorMonitor winPCABsensor;
        private winPCAB_PhaseMonitor winPCABphase;
        private bool isControl = false;
        private StreamWriter posFS;
        private StreamWriter snsSP1FS;
        private StreamWriter snsSP2FS;
        private StreamWriter snsSP3FS;
        private StreamWriter writeFS;
        private StreamWriter errFS;
        private bool saveLOG = false;

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
            SAVELOGs_CHECKBOX.IsChecked = Settings.Default.saveLogs;
            LogDirPath_TextBox.Text = Settings.Default.logSaveDirPath;
            INTERVAL_TIME_TEXTBOX.Text = Settings.Default.snsIntTime.ToString("0");
            CALCULATE_FREQUENCY_TEXTBOX.Text = Settings.Default.FreqMHz.ToString("0");

            SERIAL_PORTS_COMBOBOX_DropDownClosed(null, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isControl)
            {
                if (MessageBox.Show("Communication with PCAB\nDo you want to disconnect and exit?", "Worning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    DISCONNECT(true);
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
            if(SAVELOGs_CHECKBOX.IsChecked == true) { Settings.Default.saveLogs = true; }
            else { Settings.Default.saveLogs = false; }
            Settings.Default.logSaveDirPath = LogDirPath_TextBox.Text;
            Settings.Default.snsIntTime = uint.Parse(INTERVAL_TIME_TEXTBOX.Text);
            Settings.Default.FreqMHz = uint.Parse(CALCULATE_FREQUENCY_TEXTBOX.Text);
            Settings.Default.winMainTop = this.Top;
            Settings.Default.winMainLeft = this.Left;
            Settings.Default.winMainHeight = this.Height;
            Settings.Default.winMainWidth = this.Width;
            Settings.Default.winMainMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            Settings.Default.Save();
        }

        private void DISCONNECT(bool wait)
        {
            _pos?.POS_AutoTaskStop(wait);
            _ptp?.PANEL_SensorMonitor_TASK_Stop(wait);
            PTU11.Children.Clear();
            PTU12.Children.Clear();
            PTU13.Children.Clear();
            PTU21.Children.Clear();
            PTU22.Children.Clear();
            PTU23.Children.Clear();
            PTU31.Children.Clear();
            PTU32.Children.Clear();
            PTU33.Children.Clear();
            winPOSmonitor?.WindowClose();
            winPCABsensor?.WindowClose();
            winPCABphase?.WindowClose();
            _pos?.Close();
            _ptp?.Close();
            _pos = null;
            _ptp = null;
            if(SAVELOGs_CHECKBOX.IsChecked == true)
            {
                posFS.Close();
                snsSP1FS.Close();
                snsSP2FS.Close();
                snsSP3FS.Close();
                writeFS.Close();
                errFS.Close();
            }
            CONFIG_EXPANDER.IsExpanded = true;
            CONFIG_GRID.IsEnabled = true;
            CONTROL_GRID.IsEnabled = false;
            BOARD_CONFIG_EXPANDER.IsExpanded = false;
            CONNECT_BUTTON_CONTENT.Text = "Connect";
            isControl = false;
        }

        #region CONTROL CONFIG BUTTON EVENTs

        private void CONNECT_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (isControl) { DISCONNECT(true); }
            else
            {
                if (SAVELOGs_CHECKBOX.IsChecked == true)
                {
                    saveLOG = true;
                    if (!Directory.Exists(LogDirPath_TextBox.Text))
                    {
                        MessageBox.Show("The log destination folder does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                string portConfFilePath = "";
                using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
                {
                    ofd.Title = "Select the ports configuration file";
                    ofd.FileName = "OffsetDAT.csv";
                    ofd.Filter = "csv(*.csv)|*.csv|All files(*.*)|*.*";
                    ofd.FilterIndex = 1;
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        portConfFilePath = ofd.FileName;
                    }
                    else
                    {
                        return;
                    }
                }

                string sp1Name = "";
                string sp2Name = "";
                string sp3Name = "";
                foreach (SerialPortTable port in GetDeviceNames())
                {
                    if (port.Caption == SERIAL_PORTS_COMBOBOX0.Text)
                    {
                        _pos = new POS(port.Name, int.Parse(BAUD_RATE_COMBOBOX0.Text.Trim().Replace(",", "")));
                    }
                    if (port.Caption == SERIAL_PORTS_COMBOBOX1.Text)
                    {
                        sp1Name = port.Name;
                    }
                    if (port.Caption == SERIAL_PORTS_COMBOBOX2.Text)
                    {
                        sp2Name = port.Name;
                    }
                    if (port.Caption == SERIAL_PORTS_COMBOBOX3.Text)
                    {
                        sp3Name = port.Name;
                    }
                }
                if (_pos != null && !string.IsNullOrWhiteSpace(sp1Name) && !string.IsNullOrWhiteSpace(sp2Name) && !string.IsNullOrWhiteSpace(sp3Name))
                {
                    DateTime dt = DateTime.Now;
                    Encoding enc = Encoding.UTF8;
                    if (SAVELOGs_CHECKBOX.IsChecked == true)
                    {
                        posFS = new StreamWriter(LogDirPath_TextBox.Text + "//POS_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
                        snsSP1FS = new StreamWriter(LogDirPath_TextBox.Text + "//" + sp1Name + "_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
                        snsSP2FS = new StreamWriter(LogDirPath_TextBox.Text + "//" + sp2Name + "_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
                        snsSP3FS = new StreamWriter(LogDirPath_TextBox.Text + "//" + sp3Name + "_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
                        writeFS = new StreamWriter(LogDirPath_TextBox.Text + "//CMD_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
                        errFS = new StreamWriter(LogDirPath_TextBox.Text + "//ERR_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
                        #region Header Write
                        posFS.WriteLine(this.Title);
                        posFS.WriteLine("POS Data Update event log");
                        posFS.WriteLine("");
                        posFS.WriteLine("Date Time,Time of Validity [sec],Speed [m/s],Track [deg],Latitude,Longitude,Altitude [m],Roll [deg],Pitch [deg],Hedading [deg],Long Accel [m/s^2],Tran Accel [m/s^2],Down Accel [m/s^2]");
                        snsSP1FS.WriteLine(this.Title);
                        snsSP1FS.WriteLine("Serial Port [" + sp1Name + "] Sensor Data Update event log");
                        snsSP1FS.WriteLine("PTU11 : " + SERIAL_NUMBERS_TEXTBOX11.Text);
                        snsSP1FS.WriteLine("PTU12 : " + SERIAL_NUMBERS_TEXTBOX12.Text);
                        snsSP1FS.WriteLine("PTU13 : " + SERIAL_NUMBERS_TEXTBOX13.Text);
                        snsSP1FS.WriteLine("Monitor Interval Time [mS] : " + INTERVAL_TIME_TEXTBOX.Text);
                        snsSP1FS.WriteLine("");
                        snsSP1FS.WriteLine(",PTU11,,,,,,,,,,,,,,,,,,,,PTU12,,,,,,,,,,,,,,,,,,,,PTU13");
                        snsSP1FS.Write("Date Time,CPU TEMP [degC],Vin [V],Pin [V],Id [A],Vd [V],");
                        snsSP1FS.Write("TEMP1 [degC],TEMP2 [degC],TEMP3 [degC],TEMP4 [degC],TEMP5 [degC],TEMP6 [degC],TEMP7 [degC],TEMP8 [degC],TEMP9 [degC],TEMP10 [degC],TEMP11 [degC],TEMP12 [degC],TEMP13 [degC],TEMP14 [degC],TEMP15 [degC],");
                        snsSP1FS.Write("CPU TEMP [degC],Vin [V],Pin [V],Id [A],Vd [V],");
                        snsSP1FS.Write("TEMP1 [degC],TEMP2 [degC],TEMP3 [degC],TEMP4 [degC],TEMP5 [degC],TEMP6 [degC],TEMP7 [degC],TEMP8 [degC],TEMP9 [degC],TEMP10 [degC],TEMP11 [degC],TEMP12 [degC],TEMP13 [degC],TEMP14 [degC],TEMP15 [degC],");
                        snsSP1FS.Write("CPU TEMP [degC],Vin [V],Pin [V],Id [A],Vd [V],");
                        snsSP1FS.Write("TEMP1 [degC],TEMP2 [degC],TEMP3 [degC],TEMP4 [degC],TEMP5 [degC],TEMP6 [degC],TEMP7 [degC],TEMP8 [degC],TEMP9 [degC],TEMP10 [degC],TEMP11 [degC],TEMP12 [degC],TEMP13 [degC],TEMP14 [degC],TEMP15 [degC]");
                        snsSP1FS.WriteLine("");
                        snsSP2FS.WriteLine(this.Title);
                        snsSP2FS.WriteLine("Serial Port [" + sp2Name + "] Sensor Data Update event log");
                        snsSP2FS.WriteLine("PTU21 : " + SERIAL_NUMBERS_TEXTBOX21.Text);
                        snsSP2FS.WriteLine("PTU22 : " + SERIAL_NUMBERS_TEXTBOX22.Text);
                        snsSP2FS.WriteLine("PTU23 : " + SERIAL_NUMBERS_TEXTBOX23.Text);
                        snsSP2FS.WriteLine("Monitor Interval Time [mS] : " + INTERVAL_TIME_TEXTBOX.Text);
                        snsSP2FS.WriteLine("");
                        snsSP2FS.WriteLine(",PTU21,,,,,,,,,,,,,,,,,,,,PTU22,,,,,,,,,,,,,,,,,,,,PTU23");
                        snsSP2FS.Write("Date Time,CPU TEMP [degC],Vin [V],Pin [V],Id [A],Vd [V],");
                        snsSP2FS.Write("TEMP1 [degC],TEMP2 [degC],TEMP3 [degC],TEMP4 [degC],TEMP5 [degC],TEMP6 [degC],TEMP7 [degC],TEMP8 [degC],TEMP9 [degC],TEMP10 [degC],TEMP11 [degC],TEMP12 [degC],TEMP13 [degC],TEMP14 [degC],TEMP15 [degC],");
                        snsSP2FS.Write("CPU TEMP [degC],Vin [V],Pin [V],Id [A],Vd [V],");
                        snsSP2FS.Write("TEMP1 [degC],TEMP2 [degC],TEMP3 [degC],TEMP4 [degC],TEMP5 [degC],TEMP6 [degC],TEMP7 [degC],TEMP8 [degC],TEMP9 [degC],TEMP10 [degC],TEMP11 [degC],TEMP12 [degC],TEMP13 [degC],TEMP14 [degC],TEMP15 [degC],");
                        snsSP2FS.Write("CPU TEMP [degC],Vin [V],Pin [V],Id [A],Vd [V],");
                        snsSP2FS.Write("TEMP1 [degC],TEMP2 [degC],TEMP3 [degC],TEMP4 [degC],TEMP5 [degC],TEMP6 [degC],TEMP7 [degC],TEMP8 [degC],TEMP9 [degC],TEMP10 [degC],TEMP11 [degC],TEMP12 [degC],TEMP13 [degC],TEMP14 [degC],TEMP15 [degC]");
                        snsSP2FS.WriteLine("");
                        snsSP3FS.WriteLine(this.Title);
                        snsSP3FS.WriteLine("Serial Port [" + sp3Name + "] Sensor Data Update event log");
                        snsSP3FS.WriteLine("PTU31 : " + SERIAL_NUMBERS_TEXTBOX31.Text);
                        snsSP3FS.WriteLine("PTU32 : " + SERIAL_NUMBERS_TEXTBOX32.Text);
                        snsSP3FS.WriteLine("PTU33 : " + SERIAL_NUMBERS_TEXTBOX33.Text);
                        snsSP3FS.WriteLine("Monitor Interval Time [mS] : " + INTERVAL_TIME_TEXTBOX.Text);
                        snsSP3FS.WriteLine("");
                        snsSP3FS.WriteLine(",PTU31,,,,,,,,,,,,,,,,,,,,PTU32,,,,,,,,,,,,,,,,,,,,PTU33");
                        snsSP3FS.Write("Date Time,CPU TEMP [degC],Vin [V],Pin [V],Id [A],Vd [V],");
                        snsSP3FS.Write("TEMP1 [degC],TEMP2 [degC],TEMP3 [degC],TEMP4 [degC],TEMP5 [degC],TEMP6 [degC],TEMP7 [degC],TEMP8 [degC],TEMP9 [degC],TEMP10 [degC],TEMP11 [degC],TEMP12 [degC],TEMP13 [degC],TEMP14 [degC],TEMP15 [degC],");
                        snsSP3FS.Write("CPU TEMP [degC],Vin [V],Pin [V],Id [A],Vd [V],");
                        snsSP3FS.Write("TEMP1 [degC],TEMP2 [degC],TEMP3 [degC],TEMP4 [degC],TEMP5 [degC],TEMP6 [degC],TEMP7 [degC],TEMP8 [degC],TEMP9 [degC],TEMP10 [degC],TEMP11 [degC],TEMP12 [degC],TEMP13 [degC],TEMP14 [degC],TEMP15 [degC],");
                        snsSP3FS.Write("CPU TEMP [degC],Vin [V],Pin [V],Id [A],Vd [V],");
                        snsSP3FS.Write("TEMP1 [degC],TEMP2 [degC],TEMP3 [degC],TEMP4 [degC],TEMP5 [degC],TEMP6 [degC],TEMP7 [degC],TEMP8 [degC],TEMP9 [degC],TEMP10 [degC],TEMP11 [degC],TEMP12 [degC],TEMP13 [degC],TEMP14 [degC],TEMP15 [degC]");
                        snsSP3FS.WriteLine("");
                        writeFS.WriteLine(this.Title);
                        writeFS.WriteLine("Command Write Event Log");
                        writeFS.WriteLine("");
                        writeFS.WriteLine("Date Time,Command Line(HEX)");
                        errFS.WriteLine(this.Title);
                        errFS.WriteLine("Error Event Log");
                        errFS.WriteLine("");
                        errFS.WriteLine("Date Time,Sender,Message");
                        #endregion

                    }
                    ROTATE ang1;
                    ROTATE ang2;
                    if (VIEW_COMBOBOX.SelectedIndex == 0) { ang1 = ROTATE.RIGHT_TURN; ang2 = ROTATE.ZERO; }
                    else if (VIEW_COMBOBOX.SelectedIndex == 1) { ang1 = ROTATE.MIRROR_RIGHT_TURN; ang2 = ROTATE.MIRROR_ZERO; }
                    else { ang1 = ROTATE.MATRIX; ang2 = ROTATE.MATRIX; }
                    List<PANEL.SerialInterface> panelIF = new List<PANEL.SerialInterface>();
                    List<PANEL.UNIT> units1x = new List<PANEL.UNIT>();
                    List<PANEL.UNIT> units2x = new List<PANEL.UNIT>();
                    List<PANEL.UNIT> units3x = new List<PANEL.UNIT>();
                    units1x.Add(new PANEL.UNIT(SERIAL_NUMBERS_TEXTBOX11.Text, "PTU11", ang1));
                    units1x.Add(new PANEL.UNIT(SERIAL_NUMBERS_TEXTBOX12.Text, "PTU12", ang1));
                    units1x.Add(new PANEL.UNIT(SERIAL_NUMBERS_TEXTBOX13.Text, "PTU13", ang1));
                    units2x.Add(new PANEL.UNIT(SERIAL_NUMBERS_TEXTBOX21.Text, "PTU21", ang2));
                    units2x.Add(new PANEL.UNIT(SERIAL_NUMBERS_TEXTBOX22.Text, "PTU22", ang2));
                    units2x.Add(new PANEL.UNIT(SERIAL_NUMBERS_TEXTBOX23.Text, "PTU23", ang2));
                    units3x.Add(new PANEL.UNIT(SERIAL_NUMBERS_TEXTBOX31.Text, "PTU31", ang2));
                    units3x.Add(new PANEL.UNIT(SERIAL_NUMBERS_TEXTBOX32.Text, "PTU32", ang2));
                    units3x.Add(new PANEL.UNIT(SERIAL_NUMBERS_TEXTBOX33.Text, "PTU33", ang2));
                    panelIF.Add(new PANEL.SerialInterface(sp1Name, uint.Parse(BAUD_RATE_COMBOBOX1.Text.Trim().Replace(",", "")), units1x));
                    panelIF.Add(new PANEL.SerialInterface(sp2Name, uint.Parse(BAUD_RATE_COMBOBOX2.Text.Trim().Replace(",", "")), units2x));
                    panelIF.Add(new PANEL.SerialInterface(sp3Name, uint.Parse(BAUD_RATE_COMBOBOX3.Text.Trim().Replace(",", "")), units3x));
                    _ptp = new PANEL(panelIF);
                    _ptp.OnTaskError += PANEL_OnError;
                    _ptp.OnUpdate += PANEL_OnTaskUpdate;
                    foreach (PANEL.UNIT_IF ui in _ptp.unitIFs)
                    {
                        foreach (PANEL.UNIT unit in ui.UNITs)
                        {
                            unit.CONFIG.STBLNA_CheckboxClickEvent += STBLNA_CheckboxClick;
                        }
                    }

                    PTU11.Children.Clear();
                    PTU12.Children.Clear();
                    PTU13.Children.Clear();
                    PTU21.Children.Clear();
                    PTU22.Children.Clear();
                    PTU23.Children.Clear();
                    PTU31.Children.Clear();
                    PTU32.Children.Clear();
                    PTU33.Children.Clear();
                    PTU11.Children.Add(_ptp.unitIFs[0].UNITs[0].CONFIG);
                    PTU12.Children.Add(_ptp.unitIFs[0].UNITs[1].CONFIG);
                    PTU13.Children.Add(_ptp.unitIFs[0].UNITs[2].CONFIG);
                    PTU21.Children.Add(_ptp.unitIFs[1].UNITs[0].CONFIG);
                    PTU22.Children.Add(_ptp.unitIFs[1].UNITs[1].CONFIG);
                    PTU23.Children.Add(_ptp.unitIFs[1].UNITs[2].CONFIG);
                    PTU31.Children.Add(_ptp.unitIFs[2].UNITs[0].CONFIG);
                    PTU32.Children.Add(_ptp.unitIFs[2].UNITs[1].CONFIG);
                    PTU33.Children.Add(_ptp.unitIFs[2].UNITs[2].CONFIG);

                    winPCABsensor = new winPCAB_SensorMonitor();
                    winPCABsensor.PTU11.Children.Add(_ptp.unitIFs[0].UNITs[0].SENS_MONITOR);
                    winPCABsensor.PTU12.Children.Add(_ptp.unitIFs[0].UNITs[1].SENS_MONITOR);
                    winPCABsensor.PTU13.Children.Add(_ptp.unitIFs[0].UNITs[2].SENS_MONITOR);
                    winPCABsensor.PTU21.Children.Add(_ptp.unitIFs[1].UNITs[0].SENS_MONITOR);
                    winPCABsensor.PTU22.Children.Add(_ptp.unitIFs[1].UNITs[1].SENS_MONITOR);
                    winPCABsensor.PTU23.Children.Add(_ptp.unitIFs[1].UNITs[2].SENS_MONITOR);
                    winPCABsensor.PTU31.Children.Add(_ptp.unitIFs[2].UNITs[0].SENS_MONITOR);
                    winPCABsensor.PTU32.Children.Add(_ptp.unitIFs[2].UNITs[1].SENS_MONITOR);
                    winPCABsensor.PTU33.Children.Add(_ptp.unitIFs[2].UNITs[2].SENS_MONITOR);
                    winPCABphase = new winPCAB_PhaseMonitor();
                    if (VIEW_COMBOBOX.SelectedIndex == 1)
                    {
                        winPCABphase.GRID13.Children.Add(_ptp.unitIFs[0].UNITs[0].PHASE_MONITOR);
                        winPCABphase.GRID12.Children.Add(_ptp.unitIFs[0].UNITs[1].PHASE_MONITOR);
                        winPCABphase.GRID11.Children.Add(_ptp.unitIFs[0].UNITs[2].PHASE_MONITOR);
                        winPCABphase.GRID23.Children.Add(_ptp.unitIFs[1].UNITs[0].PHASE_MONITOR);
                        winPCABphase.GRID22.Children.Add(_ptp.unitIFs[1].UNITs[1].PHASE_MONITOR);
                        winPCABphase.GRID21.Children.Add(_ptp.unitIFs[1].UNITs[2].PHASE_MONITOR);
                        winPCABphase.GRID33.Children.Add(_ptp.unitIFs[2].UNITs[0].PHASE_MONITOR);
                        winPCABphase.GRID32.Children.Add(_ptp.unitIFs[2].UNITs[1].PHASE_MONITOR);
                        winPCABphase.GRID31.Children.Add(_ptp.unitIFs[2].UNITs[2].PHASE_MONITOR);
                    }
                    else
                    {
                        winPCABphase.GRID11.Children.Add(_ptp.unitIFs[0].UNITs[0].PHASE_MONITOR);
                        winPCABphase.GRID12.Children.Add(_ptp.unitIFs[0].UNITs[1].PHASE_MONITOR);
                        winPCABphase.GRID13.Children.Add(_ptp.unitIFs[0].UNITs[2].PHASE_MONITOR);
                        winPCABphase.GRID21.Children.Add(_ptp.unitIFs[1].UNITs[0].PHASE_MONITOR);
                        winPCABphase.GRID22.Children.Add(_ptp.unitIFs[1].UNITs[1].PHASE_MONITOR);
                        winPCABphase.GRID23.Children.Add(_ptp.unitIFs[1].UNITs[2].PHASE_MONITOR);
                        winPCABphase.GRID31.Children.Add(_ptp.unitIFs[2].UNITs[0].PHASE_MONITOR);
                        winPCABphase.GRID32.Children.Add(_ptp.unitIFs[2].UNITs[1].PHASE_MONITOR);
                        winPCABphase.GRID33.Children.Add(_ptp.unitIFs[2].UNITs[2].PHASE_MONITOR);
                    }

                    winPOSmonitor = new winPOS();
                    _pos.OnReadDAT += winPOSmonitor.OnReadDAT;
                    _pos.OnTaskError += POS_OnError;
                    winPOSmonitor.OnUpdate += POS_OnTaskUpdate;
                    try
                    {
                        if (!_ptp.Open())
                        {
                            _ptp.Close();
                            _ptp = null;
                            _pos = null;
                            MessageBox.Show("Serial Number detection error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        _pos.Open();
                    }
                    catch
                    {
                        DISCONNECT(true);
                        return;
                    }

                    READ_PORTsFile(portConfFilePath);
                    _ptp.PANEL_SensorMonitor_TASK_Start(uint.Parse(INTERVAL_TIME_TEXTBOX.Text));
                    _pos.POS_AutoTaskStart();
                    winPCABsensor.Show();
                    winPCABphase.Show();
                    winPOSmonitor.Show();


                    try { READ_CONFIG(); } catch { }
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
                    MessageBox.Show("Serial port detection error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SERIAL_PORTS_COMBOBOX_RELOAD(null, null);
                }
            }
        }

        private void LogDirSelectButton_Click(object sender, RoutedEventArgs e)
        {
            //LogDirPath_TextBox
            using (System.Windows.Forms.FolderBrowserDialog fdb = new System.Windows.Forms.FolderBrowserDialog())
            {
                fdb.Description = "Please select a log save folder.";
                fdb.RootFolder = Environment.SpecialFolder.Desktop;
                fdb.ShowNewFolderButton = true;
                if (fdb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LogDirPath_TextBox.Text = fdb.SelectedPath;
                }
            }
        }

        private void SAVELOGs_CHECKBOX_Click(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent.Name == "Checked") { LogDirPathSelect_BUTTON.IsEnabled = true; LogDirPath_TextBox.IsEnabled = true; }
            else if (e.RoutedEvent.Name == "Unchecked") { LogDirPathSelect_BUTTON.IsEnabled = false; LogDirPath_TextBox.IsEnabled = false; }
        }

        #endregion

        #region CONTROL EVENTs

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
            if (strBF.Length > 15)
            {
                MessageBox.Show("Enter the serial number between 1 and 15 characters, without spaces, separated by commas.");
                e.Handled = true;
            }
            SERIAL_PORTS_COMBOBOX_DropDownClosed(null, null);
        }

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

        private void ANGLE_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9のみ
            e.Handled = !new Regex("[0-9|.]").IsMatch(e.Text);
        }

        private void ANGLE_TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼付け場合
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9|.]|[ ]").IsMatch(strTXT[cnt].ToString()))
                    {
                        // 処理済み
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        private void ANGLE_TextBox_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                float floatVal = float.Parse(((TextBox)sender).Text);
                if (-90.0 <= floatVal && floatVal <= 90.0f) { return; }
                MessageBox.Show("Enter in the range -90.0 to +90.0");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("Enter in the range -90.0 to +90.0");
                e.Handled = true;
            }
        }

        private void SAVEADDRESS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SAVEADDRESS_COMBOBOX.SelectedIndex < 0) { SAVEADDRESS_COMBOBOX.SelectedIndex = 0; }
        }

        private void EXPANDER_Expanded(object sender, RoutedEventArgs e)
        {
            double thisHeight = this.Height;
            if (this.WindowState == WindowState.Maximized) { thisHeight = System.Windows.SystemParameters.WorkArea.Height - 50; }
            if (sender == CONFIG_EXPANDER) { CONFIG_GRID.Height = thisHeight * 0.7; }
            if (sender == BOARD_CONFIG_EXPANDER) { BOARD_CONFIG_GRID.Height = thisHeight * 0.7; }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double thisHeight = this.Height;
            if (this.WindowState == WindowState.Maximized) { thisHeight = System.Windows.SystemParameters.WorkArea.Height - 50; }
            if (CONFIG_EXPANDER.IsExpanded) { CONFIG_GRID.Height = thisHeight * 0.5; }
            if (BOARD_CONFIG_EXPANDER.IsExpanded) { BOARD_CONFIG_GRID.Height = thisHeight * 0.7; }
        }
        #endregion

        #region CONTROL COMMON BUTTON EVENTs

        private void READ_PORTFILE_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Title = "Select the ports configuration file";
                ofd.FileName = "OffsetDAT.csv";
                ofd.Filter = "csv(*.csv)|*.csv|All files(*.*)|*.*";
                ofd.FilterIndex = 1;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    READ_PORTsFile(ofd.FileName);
                }
            }
        }

        private void OFFSET_ZERO_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            foreach(PANEL.UNIT_IF unitIF in _ptp.unitIFs)
            {
                foreach(PANEL.UNIT unit in unitIF.UNITs)
                {
                    foreach(PANEL.PORT port in unit.Ports)
                    {
                        port.Offset = new MWComLibCS.ComplexAngle(1, new MWComLibCS.Angle(0));
                    }
                    unit.ReloadPorts();
                }
            }
        }

        private void SET_PHASE_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            double freq = double.Parse(CALCULATE_FREQUENCY_TEXTBOX.Text) * Math.Pow(10.0, 6.0);
            double az = double.Parse(CALCULATE_AZIMUTH_TEXTBOX.Text);
            double el = double.Parse(CALCULATE_ELEVATION_TEXTBOX.Text);
            MWComLibCS.CoordinateSystem.AntennaCS targ = new MWComLibCS.CoordinateSystem.AntennaCS(new MWComLibCS.Angle(az,false), new MWComLibCS.Angle(el, false), true);

            _ptp.PANEL_SensorMonitor_TASK_Pause();
            if(!WriteDPSxx(
                _ptp.unitIFs[0].UNITs[0].GetPhaseDelay(freq, targ), _ptp.unitIFs[0].UNITs[1].GetPhaseDelay(freq, targ), _ptp.unitIFs[0].UNITs[2].GetPhaseDelay(freq, targ),
                _ptp.unitIFs[1].UNITs[0].GetPhaseDelay(freq, targ), _ptp.unitIFs[1].UNITs[1].GetPhaseDelay(freq, targ), _ptp.unitIFs[1].UNITs[2].GetPhaseDelay(freq, targ),
                _ptp.unitIFs[2].UNITs[0].GetPhaseDelay(freq, targ), _ptp.unitIFs[2].UNITs[1].GetPhaseDelay(freq, targ), _ptp.unitIFs[2].UNITs[2].GetPhaseDelay(freq, targ)))
            {
                MessageBox.Show("Write DPS failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _ptp.PANEL_SensorMonitor_TASK_Restart();
        }

        private void STBLNA_CheckboxClick(object sender, RoutedEventArgs e, bool? isChecked)
        {
            bool ic = false;
            bool result = false;
            if (isChecked == true) { ic = true; }
            else if (isChecked == false) { ic = false; }
            if (sender is cntConfigPorts && isChecked != null)
            {
                switch (((Grid)((cntConfigPorts)sender).Parent).Name)
                {
                    case "PTU11":
                        _ptp.unitIFs[0].UNITs_SensorMonitor_TASK_Pause();
                        result = _ptp.unitIFs[0].SerialInterface.SetSTB_LNA(0, ic);
                        _ptp.unitIFs[0].UNITs_SensorMonitor_TASK_Restart();
                        break;
                    case "PTU12":
                        _ptp.unitIFs[0].UNITs_SensorMonitor_TASK_Pause();
                        result = _ptp.unitIFs[0].SerialInterface.SetSTB_LNA(1, ic);
                        _ptp.unitIFs[0].UNITs_SensorMonitor_TASK_Restart();
                        break;
                    case "PTU13":
                        _ptp.unitIFs[0].UNITs_SensorMonitor_TASK_Pause();
                        result = _ptp.unitIFs[0].SerialInterface.SetSTB_LNA(2, ic);
                        _ptp.unitIFs[0].UNITs_SensorMonitor_TASK_Restart();
                        break;
                    case "PTU21":
                        _ptp.unitIFs[1].UNITs_SensorMonitor_TASK_Pause();
                        result = _ptp.unitIFs[1].SerialInterface.SetSTB_LNA(0, ic);
                        _ptp.unitIFs[1].UNITs_SensorMonitor_TASK_Restart();
                        break;
                    case "PTU22":
                        _ptp.unitIFs[1].UNITs_SensorMonitor_TASK_Pause();
                        result = _ptp.unitIFs[1].SerialInterface.SetSTB_LNA(1, ic);
                        _ptp.unitIFs[1].UNITs_SensorMonitor_TASK_Restart();
                        break;
                    case "PTU23":
                        _ptp.unitIFs[1].UNITs_SensorMonitor_TASK_Pause();
                        result = _ptp.unitIFs[1].SerialInterface.SetSTB_LNA(2, ic);
                        _ptp.unitIFs[1].UNITs_SensorMonitor_TASK_Restart();
                        break;
                    case "PTU31":
                        _ptp.unitIFs[2].UNITs_SensorMonitor_TASK_Pause();
                        result = _ptp.unitIFs[2].SerialInterface.SetSTB_LNA(0, ic);
                        _ptp.unitIFs[2].UNITs_SensorMonitor_TASK_Restart();
                        break;
                    case "PTU32":
                        _ptp.unitIFs[2].UNITs_SensorMonitor_TASK_Pause();
                        result = _ptp.unitIFs[2].SerialInterface.SetSTB_LNA(1, ic);
                        _ptp.unitIFs[2].UNITs_SensorMonitor_TASK_Restart();
                        break;
                    case "PTU33":
                        _ptp.unitIFs[2].UNITs_SensorMonitor_TASK_Pause();
                        result = _ptp.unitIFs[2].SerialInterface.SetSTB_LNA(2, ic);
                        _ptp.unitIFs[2].UNITs_SensorMonitor_TASK_Restart();
                        break;
                }
            }
            if (!result)
            {
                MessageBox.Show("Stand by LNA switching failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void STB_LPM_CheckboxClick(object sender, RoutedEventArgs e)
        {
            bool ic = false;
            bool result = true;
            if (e.RoutedEvent.Name == "Checked") { ic = true; }
            else if (e.RoutedEvent.Name == "Unchecked") { ic = false; }
            else { return; }
            if (sender is CheckBox)
            {
                _ptp.PANEL_SensorMonitor_TASK_Pause();
                switch (((CheckBox)sender).Name)
                {
                    case "STBAMP_CHECKBOX":
                        foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
                        {
                            foreach (PANEL.UNIT unit in unitIF.UNITs)
                            {
                                if (!unitIF.SerialInterface.SetSTB_AMP(new PCAB_UnitInterface(unit.SerialNumber), ic)) { result = false; }
                            }
                        }
                        break;
                    case "STBDRA_CHECKBOX":
                        foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
                        {
                            foreach (PANEL.UNIT unit in unitIF.UNITs)
                            {
                                if (!unitIF.SerialInterface.SetSTB_DRA(new PCAB_UnitInterface(unit.SerialNumber), ic)) { result = false; }
                            }
                        }
                        break;
                    case "SETLPM_CHECKBOX":
                        foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
                        {
                            foreach (PANEL.UNIT unit in unitIF.UNITs)
                            {
                                if (!unitIF.SerialInterface.SetLowPowerMode(new PCAB_UnitInterface(unit.SerialNumber), ic)) { result = false; }
                            }
                        }
                        break;
                }
                _ptp.PANEL_SensorMonitor_TASK_Restart();
                if (!result)
                {
                    MessageBox.Show("Switching failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void WRITEDSA_Click(object sender, RoutedEventArgs e)
        {
            bool result = true;
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
            {
                foreach (PANEL.UNIT unit in unitIF.UNITs)
                {
                    if (!unitIF.SerialInterface.WriteDSAin(new PCAB_UnitInterface(unit.SerialNumber), (uint)unit.CONFIG.GetDSA(0))) { result = false; }
                    if (!unitIF.SerialInterface.WriteDSA(new PCAB_UnitInterface(unit.SerialNumber), unit.CONFIG.GetDSA())) { result = false; }
                }
            }
            _ptp.PANEL_SensorMonitor_TASK_Restart();
            if (!result)
            {
                MessageBox.Show("Write DSA failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (result && ((Button)sender).Name == "WRITEDSA")
            {
                MessageBox.Show("Set ATT Config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void WRITEDPS_Click(object sender, RoutedEventArgs e)
        {
            bool result = true;
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
            {
                foreach (PANEL.UNIT unit in unitIF.UNITs)
                {
                    if (!unitIF.SerialInterface.WriteDPS(new PCAB_UnitInterface(unit.SerialNumber), unit.CONFIG.GetDPS())) { result = false; }
                    List<float> dpsVal = new List<float>();
                    foreach (uint val in unit.CONFIG.GetDPS())
                    {
                        dpsVal.Add(val * -5.625f);
                    }
                    unit.PHASE_MONITOR.VALUEs = dpsVal;
                }
            }
            _ptp.PANEL_SensorMonitor_TASK_Restart();
            if (!result)
            {
                MessageBox.Show("Write DPS failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (result && ((Button)sender).Name == "WRITEDPS")
            {
                MessageBox.Show("Set Phase Config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void WRITE_Click(object sender, RoutedEventArgs e)
        {
            WRITEDSA_Click(sender, e);
            WRITEDPS_Click(sender, e);
            MessageBox.Show("Config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void READ_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                READ_CONFIG();
                MessageBox.Show("Config read done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void LOADDMEM_Click(object sender, RoutedEventArgs e)
        {
            uint sectorPage = 0xE0;
            uint stateNum = 0;
            string[] strARR = SAVEADDRESS_COMBOBOX.Text.Split('-');
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
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            bool errFLG = false;
            foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
            {
                foreach (PANEL.UNIT unit in unitIF.UNITs)
                {
                    if (!unitIF.SerialInterface.LoadState(new PCAB_UnitInterface(unit.SerialNumber), (byte)sectorPage, stateNum)) { errFLG = true; break; }
                }
                if (errFLG) { break; }
            }
            _ptp.PANEL_SensorMonitor_TASK_Restart();

            if (errFLG)
            { MessageBox.Show("Load memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            try
            {
                READ_CONFIG();
                MessageBox.Show("Load memory done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SAVEMEM_Click(object sender, RoutedEventArgs e)
        {
            uint sectorPage = 0xE0;
            uint stateNum = 0;
            string[] strARR = SAVEADDRESS_COMBOBOX.Text.Split('-');
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
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            bool errFLG = false;
            foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
            {
                foreach (PANEL.UNIT unit in unitIF.UNITs)
                {
                    if (!unitIF.SerialInterface.SaveState(new PCAB_UnitInterface(unit.SerialNumber), (byte)sectorPage, stateNum)) { errFLG = true; break; }
                }
                if (errFLG) { break; }
            }
            _ptp.PANEL_SensorMonitor_TASK_Restart();
            if (errFLG)
            { MessageBox.Show("Save memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            try
            {
                READ_CONFIG();
                MessageBox.Show("Save memory done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Restore default settins."
                , "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            bool errFLG = false;
            foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
            {
                foreach (PANEL.UNIT unit in unitIF.UNITs)
                {
                    if (!unitIF.SerialInterface.LoadFactoryDefault(new PCAB_UnitInterface(unit.SerialNumber))) { errFLG = true; break; }
                }
                if (errFLG) { break; }
            }
            _ptp.PANEL_SensorMonitor_TASK_Restart();
            if (errFLG)
            { MessageBox.Show("Reset error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            try
            {
                READ_CONFIG();
                MessageBox.Show("Preset done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region CLASS TASK EVENTs
        private void PANEL_OnTaskUpdate(object sender, EventArgs e)
        {
            if (saveLOG)
            {
                string portName = ((PANEL.UNIT_IF)sender).SerialInterface.PortName;
                string sp1Name = Path.GetFileNameWithoutExtension(((FileStream)snsSP1FS.BaseStream).Name).Split('_')[0];
                string sp2Name = Path.GetFileNameWithoutExtension(((FileStream)snsSP2FS.BaseStream).Name).Split('_')[0];
                string sp3Name = Path.GetFileNameWithoutExtension(((FileStream)snsSP3FS.BaseStream).Name).Split('_')[0];
                if (portName == sp1Name)
                {
                    snsSP1FS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Vin.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Pin.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Id.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Vd.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[0].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[1].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[2].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[3].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[4].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[5].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[6].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[7].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[8].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[9].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[10].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[11].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[12].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[13].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[14].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Vin.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Pin.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Id.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Vd.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[0].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[1].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[2].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[3].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[4].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[5].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[6].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[7].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[8].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[9].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[10].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[11].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[12].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[13].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[14].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Vin.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Pin.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Id.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Vd.ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[0].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[1].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[2].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[3].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[4].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[5].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[6].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[7].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[8].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[9].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[10].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[11].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[12].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[13].ToString("0.00") + ",");
                    snsSP1FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[14].ToString("0.00"));
                    snsSP1FS.WriteLine("");
                }
                else if (portName == sp2Name)
                {
                    snsSP2FS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Vin.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Pin.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Id.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Vd.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[0].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[1].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[2].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[3].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[4].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[5].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[6].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[7].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[8].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[9].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[10].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[11].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[12].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[13].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[14].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Vin.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Pin.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Id.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Vd.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[0].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[1].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[2].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[3].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[4].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[5].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[6].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[7].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[8].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[9].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[10].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[11].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[12].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[13].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[14].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Vin.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Pin.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Id.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Vd.ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[0].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[1].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[2].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[3].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[4].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[5].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[6].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[7].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[8].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[9].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[10].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[11].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[12].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[13].ToString("0.00") + ",");
                    snsSP2FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[14].ToString("0.00"));
                    snsSP2FS.WriteLine("");
                }
                else if (portName == sp3Name)
                {
                    snsSP3FS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Vin.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Pin.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Id.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Analog.Vd.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[0].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[1].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[2].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[3].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[4].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[5].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[6].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[7].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[8].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[9].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[10].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[11].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[12].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[13].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[0].SensorValuesNOW.Temprature.Values[14].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Vin.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Pin.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Id.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Analog.Vd.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[0].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[1].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[2].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[3].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[4].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[5].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[6].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[7].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[8].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[9].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[10].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[11].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[12].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[13].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[1].SensorValuesNOW.Temprature.Values[14].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Vin.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Pin.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Id.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Analog.Vd.ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[0].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[1].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[2].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[3].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[4].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[5].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[6].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[7].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[8].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[9].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[10].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[11].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[12].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[13].ToString("0.00") + ",");
                    snsSP3FS.Write(((PANEL.UNIT_IF)sender).UNITs[2].SensorValuesNOW.Temprature.Values[14].ToString("0.00"));
                    snsSP3FS.WriteLine("");
                }
            }
        }
        private void POS_OnTaskUpdate(object sender, EventArgs e)
        {
            if (saveLOG)
            {
                posFS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",");
                if (winPOSmonitor.POS_VIEWER.DATA != null)
                {
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.TIME.ToString("0.000000") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.SPEED.ToString("0.00") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.TRACK.ToString("0.00") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.LATITUDE.ToString("0.000000") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.LONGITUDE.ToString("0.000000") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.ALTITUDE.ToString("0.00") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.ROLL.ToString("0.00") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.PITCH.ToString("0.00") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.HEADING.ToString("0.00") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.LONG_ACCEL.ToString("0.0000") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.TRAN_ACCEL.ToString("0.0000") + ",");
                    posFS.Write(winPOSmonitor.POS_VIEWER.DATA.DOWN_ACCEL.ToString("0.0000"));
                    posFS.WriteLine();
                }
                else
                {
                    posFS.WriteLine("ND,ND,ND,ND,ND,ND,ND,ND,ND,ND,ND,ND");
                }
            }
        }

        private void PANEL_OnError(object sender, ErrorEventArgs e)
        {
            if (saveLOG)
            {
                errFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + "," +
                    ((PANEL.UNIT_IF)sender).SerialInterface.PortName + ",\"" + (e.GetException()).Message + "\"");
            }
                if (MessageBox.Show((e.GetException()).Message + "\nDo you want to close the port and abort the process?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error)
                    == MessageBoxResult.Yes)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DISCONNECT(false);
                }));
            }
        }

        private void POS_OnError(object sender, POSEventArgs e)
        {
            if (saveLOG)
            {
                errFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",POS,\"" + e.Message + "\"");
            }
            if (MessageBox.Show(e.Message + "\nDo you want to close the port and abort the process?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error)
                == MessageBoxResult.Yes)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DISCONNECT(false);
                }));
            }
        }

        #endregion

        private void READ_CONFIG()
        {
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            List<uint> ptu11, ptu12, ptu13, ptu21, ptu22, ptu23, ptu31, ptu32, ptu33;
            try
            {
                if (ReadDPSxx(out ptu11, out ptu12, out ptu13, out ptu21, out ptu22, out ptu23, out ptu31, out ptu32, out ptu33))
                {
                    _ptp.unitIFs[0].UNITs[0].CONFIG.SetDPS(ptu11);
                    _ptp.unitIFs[0].UNITs[1].CONFIG.SetDPS(ptu12);
                    _ptp.unitIFs[0].UNITs[2].CONFIG.SetDPS(ptu13);
                    _ptp.unitIFs[1].UNITs[0].CONFIG.SetDPS(ptu21);
                    _ptp.unitIFs[1].UNITs[1].CONFIG.SetDPS(ptu22);
                    _ptp.unitIFs[1].UNITs[2].CONFIG.SetDPS(ptu23);
                    _ptp.unitIFs[2].UNITs[0].CONFIG.SetDPS(ptu31);
                    _ptp.unitIFs[2].UNITs[1].CONFIG.SetDPS(ptu32);
                    _ptp.unitIFs[2].UNITs[2].CONFIG.SetDPS(ptu33);
                }
                if (ReadDSAxx(out ptu11, out ptu12, out ptu13, out ptu21, out ptu22, out ptu23, out ptu31, out ptu32, out ptu33))
                {
                    _ptp.unitIFs[0].UNITs[0].CONFIG.SetDSA(ptu11);
                    _ptp.unitIFs[0].UNITs[1].CONFIG.SetDSA(ptu12);
                    _ptp.unitIFs[0].UNITs[2].CONFIG.SetDSA(ptu13);
                    _ptp.unitIFs[1].UNITs[0].CONFIG.SetDSA(ptu21);
                    _ptp.unitIFs[1].UNITs[1].CONFIG.SetDSA(ptu22);
                    _ptp.unitIFs[1].UNITs[2].CONFIG.SetDSA(ptu23);
                    _ptp.unitIFs[2].UNITs[0].CONFIG.SetDSA(ptu31);
                    _ptp.unitIFs[2].UNITs[1].CONFIG.SetDSA(ptu32);
                    _ptp.unitIFs[2].UNITs[2].CONFIG.SetDSA(ptu33);
                }
                foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
                {
                    foreach (PANEL.UNIT unit in unitIF.UNITs)
                    {
                        unit.CONFIG.SetDSA(0, unitIF.SerialInterface.GetDSAin(new PCAB_UnitInterface(unit.SerialNumber)));
                        unit.CONFIG.StandbyLNA = unitIF.SerialInterface.GetSTB_LNA(new PCAB_UnitInterface(unit.SerialNumber));
                    }
                }
                for (int i = 0; i < _ptp.unitIFs.Count; i++)
                {
                    for (uint j = 0; j < _ptp.unitIFs[i].UNITs.Count; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            STBAMP_CHECKBOX.IsChecked = _ptp.unitIFs[i].SerialInterface.GetSTB_AMP(j);
                            STBDRA_CHECKBOX.IsChecked = _ptp.unitIFs[i].SerialInterface.GetSTB_DRA(j);
                            SETLPM_CHECKBOX.IsChecked = _ptp.unitIFs[i].SerialInterface.GetLowPowerMode(j);
                        }
                        else
                        {
                            if (STBAMP_CHECKBOX.IsChecked == null || STBAMP_CHECKBOX.IsChecked != _ptp.unitIFs[i].SerialInterface.GetSTB_AMP(j))
                            { STBAMP_CHECKBOX.IsChecked = null; }
                            if (STBDRA_CHECKBOX.IsChecked == null || STBDRA_CHECKBOX.IsChecked != _ptp.unitIFs[i].SerialInterface.GetSTB_DRA(j))
                            { STBDRA_CHECKBOX.IsChecked = null; }
                            if (SETLPM_CHECKBOX.IsChecked == null || SETLPM_CHECKBOX.IsChecked != _ptp.unitIFs[i].SerialInterface.GetLowPowerMode(j))
                            { SETLPM_CHECKBOX.IsChecked = null; }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ptp.PANEL_SensorMonitor_TASK_Restart();
                throw;
            }
            _ptp.PANEL_SensorMonitor_TASK_Restart();
        }

        private void READ_PORTsFile(string filePath)
        {
            List<PANEL.PORT> ptu11, ptu12, ptu13, ptu21, ptu22, ptu23, ptu31, ptu32, ptu33;
            if (clsReadDAT.ReadPortDAT(filePath, out ptu11, out ptu12, out ptu13, out ptu21, out ptu22, out ptu23, out ptu31, out ptu32, out ptu33))
            {
                _ptp.unitIFs[0].UNITs[0].Ports = ptu11;
                _ptp.unitIFs[0].UNITs[1].Ports = ptu12;
                _ptp.unitIFs[0].UNITs[2].Ports = ptu13;
                _ptp.unitIFs[1].UNITs[0].Ports = ptu21;
                _ptp.unitIFs[1].UNITs[1].Ports = ptu22;
                _ptp.unitIFs[1].UNITs[2].Ports = ptu23;
                _ptp.unitIFs[2].UNITs[0].Ports = ptu31;
                _ptp.unitIFs[2].UNITs[1].Ports = ptu32;
                _ptp.unitIFs[2].UNITs[2].Ports = ptu33;
            }
        }

        #region private Tasks
        private bool WriteDPSxx(
            List<uint> ptu11, List<uint> ptu12, List<uint> ptu13,
            List<uint> ptu21, List<uint> ptu22, List<uint> ptu23,
            List<uint> ptu31, List<uint> ptu32, List<uint> ptu33
            )
        {
            try
            {
                Task<bool> _wrt1x = Task.Factory.StartNew(() => { return WriteDPSxx(0, ptu11, ptu12, ptu13); });
                Task<bool> _wrt2x = Task.Factory.StartNew(() => { return WriteDPSxx(1, ptu21, ptu22, ptu23); });
                Task<bool> _wrt3x = Task.Factory.StartNew(() => { return WriteDPSxx(2, ptu31, ptu32, ptu33); });
                _wrt1x?.ConfigureAwait(false);
                _wrt2x?.ConfigureAwait(false);
                _wrt3x?.ConfigureAwait(false);
                Task.WaitAll(_wrt1x, _wrt2x, _wrt3x);
                if (_wrt1x.Result && _wrt2x.Result && _wrt3x.Result)
                {
                    List<float> ptuVal = new List<float>();
                    ptuVal.Clear();
                    foreach (uint val in ptu11) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[0].UNITs[0].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu12) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[0].UNITs[1].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu13) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[0].UNITs[2].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu21) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[1].UNITs[0].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu22) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[1].UNITs[1].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu23) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[1].UNITs[2].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu31) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[2].UNITs[0].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu32) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[2].UNITs[1].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu33) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[2].UNITs[2].PHASE_MONITOR.VALUEs = ptuVal;
                    return true;
                }
                else { return false; }
            }catch (Exception e) { return false; }
        }

        private bool WriteDPSxx(int interfaceNum, List<uint> ptux1, List<uint> ptux2, List<uint> ptux3)
        {
            bool result = true;
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDPS(0, ptux1)) { result = false; }
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDPS(1, ptux2)) { result = false; }
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDPS(2, ptux3)) { result = false; }
            return result;
        }

        private bool WriteDSAxx(
            List<uint> ptu11, List<uint> ptu12, List<uint> ptu13,
            List<uint> ptu21, List<uint> ptu22, List<uint> ptu23,
            List<uint> ptu31, List<uint> ptu32, List<uint> ptu33
            )
        {
            try
            {
                Task<bool> _wrt1x = Task.Factory.StartNew(() => { return WriteDSAxx(0, ptu11, ptu12, ptu13); });
                Task<bool> _wrt2x = Task.Factory.StartNew(() => { return WriteDSAxx(1, ptu21, ptu22, ptu23); });
                Task<bool> _wrt3x = Task.Factory.StartNew(() => { return WriteDSAxx(2, ptu31, ptu32, ptu33); });
                _wrt1x?.ConfigureAwait(false);
                _wrt2x?.ConfigureAwait(false);
                _wrt3x?.ConfigureAwait(false);
                Task.WaitAll(_wrt1x, _wrt2x, _wrt3x);
                if (_wrt1x.Result && _wrt2x.Result && _wrt3x.Result) { return true; }
                else { return false; }
            }
            catch { return false; }
        }

        private bool WriteDSAxx(int interfaceNum, List<uint> ptux1, List<uint> ptux2, List<uint> ptux3)
        {
            bool result = true;
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDSA(0, ptux1)) { result = false; }
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDSA(1, ptux2)) { result = false; }
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDSA(2, ptux3)) { result = false; }
            return result;
        }

        private bool ReadDPSxx(
            out List<uint> ptu11, out List<uint> ptu12, out List<uint> ptu13,
            out List<uint> ptu21, out List<uint> ptu22, out List<uint> ptu23,
            out List<uint> ptu31, out List<uint> ptu32, out List<uint> ptu33
            )
        {
            try
            {
                Task<ptuxx> _wrt1x = Task.Factory.StartNew(() => { return ReadDPSxx(0); });
                Task<ptuxx> _wrt2x = Task.Factory.StartNew(() => { return ReadDPSxx(1); });
                Task<ptuxx> _wrt3x = Task.Factory.StartNew(() => { return ReadDPSxx(2); });
                _wrt1x?.ConfigureAwait(false);
                _wrt2x?.ConfigureAwait(false);
                _wrt3x?.ConfigureAwait(false);
                Task.WaitAll(_wrt1x, _wrt2x, _wrt3x);
                ptu11 = _wrt1x.Result.ptux1;
                ptu12 = _wrt1x.Result.ptux2;
                ptu13 = _wrt1x.Result.ptux3;
                ptu21 = _wrt2x.Result.ptux1;
                ptu22 = _wrt2x.Result.ptux2;
                ptu23 = _wrt2x.Result.ptux3;
                ptu31 = _wrt3x.Result.ptux1;
                ptu32 = _wrt3x.Result.ptux2;
                ptu33 = _wrt3x.Result.ptux3;
                if (ptu11 != null && ptu12 != null && ptu13 != null &&
                    ptu21 != null && ptu22 != null && ptu23 != null &&
                    ptu31 != null && ptu23 != null && ptu33 != null)
                {
                    List<float> ptuVal = new List<float>();
                    ptuVal.Clear();
                    foreach (uint val in ptu11) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[0].UNITs[0].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu12) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[0].UNITs[1].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu13) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[0].UNITs[2].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu21) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[1].UNITs[0].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu22) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[1].UNITs[1].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu23) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[1].UNITs[2].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu31) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[2].UNITs[0].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu32) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[2].UNITs[1].PHASE_MONITOR.VALUEs = ptuVal;
                    ptuVal.Clear();
                    foreach (uint val in ptu33) { ptuVal.Add(val * -5.625f); }
                    _ptp.unitIFs[2].UNITs[2].PHASE_MONITOR.VALUEs = ptuVal;
                    return true;
                }
                else { return false; }
            }
            catch
            {
                ptu11 = null;
                ptu12 = null;
                ptu13 = null;
                ptu21 = null;
                ptu22 = null;
                ptu23 = null;
                ptu31 = null;
                ptu32 = null;
                ptu33 = null;
                return false;
            }
        }

        private ptuxx ReadDPSxx(int interfaceNum)
        {
            ptuxx ptuDAT = new ptuxx();
            try { ptuDAT.ptux1 = _ptp.unitIFs[interfaceNum].SerialInterface.GetDPS(0); }
            catch { ptuDAT.ptux1 = null; }
            try { ptuDAT.ptux2 = _ptp.unitIFs[interfaceNum].SerialInterface.GetDPS(1); }
            catch { ptuDAT.ptux2 = null; }
            try { ptuDAT.ptux3 = _ptp.unitIFs[interfaceNum].SerialInterface.GetDPS(2); }
            catch { ptuDAT.ptux3 = null; }
            return ptuDAT;
        }

        private bool ReadDSAxx(
            out List<uint> ptu11, out List<uint> ptu12, out List<uint> ptu13,
            out List<uint> ptu21, out List<uint> ptu22, out List<uint> ptu23,
            out List<uint> ptu31, out List<uint> ptu32, out List<uint> ptu33
            )
        {
            try
            {
                Task<ptuxx> _wrt1x = Task.Factory.StartNew(() => { return ReadDSAxx(0); });
                Task<ptuxx> _wrt2x = Task.Factory.StartNew(() => { return ReadDSAxx(1); });
                Task<ptuxx> _wrt3x = Task.Factory.StartNew(() => { return ReadDSAxx(2); });
                _wrt1x?.ConfigureAwait(false);
                _wrt2x?.ConfigureAwait(false);
                _wrt3x?.ConfigureAwait(false);
                Task.WaitAll(_wrt1x, _wrt2x, _wrt3x);
                ptu11 = _wrt1x.Result.ptux1;
                ptu12 = _wrt1x.Result.ptux2;
                ptu13 = _wrt1x.Result.ptux3;
                ptu21 = _wrt2x.Result.ptux1;
                ptu22 = _wrt2x.Result.ptux2;
                ptu23 = _wrt2x.Result.ptux3;
                ptu31 = _wrt3x.Result.ptux1;
                ptu32 = _wrt3x.Result.ptux2;
                ptu33 = _wrt3x.Result.ptux3;
                if (ptu11 != null && ptu12 != null && ptu13 != null &&
                    ptu21 != null && ptu22 != null && ptu23 != null &&
                    ptu31 != null && ptu23 != null && ptu33 != null) { return true; }
                else { return false; }
            }
            catch
            {
                ptu11 = null;
                ptu12 = null;
                ptu13 = null;
                ptu21 = null;
                ptu22 = null;
                ptu23 = null;
                ptu31 = null;
                ptu32 = null;
                ptu33 = null;
                return false;
            }
        }

        private ptuxx ReadDSAxx(int interfaceNum)
        {
            ptuxx ptuDAT = new ptuxx();
            try { ptuDAT.ptux1 = _ptp.unitIFs[interfaceNum].SerialInterface.GetDSA(0); }
            catch { ptuDAT.ptux1 = null; }
            try { ptuDAT.ptux2 = _ptp.unitIFs[interfaceNum].SerialInterface.GetDSA(1); }
            catch { ptuDAT.ptux2 = null; }
            try { ptuDAT.ptux3 = _ptp.unitIFs[interfaceNum].SerialInterface.GetDSA(2); }
            catch { ptuDAT.ptux3 = null; }
            return ptuDAT;
        }

        private struct ptuxx
        {
            public List<uint> ptux1;
            public List<uint> ptux2;
            public List<uint> ptux3;
            public ptuxx(List<uint> ptux1, List<uint> ptux2, List<uint> ptux3)
            {
                this.ptux1 = ptux1; this.ptux2 = ptux2; this.ptux3 = ptux3;
            }
        }
        #endregion

    }
}
