using System;
using System.Collections.Generic;
using System.Windows;
using static PCAB_Debugger_GUI.PCAB_TASK;

namespace PCAB_Debugger_GUI
{
    public class clsSerialIO
    {
        private PCAB_TASK _task;
        public bool? isOpen { get { return _task?.isOpen; } }
        public List<cntBOARD> PCAB_Boards {  get; private set; } = new List<cntBOARD>();
        public List<cntMonitor> PCAB_Monitors { get; private set; } = new List<cntMonitor>();
        public event EventHandler<PCABEventArgs> OnError;
        public PCAB_TASK serial { get { return _task; } }
        public clsSerialIO(string SerialPortName)
        {
            _task = new PCAB_TASK(SerialPortName);
            _task.OnUpdateDAT += OnUpdateDAT;
            _task.OnTaskError += PCAB_TASK_OnError;
        }

        ~clsSerialIO() { }

        public void Open(string[] SerialNumber, uint MonitorIntervalTime)
        {
            if (_task.PCAB_AutoTaskStart(MonitorIntervalTime, SerialNumber))
            {
                foreach (PCAB_SerialInterface.PCAB_UnitInterface unit in _task.UNITs)
                {
                    PCAB_Boards.Add(new cntBOARD(unit.SerialNumberASCII));
                    PCAB_Monitors.Add(new cntMonitor(unit.SerialNumberASCII));
                }
                for (int cnt = 0; cnt < _task.UNITs.Count; cnt++)
                {
                    PCAB_Monitors[cnt].TITLE = "S/N, " + _task.UNITs[cnt].SerialNumberASCII;
                }
            }
        }

        public void Close()
        {
            _task?.PCAB_AutoTaskStop();
            PCAB_Boards.Clear(); PCAB_Monitors.Clear();
        }

        private void PCAB_TASK_OnError(object sender, PCABEventArgs e)
        {
            _task.PCAB_AutoTaskStop();
            _task = null;
            MessageBox.Show(e.Message.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            OnError?.Invoke(this, e);
        }

        private void OnUpdateDAT(object sender, PCABEventArgs e)
        {
            if (e.ReceiveDAT.Count == PCAB_Monitors.Count)
            {
                for (int cnt = 0; cnt < PCAB_Monitors.Count; cnt++)
                {
                    PCAB_Monitors[cnt].TITLE = "S/N, " + e.ReceiveDAT[cnt].SerialNumberASCII;
                    PCAB_Monitors[cnt].TEMPcpu = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00");
                    PCAB_Monitors[cnt].SNSvin = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.Vin.ToString("0.00");
                    PCAB_Monitors[cnt].SNSpin = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.Pin.ToString("0.00");
                    PCAB_Monitors[cnt].SNSvd = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.Vd.ToString("0.00");
                    PCAB_Monitors[cnt].SNSid = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.Id.ToString("0.00");
                    if (e.ReceiveDAT[cnt].SensorValuesNOW.Temprature.Values?.Length == 15)
                    {
                        for (uint i = 0; i < e.ReceiveDAT[cnt].SensorValuesNOW.Temprature.Values.Length; i++)
                        {
                            PCAB_Monitors[cnt].SetTempValue(i + 1, e.ReceiveDAT[cnt].SensorValuesNOW.Temprature.Values[i].ToString("0.00"));
                        }
                        foreach(float val in e.ReceiveDAT[cnt].SensorValuesNOW.Temprature.Values)
                        {
                        }
                    }
                }
            }
        }
    }
}
