﻿using MWComLibCS.CoordinateSystem;
using MWComLibCS;
using PCAB_Debugger_ComLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static PCAB_Debugger_ComLib.cntConfigPorts;
using static PCAB_Debugger_ComLib.PCAB_SerialInterface;

namespace PCAB_Debugger_ACS
{
    public class PANEL
    {
        public List<UNIT_IF> unitIFs { get; private set; }
        public event EventHandler<ErrorEventArgs> OnTaskError;


        public class UNIT_IF
        {
            public List<UNIT> UNITs { get; private set; }
            public PCAB_SerialInterface SerialInterface { get; private set; }
            public bool? isOpen { get { return SerialInterface?.isOpen; } }
            public event EventHandler<ErrorEventArgs> OnError;
            private bool? _task;    //true:run / false:stop / null:Interrupt
            private bool _state;
            private Task _loopTask;

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
                if (_task != false) { UNITs_SensorMonitor_TASK_Stop(); }
                try { SerialInterface?.Close(); } catch { }
                SerialInterface = null;
            }

            public void UNITs_SensorMonitor_TASK_Start(UInt32 MonitorIntervalTime)
            {
                _task = true;
                _state = false;
                _loopTask = Task.Factory.StartNew(() => { UNITs_SensorMonitor_TASK(MonitorIntervalTime); });
            }

            public void UNITs_SensorMonitor_TASK_Stop()
            {
                if (_state) { _state = false; _task = true; }
                _task = false; _loopTask?.ConfigureAwait(false);_loopTask?.Wait();
            }
            public void UNITs_SensorMonitor_TASK_Pause()
            {
                if (!_state)
                {
                    _task = null;
                    while (_state) { Thread.Sleep(5); }
                    _state = true;
                }
            }
            public void UNITs_SensorMonitor_TASK_Restart()
            {
                if (_state)
                {
                    _state = false;
                    _task = true;
                }
            }

            private void UNITs_SensorMonitor_TASK(UInt32 MonitorIntervalTime)
            {
                try
                {
                    if (_task == true)
                    {
                        while (_state) { Thread.Sleep(8); }
                        _state = true;
                        try
                        {
                            SerialInterface.DiscardInBuffer();
                            foreach(UNIT unit in UNITs)
                            {
                                TempratureID ids = SerialInterface.GetTempID(new PCAB_UnitInterface(unit.SerialNumber));
                                if (ids.IDs?.Length == 15)
                                {
                                    for (int j = 0; j < ids.IDs.Length; j++)
                                    {
                                        unit.SENS_MONITOR.SetTempID((uint)(j + 1), "0x" + ids.IDs[j].ToString("X16"));
                                    }
                                }
                            }
                            try { GetSensorValue(); }
                            catch(Exception e) { OnError?.Invoke(this, new ErrorEventArgs(e)); }
                        }
                        catch { }
                        _state = false;
                    }
                    do
                    {
                        Thread.Sleep((int)MonitorIntervalTime);
                        if (_task == true)
                        {
                            while (_state) { Thread.Sleep(2); }
                            _state = true;
                            try { GetSensorValue(); }
                            catch (Exception e) { OnError?.Invoke(this, new ErrorEventArgs(e)); }
                            _state = false;
                        }
                    } while (_task != false);
                }
                catch (Exception e)
                {
                    OnError?.Invoke(this, new ErrorEventArgs(e));
                }
            }

            private void GetSensorValue(uint unitNum)
            {
                SensorValues values = SerialInterface.GetSensorValue(unitNum);
                if (UNITs[(int)unitNum].SensorValuesNOW.Analog.Vd != values.Analog.Vd ||
                    UNITs[(int)unitNum].SensorValuesNOW.Analog.Id != values.Analog.Id ||
                    UNITs[(int)unitNum].SensorValuesNOW.Analog.Vin != values.Analog.Vin ||
                    UNITs[(int)unitNum].SensorValuesNOW.Analog.Pin != values.Analog.Pin ||
                    UNITs[(int)unitNum].SensorValuesNOW.Analog.CPU_Temprature != values.Analog.CPU_Temprature ||
                    UNITs[(int)unitNum].SensorValuesNOW.Temprature.Values != values.Temprature.Values
                    )
                {
                    UNITs[(int)unitNum].SensorValuesNOW = values;
                    UNITs[(int)unitNum].SENS_MONITOR.TEMPcpu = values.Analog.CPU_Temprature.ToString("0.00");
                    UNITs[(int)unitNum].SENS_MONITOR.SNSvin = values.Analog.Vin.ToString("0.00");
                    UNITs[(int)unitNum].SENS_MONITOR.SNSpin = values.Analog.Pin.ToString("0.00");
                    UNITs[(int)unitNum].SENS_MONITOR.SNSvd = values.Analog.Vd.ToString("0.00");
                    UNITs[(int)unitNum].SENS_MONITOR.SNSid = values.Analog.Id.ToString("0.00");
                    float avg = 0;
                    if (values.Temprature.Values?.Length == 15)
                    {
                        for (int j = 0; j < values.Temprature.Values.Length; j++)
                        {
                            avg += values.Temprature.Values[j];
                            UNITs[(int)unitNum].SENS_MONITOR.SetTempValue((uint)(j + 1), values.Temprature.Values[j].ToString("0.00"));
                        }
                        UNITs[(int)unitNum].SENS_MONITOR.TEMPavg = (avg / 15.0f).ToString("0.00");
                    }
                }
            }
            private void GetSensorValue()
            {
                if (UNITs.Count != 0)
                {
                    for (int i = 0; i < UNITs.Count; i++)
                    {
                        GetSensorValue((uint)i);
                    }
                }
            }

        }

