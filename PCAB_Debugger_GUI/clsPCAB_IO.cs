using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static PCAB_Debugger_GUI.PCAB;

namespace PCAB_Debugger_GUI
{
    public class PCAB_TASK
    {
        private PCAB_SerialInterface serialInterface;
        private bool? _task;    //true:run / false:stop / null:Interrupt
        private bool _state;
        public event EventHandler<PCABEventArgs> OnUpdateDAT;
        public event EventHandler<PCABEventArgs> OnError;
        public List<PCAB_SerialInterface.PCAB_UnitInterface> UNITs { get { return serialInterface?.pcabUNITs; } }

        public PCAB_TASK(string PortName) { serialInterface = new PCAB_SerialInterface(PortName); }
        public PCAB_TASK(SerialPort serialPort) { serialInterface = new PCAB_SerialInterface(serialPort); }

        public void Close()
        {
            try { serialInterface?.Close(); } catch { }
            serialInterface = null;
        }

        public bool PCAB_AutoTaskStart(UInt32 waiteTime, string[] serialNum)
        {
            if (serialInterface.SerialOpen) { serialInterface.Close(); }
            return serialInterface.Open(serialNum);
            Task.Factory.StartNew(() => { PCAB_Task(waiteTime); });
            return true;
        }

        private void PCAB_Task(UInt32 waiteTime)
        {
            try
            {
                do
                {
                    Thread.Sleep((int)waiteTime);
                    if (_task == true)
                    {
                        while (_state) { Thread.Sleep(53); }
                        _state = true;
                        bool updateFLG = false;
                        foreach (PCAB_SerialInterface.PCAB_UnitInterface unit in serialInterface.pcabUNITs)
                        {
                            serialInterface.DiscardInBuffer();
                            PCAB_SerialInterface.SensorValues values = serialInterface.GetSensorValue(unit);
                            if (
                                values.Analog.Vd != unit.SensorValuesNOW.Analog.Vd ||
                                values.Analog.Id != unit.SensorValuesNOW.Analog.Id ||
                                values.Analog.Vin != unit.SensorValuesNOW.Analog.Vin ||
                                values.Analog.Pin != unit.SensorValuesNOW.Analog.Pin ||
                                values.Analog.CPU_Temprature != unit.SensorValuesNOW.Analog.CPU_Temprature ||
                                values.Temprature.Values != unit.SensorValuesNOW.Temprature.Values)
                            { updateFLG = true; unit.SensorValuesNOW = values; }
                        }
                        if (updateFLG)
                        {
                            OnUpdateDAT?.Invoke(null, null);
                        }
                        _state = false;
                    }
                } while (_task != false);
                serialInterface?.Close();
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, new PCABEventArgs(null, e.Message));
            }
        }

        public void PCAB_AutoTaskStop() { _task = false; }

        public bool PCAB_PRESET(PCAB_SerialInterface.PCAB_UnitInterface unit)
        {
            _task = null;
            while (_state) { Thread.Sleep(59); }
            _state = true;

        }

        public class PCABEventArgs : EventArgs
        {
            private string msg;
            private List<PCAB_SerialInterface.PCAB_UnitInterface> dat;

            public PCABEventArgs(List<PCAB_SerialInterface.PCAB_UnitInterface> ReceiveDAT, string Message) { dat = ReceiveDAT; msg = Message; }

            public string Message { get { return msg; } }

