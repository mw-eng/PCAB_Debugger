using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// winLoop.xaml の相互作用ロジック
    /// </summary>
    public partial class winLoop : Window
    {
        uint stepDPS;
        uint stepDSA;
        int waitTIME;
        List<int> dps;
        List<int> dsa;
        PCAB _mod;
        string sn;
        bool runTASK;
        int cntDPS = -1;
        int cntDSA = -1;

        public winLoop(PCAB MOD, string serialNumber, uint StepDPS, uint StepDSA, int WaitTime, List<int> DPS, List<int> DSA)
        {
            InitializeComponent();
            _mod = MOD; sn = serialNumber; stepDPS = StepDPS; stepDSA = StepDSA; waitTIME = WaitTime; dps = DPS; dsa = DSA;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            runTASK = true;
            Task tsk = Task.Factory.StartNew(() => { LOOP_Task((uint)waitTIME); });
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
                        for (cntDPS = 0; cntDPS < 64; cntDPS+=(int)stepDPS)
                        {
                            foreach (int p in dps)
                            {
                                //Write Phase State
                                if (_mod.PCAB_CMD(sn, "SetDPS " + p.ToString("0") + " " + cntDPS.ToString("0"), 1).Substring(0, 4) != "DONE") { this.DialogResult = false; this.Close(); }
                            }
                            if (_mod.PCAB_CMD(sn, " WrtDPS", 1).Substring(0, 4) != "DONE"){ ExitTASK(); return; }
                            OnUpdateDAT();
                            for (cntDSA = 0; cntDSA < 64; cntDSA+= (int)stepDSA)
                            {
                                if (!runTASK) { ExitTASK();return; }
                                foreach (int a in dsa)
                                {
                                    //Write Phase State
                                    if (_mod.PCAB_CMD(sn, "SetDSA " + a.ToString("0") + " " + cntDSA.ToString("0"), 1).Substring(0, 4) != "DONE") { this.DialogResult = false; this.Close(); }
                                }
                                if (_mod.PCAB_CMD(sn, " WrtDSA", 1).Substring(0, 4) != "DONE") { ExitTASK(); return; }
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
                            OnUpdateDAT();
                            Thread.Sleep(waitTIME);
                        }
                    }
                    Dispatcher.BeginInvoke(new Action(() => { this.DialogResult = true; this.Close(); })); ;
                }
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
            }));
        }

        private void ExitTASK()
        {
            Dispatcher.BeginInvoke(new Action(() => { this.DialogResult = true; this.Close(); }));
        }
    }
}
