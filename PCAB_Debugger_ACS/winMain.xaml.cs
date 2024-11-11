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
        private const double HEADING_ZERO = 0.0;
        private const double ROLL_ZERO= 0.0;
        private const double PITCH_ZERO = 0.0;
        private POS _pos;
        private PANEL _ptp;
        private SerialPortTable[] ports;
        private NormalizedColorChart cc;
        private winPOS winPOSmonitor;
        private winPCAB_SensorMonitor winPCABsensor;
        private winPCAB_PhaseMonitor winPCABphase;
        private bool isControl = false;
        private StreamWriter posFS;
        private StreamWriter snsSP1FS;
        private StreamWriter snsSP2FS;
        private StreamWriter snsSP3FS;
        private StreamWriter cmdFS;
        private StreamWriter errFS;
        private bool saveLOG = false;
        private bool savePosLOG = false;
        private bool savePTU1xLOG = false;
        private bool savePTU2xLOG = false;
        private bool savePTU3xLOG = false;
        private bool _trackTASK_state = false;
        private double _trackTASK_Freq;
        private Task _trackTASK;
        private double _trackTASK_az = 0;
        private double _trackTASK_pol = 0;
        private uint trackSNS = 5;
        private double trackCalc_az = 0;
        private double trackCalc_pol = 0;
        private double limitAz = 20;
        private double limitPol = 20;
        private double corrLatitude = 0;
        private double corrLongitude = 0;
        private double corrAltitude = 0;
        private double corrAz = 0;
        private double corrPol = 0;
        private double targLatitude = 0;
        private double targLongitude = 0;
        private double targAltitude = 0;
        private bool? coordSYS = true;


        public winMain()
        {
#if DEBUG_RESET
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

#if DEBUG_RESET
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
            if (SERIAL_PORTS_COMBOBOX3.Items.Count > 3) { SERIAL_PORTS_COMBOBOX3.SelectedIndex = 3; }
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
            SAVELOGsPOS_CHECKBOX.IsChecked = Settings.Default.saveLogsPOS;
            SAVELOGsPTU1x_CHECKBOX.IsChecked = Settings.Default.saveLogsPTU1x;
            SAVELOGsPTU2x_CHECKBOX.IsChecked = Settings.Default.saveLogsPTU2x;
            SAVELOGsPTU3x_CHECKBOX.IsChecked = Settings.Default.saveLogsPTU3x;
            if (Settings.Default.csLudwig3) { CsPhiTheta_RADIOBUTTON.IsChecked = true; coordSYS = null; }
            else { CsAzEl_RADIOBUTTON.IsChecked = true; coordSYS = true; }
            INTERVAL_TIME_TEXTBOX.Text = Settings.Default.snsIntTime.ToString("0");
            INTERVAL_COUNT_TEXTBOX.Text = Settings.Default.snsIntCount.ToString("0");
            CALCULATE_FREQUENCY_TEXTBOX.Text = Settings.Default.FreqMHz.ToString("0");

            targLatitude = TARGET_POSITION_INPUT.Latitude = Settings.Default.targetLATITUDE;
            targLongitude = TARGET_POSITION_INPUT.Longitude = Settings.Default.targetLOGITUDE;
            targAltitude = TARGET_POSITION_INPUT.Altitude = Settings.Default.targetALTITUDE;

            LIMIT_AZIMUTH_TEXTBOX.Text = Math.Abs(limitAz).ToString("0.00");
            LIMIT_POLAR_TEXTBOX.Text = Math.Abs(limitPol).ToString("0.00");
            CORRECTION_LATITUDE_TEXTBOX.Text = corrLatitude.ToString("0.00");
            CORRECTION_LONGITUDE_TEXTBOX.Text = corrLongitude.ToString("0.00");
            CORRECTION_ALTITUDE_TEXTBOX.Text = corrAltitude.ToString("0.00");
            CORRECTION_AZIMUTH_TEXTBOX.Text = corrAz.ToString("0.00");
            CORRECTION_POLAR_TEXTBOX.Text = corrPol.ToString("0.00");
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
            Settings.Default.spCaption0 = SERIAL_PORTS_COMBOBOX0.Text;
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
            if (SAVELOGs_CHECKBOX.IsChecked == true) { Settings.Default.saveLogs = true; }
            else { Settings.Default.saveLogs = false; }
            if (SAVELOGsPOS_CHECKBOX.IsChecked == true) { Settings.Default.saveLogsPOS = true; }
            else { Settings.Default.saveLogsPOS = false; }
            if (SAVELOGsPTU1x_CHECKBOX.IsChecked == true) { Settings.Default.saveLogsPTU1x = true; }
            else { Settings.Default.saveLogsPTU1x = false; }
            if (SAVELOGsPTU2x_CHECKBOX.IsChecked == true) { Settings.Default.saveLogsPTU2x = true; }
            else { Settings.Default.saveLogsPTU2x = false; }
            if (SAVELOGsPTU3x_CHECKBOX.IsChecked == true) { Settings.Default.saveLogsPTU3x = true; }
            else { Settings.Default.saveLogsPTU3x = false; }
            if (CsPhiTheta_RADIOBUTTON.IsChecked == true) { Settings.Default.csLudwig3 = true; }
            else { Settings.Default.csLudwig3 = false; }
            Settings.Default.logSaveDirPath = LogDirPath_TextBox.Text;
            Settings.Default.snsIntTime = uint.Parse(INTERVAL_TIME_TEXTBOX.Text);
            Settings.Default.snsIntCount = uint.Parse(INTERVAL_COUNT_TEXTBOX.Text);
            Settings.Default.FreqMHz = uint.Parse(CALCULATE_FREQUENCY_TEXTBOX.Text);

            TARGET_POSITION_INPUT.Latitude = Settings.Default.targetLATITUDE = (float)targLatitude;
            TARGET_POSITION_INPUT.Longitude = Settings.Default.targetLOGITUDE = (float)targLongitude;
            TARGET_POSITION_INPUT.Altitude = Settings.Default.targetALTITUDE = (float)targAltitude;

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
            if (_trackTASK_state) { TrackingTASK_STOP(wait); }
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
                cmdFS.Close();
                errFS.Close();
                if (savePosLOG) { posFS.Close(); }
                if (savePTU1xLOG) { snsSP1FS.Close(); }
                if (savePTU2xLOG) { snsSP2FS.Close(); }
                if (savePTU3xLOG) { snsSP3FS.Close(); }
            }
            CONFIG_EXPANDER.IsExpanded = true;
            CONFIG_GRID.IsEnabled = true;
            CONTROL_GRID.IsEnabled = false;
            BOARD_CONFIG_EXPANDER.IsExpanded = false;
            CONNECT_BUTTON_CONTENT.Text = "Connect";
            isControl = false;
            saveLOG = false;
            savePosLOG = false;
            savePTU1xLOG = false;
            savePTU2xLOG = false;
            savePTU3xLOG = false;
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
                    if (SAVELOGsPOS_CHECKBOX.IsChecked == true) { savePosLOG = true; }
                    if (SAVELOGsPTU1x_CHECKBOX.IsChecked == true) { savePTU1xLOG = true; }
                    if (SAVELOGsPTU2x_CHECKBOX.IsChecked == true) { savePTU2xLOG = true; }
                    if (SAVELOGsPTU3x_CHECKBOX.IsChecked == true) { savePTU3xLOG = true; }
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
                        if (!clsReadDAT.ReadPortDAT(ofd.FileName, out _, out _, out _, out _, out _, out _, out _, out _, out _))
                        {
                            MessageBox.Show("Failed to load port configuration list.\nPlease check file format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
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
                    if (saveLOG)
                    {
                        cmdFS = new StreamWriter(LogDirPath_TextBox.Text + "//CMD_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
                        errFS = new StreamWriter(LogDirPath_TextBox.Text + "//ERR_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
                        cmdFS.WriteLine(this.Title);
                        cmdFS.WriteLine("All (DPS / DSA / DSAin) (Read / Write) Command Event Log");
                        cmdFS.WriteLine("");
                        cmdFS.WriteLine("Date Time,DPS or DSA or DSAin,Read or Write,PTU11(HEX),PTU12(HEX),PTU13(HEX),PTU21(HEX),PTU22(HEX),PTU23(HEX),PTU31(HEX),PTU32(HEX),PTU33(HEX),Date Time,true or false");
                        errFS.WriteLine(this.Title);
                        errFS.WriteLine("Error Event Log");
                        errFS.WriteLine("");
                        errFS.WriteLine("Date Time,Sender,Message");
                        if (savePosLOG)
                        {
                            posFS = new StreamWriter(LogDirPath_TextBox.Text + "//POS_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
                            posFS.WriteLine(this.Title);
                            posFS.WriteLine("POS Data Update event log");
                            posFS.WriteLine("");
                            posFS.WriteLine("Date Time,Time of Validity [sec],Speed [m/s],Track [deg],Latitude,Longitude,Altitude [m],Roll [deg],Pitch [deg],Hedading [deg],Long Accel [m/s^2],Tran Accel [m/s^2],Down Accel [m/s^2]");
                        }
                        if (savePTU1xLOG)
                        {
                            snsSP1FS = new StreamWriter(LogDirPath_TextBox.Text + "//" + sp1Name + "_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
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
                        }
                        if (savePTU2xLOG)
                        {
                            snsSP2FS = new StreamWriter(LogDirPath_TextBox.Text + "//" + sp2Name + "_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
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
                        }
                        if (savePTU3xLOG)
                        {
                            snsSP3FS = new StreamWriter(LogDirPath_TextBox.Text + "//" + sp3Name + "_" + dt.ToString("yyyyMMdd-HHmmss") + ".csv", true, enc);
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
                        }
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
                    _ptp.PANEL_SensorMonitor_TASK_Start(uint.Parse(INTERVAL_TIME_TEXTBOX.Text), uint.Parse(INTERVAL_COUNT_TEXTBOX.Text));
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

        #endregion

        #region CONTROL CONFIG EVENTs
        private void SAVELOGs_CHECKBOX_Click(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent.Name == "Checked") { LogDirPathSelect_BUTTON.IsEnabled = true; LogDirPath_TextBox.IsEnabled = true; SAVELOGs_SELECT_GRID.IsEnabled = true; }
            else if (e.RoutedEvent.Name == "Unchecked") { LogDirPathSelect_BUTTON.IsEnabled = false; LogDirPath_TextBox.IsEnabled = false; SAVELOGs_SELECT_GRID.IsEnabled = false; }
        }

        private void CoordinateSystem_RADIOBUTTON_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                if (rb.Name == "CsPhiTheta_RADIOBUTTON")
                {
                    coordSYS = null;
                    az_label.Content = "      Phi [deg]";
                    pol_label.Content = "    Theta [deg]";
                    targ_az_label.Content = "      Phi [deg]";
                    targ_pol_label.Content = "    Theta [deg]";
                    calc_az_label.Content = "      Phi [deg]";
                    calc_pol_label.Content = "    Theta [deg]";
                    limit_az_label.Content = "    Phi   (+/-)";
                    limit_pol_label.Content = "    Theta (+/-)";
                    corr_az_label.Content = "Phi";
                    corr_pol_label.Content = "Theta";
                }
                if (rb.Name == "CsAzEl_RADIOBUTTON")
                {
                    coordSYS = true;
                    az_label.Content = "  Azimuth [deg]";
                    pol_label.Content = "Elevation [deg]";
                    targ_az_label.Content = "  Azimuth [deg]";
                    targ_pol_label.Content = "Elevation [deg]";
                    calc_az_label.Content = "  Azimuth [deg]";
                    calc_pol_label.Content = "Elevation [deg]";
                    limit_az_label.Content = "Azimuth   (+/-)";
                    limit_pol_label.Content = "Elevation (+/-)";
                    corr_az_label.Content = " Azimuth";
                    corr_pol_label.Content = "Elevation";
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
            if (strBF.Length > 15)
            {
                MessageBox.Show("Enter the serial number between 1 and 15 characters, without spaces, separated by commas.");
                e.Handled = true;
            }
            SERIAL_PORTS_COMBOBOX_DropDownClosed(null, null);
        }
        
        #endregion

        #region CONTROL EVENTs

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
            e.Handled = !new Regex("[0-9|.|-]").IsMatch(e.Text);
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
            if (sender == CONFIG_EXPANDER) { CONFIG_GRID.Height = thisHeight * 0.85; }
            if (sender == BOARD_CONFIG_EXPANDER) { BOARD_CONFIG_GRID.Height = thisHeight * 0.75; }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double thisHeight = this.Height;
            if (this.WindowState == WindowState.Maximized) { thisHeight = System.Windows.SystemParameters.WorkArea.Height - 50; }
            if (CONFIG_EXPANDER.IsExpanded) { CONFIG_GRID.Height = thisHeight * 0.85; }
            if (BOARD_CONFIG_EXPANDER.IsExpanded) { BOARD_CONFIG_GRID.Height = thisHeight * 0.75; }
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
                    if (READ_PORTsFile(ofd.FileName))
                    {
                        MessageBox.Show("Port setting list loading completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to load port configuration list.\nPlease check file format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OFFSET_ZERO_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("All port compensation values ​​will be deleted.\nDo you want to continue?",
                "Question", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
                {
                    foreach (PANEL.UNIT unit in unitIF.UNITs)
                    {
                        foreach (PANEL.PORT port in unit.Ports)
                        {
                            port.Offset = new MWComLibCS.ComplexAngle(1, new MWComLibCS.Angle(0));
                        }
                        unit.ReloadPorts();
                    }
                }
                MessageBox.Show("Port compensation value deletion is complete.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SET_PHASE_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            double freq = double.Parse(CALCULATE_FREQUENCY_TEXTBOX.Text) * Math.Pow(10.0, 6.0);
            double az = double.Parse(CALCULATE_AZIMUTH_TEXTBOX.Text);
            double pol = double.Parse(CALCULATE_POLAR_TEXTBOX.Text);
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            bool result = WriteTarget(freq, az, pol);
            _ptp.PANEL_SensorMonitor_TASK_Restart();
            if (result) { MessageBox.Show("Write DPS Done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
            else { MessageBox.Show("Write DPS failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void TRY_SET_ATT_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            try { READ_CONFIG(); }
            catch (Exception ex)
            {
                MessageBox.Show("Read config failed\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            int dsaInAdd = DSAin_COMBOBOX.SelectedIndex;
            int dsaPxxAdd = DSAxx_COMBOBOX.SelectedIndex;
            if (dsaInAdd != 0 && DSAinNP_COMBOBOX.Text == "-") { dsaInAdd *= -1; }
            if (dsaPxxAdd != 0 && DSAxxNP_COMBOBOX.Text == "-") { dsaPxxAdd *= -1; }
            if (dsaInAdd == 0 && dsaPxxAdd == 0)
            {
                MessageBox.Show("It will not changed.\nRead config done.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                if(dsaInAdd != 0)
                {
                    _ptp.unitIFs[0].UNITs[0].CONFIG.SetDSA(0, _ptp.unitIFs[0].UNITs[0].CONFIG.GetDSA(0) + dsaInAdd);
                    _ptp.unitIFs[0].UNITs[1].CONFIG.SetDSA(0, _ptp.unitIFs[0].UNITs[1].CONFIG.GetDSA(0) + dsaInAdd);
                    _ptp.unitIFs[0].UNITs[2].CONFIG.SetDSA(0, _ptp.unitIFs[0].UNITs[2].CONFIG.GetDSA(0) + dsaInAdd);
                    _ptp.unitIFs[1].UNITs[0].CONFIG.SetDSA(0, _ptp.unitIFs[1].UNITs[0].CONFIG.GetDSA(0) + dsaInAdd);
                    _ptp.unitIFs[1].UNITs[1].CONFIG.SetDSA(0, _ptp.unitIFs[1].UNITs[1].CONFIG.GetDSA(0) + dsaInAdd);
                    _ptp.unitIFs[1].UNITs[2].CONFIG.SetDSA(0, _ptp.unitIFs[1].UNITs[2].CONFIG.GetDSA(0) + dsaInAdd);
                    _ptp.unitIFs[2].UNITs[0].CONFIG.SetDSA(0, _ptp.unitIFs[2].UNITs[0].CONFIG.GetDSA(0) + dsaInAdd);
                    _ptp.unitIFs[2].UNITs[1].CONFIG.SetDSA(0, _ptp.unitIFs[2].UNITs[1].CONFIG.GetDSA(0) + dsaInAdd);
                    _ptp.unitIFs[2].UNITs[2].CONFIG.SetDSA(0, _ptp.unitIFs[2].UNITs[2].CONFIG.GetDSA(0) + dsaInAdd);
                }
                if(dsaPxxAdd != 0)
                {
                    for(uint count = 1; count < 16; count++)
                    {
                        _ptp.unitIFs[0].UNITs[0].CONFIG.SetDSA(count, _ptp.unitIFs[0].UNITs[0].CONFIG.GetDSA(count) + dsaPxxAdd);
                        _ptp.unitIFs[0].UNITs[1].CONFIG.SetDSA(count, _ptp.unitIFs[0].UNITs[1].CONFIG.GetDSA(count) + dsaPxxAdd);
                        _ptp.unitIFs[0].UNITs[2].CONFIG.SetDSA(count, _ptp.unitIFs[0].UNITs[2].CONFIG.GetDSA(count) + dsaPxxAdd);
                        _ptp.unitIFs[1].UNITs[0].CONFIG.SetDSA(count, _ptp.unitIFs[1].UNITs[0].CONFIG.GetDSA(count) + dsaPxxAdd);
                        _ptp.unitIFs[1].UNITs[1].CONFIG.SetDSA(count, _ptp.unitIFs[1].UNITs[1].CONFIG.GetDSA(count) + dsaPxxAdd);
                        _ptp.unitIFs[1].UNITs[2].CONFIG.SetDSA(count, _ptp.unitIFs[1].UNITs[2].CONFIG.GetDSA(count) + dsaPxxAdd);
                        _ptp.unitIFs[2].UNITs[0].CONFIG.SetDSA(count, _ptp.unitIFs[2].UNITs[0].CONFIG.GetDSA(count) + dsaPxxAdd);
                        _ptp.unitIFs[2].UNITs[1].CONFIG.SetDSA(count, _ptp.unitIFs[2].UNITs[1].CONFIG.GetDSA(count) + dsaPxxAdd);
                        _ptp.unitIFs[2].UNITs[2].CONFIG.SetDSA(count, _ptp.unitIFs[2].UNITs[2].CONFIG.GetDSA(count) + dsaPxxAdd);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Processing was aborted because some value settings were outside the acceptable range.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            WRITEDPS_Click(sender, e);
            MessageBox.Show("DSA incremental configuration completed.\n" +
                "Add DSA IN  > " + dsaInAdd.ToString() +
                "Add DSA Pxx > " + dsaPxxAdd.ToString(),
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            List<uint> ptu11 = _ptp.unitIFs[0].UNITs[0].CONFIG.GetDSA();
            List<uint> ptu12 = _ptp.unitIFs[0].UNITs[1].CONFIG.GetDSA();
            List<uint> ptu13 = _ptp.unitIFs[0].UNITs[2].CONFIG.GetDSA();
            List<uint> ptu21 = _ptp.unitIFs[1].UNITs[0].CONFIG.GetDSA();
            List<uint> ptu22 = _ptp.unitIFs[1].UNITs[1].CONFIG.GetDSA();
            List<uint> ptu23 = _ptp.unitIFs[1].UNITs[2].CONFIG.GetDSA();
            List<uint> ptu31 = _ptp.unitIFs[2].UNITs[0].CONFIG.GetDSA();
            List<uint> ptu32 = _ptp.unitIFs[2].UNITs[1].CONFIG.GetDSA();
            List<uint> ptu33 = _ptp.unitIFs[2].UNITs[2].CONFIG.GetDSA();
            uint ptu11in = (uint)(_ptp.unitIFs[0].UNITs[0].CONFIG.GetDSA(0));
            uint ptu12in = (uint)(_ptp.unitIFs[0].UNITs[1].CONFIG.GetDSA(0));
            uint ptu13in = (uint)(_ptp.unitIFs[0].UNITs[2].CONFIG.GetDSA(0));
            uint ptu21in = (uint)(_ptp.unitIFs[1].UNITs[0].CONFIG.GetDSA(0));
            uint ptu22in = (uint)(_ptp.unitIFs[1].UNITs[1].CONFIG.GetDSA(0));
            uint ptu23in = (uint)(_ptp.unitIFs[1].UNITs[2].CONFIG.GetDSA(0));
            uint ptu31in = (uint)(_ptp.unitIFs[2].UNITs[0].CONFIG.GetDSA(0));
            uint ptu32in = (uint)(_ptp.unitIFs[2].UNITs[1].CONFIG.GetDSA(0));
            uint ptu33in = (uint)(_ptp.unitIFs[2].UNITs[2].CONFIG.GetDSA(0));
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            if (!WriteDSAinxx(ptu11in, ptu12in, ptu13in, ptu21in, ptu22in, ptu23in, ptu31in, ptu32in, ptu33in)) { result = false; }
            if (!WriteDSAxx(ptu11, ptu12, ptu13, ptu21, ptu22, ptu23, ptu31, ptu32, ptu33)) { result = false; }
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
            List<uint> ptu11 = _ptp.unitIFs[0].UNITs[0].CONFIG.GetDPS();
            List<uint> ptu12 = _ptp.unitIFs[0].UNITs[1].CONFIG.GetDPS();
            List<uint> ptu13 = _ptp.unitIFs[0].UNITs[2].CONFIG.GetDPS();
            List<uint> ptu21 = _ptp.unitIFs[1].UNITs[0].CONFIG.GetDPS();
            List<uint> ptu22 = _ptp.unitIFs[1].UNITs[1].CONFIG.GetDPS();
            List<uint> ptu23 = _ptp.unitIFs[1].UNITs[2].CONFIG.GetDPS();
            List<uint> ptu31 = _ptp.unitIFs[2].UNITs[0].CONFIG.GetDPS();
            List<uint> ptu32 = _ptp.unitIFs[2].UNITs[1].CONFIG.GetDPS();
            List<uint> ptu33 = _ptp.unitIFs[2].UNITs[2].CONFIG.GetDPS();
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            if (!WriteDPSxx(ptu11, ptu12, ptu13, ptu21, ptu22, ptu23, ptu31, ptu32, ptu33)) { result = false; }
            _ptp.PANEL_SensorMonitor_TASK_Restart();
            if (result)
            {
                foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
                {
                    foreach (PANEL.UNIT unit in unitIF.UNITs)
                    {
                        List<float> dpsVal = new List<float>();
                        foreach (uint val in unit.CONFIG.GetDPS())
                        {
                            dpsVal.Add(val * -5.625f);
                        }
                        unit.PHASE_MONITOR.VALUEs = dpsVal;
                    }
                }
            }
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
                MessageBox.Show("Config read error.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Config read error.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Config read error.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region CLASS TASK EVENTs
        private void PANEL_OnTaskUpdate(object sender, EventArgs e)
        {
            if (saveLOG)
            {
                string portName = ((PANEL.UNIT_IF)sender).SerialInterface.PortName;
                if (savePTU1xLOG)
                {
                    string sp1Name = Path.GetFileNameWithoutExtension(((FileStream)snsSP1FS.BaseStream).Name).Split('_')[0];
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
                }
                if (savePTU2xLOG)
                {
                    string sp2Name = Path.GetFileNameWithoutExtension(((FileStream)snsSP2FS.BaseStream).Name).Split('_')[0];
                    if (portName == sp2Name)
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
                }
                if (savePTU3xLOG)
                {
                    string sp3Name = Path.GetFileNameWithoutExtension(((FileStream)snsSP3FS.BaseStream).Name).Split('_')[0];
                    if (portName == sp3Name)
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
        }
        private void POS_OnTaskUpdate(object sender, EventArgs e)
        {
            if (savePosLOG)
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
            CalcTargetAngle();
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
            int ptu11in, ptu12in, ptu13in, ptu21in, ptu22in, ptu23in, ptu31in, ptu32in, ptu33in;
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
                if (ReadDSAinxx(out ptu11in, out ptu12in, out ptu13in, out ptu21in, out ptu22in, out ptu23in, out ptu31in, out ptu32in, out ptu33in))
                {
                    _ptp.unitIFs[0].UNITs[0].CONFIG.SetDSA(0, ptu11in);
                    _ptp.unitIFs[0].UNITs[1].CONFIG.SetDSA(0, ptu12in);
                    _ptp.unitIFs[0].UNITs[2].CONFIG.SetDSA(0, ptu13in);
                    _ptp.unitIFs[1].UNITs[0].CONFIG.SetDSA(0, ptu21in);
                    _ptp.unitIFs[1].UNITs[1].CONFIG.SetDSA(0, ptu22in);
                    _ptp.unitIFs[1].UNITs[2].CONFIG.SetDSA(0, ptu23in);
                    _ptp.unitIFs[2].UNITs[0].CONFIG.SetDSA(0, ptu31in);
                    _ptp.unitIFs[2].UNITs[1].CONFIG.SetDSA(0, ptu32in);
                    _ptp.unitIFs[2].UNITs[2].CONFIG.SetDSA(0, ptu33in);
                }
                foreach (PANEL.UNIT_IF unitIF in _ptp.unitIFs)
                {
                    foreach (PANEL.UNIT unit in unitIF.UNITs)
                    {
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
                throw ex;
            }
            _ptp.PANEL_SensorMonitor_TASK_Restart();
        }

        private bool READ_PORTsFile(string filePath)
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
                return true;
            }
            return false;
        }

        #region private Tasks
        private bool WriteDPSxx(
            List<uint> ptu11, List<uint> ptu12, List<uint> ptu13,
            List<uint> ptu21, List<uint> ptu22, List<uint> ptu23,
            List<uint> ptu31, List<uint> ptu32, List<uint> ptu33
            )
        {
            if (saveLOG)
            {
                cmdFS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",DPS,WRITE,");
                cmdFS.Write(BitConverter.ToString(ptu11.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu12.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu13.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu21.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu22.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu23.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu31.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu32.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu33.ConvertAll(x => (byte)x).ToArray()) + ",");
            }
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
                    List<float> ptuVal11 = new List<float>();
                    List<float> ptuVal12 = new List<float>();
                    List<float> ptuVal13 = new List<float>();
                    List<float> ptuVal21 = new List<float>();
                    List<float> ptuVal22 = new List<float>();
                    List<float> ptuVal23 = new List<float>();
                    List<float> ptuVal31 = new List<float>();
                    List<float> ptuVal32 = new List<float>();
                    List<float> ptuVal33 = new List<float>();
                    foreach (uint val in ptu11) { ptuVal11.Add(val * -5.625f); }
                    foreach (uint val in ptu12) { ptuVal12.Add(val * -5.625f); }
                    foreach (uint val in ptu13) { ptuVal13.Add(val * -5.625f); }
                    foreach (uint val in ptu21) { ptuVal21.Add(val * -5.625f); }
                    foreach (uint val in ptu22) { ptuVal22.Add(val * -5.625f); }
                    foreach (uint val in ptu23) { ptuVal23.Add(val * -5.625f); }
                    foreach (uint val in ptu31) { ptuVal31.Add(val * -5.625f); }
                    foreach (uint val in ptu32) { ptuVal32.Add(val * -5.625f); }
                    foreach (uint val in ptu33) { ptuVal33.Add(val * -5.625f); }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _ptp.unitIFs[0].UNITs[0].PHASE_MONITOR.VALUEs = ptuVal11;
                        _ptp.unitIFs[0].UNITs[1].PHASE_MONITOR.VALUEs = ptuVal12;
                        _ptp.unitIFs[0].UNITs[2].PHASE_MONITOR.VALUEs = ptuVal13;
                        _ptp.unitIFs[1].UNITs[0].PHASE_MONITOR.VALUEs = ptuVal21;
                        _ptp.unitIFs[1].UNITs[1].PHASE_MONITOR.VALUEs = ptuVal22;
                        _ptp.unitIFs[1].UNITs[2].PHASE_MONITOR.VALUEs = ptuVal23;
                        _ptp.unitIFs[2].UNITs[0].PHASE_MONITOR.VALUEs = ptuVal31;
                        _ptp.unitIFs[2].UNITs[1].PHASE_MONITOR.VALUEs = ptuVal32;
                        _ptp.unitIFs[2].UNITs[2].PHASE_MONITOR.VALUEs = ptuVal33;

                    }));
                    if (saveLOG)
                    {
                        cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",TRUE");
                    }
                    return true;
                }
                else
                {
                    if (saveLOG)
                    {
                        cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                if (saveLOG)
                {
                    cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                }
                return false;
            }
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
            if (saveLOG)
            {
                cmdFS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",DSA,WRITE,");
                cmdFS.Write(BitConverter.ToString(ptu11.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu12.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu13.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu21.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu22.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu23.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu31.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu32.ConvertAll(x => (byte)x).ToArray()) + ",");
                cmdFS.Write(BitConverter.ToString(ptu33.ConvertAll(x => (byte)x).ToArray()) + ",");
            }
            try
            {
                Task<bool> _wrt1x = Task.Factory.StartNew(() => { return WriteDSAxx(0, ptu11, ptu12, ptu13); });
                Task<bool> _wrt2x = Task.Factory.StartNew(() => { return WriteDSAxx(1, ptu21, ptu22, ptu23); });
                Task<bool> _wrt3x = Task.Factory.StartNew(() => { return WriteDSAxx(2, ptu31, ptu32, ptu33); });
                _wrt1x?.ConfigureAwait(false);
                _wrt2x?.ConfigureAwait(false);
                _wrt3x?.ConfigureAwait(false);
                Task.WaitAll(_wrt1x, _wrt2x, _wrt3x);
                if (_wrt1x.Result && _wrt2x.Result && _wrt3x.Result)
                {
                    if (saveLOG)
                    {
                        cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",TRUE");
                    }
                    return true;
                }
                else
                {
                    if (saveLOG)
                    {
                        cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                if (saveLOG)
                {
                    cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                }
                return false;
            }
        }

        private bool WriteDSAxx(int interfaceNum, List<uint> ptux1, List<uint> ptux2, List<uint> ptux3)
        {
            bool result = true;
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDSA(0, ptux1)) { result = false; }
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDSA(1, ptux2)) { result = false; }
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDSA(2, ptux3)) { result = false; }
            return result;
        }


        private bool WriteDSAinxx(
            uint ptu11, uint ptu12, uint ptu13,
            uint ptu21, uint ptu22, uint ptu23,
            uint ptu31, uint ptu32, uint ptu33
            )
        {
            if (saveLOG)
            {
                cmdFS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",DSAin,WRITE,");
                cmdFS.Write(ptu11.ToString("X2") + ",");
                cmdFS.Write(ptu12.ToString("X2") + ",");
                cmdFS.Write(ptu13.ToString("X2") + ",");
                cmdFS.Write(ptu21.ToString("X2") + ",");
                cmdFS.Write(ptu22.ToString("X2") + ",");
                cmdFS.Write(ptu23.ToString("X2") + ",");
                cmdFS.Write(ptu31.ToString("X2") + ",");
                cmdFS.Write(ptu32.ToString("X2") + ",");
                cmdFS.Write(ptu33.ToString("X2") + ",");
            }
            try
            {
                Task<bool> _wrt1x = Task.Factory.StartNew(() => { return WriteDSAinxx(0, ptu11, ptu12, ptu13); });
                Task<bool> _wrt2x = Task.Factory.StartNew(() => { return WriteDSAinxx(1, ptu21, ptu22, ptu23); });
                Task<bool> _wrt3x = Task.Factory.StartNew(() => { return WriteDSAinxx(2, ptu31, ptu32, ptu33); });
                _wrt1x?.ConfigureAwait(false);
                _wrt2x?.ConfigureAwait(false);
                _wrt3x?.ConfigureAwait(false);
                Task.WaitAll(_wrt1x, _wrt2x, _wrt3x);
                if (_wrt1x.Result && _wrt2x.Result && _wrt3x.Result)
                {
                    if (saveLOG)
                    {
                        cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",TRUE");
                    }
                    return true;
                }
                else
                {
                    if (saveLOG)
                    {
                        cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                if (saveLOG)
                {
                    cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                }
                return false;
            }
        }

        private bool WriteDSAinxx(int interfaceNum, uint ptux1, uint ptux2, uint ptux3)
        {
            bool result = true;
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDSAin(0, ptux1)) { result = false; }
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDSAin(1, ptux2)) { result = false; }
            if (!_ptp.unitIFs[interfaceNum].SerialInterface.WriteDSAin(2, ptux3)) { result = false; }
            return result;
        }

        private bool ReadDPSxx(
            out List<uint> ptu11, out List<uint> ptu12, out List<uint> ptu13,
            out List<uint> ptu21, out List<uint> ptu22, out List<uint> ptu23,
            out List<uint> ptu31, out List<uint> ptu32, out List<uint> ptu33
            )
        {
            if (saveLOG)
            {
                cmdFS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",DPS,READ,");
            }
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
                    if (saveLOG)
                    {
                        cmdFS.Write(BitConverter.ToString(ptu11.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu12.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu13.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu21.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu22.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu23.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu31.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu32.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu33.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",TRUE");
                    }
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
                else
                {
                    if (saveLOG)
                    {
                        cmdFS.WriteLine(",,,,,,,,," + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                    }
                    return false; }
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
                if (saveLOG)
                {
                    cmdFS.WriteLine(",,,,,,,,," + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                }
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
            if (saveLOG)
            {
                cmdFS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",DSA,READ,");
            }
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
                    ptu31 != null && ptu23 != null && ptu33 != null)
                {
                    if (saveLOG)
                    {
                        cmdFS.Write(BitConverter.ToString(ptu11.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu12.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu13.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu21.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu22.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu23.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu31.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu32.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.Write(BitConverter.ToString(ptu33.ConvertAll(x => (byte)x).ToArray()) + ",");
                        cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",TRUE");
                    }
                    return true; }
                else
                {
                    if (saveLOG)
                    {
                        cmdFS.WriteLine(",,,,,,,,," + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                    }
                    return false; }
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
                if (saveLOG)
                {
                    cmdFS.WriteLine(",,,,,,,,," + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                }
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


        private bool ReadDSAinxx(
            out int ptu11, out int ptu12, out int ptu13,
            out int ptu21, out int ptu22, out int ptu23,
            out int ptu31, out int ptu32, out int ptu33
            )
        {
            if (saveLOG)
            {
                cmdFS.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",DSAin,READ,");
            }
            try
            {
                Task<dsaINxx> _wrt1x = Task.Factory.StartNew(() => { return ReadDSAinxx(0); });
                Task<dsaINxx> _wrt2x = Task.Factory.StartNew(() => { return ReadDSAinxx(1); });
                Task<dsaINxx> _wrt3x = Task.Factory.StartNew(() => { return ReadDSAinxx(2); });
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
                if (ptu11 >= 0 && ptu12 >= 0 && ptu13 >= 0 &&
                    ptu21 >= 0 && ptu22 >= 0 && ptu23 >= 0 &&
                    ptu31 >= 0 && ptu23 >= 0 && ptu33 >= 0)
                {
                    if (saveLOG)
                    {
                        cmdFS.Write(ptu11.ToString("X2") + ",");
                        cmdFS.Write(ptu12.ToString("X2") + ",");
                        cmdFS.Write(ptu13.ToString("X2") + ",");
                        cmdFS.Write(ptu21.ToString("X2") + ",");
                        cmdFS.Write(ptu22.ToString("X2") + ",");
                        cmdFS.Write(ptu23.ToString("X2") + ",");
                        cmdFS.Write(ptu31.ToString("X2") + ",");
                        cmdFS.Write(ptu32.ToString("X2") + ",");
                        cmdFS.Write(ptu33.ToString("X2") + ",");
                        cmdFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",TRUE");
                    }
                    return true; }
                else
                {
                    if (saveLOG)
                    {
                        cmdFS.WriteLine(",,,,,,,,," + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                    }
                    return false; }
            }
            catch
            {
                ptu11 = -1;
                ptu12 = -1;
                ptu13 = -1;
                ptu21 = -1;
                ptu22 = -1;
                ptu23 = -1;
                ptu31 = -1;
                ptu32 = -1;
                ptu33 = -1;
                if (saveLOG)
                {
                    cmdFS.WriteLine(",,,,,,,,," + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",FALSE");
                }
                return false;
            }
        }

        private dsaINxx ReadDSAinxx(int interfaceNum)
        {
            dsaINxx ptuDAT = new dsaINxx();
            try { ptuDAT.ptux1 = _ptp.unitIFs[interfaceNum].SerialInterface.GetDSAin(0); }
            catch { ptuDAT.ptux1 = -1; }
            try { ptuDAT.ptux2 = _ptp.unitIFs[interfaceNum].SerialInterface.GetDSAin(1); }
            catch { ptuDAT.ptux2 = -1; }
            try { ptuDAT.ptux3 = _ptp.unitIFs[interfaceNum].SerialInterface.GetDSAin(2); }
            catch { ptuDAT.ptux3 = -1; }
            return ptuDAT;
        }

        private void GetSensorValue()
        {
            try
            {
                Task _wrt1x = Task.Factory.StartNew(() => { _ptp.unitIFs[0].GetSensorValue(); });
                Task _wrt2x = Task.Factory.StartNew(() => { _ptp.unitIFs[0].GetSensorValue(); });
                Task _wrt3x = Task.Factory.StartNew(() => { _ptp.unitIFs[0].GetSensorValue(); });
                _wrt1x?.ConfigureAwait(false);
                _wrt2x?.ConfigureAwait(false);
                _wrt3x?.ConfigureAwait(false);
                Task.WaitAll(_wrt1x, _wrt2x, _wrt3x);
            }
            catch(Exception e)
            {
                if (saveLOG)
                {
                    errFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",GetSensorValue,\"" + e.Message + "\"");
                }
            }
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

        private struct dsaINxx
        {
            public int ptux1;
            public int ptux2;
            public int ptux3;
            public dsaINxx(int ptux1, int ptux2, int ptux3)
            {
                this.ptux1 = ptux1; this.ptux2 = ptux2; this.ptux3 = ptux3;
            }
        }
        #endregion

        private void SET_TARGET_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            limitAz = Math.Abs(double.Parse(LIMIT_AZIMUTH_TEXTBOX.Text));
            limitPol = Math.Abs(double.Parse(LIMIT_POLAR_TEXTBOX.Text));
            corrLatitude = double.Parse(CORRECTION_LATITUDE_TEXTBOX.Text);
            corrLongitude = double.Parse(CORRECTION_LONGITUDE_TEXTBOX.Text);
            corrAltitude = double.Parse(CORRECTION_ALTITUDE_TEXTBOX.Text);
            corrAz = double.Parse(CORRECTION_AZIMUTH_TEXTBOX.Text);
            corrPol = double.Parse(CORRECTION_POLAR_TEXTBOX.Text);
            targLatitude = TARGET_POSITION_INPUT.Latitude;
            targLongitude = TARGET_POSITION_INPUT.Longitude;
            targAltitude = TARGET_POSITION_INPUT.Altitude;
        }

        private void TRACK_TASK_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (_trackTASK_state)
            {
                TrackingTASK_STOP(true);
            }
            else
            {
                TrackingTASK_START();
            }
        }

        #region Track calculate and tasks

        private void CalcTargetAngle()
        {
            if (winPOSmonitor.DATA != null)
            {
                //trackCalc_az = -(winPOSmonitor.DATA.PITCH - PITCH_ZERO);
                //trackCalc_pol = -(winPOSmonitor.DATA.HEADING - HEADING_ZERO);
                trackCalc_az = winPOSmonitor.DATA.PITCH;
                trackCalc_pol = winPOSmonitor.DATA.ROLL;

                if (Math.Abs(trackCalc_az) <= Math.Abs(limitAz)) { _trackTASK_az = trackCalc_az; }
                else { _trackTASK_az = Math.Sign(trackCalc_az) * Math.Abs(limitAz); }
                if (Math.Abs(trackCalc_pol) <= Math.Abs(limitPol)) { _trackTASK_pol = trackCalc_pol; }
                else { _trackTASK_pol = Math.Sign(trackCalc_pol) * Math.Abs(limitPol); }
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                targ_az.Content = _trackTASK_az.ToString("0.0000").PadLeft(9, ' ');
                targ_pol.Content = _trackTASK_pol.ToString("0.0000").PadLeft(9, ' ');
                calc_az.Content = trackCalc_az.ToString("0.0000").PadLeft(9, ' ');
                calc_pol.Content = trackCalc_pol.ToString("0.0000").PadLeft(9, ' ');
            }));
        }

        private bool WriteTarget(double frequency ,double az, double pol)
        {
            bool result = false;
            MWComLibCS.CoordinateSystem.AntennaCS targ = new MWComLibCS.CoordinateSystem.AntennaCS(new MWComLibCS.Angle(az, false), new MWComLibCS.Angle(pol, false), coordSYS);
            result = WriteDPSxx(
                _ptp.unitIFs[0].UNITs[0].GetPhaseDelay(frequency, targ), _ptp.unitIFs[0].UNITs[1].GetPhaseDelay(frequency, targ), _ptp.unitIFs[0].UNITs[2].GetPhaseDelay(frequency, targ),
                _ptp.unitIFs[1].UNITs[0].GetPhaseDelay(frequency, targ), _ptp.unitIFs[1].UNITs[1].GetPhaseDelay(frequency, targ), _ptp.unitIFs[1].UNITs[2].GetPhaseDelay(frequency, targ),
                _ptp.unitIFs[2].UNITs[0].GetPhaseDelay(frequency, targ), _ptp.unitIFs[2].UNITs[1].GetPhaseDelay(frequency, targ), _ptp.unitIFs[2].UNITs[2].GetPhaseDelay(frequency, targ));
            return result;
        }

        private void TrackingTASK_START()
        {
            _trackTASK_Freq = double.Parse(CALCULATE_FREQUENCY_TEXTBOX.Text) * Math.Pow(10.0, 6.0);
            trackSNS = uint.Parse(INTERVAL_COUNT_TEXTBOX.Text);
            _ptp.PANEL_SensorMonitor_TASK_Pause();
            _trackTASK_state = true;
            _trackTASK = Task.Factory.StartNew(() => { TrackingTASK(); });
            BOARD_CONFIG_EXPANDER.IsEnabled = false;
            SET_TARGET_BUTTON.IsEnabled = false;
            TRACKING_TARGET_GRID.IsEnabled = false;
            TRACKING_LIMIT_GRID.IsEnabled = false;
            TRACKING_CORRECTION_GRID.IsEnabled = false;
            TRACK_TASK_BUTTON_TEXT.Text = "STOP";
        }

        private void TrackingTASK_STOP(bool wait)
        {
            _trackTASK_state = false;
            if (wait)
            {
                _trackTASK?.ConfigureAwait(false);
                _trackTASK?.Wait();
            }
            _ptp.PANEL_SensorMonitor_TASK_Restart();
            BOARD_CONFIG_EXPANDER.IsEnabled = true;
            SET_TARGET_BUTTON.IsEnabled = true;
            TRACKING_TARGET_GRID.IsEnabled = true;
            TRACKING_LIMIT_GRID.IsEnabled = true;
            TRACKING_CORRECTION_GRID.IsEnabled = true;
            TRACK_TASK_BUTTON_TEXT.Text = "Tracking Start";
        }

        private void TrackingTASK()
        {
            uint count = 0;
            while (_trackTASK_state)
            {
                double az = _trackTASK_az;
                double pol = _trackTASK_pol;
                if (!WriteTarget(_trackTASK_Freq,_trackTASK_az,_trackTASK_pol)) { TRACK_TASK_OnError(); }
                if (count >= trackSNS)
                {
                    Task _getSens1x = Task.Factory.StartNew(() => { _ptp.unitIFs[0].GetAnalogValue(); });
                    Task _getSens2x = Task.Factory.StartNew(() => { _ptp.unitIFs[1].GetAnalogValue(); });
                    Task _getSens3x = Task.Factory.StartNew(() => { _ptp.unitIFs[2].GetAnalogValue(); });
                    _getSens1x?.ConfigureAwait(false);
                    _getSens2x?.ConfigureAwait(false);
                    _getSens3x?.ConfigureAwait(false);
                    Task.WaitAll(_getSens1x, _getSens2x, _getSens3x);
                    count = 0;
                }
                count++;
            }
        }
        private void TRACK_TASK_OnError()
        {
            if (saveLOG)
            {
                errFS.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff") + ",TRACKING,WRITE DPS FAILED");
            }
            if (MessageBox.Show("Write dps failed.\nDo you want to close the port and abort the process?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error)
                == MessageBoxResult.Yes)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DISCONNECT(false);
                }));
            }
        }

        #endregion
    }
}