            public List<PCAB_SerialInterface.PCAB_UnitInterface> ReceiveDAT { get { return dat; } }

        }
    }

    public class PCAB_SerialInterface
    {
        private SerialPort _serialPort;
        public bool SerialOpen { get; private set; } = false;
        public List<PCAB_UnitInterface> pcabUNITs { get; private set; }
        private List<byte> serialBF = new List<byte>();

        /// <summary>Constructor</summary>
        /// <param name="PortName">Serial Port Name</param>
        public PCAB_SerialInterface(string PortName)
            : this(PortName, 3686400, 8, Parity.Even, StopBits.One, 4096, 5000, 5000) { }

        public PCAB_SerialInterface(string PortName,
            int baudRate, int dataBits, Parity parity, StopBits stopbit, int readBufferSize, int writeTimeOut, int readTimeOut)
        {
            _serialPort = new SerialPort(PortName);
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBits;
            _serialPort.Parity = parity;
            _serialPort.StopBits = stopbit;
            _serialPort.Handshake = Handshake.None;
            _serialPort.DtrEnable = true;
            _serialPort.Encoding = Encoding.ASCII;
            _serialPort.NewLine = "\r\n";
            _serialPort.ReadBufferSize = readBufferSize;
            _serialPort.WriteTimeout = writeTimeOut;
            _serialPort.ReadTimeout = readTimeOut;
        }

        public PCAB_SerialInterface(SerialPort serialPort) {  _serialPort = serialPort; }

        ~PCAB_SerialInterface() { this.Close(); _serialPort = null; }

        public void Close()
        {
            if (_serialPort?.IsOpen == true)
            {
                try
                {
                    foreach (PCAB_UnitInterface unit in pcabUNITs)
                    {
                        try
                        {
                            WriteSLIP(unit.GetCommandCode(new List<byte>(0xFE)));
                        }
                        catch { }
                        Thread.Sleep(500);
                        DiscardInBuffer();
                    }
                    _serialPort.Close();
                }
                catch { }
            }
            pcabUNITs.Clear();
        }

        /// <summary>Open Serial Port</summary>
        /// <param name="SerialNumbers">Serial Number List</param>
        /// <returns></returns>
        public bool Open(string[] SerialNumbers)
        {
            if (SerialNumbers.Length <= 0)
            {
                return false;
            }
            if (_serialPort?.IsOpen == true) { return false; }
            try { _serialPort.Open(); }
            catch (UnauthorizedAccessException) { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
            catch (Exception) { MessageBox.Show("Serial port open Error.\n{e.ToString()}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
            try
            {
                DiscardInBuffer();
                _serialPort.WriteLine("");
                foreach (string s in SerialNumbers)
                {
                    _serialPort.WriteLine("#" + s + " CUI 0");
                }
                Thread.Sleep(500);
                DiscardInBuffer();
                int rTObf = _serialPort.ReadTimeout;
                _serialPort.ReadTimeout = 100;
                foreach (string s in SerialNumbers)
                {
                    try
                    {
                        _serialPort.WriteLine("#" + s + " GetIDN");
                        string[] arrBf = _serialPort.ReadLine().Split(',');
                        if (arrBf.Length != 4) { _serialPort.Close(); return false; }
                        if (arrBf.Length == 4)
                        {
                            if (arrBf[0] == "Orient Microwave Corp." && arrBf[1] == "LX00-0004-00" && arrBf[3].Substring(0, 5) == "1.3.3" && (arrBf[2] == s || "*" == s))
                            {
                                pcabUNITs.Add(new PCAB_UnitInterface(s));
                            }
                        }
                    }
                    catch
                    {
                        _serialPort.WriteLine("#" + s + " CUI 1");
                        Thread.Sleep(100);
                        DiscardInBuffer();
                    }
                }
                _serialPort.ReadTimeout = rTObf;
                if (pcabUNITs.Count <= 0)
                {
                    _serialPort.Close();
                    return false;
                }
                foreach (PCAB_UnitInterface unit in pcabUNITs)
                {
                    _serialPort.WriteLine("#" + unit.SerialNumberASCII + " BCM");
                }
                _serialPort.Write(new byte[] { 0xC0 }, 0, 1);
                Thread.Sleep(500);
                DiscardInBuffer();
            }
            catch (Exception)
            {
                return false;
            }
            SerialOpen = true;
            return true;
        }

        public void DiscardInBuffer()
        {
            _serialPort.DiscardInBuffer();
            serialBF.Clear();
        }
        public void WriteSLIP(List<byte> dat)
        {
            _serialPort.Write(clsSLIP.EncodeSLIP(dat).ToArray(), 0, dat.Count);
        }
        public List<byte> ReadSLIP() { return ReadSLIP(_serialPort.ReadBufferSize); }
        public List<byte> ReadSLIP(int bfLEN)
        {
            List<byte> ret = new List<byte>();
            if (serialBF.Count != 0)
            {
                int cnt = serialBF.IndexOf(0xC0);
                if (cnt != -1)
                {
                    ret = new List<byte>(serialBF.Skip(0).Take(cnt + 1).ToArray());
                    serialBF = new List<byte>(serialBF.Skip(cnt + 1).Take(serialBF.Count - cnt - 1).ToArray());
                }
                else
                {
                    ret = new List<byte>(serialBF.Skip(0).Take(serialBF.Count).ToArray());
                    serialBF.Clear();
                }
            }

            byte[] dat = new byte[bfLEN];
            DateTime startTime = DateTime.Now;
            int num;
            try
            {
                do
                {
                    TimeSpan span = DateTime.Now - startTime;
                    if (span.TotalMilliseconds > _serialPort.ReadTimeout) { throw new Exception("ReadSLPI TimeOut"); }
                    int count = _serialPort.Read(dat, 0, bfLEN);
                    ret.AddRange(new List<byte>(dat.Skip(0).Take(count).ToArray()));
                    num = ret.IndexOf(0xC0);
                    if (num != -1)
                    {
                        if (ret.Count != num + 1)
                        {
                            serialBF = new List<byte>(ret.Skip(num).Take(ret.Count - num - 1).ToArray());
                            ret = new List<byte>(ret.Skip(0).Take(num + 1).ToArray());
                        }
                    }
                } while (num == -1);
                return clsSLIP.DecodeSLIP(ret);
            }
            catch (Exception ex) { throw; }
        }
        public List<byte> WriteReadSLIP(List<byte> dat)
        {
            WriteSLIP(dat);
            return ReadSLIP();
        }

        public bool WriteDSAin(PCAB_UnitInterface unit, uint config)
        { return WriteDSAin(unit, (byte)config); }
        public bool WriteDSAin(PCAB_UnitInterface unit, byte config)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xB0, config }));
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw; }
        }
        public bool WriteDSA(PCAB_UnitInterface unit, List<uint> configs)
        { return WriteDSA(unit, configs.ConvertAll(x => (byte)x)); }
        public bool WriteDSA(PCAB_UnitInterface unit, List<byte> configs)
        {
            if (configs.Count != 15) { return false; }
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC1 }.Concat(configs).ToList()));
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw; }
        }
        public bool WriteDPS(PCAB_UnitInterface unit, List<uint> configs)
        { return WriteDPS(unit, configs.ConvertAll(x => (byte)x)); }
        public bool WriteDPS(PCAB_UnitInterface unit, List<byte> configs)
        {
            if (configs.Count != 15) { return false; }
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC2 }.Concat(configs).ToList()));
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw; }
        }

        public bool SetSTB_AMP(PCAB_UnitInterface unit, bool mode)
        {
            try
            {
                List<byte> ret;
                if (mode) { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC3, 0x01 })); }
                else { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC3, 0x00 })); }
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw; }
        }
        public bool SetSTB_DRA(PCAB_UnitInterface unit, bool mode)
        {
            try
            {
                List<byte> ret;
                if (mode) { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC4, 0x01 })); }
                else { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC4, 0x00 })); }
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw; }
        }
        public bool SetSTB_LNA(PCAB_UnitInterface unit, bool mode)
        {
            try
            {
                List<byte> ret;
                if (mode) { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC5, 0x01 })); }
                else { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC5, 0x00 })); }
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw; }
        }
        public bool SetLowPowerMode(PCAB_UnitInterface unit, bool mode)
        {
            try
            {
                List<byte> ret;
                if (mode) { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC6, 0x01 })); }
                else { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC6, 0x00 })); }
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw; }
        }

        public int GetDSAin(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD0 }));
                if (ret[0] > 0xF0) { return -1; }
                return (int)ret[0];
            }
            catch (Exception ex) { throw; }
        }
        public List<int> GetDSA(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD1 }));
                if (ret.Count == 15) { return ret.ConvertAll(x => (int)x); }
                else { throw new Exception("GetDSA Error"); }
            }
            catch (Exception ex) { throw; }
        }
        public List<int> GetDPS(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD2 }));
                if (ret.Count == 15) { return ret.ConvertAll(x => (int)x); }
                else { throw new Exception("GetDSA Error"); }
            }
            catch (Exception ex) { throw; }
        }
        public bool GetSTB_AMP(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD3 }));
                if (ret[0] == 0x00) { return false; }
                else if (ret[0] == 0x01) { return true; }
                else { throw new Exception("GetSTB_AMP Error"); }
            }
            catch (Exception ex) { throw; }
        }
        public bool GetSTB_DRA(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD4 }));
                if (ret[0] == 0x00) { return false; }
                else if (ret[0] == 0x01) { return true; }
                else { throw new Exception("GetSTB_DRA Error"); }
            }
            catch (Exception ex) { throw; }
        }
        public bool GetSTB_LNA(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD5 }));
                if (ret[0] == 0x00) { return false; }
                else if (ret[0] == 0x01) { return true; }
                else { throw new Exception("GetSTB_LNA Error"); }
            }
            catch (Exception ex) { throw; }
        }
        public bool GetLowPowerMode(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD6 }));
                if (ret[0] == 0x00) { return false; }
                else if (ret[0] == 0x01) { return true; }
                else { throw new Exception("GetLowPowerMode Error"); }
            }
            catch (Exception ex) { throw; }
        }
        public List<UInt64> GetTempID(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xE1 }));
                if (ret.Count % 8 == 0)
                {
                    List<UInt64> result = new List<UInt64>();
                    for (int i = 0; i < ret.Count; i += 8)
                    {
                        result.Add(
                            (1u << 56) * (UInt64)ret[i] +
                            (1u << 48) * (UInt64)ret[i + 1] +
                            (1u << 40) * (UInt64)ret[i + 2] +
                            (1u << 32) * (UInt64)ret[i + 3] +
                            (1u << 24) * (UInt64)ret[i + 4] +
                            (1u << 16) * (UInt64)ret[i + 5] +
                            (1u << 8) * (UInt64)ret[i + 6] +
                            (UInt64)ret[i + 7]);
                    }
                    return result;
                }
                else { throw new Exception("GetLowPowerMode Error"); }
            }
            catch (Exception ex) { throw; }
        }
        public TempratureValue GetTempValue(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xE1 }));
                return new TempratureValue(ret);
            }
            catch (Exception ex) { throw; }
        }
        public AnalogValues GetAnalogValue(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xEE }));
                return new AnalogValues(ret);
            }
            catch (Exception ex) { throw; }
        }
        public SensorValues GetSensorValue(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xEE }));
                if (ret.Count == 10 + 2 * 15) { return new SensorValues(ret); }
                else { throw new Exception("GetLowPowerMode Error"); }
            }
            catch (Exception ex) { throw; }
        }

        public byte GetMode(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xEE }));
                if (ret[0] > 0x0F) { throw new Exception("GetMode Error"); }
                else { return ret[0]; }
            }
            catch (Exception ex) { throw; }
        }

        public class PCAB_UnitInterface
        {
            private string _sn;
            public string SerialNumberASCII { get { return _sn; } }
            public byte[] SerialNumberBinary { get { return Encoding.ASCII.GetBytes(_sn); } }
            public SensorValues SensorValuesNOW { get; set; }

            public PCAB_UnitInterface(string SerialNumber)
            {
                _sn = SerialNumber;
                SensorValuesNOW = new SensorValues();
            }

            public List<byte> GetCommandCode(List<byte> cmd)
            {
                List<byte> ret = new List<byte>();
                ret.AddRange(Encoding.ASCII.GetBytes("#"));
                ret.AddRange(Encoding.ASCII.GetBytes(_sn));
                ret.Add(0xFF);
                ret.AddRange(cmd);
                return ret;
            }

        }

        public struct SensorValues
        {
            public AnalogValues Analog { get; private set; }
            public TempratureValue Temprature { get; private set; }
            public SensorValues(List<byte> dat)
            {
                if (dat.Count == 10 + 2 * 15)
                {
                    Analog = new AnalogValues(dat.Skip(0).Take(10).ToList());
                    Temprature = new TempratureValue(dat.Skip(10).Take(2 * 15).ToList());
                }
                else if (dat.Count == 10)
                {
                    Analog = new AnalogValues(dat);
                    Temprature = new TempratureValue();
                }
                else if (dat.Count == 2 * 15)
                {
                    Analog = new AnalogValues();
                    Temprature = new TempratureValue(dat);
                }
                else
                {
                    Analog = new AnalogValues();
                    Temprature = new TempratureValue();
                }
            }
            public void Clear() { Analog.Clear(); Temprature.Clear(); }
        }

        public struct AnalogValues
        {
            public float CPU_Temprature { get; private set; }
            public float Vd { get; private set; }
            public float Id { get; private set; }
            public float Vin { get; private set; }
            public float Pin { get; private set; }
            public AnalogValues(List<byte> dat)
            {
                if (dat.Count == 10)
                {
                    Vd = ((1u << 8) * (UInt16)dat[0] + (UInt16)dat[1]) * 3.3f / (1 << 12);
                    Id = ((1u << 8) * (UInt16)dat[2] + (UInt16)dat[3]) * 3.3f / (1 << 12);
                    Vin = ((1u << 8) * (UInt16)dat[4] + (UInt16)dat[5]) * 3.3f / (1 << 12);
                    Pin = ((1u << 8) * (UInt16)dat[6] + (UInt16)dat[7]) * 3.3f / (1 << 12);
                    CPU_Temprature = ((1u << 8) * (UInt16)dat[8] + (UInt16)dat[9]) * 3.3f / (1 << 12);
                }
                else { CPU_Temprature = float.NaN; Vd = float.NaN; Id = float.NaN; Vin = float.NaN; Pin = float.NaN; }
            }
            public void Clear() { CPU_Temprature = float.NaN; Vd = float.NaN; Id = float.NaN; Vin = float.NaN; Pin = float.NaN; }
        }

        public struct TempratureValue
        {
            public float[] Values { get; private set; }
            public TempratureValue(List<byte> dat)
            {
                if (dat.Count == 2 * 15)
                {
                    Values = new float[15];
                    for(int i = 0; i < 16; i++)
                    {
                        Values[i] = ((1u << 8) * (UInt16)dat[2 * i] + (UInt16)dat[2 * i + 1]) / 16.0f;
                    }
                }
                else { Values = null; }
            }
            public void Clear() { Values = null; }
        }
    }


}
