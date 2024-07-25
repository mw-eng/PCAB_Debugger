using MWComLibCS.ExternalControl;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        List<int> dps = new List<int>();
        List<int> dsa = new List<int>();
        PCAB _mod;

        public winLoop(winMain WINowner)
        {
            InitializeComponent();
            owner = WINowner;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get Configuration
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
            if (waitTIME < 0) { return; }
            _mod = owner._mod;
            sn = owner.SERIAL_NUMBERS_COMBOBOX.Text;
            _mod.PCAB_CMD(sn, "CUI 0", 1);
            _mod.DiscardInBuffer();
            runTASK = true;
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
                        for (cntDPS = 0; cntDPS < 64; cntDPS+=(int)stepDPS)
                        {
                            foreach (int p in dps)
                            {
                                //Write Phase State
                                if (_mod.PCAB_CMD(sn, "SetDPS " + p.ToString("0") + " " + cntDPS.ToString("0"), 1).Substring(0, 4) != "DONE") { this.DialogResult = false; this.Close(); }
                            }
                            if (_mod.PCAB_CMD(sn, " WrtDPS", 1).Substring(0, 4) != "DONE"){ ExitTASK(); return; }
                            for (cntDSA = 0; cntDSA < 64; cntDSA+= (int)stepDSA)
                            {
                                if (!runTASK) { ExitTASK();return; }
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
    }
}
