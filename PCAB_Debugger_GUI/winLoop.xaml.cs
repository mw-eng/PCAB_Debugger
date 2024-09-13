using MWComLibCS.ExternalControl;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static PCAB_Debugger_GUI.agPNA835x;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// winLoop.xaml の相互作用ロジック
    /// </summary>
    public partial class winLoop : Window
    {
        winMain owner;
        bool runTASK = true;
        string sn;
        string dirPath;
        string fileHeader;
        bool singTRIG = false;
        bool saveSCR = false;
        bool saveTRA = false;
        List<int> dps = new List<int>();
        List<int> dsa = new List<int>();
        List<uint> channels = new List<uint>();
        List<uint> sheets = new List<uint>();
        List<loopCONF> loops = new List<loopCONF>();
        List<SweepMode> trig = new List<SweepMode>();
        PCAB _mod;
        agPNA835x instr;

        public winLoop(winMain WINowner, string DirPATH)
        {
            InitializeComponent();
            owner = WINowner;
            dirPath = DirPATH;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get Configuration
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            runTASK = false;
        }

        private void LOOP_Task(int waiteTime)
        {
            int loopCNT = 0;
            if (waiteTime < 0) { ExitErrTASK(); return; }
            try
            {
                OnUpdateDAT(loopCNT);
                if (runTASK)
                {
                    foreach (loopCONF cnf in loops)
                    {
                        string filePath = dirPath + "\\" + fileHeader;
                        if (!runTASK) { ExitCancelTASK(); return; }
                        if (cnf.dps >= 0)
                        {
                            foreach (int p in dps)
                            {
                                //Write Phase State
                                if (_mod.PCAB_CMD(sn, "SetDPS " + p.ToString("0") + " " + cnf.dps.ToString("0"), 1).Substring(0, 4) != "DONE") { ExitErrTASK(); return; }
                            }
                            if (_mod.PCAB_CMD(sn, "WrtDPS", 1).Substring(0, 4) != "DONE") { ExitErrTASK(); return; }
                            filePath += "_DPS" + cnf.dps.ToString("00");
                        }
                        if (!runTASK) { ExitCancelTASK(); return; }
                        if (cnf.dsa >= 0)
                        {
                            foreach (int a in dsa)
                            {
                                //Write ATT State
                                if (_mod.PCAB_CMD(sn, "SetDSA " + a.ToString("0") + " " + cnf.dsa.ToString("0"), 1).Substring(0, 4) != "DONE") { ExitErrTASK(); return; }
                            }
                            if (_mod.PCAB_CMD(sn, "WrtDSA", 1).Substring(0, 4) != "DONE") { ExitErrTASK(); return; }
                            filePath += "_DSA" + cnf.dsa.ToString("00");
                        }
                        if (!runTASK) { ExitCancelTASK(); return; }
                        Thread.Sleep(waiteTime);
                        if (!runTASK) { ExitCancelTASK(); return; }
                        //Single Trigger Set
                        if (singTRIG)
                        {
                            foreach (uint ch in channels)
                            {
                                instr.trigSingle(ch);
                            }
                        }
                        Thread.Sleep(100);
                        if (!runTASK)
                        {
                            //Trigger ReSET
                            if (singTRIG)
                            {
                                for (int i = 0; i < channels.Count; i++)
                                {
                                    instr.setTriggerMode(channels[i], trig[i]);
                                }
                            }
                            ExitCancelTASK();
                            return;
                        }

                        if(!string.IsNullOrWhiteSpace(dirPath)){
                            //Save Screen
                            if (saveSCR)
                            {
                                foreach (uint sh in sheets)
                                {
                                    instr.selectSheet(sh);
                                    if (!instr.GetScreen(filePath + "_Sheet" + sh.ToString() + ".png", out _))
                                    {
                                        if (singTRIG)
                                        {
                                            for (int i = 0; i < channels.Count; i++)
                                            {
                                                instr.setTriggerMode(channels[i], trig[i]);
                                            }
                                        }
                                        ExitErrTASK();
                                        return;
                                    }
                                }
                            }

                            if (!runTASK)
                            {
                                //Trigger ReSET
                                if (singTRIG)
                                {
                                    for (int i = 0; i < channels.Count; i++)
                                    {
                                        instr.setTriggerMode(channels[i], trig[i]);
                                    }
                                }
                                ExitCancelTASK();
                                return;
                            }

                            //Save Trace
                            if (saveTRA)
                            {
                                try
                                {
                                    foreach (uint sh in sheets)
                                    {
                                        //Select Sheet
                                        instr.selectSheet(sh);
                                        //Get Trace DAT
                                        List<ChartDAT> dat = new List<ChartDAT>();
                                        foreach (uint win in instr.getWindowCatalog(sh))
                                        {
                                            List<TraceDAT> trace = new List<TraceDAT>();
                                            foreach (uint tra in instr.getTraceCatalog(win))
                                            {
                                                instr.selectTrace(win, tra);
                                                uint ch = instr.getSelectChannel();
                                                uint num = instr.getSelectMeasurementNumber();
                                                //string x = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X:AXIS:UNIT?");
                                                string x = "X";
                                                string y = instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":PAR?");
                                                y += "_" + instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":FORM?");
                                                //y += "_" + pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X:AXIS:UNIT?");
                                                string mem = instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":MATH:FUNC?");
                                                if (mem.ToUpper() != "NORM")
                                                {
                                                    y += "@" + mem + "[MEM]";
                                                }
                                                string[] valx = instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X?").Trim().Split(',');
                                                //string[] valy = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":Y?").Trim().Split(',');
                                                string[] valy = instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":DATA:FDAT?").Trim().Split(',');
                                                trace.Add(new TraceDAT("CH" + ch.ToString(), x, y, valx, valy));
                                            }
                                            dat.Add(new ChartDAT("Win" + win.ToString(), trace.ToArray()));
                                        }
                                        //Write CSV Data
                                        using (StreamWriter sw = new StreamWriter(filePath + "_Sheet" + sh.ToString() + ".csv", false, Encoding.UTF8))
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
                                catch
                                {
                                    if (singTRIG)
                                    {
                                        for (int i = 0; i < channels.Count; i++)
                                        {
                                            instr.setTriggerMode(channels[i], trig[i]);
                                        }
                                    }
                                    ExitErrTASK();
                                    return;
                                }
                            }

                            if (!runTASK)
                            {
                                //Trigger ReSET
                                if (singTRIG)
                                {
                                    for (int i = 0; i < channels.Count; i++)
                                    {
                                        instr.setTriggerMode(channels[i], trig[i]);
                                    }
                                }
                                ExitCancelTASK();
                                return;
                            }
                        }

                        //Trigger ReSET
                        if (singTRIG)
                        {
                            for (int i = 0; i < channels.Count; i++)
                            {
                                instr.setTriggerMode(channels[i], trig[i]);
                            }
                        }
                        if (!runTASK) { ExitCancelTASK(); return; }
                        OnUpdateDAT(loopCNT);
                        loopCNT++;
                    }
                    ExitNormTASK();
                }
                else { ExitCancelTASK(); return; }
            }
            catch { ExitErrTASK(); }
        }

        private void OnUpdateDAT(int cnt)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (loops[cnt].dps < 0) { dps_state.Content = "LOCK"; }
                else { dps_state.Content = (5.625 * loops[cnt].dps).ToString() + "deg (" + loops[cnt].dps.ToString() + ")"; }
                if (loops[cnt].dsa < 0) { dsa_state.Content = "LOCK"; }
                else { dsa_state.Content = (0.25 * loops[cnt].dsa).ToString() + "dB (" + loops[cnt].dsa.ToString() + ")"; }
                Progress.Value = (int)((double)cnt / (loops.Count - 1) * 100.0);
            }));
        }

        private void ExitNormTASK()
        {
            Dispatcher.BeginInvoke(new Action(() => {
                MessageBox.Show("Loop function is done.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }));
        }

        private void ExitErrTASK()
        {
            Dispatcher.BeginInvoke(new Action(() => {
                MessageBox.Show("Loop function.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
                this.Close(); 
            }));
        }

        private void ExitCancelTASK()
        {
            Dispatcher.BeginInvoke(new Action(() => {
                MessageBox.Show("Loop function is cancel.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; 
                this.Close();
            }));
        }

        private void Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageLabel.Content = "Sweep... " + Progress.Value.ToString() + "%";
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

        private struct loopCONF
        {
            public int dps { get; set; }
            public int dsa { get; set; }
        }
        #endregion

    }
}
