﻿using MWComLibCS.ExternalControl;
using PCAB_Debugger_GUI.Properties;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public int sesn;

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
            CONTL_GRID.IsEnabled = true;
#endif
            _state = false;
            SERIAL_PORTS_COMBOBOX_RELOAD();
            if (SERIAL_PORTS_COMBOBOX.Items.Count > 0) { SERIAL_PORTS_COMBOBOX.SelectedIndex = 0; CONNECT_BUTTON.IsEnabled = true; }
            sesn = VisaControlNI.NewResourceManager();
            if (ports != null) { for (int i = 0; i < ports.Length; i++) { if (Settings.Default.spCaption == ports[i].Caption) { SERIAL_PORTS_COMBOBOX.SelectedIndex = i; break; } } }
            WAITE_TIME_TEXTBOX.Text = Settings.Default.mli.ToString("0");
            SERIAL_NUMBERS_TEXTBOX.Text = Settings.Default.sn;

            DPS_VnaLoopEnable.IsChecked = true;
            DSA_VnaLoopEnable.IsChecked = true;
            //VNALOOP_SCRE_CHECKBOX.IsChecked = true;
            //VNALOOP_TRA_CHECKBOX.IsChecked = true;
            //VNALOOP_SNP_CHECKBOX.IsChecked = true;
            VNALOOP_VISAADDR_TEXTBOX.Text = Settings.Default.visaAddr;
            VNALOOP_TIMEOUT_TEXTBOX.Text = Settings.Default.visaTO.ToString("0");
            VNALOOP_FILEHEADER_TEXTBOX.Text = Settings.Default.fnHeader;
            foreach (object objBF in DPS_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = true; }
            }
            foreach (object objBF in DSA_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = true; }
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
                    if(SERIAL_NUMBERS_COMBOBOX.Text == "*" && _mod.PCAB_CMD("*", "GetMODE", 1) == "0x0F\n")
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
                if (btn.Name == "WRITEDSA" || btn.Name == "WRITE" || btn.Name == "WRITE_EVENT")
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
                if (btn.Name == "WRITEDPS" || btn.Name == "WRITE" || btn.Name == "WRITE_EVENT")
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

        private void ALL_DSA_CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            ALL_DSA_COMBOBOX.IsEnabled = true;
            foreach (object objBf in Port_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID") && ((Grid)objBf).Name != "P16_GRID")
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (Regex.IsMatch(((ComboBox)objChild).Name, "DSA[0-1][0-9]_COMBOBOX"))
                                {
                                    ((ComboBox)objChild).IsEnabled = false;
                                    ((ComboBox)objChild).SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DSA_CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            ALL_DSA_COMBOBOX.IsEnabled = false;
            foreach (object objBf in Port_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID") && ((Grid)objBf).Name != "P16_GRID")
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (Regex.IsMatch(((ComboBox)objChild).Name, "DSA[0-1][0-9]_COMBOBOX"))
                                {
                                    ((ComboBox)objChild).IsEnabled = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DPS_CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            ALL_DPS_COMBOBOX.IsEnabled = true;
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
                                    ((ComboBox)objChild).IsEnabled = false;
                                    ((ComboBox)objChild).SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DPS_CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            ALL_DPS_COMBOBOX.IsEnabled = false;
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
                                    ((ComboBox)objChild).IsEnabled = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DSA_COMBOBOX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Port_GRID == null) { return; }
            foreach (object objBf in Port_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID") && ((Grid)objBf).Name != "P16_GRID")
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (Regex.IsMatch(((ComboBox)objChild).Name, "DSA[0-1][0-9]_COMBOBOX"))
                                {
                                    ((ComboBox)objChild).SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DPS_COMBOBOX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Port_GRID == null) { return; }
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
                                    ((ComboBox)objChild).SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region VNALOOP EVENT

        private void VNALOOP_START_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            string dirPath = "";
            if (VNALOOP_SCRE_CHECKBOX.IsChecked == true ||
                VNALOOP_TRA_CHECKBOX.IsChecked == true)
            {
                using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    fbd.Description = "Please Select Folder";
                    fbd.RootFolder = Environment.SpecialFolder.Desktop;
                    fbd.ShowNewFolderButton = true;
                    if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return;
                    }
                    dirPath = fbd.SelectedPath;
                }
            }

            _mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "CUI 0", 1);
            _mod.DiscardInBuffer();
            read_conf(SERIAL_NUMBERS_COMBOBOX.Text);
            winLoop win = new winLoop(this, dirPath);
            win.ShowDialog();
            Button btn = new Button();
            btn.Name = "WRITE_EVENT";
            WRITE_Click(btn, e);
        }

        private void VNALOOP_VISA_CONNECT_CHECK_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            string strBF = VNALOOP_VISAADDR_TEXTBOX.Text;
            try
            {
                IEEE488 instr;
                instr = new IEEE488(new VisaControlNI(sesn, strBF));
                instr.IEEE488_VisaControl.SetTimeout(uint.Parse(VNALOOP_TIMEOUT_TEXTBOX.Text));
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

        private void VNALOOP_CH_Click(object sender, RoutedEventArgs e)
        {
            if (((RadioButton)sender).Name == "VNALOOP_CH_ALL")
            {
                VNALOOP_CHANNEL_COMBOBOX.IsEnabled = false;
                VNALOOP_CH_SEL.IsChecked = false;
            }
            else
            {
                VNALOOP_CHANNEL_COMBOBOX.IsEnabled = true;
                VNALOOP_CH_ALL.IsChecked = false;
                VNALOOP_CHANNEL_COMBOBOX.Items.Clear();
                try
                {
                    agPNA835x pna = new agPNA835x(new IEEE488(new VisaControlNI(sesn, VNALOOP_VISAADDR_TEXTBOX.Text)));
                    foreach (uint i in pna.getChannelCatalog())
                    {
                        VNALOOP_CHANNEL_COMBOBOX.Items.Add(i.ToString());
                    }
                    pna.Instrument.Dispose();
                    pna = null;
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    VNALOOP_CHANNEL_COMBOBOX.IsEnabled = false;
                    VNALOOP_CH_SEL.IsChecked = false;
                    VNALOOP_CH_ALL.IsChecked = true;
                }
            }
        }

        private void DPS_VnaLoopEnable_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_DPSstep_COMBOBOX.IsEnabled = true;
            DPS_VNALOOP_GRID.IsEnabled = true;
            foreach (object objBF in DPS_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType())
                {
                    if (((CheckBox)objBF).IsChecked == true)
                    {
                        VNALOOP_CONF_GRID.IsEnabled = true;
                    }
                }
            }
        }

        private void DPS_VnaLoopEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            VNALOOP_DPSstep_COMBOBOX.IsEnabled = false;
            DPS_VNALOOP_GRID.IsEnabled = false;
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DSA_VnaLoopEnable.IsChecked != true)
                {
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
                else
                {
                    foreach (object objBF in DSA_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
            }
        }

        private void DSA_VnaLoopEnable_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_DSAstep_COMBOBOX.IsEnabled = true;
            DSA_VNALOOP_GRID.IsEnabled = true;
            foreach (object objBF in DSA_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType())
                {
                    if (((CheckBox)objBF).IsChecked == true)
                    {
                        VNALOOP_CONF_GRID.IsEnabled = true;
                    }
                }
            }
        }

        private void DSA_VnaLoopEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            VNALOOP_DSAstep_COMBOBOX.IsEnabled = false;
            DSA_VNALOOP_GRID.IsEnabled = false;
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DPS_VnaLoopEnable.IsChecked != true)
                {
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
                else
                {
                    foreach (object objBF in DPS_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
            }
        }

        private void DPSn_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_CONF_GRID.IsEnabled = true;
        }

        private void DPSn_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DPS_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType())
                {
                    if (((CheckBox)objBF).IsChecked == true)
                    {
                        return;
                    }
                }
            }
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DSA_VnaLoopEnable.IsChecked != true)
                {
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
                else
                {
                    foreach (object objBF in DSA_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
            }
        }

        private void DSAn_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_CONF_GRID.IsEnabled = true;
        }

        private void DSAn_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DSA_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType())
                {
                    if (((CheckBox)objBF).IsChecked == true)
                    {
                        return;
                    }
                }
            }
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DPS_VnaLoopEnable.IsChecked != true)
                {
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
                else
                {
                    foreach (object objBF in DPS_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
            }
        }

        private void VNALOOP_SaveTarget_CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_CONF_GRID.IsEnabled = true;
        }

        private void VNALOOP_SaveTarget_CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DPS_VnaLoopEnable.IsChecked == true)
                {
                    foreach (object objBF in DPS_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                }
                if (DSA_VnaLoopEnable.IsChecked == true)
                {
                    foreach (object objBF in DSA_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                }
                VNALOOP_CONF_GRID.IsEnabled = false;
            }
        }

        private void DPS_CHECK_ALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DPS_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = true; }
            }
        }

        private void DPS_UNCHECK_ALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DPS_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = false; }
            }
        }

        private void DSA_CHECK_ALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DSA_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = true; }
            }
        }

        private void DSA_UNCHECK_ALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DSA_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = false; }
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
            Settings.Default.visaAddr = VNALOOP_VISAADDR_TEXTBOX.Text;
            Settings.Default.visaTO = long.Parse(VNALOOP_TIMEOUT_TEXTBOX.Text);
            Settings.Default.fnHeader = VNALOOP_FILEHEADER_TEXTBOX.Text;
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
