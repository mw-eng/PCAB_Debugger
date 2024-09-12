using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;

namespace PCAB_Debugger_GUI
{
    public class PCAB_SerialInterface
    {
        private SerialPort Serial;
        private bool? _task;    //true:run / false:stop / null:Interrupt
        private SerialPort _mod;
        private bool _state;
        public List<PCAB_UnitInterface> pcabUNITs { get; private set; }

        public PCAB_SerialInterface(string PortName, List<string> SNs)
            : this(SNs, PortName, 3686400, 8, Parity.Even, StopBits.One, 4096, 5000, 5000) { }

        public PCAB_SerialInterface(List<string> SNs, string PortName, 
            int baudRate, int dataBits, Parity parity, StopBits stopbit, int readBufferSize, int writeTimeOut, int readTimeOut)
        {
            Serial = new SerialPort(PortName);
            Serial.BaudRate = baudRate;
            Serial.DataBits = dataBits;
            Serial.Parity = parity;
            Serial.StopBits = stopbit;
            Serial.Handshake = Handshake.None;
            Serial.DtrEnable = true;
            Serial.Encoding = Encoding.ASCII;
            Serial.NewLine = "\r\n";
            Serial.ReadBufferSize = readBufferSize;
            Serial.WriteTimeout = writeTimeOut;
            Serial.ReadTimeout = readTimeOut;
        }

        ~PCAB_SerialInterface() { this.Close(); }

        public void Close() { if (Serial?.IsOpen == true) { try { Serial.Close(); } catch { } } Serial = null; pcabUNITs.Clear(); }



    }

    public class PCAB_UnitInterface
    {
        private string _name;
        private cntConfig _config;
        private cntMonitor _monitor;

        public PCAB_UnitInterface(string name, cntConfig config, cntMonitor monitor)
        {
            _name = name;
            _config = config;
            _monitor = monitor;
        }
    }

    public class PCAB_CommandInterface
    {
    }

    public class PCAB
    {
        private bool? _task;    //true:run / false:stop / null:Interrupt
        private SerialPort _mod;
        private bool _state;
        public event EventHandler<PCABEventArgs> OnUpdateDAT;
        public event EventHandler<PCABEventArgs> OnError;
        public struct condDAT
        {
            public string SN { get; set; }
            public string Vin { get; set; }
            public string Vd { get; set; }
            public string Id { get; set; }
            public string Pin { get; set; }
            public string TEMPs { get; set; }
            public string CPU_TEMP { get; set; }

            public condDAT(string sn, string vin, string vd, string id, string pin, string tmp, string cpu)
            { SN = sn; Vin = vin; Vd = vd; Id = id; Pin = pin; TEMPs = tmp; CPU_TEMP = cpu; }

        }
        public condDAT CondNOW;
        public List<condDAT> DAT;

        public PCAB(string PortName)
        {
            _mod = new SerialPort(PortName);
            CondNOW = new condDAT();
            DAT = new List<condDAT>();
            //_mod.BaudRate = 9600;
            _mod.BaudRate = 115200;
            _mod.DataBits = 8;
            _mod.Parity = Parity.None;
            _mod.StopBits = StopBits.One;
            _mod.Handshake = Handshake.None;
            _mod.DtrEnable = true;
            _mod.Encoding = Encoding.ASCII;
            _mod.NewLine = "\r\n";
            _mod.ReadBufferSize = 2048;
            _mod.WriteTimeout = 5000;
            _mod.ReadTimeout = 5000;
        }

        ~PCAB() { this.Close(); }

        public void Close() { if (_mod?.IsOpen == true) { try { _mod.Close(); } catch { } } _mod = null; DAT.Clear(); }

        public bool PCAB_AutoTaskStart(UInt32 waiteTime, string[] serialNum)
        {
            if (!autoOpen()) { return false; }
            _mod.DiscardInBuffer();
            try
            {
                _mod.WriteLine("");
                foreach (string s in serialNum)
                {
                    _mod.WriteLine("#" + s + " CUI 0");
                }
                Thread.Sleep(500);
                _mod.DiscardInBuffer();
                DAT.Clear();
                foreach (string s in serialNum)
                {
                    _mod.WriteLine("#" + s + " GetIDN");
                    string[] arrBf = _mod.ReadLine().Split(',');
                    if (arrBf.Length != 4) { _mod.Close(); return false; }
                    if (arrBf[0] == "Orient Microwave Corp." && arrBf[1] == "LX00-0004-00" && arrBf[3].Substring(0, 4) == "1.3." && (arrBf[2] == s || "*" == s))
                    {
                        DAT.Add(new condDAT(s.Replace(" ", ""), "", "", "", "", "", ""));
                    }
                }
                if (DAT.Count < 1) { _mod.Close(); return false; }
            }
            catch { OnError?.Invoke(this, new PCABEventArgs(new condDAT(), "Serial Connect Error.")); return false; }

            _task = true;
            Task.Factory.StartNew(() => { PCAB_Task(waiteTime); });
            return true;
        }