        public class UNIT
        {
            public string SerialNumber { get; private set; }
            public string Name { get; private set; }
            public cntConfigPorts CONFIG { get; private set; }
            public cntMonitor SENS_MONITOR { get; private set; }
            public cntMonitorMagPhase PHASE_MONITOR { get; private set; }
            public SensorValues SensorValuesNOW { get; set; }
            private List<PORT> _ports;
            public List<PORT> Ports
            {
                get { return _ports; }
                set
                {
                    if (value.Count == 15)
                    {
                        _ports = value;
                        List<float> values = new List<float>();
                        foreach(PORT port in value) { values.Add((float)(port.Offset.Phase.Degree)); }
                        PHASE_MONITOR.OFFSETs = values;
                    }
                }
            }
            public void ReloadPorts()
            {
                if (_ports.Count == 15)
                {
                    List<float> values = new List<float>();
                    foreach (PORT port in _ports) { values.Add((float)(port.Offset.Phase.Degree)); }
                    PHASE_MONITOR.OFFSETs = values;
                }
            }

            public UNIT(string _serialNumber, string _name, ROTATE angle)
            {
                SerialNumber = _serialNumber;  Name = _name;
                CONFIG = new cntConfigPorts(SerialNumber, angle);
                SENS_MONITOR = new cntMonitor(SerialNumber);
                SENS_MONITOR.TITLE = _name;
                SENS_MONITOR.TEMPviewIDs = true;
                SENS_MONITOR.TEMPviewIDratio = 2;
                PHASE_MONITOR = new cntMonitorMagPhase(angle, new NormalizedColorChart(-180, 180));
            }

            public List<uint> GetPhaseDelay(double Frequency, AntennaCS BeamDirection)
            {
                List<uint> resutlt = new List<uint>();
                foreach (PORT port in _ports) { resutlt.Add(port.PhaseDelayConfig(Frequency, BeamDirection)); }
                return resutlt;
            }
        }

        public class PORT
        {
            public uint PortNumber { get; set; }
            public MWComLibCS.CoordinateSystem.CoordinateSystem3D Position { get; set; }
            public MWComLibCS.ComplexAngle Offset { get; set; }

            public PORT(uint portNumber, MWComLibCS.CoordinateSystem.CoordinateSystem3D _position, MWComLibCS.ComplexAngle _offset)
            {
                PortNumber = portNumber;
                Position = _position;
                Offset = _offset;
            }

            public Angle PhaseDelay(double Frequency, AntennaCS BeamDirection)
            {
                CoordinateSystem3D csBD = new CoordinateSystem3D(BeamDirection.OrthogonalCoordinate);
                if ((float)Position.Abs == 0.000000f) { return new Angle(0); }
                double phase = CoordinateSystem3D.Dot(csBD, Position);
                phase /= (csBD.Abs * Position.Abs);
                phase = Math.Acos(phase);
                phase = Position.Abs * Math.Cos(phase);
                phase *= 2.0 * Math.PI * Frequency / PhysicalConstant.c0;
                return new Angle(-phase);
            }

            public Angle OffsetPhaseDelay(double Frequency, AntennaCS BeamDirection)
            {
                return PhaseDelay(Frequency, BeamDirection) - Offset.Phase;
            }

            public byte PhaseDelayConfig(double Frequency, AntennaCS BeamDirection)
            {
                return (byte)Math.Round(Angle.Normalize360(OffsetPhaseDelay(Frequency, BeamDirection)).Degree / 5.625, MidpointRounding.AwayFromZero);
            }
        }

        public PANEL(List<SerialInterface> serialInterfaces)
        {
            unitIFs = new List<UNIT_IF>();
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

        public void Close()
        {
            foreach (UNIT_IF unitIF in unitIFs)
            {
                unitIF.Close();
            }
        }

        public void PANEL_SensorMonitor_TASK_Start(UInt32 MonitorIntervalTime)
        {
            foreach (UNIT_IF unitIF in unitIFs)
            {
                unitIF.OnError += OnError_UnitIF_TASK;
                unitIF.UNITs_SensorMonitor_TASK_Start(MonitorIntervalTime);
            }
        }
        public void PANEL_SensorMonitor_TASK_Stop()
        {
            foreach (UNIT_IF unitIF in unitIFs)
            {
                unitIF.UNITs_SensorMonitor_TASK_Stop();
            }
        }
        public void PANEL_SensorMonitor_TASK_Pause()
        {
            foreach (UNIT_IF unitIF in unitIFs)
            {
                unitIF.UNITs_SensorMonitor_TASK_Pause();
            }
        }
        public void PANEL_SensorMonitor_TASK_Restart()
        {
            foreach (UNIT_IF unitIF in unitIFs)
            {
                unitIF.UNITs_SensorMonitor_TASK_Restart();
            }
        }

        private void OnError_UnitIF_TASK(object sender, ErrorEventArgs e)
        {
            OnTaskError?.Invoke(this, e);
        }

        public struct SerialInterface
        {
            public string SerialPortName { get; set; }
            public uint BaudRate { get; set; }
            public List<UNIT> UNITs { get; set; }
            public SerialInterface(string SerialPortName, uint BaudRate,List<UNIT> units) { this.SerialPortName = SerialPortName; this.BaudRate = BaudRate; UNITs = units; }
        }
    }
}