using System;
using System.Collections.Generic;
using System.Windows;
using static PCAB_Debugger_ComLib.PCAB_TASK;
using static PCAB_Debugger_ComLib.cntConfigPorts;
using PCAB_Debugger_ComLib;
using static PCAB_Debugger_ComLib.PCAB_SerialInterface;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace PCAB_Debugger_ACS
{
    public class PANEL
    {
        private List<UNIT_IF> unitIFs = new List<UNIT_IF>();
        public event EventHandler<PanelEventArgs> OnUpdateDAT;
        public event EventHandler<PanelEventArgs> OnTaskError;
        public event EventHandler<ErrorEventArgs> OnError;
        private bool? _task;    //true:run / false:stop / null:Interrupt
        private bool _state;
        private Task _loopTask;

        public class UNIT_IF
        {
            public List<UNIT> UNITs { get; private set; }
            public PCAB_SerialInterface SerialInterface { get; private set; }
            public bool? isOpen { get { return SerialInterface?.isOpen; } }

            public UNIT_IF(string SerialPortName, uint BaudRate, List<UNIT> units)
            {
                SerialInterface = new PCAB_SerialInterface(SerialPortName, BaudRate);
                UNITs=units;
            }

            public bool Open()
            {
                List<string> serialNum = new List<string>();
                foreach(UNIT unit in UNITs) { serialNum.Add(unit.SerialNumber); }
                List<bool> results = SerialInterface.Open(serialNum);
                if (results?.Count == serialNum.Count)
                {
                    foreach (bool ret in results)
                    {
                        if (!ret)
                        {
                            if (results == null) { SerialInterface?.Close(); return false; }
                            return false;
                        }
                    }
                    return true;
                }
                else { SerialInterface?.Close(); return false; }
            }

            public void Close()
            {
                try { SerialInterface?.Close(); } catch { }
                SerialInterface = null;
            }
        }

        public class UNIT
        {
            public string SerialNumber { get; private set; }
            public string Name { get; private set; }
            public List<PORT> Ports { get; private set; }
            public cntConfigPorts CONFIG { get; private set; }
            public cntMonitor SENS_MONITOR { get; private set; }
            public cntMonitorPHASE PHASE_MONITOR { get; private set; }
            public UNIT(string _serialNumber, string _name, List<PORT> _ports, ROTATE angle)
            {
                SerialNumber = _serialNumber;  Name = _name;
                CONFIG = new cntConfigPorts(SerialNumber, angle);
                SENS_MONITOR = new cntMonitor(SerialNumber);
                PHASE_MONITOR = new cntMonitorPHASE(angle, new ColorChart(-180, 180));
            }
        }

        public class PORT
        {
            public uint PortNumber { get; set; }
            public double Xposi { get; set; }
            public double Yposi { get; set; }
            public double Zposi { get; set; }
            public uint OffsetATT { get; set; }
            public uint OffsetPHA { get; set; }

            public PORT(uint portNumber, double X, double Y, double Z, uint ATT_VALUE, uint Phase_VALUE)
            {
                PortNumber = portNumber;
                Xposi = X;
                Yposi = Y;
                Zposi = Z;
                OffsetATT = 0;
                OffsetPHA = 0;
            }
        }

        public PANEL(List<SerialInterface> serialInterfaces)
        {
            unitIFs.Clear();
            foreach (SerialInterface serialInterface in serialInterfaces)
            {
                unitIFs.Add(new UNIT_IF(serialInterface.SerialPortName, serialInterface.BaudRate, serialInterface.UNITs));
            }
        }

        public bool Open()
        {
            bool result = true;
            foreach(UNIT_IF unitIF in unitIFs)
            {
                if (!unitIF.Open()) { result =  false; break; }
            }
            if (!result)
            {
                foreach (UNIT_IF unitIF in unitIFs)
                {
                    unitIF?.Close();
                }
                return false;
            }
            return true;
        }

        public void PANEL_SensorMonitor_TASK_Start(UInt32 MonitorIntervalTime)
        {

        }
        public void PANEL_SensorMonitor_TASK_Stpp()
        {
            if (_state) { _state = false; _task = true; }
            _task = false; _loopTask?.ConfigureAwait(false);
        }
        public void PANEL_SensorMonitor_TASK_Pause()
        {
            if (!_state)
            {
                _task = null;
                while (_state) { Thread.Sleep(5); }
                _state = true;
            }
        }
        public void PANEL_SensorMonitor_TASK_Restart()
        {
            if (_state)
            {
                _state = false;
                _task = true;
            }
        }

        public void PANEL_SensorMonitor_TASK(uint interfaceNum, UInt32 MonitorIntervalTime) { PANEL_SensorMonitor_TASK(unitIFs[(int)interfaceNum], MonitorIntervalTime); }
        public void PANEL_SensorMonitor_TASK(UNIT_IF unitIF, UInt32 MonitorIntervalTime)
        {

        }

        public class PanelEventArgs : EventArgs
        {
            private string msg;
            private List<PCAB_UnitInterface> dat;

            public PanelEventArgs(List<PCAB_UnitInterface> ReceiveDAT, string Message) { dat = ReceiveDAT; msg = Message; }

            public string Message { get { return msg; } }

            public List<PCAB_UnitInterface> ReceiveDAT { get { return dat; } }

        }

        public struct SerialInterface
        {
            public string SerialPortName;
            public uint BaudRate;
            public List<UNIT> UNITs;
            public SerialInterface(string SerialPortName, uint BaudRate,List<UNIT> units) { this.SerialPortName = SerialPortName; this.BaudRate = BaudRate; UNITs = units; }
        }
    }
}
