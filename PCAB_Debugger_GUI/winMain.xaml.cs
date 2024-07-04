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
        public PCAB _mod;
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
            //Settings.Default.Reset();
            this.Title += "_DEBUG MODE";
#endif
            _state = false;
            SERIAL_PORTS_COMBOBOX_RELOAD();
            if(SERIAL_PORTS_COMBOBOX.Items.Count > 0) { SERIAL_PORTS_COMBOBOX.SelectedIndex = 0; CONNECT_BUTTON.IsEnabled = true; }
            sesn = VisaControlNI.NewResourceManager();
            for(int i = 0; i < ports.Length; i++) { if(Settings.Default.spCaption == ports[i].Caption) { SERIAL_PORTS_COMBOBOX.SelectedIndex = i; break; } }
            WAITE_TIME_TEXTBOX.Text = Settings.Default.mli.ToString("0");
            SERIAL_NUMBERS_TEXTBOX.Text = Settings.Default.sn;
            VISAADDR_TEXTBOX.Text = Settings.Default.visaAddr;
            TIMEOUT_TEXTBOX.Text = Settings.Default.visaTO.ToString("0");
            FILEHEADER_TEXTBOX.Text = Settings.Default.fnHeader;

            DPS_LoopEnable.IsChecked = true;
            DSA_LoopEnable.IsChecked = true;
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
                SERIAL_CONFIG_GRID.IsEnabled = true;
                CONTL_GRID.IsEnabled = false;
                ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Connect";
            }
            else
            {
                _mod = new PCAB(ports[SERIAL_PORTS_COMBOBOX.SelectedIndex].Name);
                string[] sn = SERIAL_NUMBERS_TEXTBOX.Text.Replace(" ", "").Split(',');
                _mod.OnUpdateDAT += OnUpdateDAT;
                _mod.OnError += OnError;
                if (_mod.PCAB_AutoTaskStart(uint.Parse(WAITE_TIME_TEXTBOX.Text), sn))
                {
                    SERIAL_NUMBERS_COMBOBOX.Items.Clear();
                    foreach (condDAT condDAT in _mod.DAT) { SERIAL_NUMBERS_COMBOBOX.Items.Add(condDAT.SN); }
                    SERIAL_NUMBERS_COMBOBOX.SelectedIndex = 0;
                    SERIAL_NUMBERS_COMBOBOX_DropDownClosed(this, e);
                    SERIAL_PORTS_COMBOBOX.IsEnabled = false;
                    SERIAL_CONFIG_GRID.IsEnabled = false;
                    CONTL_GRID.IsEnabled = true;
                    _state = true;
                    ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Disconnect";
                    read_conf(SERIAL_NUMBERS_COMBOBOX.Text);
                    if(SERIAL_NUMBERS_COMBOBOX.Text == "*" && _mod.PCAB_CMD("*", "GetMODE", 1) == "0x2A\n")
                    {
                        if(MessageBox.Show("Do you want to launch a binary editor?", "Binary editor",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            this.Hide();
                            Window win = new winEditor(_mod);
                            win.ShowDialog();
                            this.Show();
                        }
                    }
                }
                else
                {
                    _mod = null;
                    MessageBox.Show("This PCAB does not apply.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SERIAL_NUMBERS_COMBOBOX_DropDownOpened(object sender, EventArgs e)
        {
        }

        private void SERIAL_NUMBERS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SERIAL_NUMBERS_COMBOBOX.SelectedIndex < 0) { SERIAL_NUMBERS_COMBOBOX.SelectedIndex = 0; }
            if (_mod != null)
            {
                _mod.CondNOW.SN = SERIAL_NUMBERS_COMBOBOX.Text;
            }
        }

        private void SAVEADDRESS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SAVEADDRESS_COMBOBOX.SelectedIndex < 0) { SAVEADDRESS_COMBOBOX.SelectedIndex = 0; }
        }

        private void LOADMEM_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Load configuration from memory."
                , "Warning",MessageBoxButton.OKCancel,MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
            string strBf = _mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text , "LMEM " + SAVEADDRESS_COMBOBOX.Text, 1);
            if (strBf.Substring(0, 3) != "ERR")
            {
                _mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "CUI 0", 1);
                _mod.DiscardInBuffer();
                read_conf(SERIAL_NUMBERS_COMBOBOX.Text);
                MessageBox.Show("Load memory done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Load memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Restore default settins."
                , "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
            if (_mod.PCAB_PRESET(SERIAL_NUMBERS_COMBOBOX.Text))
            {
                read_conf(SERIAL_NUMBERS_COMBOBOX.Text);
                MessageBox.Show("Preset done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Reset error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SAVEMEM_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Save settings to memory."
                , "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
            if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SMEM " + SAVEADDRESS_COMBOBOX.Text, 1).Substring(0, 4) == "DONE")
            {
                MessageBox.Show("Save memory done.","Success",MessageBoxButton.OK,MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Save memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WRITE_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                Button btn = (Button)sender;
                bool res = true;
                if (btn.Name == "WRITEDSA" || btn.Name == "WRITE")
                {
                    res = true;
                    foreach (object objBf in Port_GRID.Children)
                    {
                        if (typeof(Grid) == objBf.GetType())
                        {
                            if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID"))
                            {
                                foreach (object objChild in ((Grid)objBf).Children)
                                {
                                    if (typeof(ComboBox) == objChild.GetType())
                                    {
                                        if (Regex.IsMatch(((ComboBox)objChild).Name, "DSA[0-1][0-9]_COMBOBOX"))
                                        {
                                            if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetDSA " + int.Parse(((Grid)objBf).Name.Substring(1, 2)).ToString("0") +
                                            " " + ((ComboBox)objChild).SelectedIndex.ToString("0"), 1).Substring(0, 4) != "DONE")
                                            {
                                                res = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (res && _mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "WrtDSA", 1).Substring(0, 4) == "DONE")
                    {
                        if (btn.Name == "WRITEDSA")
                        {
                            MessageBox.Show("Write att config done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Write att error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if (btn.Name == "WRITEDPS" || btn.Name == "WRITE")
                {
                    res = true;
                    foreach (object objBf in Port_GRID.Children)
                    {
                        if (typeof(Grid) == objBf.GetType())
                        {
                            if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID"))
                            {
                                foreach (object objChild in ((Grid)objBf).Children)
                                {
                                    if (typeof(ComboBox) == objChild.GetType())
                                    {
                                        if (Regex.IsMatch(((ComboBox)objChild).Name, "DPS[0-1][0-9]_COMBOBOX"))
                                        {
                                            if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetDPS " + int.Parse(((Grid)objBf).Name.Substring(1, 2)).ToString("0") +
                                            " " + ((ComboBox)objChild).SelectedIndex.ToString("0"), 1).Substring(0, 4) != "DONE")
                                            {
                                                res = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (res && _mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "WrtDPS", 1).Substring(0, 4) == "DONE")
                    {
                        if (btn.Name == "WRITEDPS")
                        {
                            MessageBox.Show("Write phase config done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if(btn.Name == "WRITE")
                {
                    MessageBox.Show("Write att and phase config done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            if(typeof(CheckBox) == sender.GetType())
            {
                switch (((CheckBox)sender).Name)
                {
                    case "STBAMP_CHECKBOX":
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetSTB.AMP 1", 1).Substring(0, 4) != "DONE")
                        { ((CheckBox)sender).IsChecked = null; MessageBox.Show("SetSTB.AMP Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "STBDRA_CHECKBOX":
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetSTB.DRA 1", 1).Substring(0, 4) != "DONE")
                        { ((CheckBox)sender).IsChecked = null; MessageBox.Show("SetSTB.DRA Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "STBLNA_CHECKBOX":
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetSTB.LNA 1", 1).Substring(0, 4) != "DONE")
                        { ((CheckBox)sender).IsChecked = null; MessageBox.Show("SetSTB.LNA Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "SETLPM_CHECKBOX":
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetLPM 1", 1).Substring(0, 4) != "DONE")
                        { ((CheckBox)sender).IsChecked = null; MessageBox.Show("SetLPM Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
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
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetSTB.AMP 0", 1).Substring(0, 4) != "DONE")
                        { ((CheckBox)sender).IsChecked = null; MessageBox.Show("SetSTB.AMP Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "STBDRA_CHECKBOX":
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetSTB.DRA 0", 1).Substring(0, 4) != "DONE")
                        { ((CheckBox)sender).IsChecked = null; MessageBox.Show("SetSTB.DRA Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "STBLNA_CHECKBOX":
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetSTB.LNA 0", 1).Substring(0, 4) != "DONE")
                        { ((CheckBox)sender).IsChecked = null; MessageBox.Show("SetSTB.LNA Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case "SETLPM_CHECKBOX":
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetLPM 0", 1).Substring(0, 4) != "DONE")
                        { ((CheckBox)sender).IsChecked = null; MessageBox.Show("SetLPM Command Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
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
                    default: break;
                }
            }
        }

        private void CHECKBOX_Indeterminate(object sender, RoutedEventArgs e)
        {
            if(typeof(CheckBox) != sender.GetType())
            {
                switch ((string)sender)
                {
                    case "STBAMP":
                        STBAMP_CHECKBOX.IsChecked = null;
                        break;
                    case "STBDRA":
                        STBDRA_CHECKBOX.IsChecked = null;
                        break;
                    case "STBLNA":
                        STBLNA_CHECKBOX.IsChecked = null;
                        break;
                    case "LPM":
                        SETLPM_CHECKBOX.IsChecked = null;
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
                List<int> ps = new List<int>();
                string message;
                ps.Clear();
                foreach (object objBF in LOOP_GRID.Children)
                {
                    if (typeof(CheckBox) == objBF.GetType())
                    {
                        if (((CheckBox)objBF).IsChecked == true)
                        {
                            ps.Add(int.Parse(((CheckBox)objBF).Content.ToString().Substring(2)));
                        }
                    }
                }
                if(ps.Count <= 0)
                {
                    MessageBox.Show("Please select one or more PS.", "Warning",MessageBoxButton.OK,MessageBoxImage.Warning);
                    return;
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
                        foreach (int p in ps)
                        {
                            //Write Phase State
                            if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetDPS " + p.ToString("0") + " " + pss_num.ToString("0"), 1).Substring(0, 4) != "DONE")
                            {
                                MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, " WrtDPS", 1).Substring(0, 4) != "DONE")
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
                    foreach (int p in ps)
                    {
                        //Write Phase State
                        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetDPS " + p.ToString("0") + " 0", 1).Substring(0, 4) != "DONE")
                        {
                            MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, " WrtDPS", 1).Substring(0, 4) != "DONE")
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

        #region LOOP EVENT
        private void LOOP_START_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            //Get Configuration
            uint stepDPS = 0;
            uint stepDSA = 0;
            int waitTIME = -1;
            List<int> dps = new List<int>();
            List<int> dsa = new List<int>();
            dps.Clear();
            dsa.Clear();
            if (DPS_LoopEnable.IsChecked == true)
            {
                waitTIME = int.Parse(WAITTIME_TEXTBOX.Text);
                stepDPS = (uint)Math.Pow(2, (double)DPSstep_COMBOBOX.SelectedIndex);
                foreach (object objBF in DPS_LOOP_GRID.Children)
                {
                    if (typeof(CheckBox) == objBF.GetType())
                    {
                        if (((CheckBox)objBF).IsChecked == true)
                        {
                            dps.Add(int.Parse(((CheckBox)objBF).Content.ToString().Substring(3)));
                        }
                    }
                }
            }
            if (DSA_LoopEnable.IsChecked == true)
            {
                waitTIME = int.Parse(WAITTIME_TEXTBOX.Text);
                stepDSA = (uint)Math.Pow(2, (double)DSAstep_COMBOBOX.SelectedIndex);
                foreach (object objBF in DSA_LOOP_GRID.Children)
                {
                    if (typeof(CheckBox) == objBF.GetType())
                    {
                        if (((CheckBox)objBF).IsChecked == true)
                        {
                            dsa.Add(int.Parse(((CheckBox)objBF).Content.ToString().Substring(3)));
                        }
                    }
                }
            }
            if(waitTIME < 0) { return; }
            _mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "CUI 0", 1);
            _mod.DiscardInBuffer();
            read_conf(SERIAL_NUMBERS_COMBOBOX.Text);

            winLoop win = new winLoop(_mod, SERIAL_NUMBERS_COMBOBOX.Text, stepDPS, stepDSA, waitTIME, dps, dsa);
            if (win.ShowDialog() != false) { WRITE_Click(WRITE, e); }
            else { MessageBox.Show("Loop function.","Error",MessageBoxButton.OK,MessageBoxImage.Error); }
        }

        private void DPS_LoopEnable_Checked(object sender, RoutedEventArgs e)
        {
            DPSstep_COMBOBOX.IsEnabled = true;
            DPS_LOOP_GRID.IsEnabled = true;
            LOOP_CONF_GRID.IsEnabled = true;
        }
        private void DPS_LoopEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            DPSstep_COMBOBOX.IsEnabled = false;
            DPS_LOOP_GRID.IsEnabled = false;
            if (DSA_LoopEnable.IsChecked != true) { LOOP_CONF_GRID.IsEnabled = false; }
        }

        private void DSA_LoopEnable_Checked(object sender, RoutedEventArgs e)
        {
            DSAstep_COMBOBOX.IsEnabled = true;
            DSA_LOOP_GRID.IsEnabled = true;
            LOOP_CONF_GRID.IsEnabled = true;
        }

        private void DSA_LoopEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            DSAstep_COMBOBOX.IsEnabled = false;
            DSA_LOOP_GRID.IsEnabled = false;
            if (DPS_LoopEnable.IsChecked != true) { LOOP_CONF_GRID.IsEnabled = false; }
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
            if (_state)
            {
                if (MessageBox.Show("Communication with PCAB\nDo you want to disconnect and exit?", "Worning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    _mod.PCAB_AutoTaskStop(); _mod = null; _state = false;
                }
                else { e.Cancel = true; }
            }
            Settings.Default.spCaption = SERIAL_PORTS_COMBOBOX.Text;
            Settings.Default.mli = long.Parse(WAITE_TIME_TEXTBOX.Text);
            Settings.Default.sn = SERIAL_NUMBERS_TEXTBOX.Text;
            Settings.Default.visaAddr = VISAADDR_TEXTBOX.Text;
            Settings.Default.visaTO = long.Parse(TIMEOUT_TEXTBOX.Text);
            Settings.Default.fnHeader = FILEHEADER_TEXTBOX.Text;
            Settings.Default.Save();
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
                SERIAL_CONFIG_GRID.IsEnabled = true;
                CONTL_GRID.IsEnabled = false;
                ((TextBlock)((Viewbox)CONNECT_BUTTON.Content).Child).Text = "Connect";
            }));
        }

        private void OnUpdateDAT(object sender, PCABEventArgs e)
        {
            uint uiBF;
            Int16 i16BF;
            string id = "ND", vd = "ND", vin = "ND", cpu = "ND", pin = "ND";
            string[] tmp = { "ND", "ND", "ND", "ND", "ND", "ND", "ND", "ND", "ND", "ND", "ND", "ND", "ND", "ND", "ND" };
            if (_mod != null)
            {
                _mod.CondNOW.SN = e.ReceiveDAT.SN;
                _mod.CondNOW.Id = e.ReceiveDAT.Id;
                _mod.CondNOW.Vd = e.ReceiveDAT.Vd;
                _mod.CondNOW.TEMPs = e.ReceiveDAT.TEMPs;
                _mod.CondNOW.Vin = e.ReceiveDAT.Vin;
                _mod.CondNOW.CPU_TEMP = e.ReceiveDAT.CPU_TEMP;
                if (!uint.TryParse(e.ReceiveDAT.Id, out uiBF)) { id = "ND"; }
                else { id = ((uiBF * 3.3f / (1 << 12) - 0.08) / 0.737).ToString("0.00"); }
                //else { id = ((uiBF * 3.3f / (1 << 12) - 0.08) / 0.737).ToString("0.00"); }
                if (!uint.TryParse(e.ReceiveDAT.Vd, out uiBF)) { vd = "ND"; }
                else { vd = (uiBF * 3.3f / (1 << 12) * 10.091f).ToString("0.00"); }
                if (!uint.TryParse(e.ReceiveDAT.Vin, out uiBF)) { vin = "ND"; }
                else { vin = (uiBF * 3.3f / (1 << 12) * 15).ToString("0.00"); }
                if (!uint.TryParse(e.ReceiveDAT.CPU_TEMP, out uiBF)) { cpu = "ND"; }
                else { cpu = (27.0f - (uiBF * 3.3f / (1 << 12) - 0.706f) / 0.001721f).ToString("0.00"); }
                if (!uint.TryParse(e.ReceiveDAT.Pin, out uiBF)) { pin = "ND"; }
                else { pin = (uiBF * 3.3f / (1 << 12)).ToString("0.00"); }
                string[] arrBf = e.ReceiveDAT.TEMPs.Split(',');
                for (int i = 0; i < arrBf.Length; i++)
                {
                    if (!Int16.TryParse(arrBf[i], out i16BF)) { tmp[i] = "ND"; }
                    else { tmp[i] = (i16BF / 16.0f).ToString("0.00"); }
                }
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SNS_ID_LABEL.Content = id;
                SNS_VD_LABEL.Content = vd;
                SNS_VIN_LABEL.Content = vin;
                SNS_CPU_TEMP_LABEL.Content = cpu;
                SNS_PIN_LABEL.Content = pin;
                TEMP01.Content = tmp[0];
                TEMP02.Content = tmp[1];
                TEMP03.Content = tmp[2];
                TEMP04.Content = tmp[3];
                TEMP05.Content = tmp[4];
                TEMP06.Content = tmp[5];
                TEMP07.Content = tmp[6];
                TEMP08.Content = tmp[7];
                TEMP09.Content = tmp[8];
                TEMP10.Content = tmp[9];
                TEMP11.Content = tmp[10];
                TEMP12.Content = tmp[11];
                TEMP13.Content = tmp[12];
                TEMP14.Content = tmp[13];
                TEMP15.Content = tmp[14];
            }));
        }

        #endregion

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

        private void read_conf(string serialNum)
        {
            string strBf;
            strBf = _mod.PCAB_CMD(serialNum, "GetSTB.AMP", 1);
            if (strBf == "1\n") { CHECKBOX_Checked("STBAMP", null); } else if (strBf == "0\n") { CHECKBOX_Unchecked("STBAMP", null); } else { CHECKBOX_Indeterminate("STBAMP", null); }
            strBf = _mod.PCAB_CMD(serialNum, "GetSTB.DRA", 1);
            if (strBf == "1\n") { CHECKBOX_Checked("STBDRA", null); } else if (strBf == "0\n") { CHECKBOX_Unchecked("STBDRA", null); } else { CHECKBOX_Indeterminate("STBDRA", null); }
            strBf = _mod.PCAB_CMD(serialNum, "GetSTB.LNA", 1);
            if (strBf == "1\n") { CHECKBOX_Checked("STBLNA", null); } else if (strBf == "0\n") { CHECKBOX_Unchecked("STBLNA", null); } else { CHECKBOX_Indeterminate("STBLNA", null); }
            strBf = _mod.PCAB_CMD(serialNum, "GetLPM", 1);
            if (strBf == "1\n") { CHECKBOX_Checked("LPM", null); } else if (strBf == "0\n") { CHECKBOX_Unchecked("LPM", null); } else { CHECKBOX_Indeterminate("LPM", null); }
            for (int i = 0; i < 16; i++)
            {
                if (int.TryParse(strBf.Trim('\n').Trim(' '), out _))
                {
                    foreach (object objBf in Port_GRID.Children)
                    {
                        if (typeof(Grid) == objBf.GetType())
                        {
                            if (((Grid)objBf).Name == "P" + (i + 1).ToString("00") + "_GRID")
                            {
                                foreach (object objChild in ((Grid)objBf).Children)
                                {
                                    if (typeof(ComboBox) == objChild.GetType())
                                    {
                                        if (((ComboBox)objChild).Name == "DSA" + (i + 1).ToString("00") + "_COMBOBOX")
                                        {
                                            strBf = _mod.PCAB_CMD(serialNum, "GetDSA now " + (i + 1).ToString(), 1);
                                            ((ComboBox)objChild).SelectedIndex = int.Parse(strBf.Trim('\n').Trim(' '));
                                        }
                                        if (((ComboBox)objChild).Name == "DPS" + (i + 1).ToString("00") + "_COMBOBOX")
                                        {
                                            strBf = _mod.PCAB_CMD(serialNum, "GetDPS now " + (i + 1).ToString(), 1);
                                            ((ComboBox)objChild).SelectedIndex = int.Parse(strBf.Trim('\n').Trim(' '));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
