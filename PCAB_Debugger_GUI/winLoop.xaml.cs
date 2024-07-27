using MWComLibCS.ExternalControl;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// winLoop.xaml の相互作用ロジック
    /// </summary>
    public partial class winLoop : Window
    {
        winMain owner;
        bool runTASK = false;
        uint stepDPS = 0;
        uint stepDSA = 0;
        int waitTIME = -1;
        int cntDPS = -1;
        int cntDSA = -1;
        double proc = 0;
        string sn;
        string dirPath;
        List<int> dps = new List<int>();
        List<int> dsa = new List<int>();
        PCAB _mod;
        IEEE488 instr;

        public winLoop(winMain WINowner,string DirPATH)
        {
            InitializeComponent();
            owner = WINowner;
            dirPath = DirPATH;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get Configuration
            _mod = owner._mod;
            sn = owner.SERIAL_NUMBERS_COMBOBOX.Text;
            _mod.PCAB_CMD(sn, "CUI 0", 1);
            _mod.DiscardInBuffer();
            dps.Clear();
            dsa.Clear();
            if (owner.DPS_VnaLoopEnable.IsChecked == true)
            {
                waitTIME = int.Parse(owner.VNALOOP_WAITTIME_TEXTBOX.Text);
                stepDPS = (uint)Math.Pow(2, (double)owner.VNALOOP_DPSstep_COMBOBOX.SelectedIndex);
                foreach (object objBF in owner.DPS_VNALOOP_GRID.Children)
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
            if (owner.DSA_VnaLoopEnable.IsChecked == true)
            {
                waitTIME = int.Parse(owner.VNALOOP_WAITTIME_TEXTBOX.Text);
                stepDSA = (uint)Math.Pow(2, (double)owner.VNALOOP_DSAstep_COMBOBOX.SelectedIndex);
                foreach (object objBF in owner.DSA_VNALOOP_GRID.Children)
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

            if (owner.VNALOOP_SCRE_CHECKBOX.IsChecked == true ||
                owner.VNALOOP_TRA_CHECKBOX.IsChecked == true ||
                owner.VNALOOP_SNP_CHECKBOX.IsChecked == true)
            {
                instr = new IEEE488(new VisaControlNI(owner.sesn, owner.VNALOOP_VISAADDR_TEXTBOX.Text));
                try
                {
                    IEEE488_IDN idn = instr.IDN();
                }
                catch
                {
                    MessageBox.Show("GPIB Connection Error.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;
                    this.Close();
                }
            }

            if (waitTIME == 0) { runTASK = false; }
            else{ runTASK = true;}
            Task task = Task.Factory.StartNew(() => { LOOP_Task((uint)waitTIME); });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            runTASK = false;
        }

        private void LOOP_Task(UInt32 waiteTime)
        {
            try
            {
                if (runTASK)
                {
                    if (stepDPS > 0 && stepDSA > 0)
                    {
                        cntDPS = 0;
                        cntDSA = 0;
                        OnUpdateDAT();
                        for (cntDPS = 0; cntDPS < 64; cntDPS += (int)stepDPS)
                        {
                            foreach (int p in dps)
                            {
                                //Write Phase State
                                if (_mod.PCAB_CMD(sn, "SetDPS " + p.ToString("0") + " " + cntDPS.ToString("0"), 1).Substring(0, 4) != "DONE") { this.DialogResult = false; this.Close(); }
                            }
                            if (_mod.PCAB_CMD(sn, " WrtDPS", 1).Substring(0, 4) != "DONE") { ExitTASK(); return; }
                            for (cntDSA = 0; cntDSA < 64; cntDSA += (int)stepDSA)
                            {
                                if (!runTASK) { ExitTASK(); return; }
                                foreach (int a in dsa)
                                {
                                    //Write Phase State
                                    if (_mod.PCAB_CMD(sn, "SetDSA " + a.ToString("0") + " " + cntDSA.ToString("0"), 1).Substring(0, 4) != "DONE") { this.DialogResult = false; this.Close(); }
                                }
                                if (_mod.PCAB_CMD(sn, " WrtDSA", 1).Substring(0, 4) != "DONE") { ExitTASK(); return; }
                                proc = (cntDPS / stepDPS * 64.0 / stepDPS + cntDSA / stepDSA) / (64.0 / stepDPS * 64.0 / stepDSA);
                                OnUpdateDAT();
                                Thread.Sleep(waitTIME);
                            }
                        }
                    }
                    else if (stepDPS > 0)
                    {
                        cntDPS = 0;
                        OnUpdateDAT();
                        for (cntDPS = 0; cntDPS < 64; cntDPS += (int)stepDPS)
                        {
                            if (!runTASK) { ExitTASK(); return; }
                            foreach (int p in dps)
                            {
                                //Write Phase State
                                if (_mod.PCAB_CMD(sn, "SetDPS " + p.ToString("0") + " " + cntDPS.ToString("0"), 1).Substring(0, 4) != "DONE") { ExitTASK(); return; }
                            }
                            if (_mod.PCAB_CMD(sn, " WrtDPS", 1).Substring(0, 4) != "DONE") { ExitTASK(); return; }
                            proc = (cntDPS / stepDPS) / (64.0 / stepDPS);
                            OnUpdateDAT();
                            Thread.Sleep(waitTIME);
                        }
                    }
                    else if (stepDSA > 0)
                    {
                        cntDSA = 0;
                        OnUpdateDAT();
                        for (cntDSA = 0; cntDSA < 64; cntDSA += (int)stepDSA)
                        {
                            if (!runTASK) { ExitTASK(); return; }
                            foreach (int a in dsa)
                            {
                                //Write Phase State
                                if (_mod.PCAB_CMD(sn, "SetDSA " + a.ToString("0") + " " + cntDSA.ToString("0"), 1).Substring(0, 4) != "DONE") { ExitTASK(); return; }
                            }
                            if (_mod.PCAB_CMD(sn, " WrtDSA", 1).Substring(0, 4) != "DONE") { ExitTASK(); return; }
                            proc = (cntDSA / stepDSA) / (64.0 / stepDSA);
                            OnUpdateDAT();
                            Thread.Sleep(waitTIME);
                        }
                    }
                    Dispatcher.BeginInvoke(new Action(() => { this.DialogResult = true; this.Close(); })); ;
                }
                else { ExitTASK(); return; }
            }
            catch
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.DialogResult = false;
                    this.Close();
                }));
            }
        }

        private void OnUpdateDAT()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cntDPS < 0) { dps_state.Content = "LOCK"; }
                else { dps_state.Content = (5.625 * cntDPS).ToString() + "deg (" + cntDPS.ToString() + ")"; }
                if (cntDSA < 0) { dsa_state.Content = "LOCK"; }
                else { dsa_state.Content = (0.25 * cntDSA).ToString() + "dB (" + cntDSA.ToString() + ")"; }
                Progress.Value = (int)(proc * 100);
            }));
        }

        private void ExitTASK()
        {
            Dispatcher.BeginInvoke(new Action(() => { this.DialogResult = true; this.Close(); }));
        }

        private void Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageLabel.Content = "Sweep... " + Progress.Value.ToString() + "%";
            }));
        }

        private void vna()
        {
            //bool fileFLG = false;
            //string filePath;
            //List<int> ps = new List<int>();
            //string message;
            //ps.Clear();
            //foreach (object objBF in LOOP_GRID.Children)
            //{
            //    if (typeof(CheckBox) == objBF.GetType())
            //    {
            //        if (((CheckBox)objBF).IsChecked == true)
            //        {
            //            ps.Add(int.Parse(((CheckBox)objBF).Content.ToString().Substring(2)));
            //        }
            //    }
            //}
            //if (ps.Count <= 0)
            //{
            //    MessageBox.Show("Please select one or more PS.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}
            //try
            //{
            //    IEEE488 instr;
            //    instr = new IEEE488(new VisaControlNI(sesn, VISAADDR_TEXTBOX.Text));
            //    instr.IEEE488_VisaControl.SetTimeout(uint.Parse(TIMEOUT_TEXTBOX.Text));
            //    agPNA835x pna = new agPNA835x(instr);
            //    uint[] channels;
            //    uint[] sheets;
            //    List<SweepMode> trigMODE = new List<SweepMode>();
            //    //Get Channel Lists
            //    if (CH_ALL.IsChecked == true) { channels = pna.getChannelCatalog(); }
            //    else { channels = new uint[] { uint.Parse(CHANNEL_COMBOBOX.Text) }; }
            //    //Get Sheet Lists
            //    sheets = pna.getSheetsCatalog();

            //    //File Check
            //    if (SCRE_CHECKBOX.IsChecked == true)
            //    {
            //        foreach (uint i in sheets)
            //        {
            //            for (int num = 0; num < 64; num++)
            //            {
            //                if (System.IO.File.Exists(dirPath + "\\" + FILEHEADER_TEXTBOX.Text + "_" + num.ToString("00") + "_Sheet" + i.ToString() + ".png")) { fileFLG = true; }
            //            }
            //        }
            //    }
            //    if (TRA_CHECKBOX.IsChecked == true)
            //    {
            //        foreach (uint i in sheets)
            //        {
            //            for (int num = 0; num < 64; num++)
            //            {
            //                if (System.IO.File.Exists(dirPath + "\\" + FILEHEADER_TEXTBOX.Text + "_" + num.ToString("00") + "_Sheet" + i.ToString() + ".csv")) { fileFLG = true; }
            //            }
            //        }
            //    }
            //    if (fileFLG)
            //    {
            //        if (MessageBox.Show("The file exists in the specified folder.\nDo you want to overwrite?",
            //            "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) { return; }
            //    }

            //    for (int pss_num = 0; pss_num < 64; pss_num++)
            //    {
            //        foreach (int p in ps)
            //        {
            //            //Write Phase State
            //            if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetDPS " + p.ToString("0") + " " + pss_num.ToString("0"), 1).Substring(0, 4) != "DONE")
            //            {
            //                MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //                return;
            //            }
            //        }
            //        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, " WrtDPS", 1).Substring(0, 4) != "DONE")
            //        {
            //            MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //        }
            //        //Trigger SET
            //        if (SING_CHECKBOX.IsChecked == true)
            //        {
            //            foreach (uint i in channels)
            //            {
            //                trigMODE.Add(pna.getTriggerMode(i));
            //                pna.trigSingle(i);
            //            }
            //        }
            //        //Save Screen
            //        if (SCRE_CHECKBOX.IsChecked == true)
            //        {
            //            foreach (uint sh in sheets)
            //            {
            //                filePath = dirPath + "\\" + FILEHEADER_TEXTBOX.Text + "_" + pss_num.ToString("00") + "_Sheet" + sh.ToString() + ".png";

            //                pna.selectSheet(sh);
            //                if (!pna.GetScreen(filePath, out message))
            //                {
            //                    if (SING_CHECKBOX.IsChecked == true)
            //                    {
            //                        for (int i = 0; i < channels.Length; i++)
            //                        {
            //                            pna.setTriggerMode(channels[i], trigMODE[i]);
            //                        }
            //                    }
            //                    MessageBox.Show(message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            //                    return;
            //                }
            //            }
            //        }

            //        //Save Trace
            //        if (TRA_CHECKBOX.IsChecked == true)
            //        {
            //            foreach (uint sh in sheets)
            //            {
            //                filePath = dirPath + "\\" + FILEHEADER_TEXTBOX.Text + "_" + pss_num.ToString("00") + "_Sheet" + sh.ToString() + ".csv";
            //                //Select Sheet
            //                pna.selectSheet(sh);
            //                //Get Trace DAT
            //                List<ChartDAT> dat = new List<ChartDAT>();
            //                foreach (uint win in pna.getWindowCatalog(sh))
            //                {
            //                    List<TraceDAT> trace = new List<TraceDAT>();
            //                    foreach (uint tra in pna.getTraceCatalog(win))
            //                    {
            //                        pna.selectTrace(win, tra);
            //                        uint ch = pna.getSelectChannel();
            //                        uint num = pna.getSelectMeasurementNumber();
            //                        //string x = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X:AXIS:UNIT?");
            //                        string x = "X";
            //                        string y = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":PAR?");
            //                        y += "_" + pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":FORM?");
            //                        //y += "_" + pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X:AXIS:UNIT?");
            //                        string mem = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":MATH:FUNC?");
            //                        if (mem.ToUpper() != "NORM")
            //                        {
            //                            y += "@" + mem + "[MEM]";
            //                        }
            //                        string[] valx = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X?").Trim().Split(',');
            //                        //string[] valy = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":Y?").Trim().Split(',');
            //                        string[] valy = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":DATA:FDAT?").Trim().Split(',');
            //                        trace.Add(new TraceDAT("CH" + ch.ToString(), x, y, valx, valy));
            //                    }
            //                    dat.Add(new ChartDAT("Win" + win.ToString(), trace.ToArray()));
            //                }
            //                //Write CSV Data
            //                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            //                {
            //                    System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //                    sw.WriteLine("\"PCAB Debugger Ver," + fvi.ProductVersion + "\"");
            //                    sw.WriteLine(fvi.LegalCopyright);
            //                    sw.WriteLine();
            //                    string strBF1 = "";
            //                    string strBF2 = "";
            //                    string strBF3 = "";
            //                    int cnt = 0;
            //                    foreach (ChartDAT chart in dat)
            //                    {
            //                        strBF1 += chart.WindowNumber + ",,";
            //                        for (int i = 0; i < chart.Trace.Length - 1; i++)
            //                        {
            //                            strBF1 += "," + ",";
            //                        }
            //                        foreach (TraceDAT trace in chart.Trace)
            //                        {
            //                            strBF2 += trace.ChannelNumber + ",,";
            //                            strBF3 += trace.AxisX + "," + trace.AxisY + ",";
            //                            if (cnt < trace.ValueX.Length) { cnt = trace.ValueX.Length; }
            //                            if (cnt < trace.ValueY.Length) { cnt = trace.ValueY.Length; }
            //                        }
            //                    }
            //                    sw.WriteLine(strBF1.Trim(','));
            //                    sw.WriteLine(strBF2.Trim(','));
            //                    sw.WriteLine(strBF3.Trim(','));

            //                    for (int i = 0; i < cnt; i++)
            //                    {
            //                        strBF1 = "";
            //                        foreach (ChartDAT chart in dat)
            //                        {
            //                            foreach (TraceDAT trace in chart.Trace)
            //                            {
            //                                if (trace.ValueX.Length > i) { strBF1 += trace.ValueX[i]; }
            //                                strBF1 += ",";
            //                                if (trace.ValueY.Length > i) { strBF1 += trace.ValueY[i]; }
            //                                strBF1 += ",";
            //                            }
            //                        }
            //                        sw.WriteLine(strBF1.Trim(','));
            //                    }
            //                    sw.Close();
            //                }
            //            }
            //        }
            //        //Trigger ReSET
            //        if (SING_CHECKBOX.IsChecked == true)
            //        {
            //            for (int i = 0; i < channels.Length; i++)
            //            {
            //                pna.setTriggerMode(channels[i], trigMODE[i]);
            //            }
            //        }
            //    }

            //    //Write Phase State0
            //    foreach (int p in ps)
            //    {
            //        //Write Phase State
            //        if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, "SetDPS " + p.ToString("0") + " 0", 1).Substring(0, 4) != "DONE")
            //        {
            //            MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //            return;
            //        }
            //    }
            //    if (_mod.PCAB_CMD(SERIAL_NUMBERS_COMBOBOX.Text, " WrtDPS", 1).Substring(0, 4) != "DONE")
            //    {
            //        MessageBox.Show("Write phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }

            //    MessageBox.Show("Data acquisition completed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            //catch (Exception err)
            //{
            //    MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }
    }
}