        public void PCAB_AutoTaskStop() { _task = false; }

        public bool PCAB_PRESET(string serialNum)
        {
            if (!autoOpen()) { return false; }
            _task = null;
            while (_state) { Thread.Sleep(59); }
            _state = true;
            try
            {
                _mod.WriteLine("#" + serialNum + " RST");
                _mod.WriteLine("#" + serialNum + " CUI 0");
                Thread.Sleep(2000);
                _mod.DiscardInBuffer();
                _mod.WriteLine("#" + serialNum + " GetIDN");
                string[] arrBf = _mod.ReadLine().Split(',');
                _state = false;
                _task = true;
                if (arrBf.Length != 4) { return false; }
                if (arrBf[0] == "Orient Microwave Corp." && arrBf[1] == "LX00-0004-00" && arrBf[2] == serialNum && arrBf[3].Substring(0, 4) == "1.3.")
                { return true; }
                else { return false; }
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, new PCABEventArgs(new condDAT(), e.Message));
                return false;
            }
        }

        public string PCAB_CMD(string serialNum, string cmd, int readLine)
        {
            if (!autoOpen()) { return "ERR"; }
            _task = null;
            while (_state) { Thread.Sleep(59); }
            _state = true;
            try
            {
                _mod.WriteLine("#" + serialNum + " " + cmd);
                string ret = "";
                for (int i = 0; i < readLine; i++) { ret += _mod.ReadLine() + "\n"; }
                _state = false;
                _task = true;
                return ret;
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, new PCABEventArgs(new condDAT(), e.Message));
                return "ERR\n";
            }
        }

        public void DiscardInBuffer() { _mod.DiscardInBuffer(); }

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
                        foreach (condDAT cdat in DAT)
                        {
                            _mod.DiscardInBuffer();
                            _mod.WriteLine("#" + cdat.SN + " GetId");
                            string id = _mod.ReadLine();
                            _mod.WriteLine("#" + cdat.SN + " GetVd");
                            string vd = _mod.ReadLine();
                            _mod.WriteLine("#" + cdat.SN + " GetTMP.Val 0");
                            string temp = _mod.ReadLine();
                            _mod.WriteLine("#" + cdat.SN + " GetVin");
                            string vin = _mod.ReadLine();
                            _mod.WriteLine("#" + cdat.SN + " GetPin");
                            string pin = _mod.ReadLine();
                            _mod.WriteLine("#" + cdat.SN + " GetTMP.CPU");
                            string cpu_tmp = _mod.ReadLine();
                            if (CondNOW.SN == cdat.SN && (CondNOW.Id != id || CondNOW.Vd != vd || CondNOW.TEMPs != temp || CondNOW.Vin != vin || CondNOW.CPU_TEMP != cpu_tmp))
                            {
                                condDAT dat = new condDAT(cdat.SN, vin, vd, id, pin, temp, cpu_tmp);
                                OnUpdateDAT?.Invoke(this, new PCABEventArgs(dat, null));
                            }
                        }
                        _state = false;
                    }
                } while (_task != false);
                _mod?.Close();
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, new PCABEventArgs(new condDAT(), e.Message));
            }
        }

        private bool autoOpen()
        {
            if (_mod?.IsOpen != true)
            {
                try { _mod.Open(); _state = false; return true; }
                catch (UnauthorizedAccessException) { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
                catch (Exception) { MessageBox.Show("Serial port open Error.\n{e.ToString()}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
            }
            else { return true; }
        }

        public class PCABEventArgs : EventArgs
        {
            private string msg;
            private condDAT dat;

            public PCABEventArgs(condDAT ReceiveDAT, string Message) { dat = ReceiveDAT; msg = Message; }

            public string Message { get { return msg; } }

            public condDAT ReceiveDAT { get { return dat; } }

        }
    }
}
