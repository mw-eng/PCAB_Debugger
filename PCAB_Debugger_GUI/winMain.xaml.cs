using MWComLibCS.ExternalControl;
using PCAB_Debugger_GUI.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PCAB_Debugger_GUI.agPNA835x;
using static PCAB_Debugger_GUI.PCAB;
using static PCAB_Debugger_GUI.ShowSerialPortName;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class winMain : Window
    {
        SerialPortTable[] ports;
        PCAB _mod;
        bool _state;
        int sesn;

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
            if(SERIAL_PORTS_COMBOBOX.Items.Count > 0) { SERIAL_PORTS_COMBOBOX.SelectedIndex = 0; CONNECT_BUTTON.IsEnabled = true; }
            sesn = VisaControlNI.NewResourceManager();
        }

        #region Serial EVENT

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
                    read_conf();
                }
                else
                {
                    _mod = null;
                    MessageBox.Show("This PCAB does not apply.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void read_conf()
        {
            string strBf;
            if (_mod.PCAB_CMD("GetSTB.AMP", 1) == "STB\n") { CHECKBOX_Checked("STBAMP", null); } else { CHECKBOX_Unchecked("STBAMP", null); }
            if (_mod.PCAB_CMD("GetSTB.DRA", 1) == "STB\n") { CHECKBOX_Checked("STBDRA", null); } else { CHECKBOX_Unchecked("STBDRA", null); }
            if (_mod.PCAB_CMD("GetSTB.LNA", 1) == "STB\n") { CHECKBOX_Checked("STBLNA", null); } else { CHECKBOX_Unchecked("STBLNA", null); }
            if (_mod.PCAB_CMD("GetLPM", 1) == "LOW\n") { CHECKBOX_Checked("LPM", null); } else { CHECKBOX_Unchecked("LPM", null); }
            if (_mod.PCAB_CMD("GetALD", 1) == "ENB\n") { CHECKBOX_Checked("ALD", null); } else { CHECKBOX_Unchecked("ALD", null); }
            for(int i = 0; i < 15; i++)
            {
                strBf = _mod.PCAB_CMD("GetPS" + (i + 1).ToString(), 1);
                if(int.TryParse(strBf.Trim('\n').Trim(' '),out _))
                {
                    foreach(object objBf in PS_GRID.Children)
                    {
                        if(typeof(Grid) == objBf.GetType())
                        {
                            if (((Grid)objBf).Name == "PS" + (i + 1).ToString("00") + "_GRID")
                            {
                                foreach (object objChild in ((Grid)objBf).Children)
                                {
                                    if (typeof(ComboBox) == objChild.GetType())
                                    {
                                        ((ComboBox)objChild).SelectedIndex = int.Parse(strBf.Trim('\n').Trim(' '));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LOADMEM_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(""
                , "Warning",MessageBoxButton.OKCancel,MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
            string strBf = _mod.PCAB_CMD("LMEM", 1);
            if (strBf != "ERR\n")
            {
                _mod.PCAB_CMD("CUI 1", 1);
                _mod.DiscardInBuffer();
                read_conf();
                MessageBox.Show("Load memory done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Load memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SAVEMEM_Click(object sender, RoutedEventArgs e)
        {
            if (_mod.PCAB_CMD("SMEM", 1) == "DONE\n")
            {
                MessageBox.Show("Save memory done.","Success",MessageBoxButton.OK,MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Save memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WRITEPS_Click(object sender, RoutedEventArgs e)
        {
            bool res = true;
            foreach (object objBf in PS_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "PS[0-1][0-9]_GRID"))
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (_mod.PCAB_CMD("SetPS" + int.Parse(((Grid)objBf).Name.Substring(2, 2)).ToString("0") +
                                    " " + ((ComboBox)objChild).SelectedIndex.ToString("0"), 1) != "DONE\n")
                                {
                                    res = false;
                                }
                            }
                        }
                    }
                }
            }
            if (res && _mod.PCAB_CMD("WrtPS",1) == "DONE\n")
            {
                MessageBox.Show("Write phase config done.","Success",MessageBoxButton.OK,MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {
            if (_mod.PCAB_PRESET())
            {
                read_conf();
                MessageBox.Show("Preset done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Reset error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            if(typeof(CheckBox) == sender.GetType())
            {
                switch (((CheckBox)sender).Name)
                {
                    case "STBAMP_CHECKBOX":
                        if (_mod.PCAB_CMD("SetSTB.AMP 1", 1) != "DONE\n") { MessageBox.Show("SetSTB.AMP Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "STBDRA_CHECKBOX":
                        if (_mod.PCAB_CMD("SetSTB.DRA 1", 1) != "DONE\n") { MessageBox.Show("SetSTB.DRA Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "STBLNA_CHECKBOX":
                        if (_mod.PCAB_CMD("SetSTB.LNA 1", 1) != "DONE\n") { MessageBox.Show("SetSTB.LNA Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "SETLPM_CHECKBOX":
                        if (_mod.PCAB_CMD("SetLPM 1", 1) != "DONE\n") { MessageBox.Show("SetLPM Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "SETALD_CHECKBOX":
                        if (_mod.PCAB_CMD("SetALD 1", 1) != "DONE\n") { MessageBox.Show("SetALD Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    default: break;
                }
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
                switch (((CheckBox)sender).Name)
                {
                    case "STBAMP_CHECKBOX":
                        if (_mod.PCAB_CMD("SetSTB.AMP 0", 1) != "DONE\n") { MessageBox.Show("SetSTB.AMP Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "STBDRA_CHECKBOX":
                        if (_mod.PCAB_CMD("SetSTB.DRA 0", 1) != "DONE\n") { MessageBox.Show("SetSTB.DRA Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "STBLNA_CHECKBOX":
                        if (_mod.PCAB_CMD("SetSTB.LNA 0", 1) != "DONE\n") { MessageBox.Show("SetSTB.LNA Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "SETLPM_CHECKBOX":
                        if (_mod.PCAB_CMD("SetLPM 0", 1) != "DONE\n") { MessageBox.Show("SetLPM Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "SETALD_CHECKBOX":
                        if (_mod.PCAB_CMD("SetALD 0", 1) != "DONE\n") { MessageBox.Show("SetALD Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    default: break;
                }
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

        #endregion

        #region VISA EVENT

        private void VISA_CONNECT_CHECK_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            string strBF = VISAADDR_TEXTBOX.Text;
            try
            {
                IEEE488 instr;
                instr = new IEEE488(new VisaControlNI(sesn, strBF));
                IEEE488_IDN idn = instr.IDN();
                MessageBox.Show("Vender\t\t: " + idn.Vender +
                              "\nModel Number\t: " + idn.ModelNumber +
                              "\nRevision Code\t: " + idn.RevisionCode +
                              "\nSerial Number\t: " + idn.SerialNumber, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CH_Click(object sender, RoutedEventArgs e)
        {
            if (((RadioButton)sender).Name == "CH_ALL")
            {
                CHANNEL_COMBOBOX.IsEnabled = false;
                CH_SEL.IsChecked = false;
            }
            else
            {
                CHANNEL_COMBOBOX.IsEnabled = true;
                CH_ALL.IsChecked = false;
                CHANNEL_COMBOBOX.Items.Clear();
                try
                {
                    IEEE488 instr;
                    instr = new IEEE488(new VisaControlNI(sesn, VISAADDR_TEXTBOX.Text));
                    agPNA835x pna = new agPNA835x(instr);
                    foreach (uint i in pna.getChannelCatalog())
                    {
                        CHANNEL_COMBOBOX.Items.Add(i.ToString());
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GETDAT_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Please Select Folder";
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string dirPath = fbd.SelectedPath;
                bool fileFLG = false;
                string filePath;
                int ps = -1;
                string message;
                foreach (object objBF in LOOP_GRID.Children)
                {
                    if (typeof(RadioButton) == objBF.GetType())
                    {
                        if (((RadioButton)objBF).IsChecked == true)
                        {
                            ps = int.Parse(((RadioButton)objBF).Content.ToString().Substring(2));
                            break;
                        }
                    }
                }
                try
                {
                    IEEE488 instr;
                    instr = new IEEE488(new VisaControlNI(sesn, VISAADDR_TEXTBOX.Text));
                    instr.IEEE488_VisaControl.SetTimeout(uint.Parse(TIMEOUT_TEXTBOX.Text));
                    agPNA835x pna = new agPNA835x(instr);
                    uint[] channels;
                    uint[] sheets;
                    List<SweepMode> trigMODE = new List<SweepMode>();
                    //Get Channel Lists
                    if (CH_ALL.IsChecked == true) { channels = pna.getChannelCatalog(); }
                    else { channels = new uint[] { uint.Parse(CHANNEL_COMBOBOX.Text) }; }
                    //Get Sheet Lists
                    sheets = pna.getSheetsCatalog();

                    //File Check
                    if (SCRE_CHECKBOX.IsChecked == true)
                    {
                        foreach (uint i in sheets)
                        {
                            for (int num = 0; num < 64; num++)
                            {
                                if (System.IO.File.Exists(dirPath + "\\" + FILEHEADER_TEXTBOX.Text + "_" + num.ToString("00") + "_Sheet" + i.ToString() + ".png")) { fileFLG = true; }
                            }
                        }
                    }
                    if (TRA_CHECKBOX.IsChecked == true)
                    {
                        foreach (uint i in sheets)
                        {
                            for (int num = 0; num < 64; num++)
                            {
                                if (System.IO.File.Exists(dirPath + "\\" + FILEHEADER_TEXTBOX.Text + "_" + num.ToString("00") + "_Sheet" + i.ToString() + ".csv")) { fileFLG = true; }
                            }
                        }
                    }
                    if (fileFLG)
                    {
                        if (MessageBox.Show("The file exists in the specified folder.\nDo you want to overwrite?",
                            "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) { return; }
                    }

                    for (int pss_num = 0; pss_num < 64; pss_num++)
                    {
                        //Write Phase State
                        if (_mod.PCAB_CMD("SetPS" + ps.ToString("0") + " " + pss_num.ToString("0"), 1) != "DONE\n")
                        {
                            MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (_mod.PCAB_CMD("WrtPS", 1) != "DONE\n")
                        {
                            MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        //Trigger SET
                        if (SING_CHECKBOX.IsChecked == true)
                        {
                            foreach (uint i in channels)
                            {
                                trigMODE.Add(pna.getTriggerMode(i));
                                pna.trigSingle(i);
                            }
                        }
                        //Save Screen
                        if (SCRE_CHECKBOX.IsChecked == true)
                        {
                            foreach (uint sh in sheets)
                            {
                                filePath = dirPath + "\\" + FILEHEADER_TEXTBOX.Text + "_" + pss_num.ToString("00") + "_Sheet" + sh.ToString() + ".png";

                                pna.selectSheet(sh);
                                if (!pna.GetScreen(filePath, out message))
                                {
                                    if (SING_CHECKBOX.IsChecked == true)
                                    {
                                        for (int i = 0; i < channels.Length; i++)
                                        {
                                            pna.setTriggerMode(channels[i], trigMODE[i]);
                                        }
                                    }
                                    MessageBox.Show(message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                        }

                        //Save Trace
                        if (TRA_CHECKBOX.IsChecked == true)
                        {
                            foreach (uint sh in sheets)
                            {
                                filePath = dirPath + "\\" + FILEHEADER_TEXTBOX.Text + "_" + pss_num.ToString("00") + "_Sheet" + sh.ToString() + ".csv";
                                //Select Sheet
                                pna.selectSheet(sh);
                                //Get Trace DAT
                                List<ChartDAT> dat = new List<ChartDAT>();
                                foreach (uint win in pna.getWindowCatalog(sh))
                                {
                                    List<TraceDAT> trace = new List<TraceDAT>();
                                    foreach (uint tra in pna.getTraceCatalog(win))
                                    {
                                        pna.selectTrace(win, tra);
                                        uint ch = pna.getSelectChannel();
                                        uint num = pna.getSelectMeasurementNumber();
                                        //string x = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X:AXIS:UNIT?");
                                        string x = "X";
                                        string y = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":PAR?");
                                        y += "_" + pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":FORM?");
                                        //y += "_" + pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X:AXIS:UNIT?");
                                        string mem = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":MATH:FUNC?");
                                        if (mem.ToUpper() != "NORM")
                                        {
                                            y += "@" + mem + "[MEM]";
                                        }
                                        string[] valx = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X?").Trim().Split(',');
                                        //string[] valy = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":Y?").Trim().Split(',');
                                        string[] valy = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":DATA:FDAT?").Trim().Split(',');
                                        trace.Add(new TraceDAT("CH" + ch.ToString(), x, y, valx, valy));
                                    }
                                    dat.Add(new ChartDAT("Win" + win.ToString(), trace.ToArray()));
                                }
                                //Write CSV Data
                                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                                {
                                    System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

                                    sw.WriteLine("\"PCAB Debugger Ver," + fvi.ProductVersion + "\"");
                                    sw.WriteLine(fvi.LegalCopyright);
                                    sw.WriteLine();
                                    string strBF1 = "";
                                    string strBF2 = "";
                                    string strBF3 = "";
                                    int cnt = 0;
                                    foreach (ChartDAT chart in dat)
                                    {
                                        strBF1 += chart.WindowNumber + ",,";
                                        for (int i = 0; i < chart.Trace.Length - 1; i++)
                                        {
                                            strBF1 += "," + ",";
                                        }
                                        foreach (TraceDAT trace in chart.Trace)
                                        {
                                            strBF2 += trace.ChannelNumber + ",,";
                                            strBF3 += trace.AxisX + "," + trace.AxisY + ",";
                                            if (cnt < trace.ValueX.Length) { cnt = trace.ValueX.Length; }
                                            if (cnt < trace.ValueY.Length) { cnt = trace.ValueY.Length; }
                                        }
                                    }
                                    sw.WriteLine(strBF1.Trim(','));
                                    sw.WriteLine(strBF2.Trim(','));
                                    sw.WriteLine(strBF3.Trim(','));

                                    for (int i = 0; i < cnt; i++)
                                    {
                                        strBF1 = "";
                                        foreach (ChartDAT chart in dat)
                                        {
                                            foreach (TraceDAT trace in chart.Trace)
                                            {
                                                if (trace.ValueX.Length > i) { strBF1 += trace.ValueX[i]; }
                                                strBF1 += ",";
                                                if (trace.ValueY.Length > i) { strBF1 += trace.ValueY[i]; }
                                                strBF1 += ",";
                                            }
                                        }
                                        sw.WriteLine(strBF1.Trim(','));
                                    }
                                    sw.Close();
                                }
                            }
                        }
                        //Trigger ReSET
                        if (SING_CHECKBOX.IsChecked == true)
                        {
                            for (int i = 0; i < channels.Length; i++)
                            {
                                pna.setTriggerMode(channels[i], trigMODE[i]);
                            }
                        }
                    }

                    //Write Phase State0
                    if (_mod.PCAB_CMD("SetPS" + ps.ToString("0") + " 0", 1) != "DONE\n")
                    {
                        MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (_mod.PCAB_CMD("WrtPS", 1) != "DONE\n")
                    {
                        MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    MessageBox.Show("Data acquisition completed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

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

        private void OnError(object sender, PCABEventArgs e)
        {
            _mod.PCAB_AutoTaskStop();
            _mod.Close();
            MessageBox.Show(e.Message.ToString(), "Error",MessageBoxButton.OK,MessageBoxImage.Error);
            _state = false;
            _mod = null;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SERIAL_PORTS_COMBOBOX.IsEnabled = true;
            CONTL_GRID.IsEnabled = false;
            ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Connect";
            }));
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
            foreach(string strBf in arrBf)
            {
                string[] bf = strBf.Split(':');
                list.Add(bf[1]);
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SNS_ID_LABEL.Content = ((double.Parse(e.ReceiveDAT[0]) - 1.65) / 0.09).ToString("0.00");
                SNS_VD_LABEL.Content = (double.Parse(e.ReceiveDAT[1]) * 10.091).ToString("0.00");
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

        #region Structure
        private struct ChartDAT
        {
            public string WindowNumber { get; set; }
            public TraceDAT[] Trace { get; set; }

            public ChartDAT(string winNum, TraceDAT[] trace) { WindowNumber = winNum; Trace = trace; }
        }

        private struct TraceDAT
        {
            public string ChannelNumber { get; set; }
            public string AxisX { get; set; }
            public string AxisY { get; set; }
            public string[] ValueX { get; set; }
            public string[] ValueY { get; set; }
            public TraceDAT(string ch, string x, string y, string[] val_x, string[] val_y)
            {
                ChannelNumber = ch; AxisX = x; AxisY = y; ValueX = val_x; ValueY = val_y;
            }
        }
        #endregion

    }
}
