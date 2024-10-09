using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows;

namespace PCAB_Debugger_ComLib
{
    public class POS
    {
        public event EventHandler<POSEventArgs> OnReadDAT;
        public event EventHandler<POSEventArgs> OnTaskError;
        public event EventHandler<POSEventArgs> OnTimeoutError;
        private SerialPort _serial;
        private bool _task = false;
        private List<byte> datBF = new List<byte>();
        private Task<int> _loopTask;

        public POS(SerialPort _serialPort)
        {
            _serial = _serialPort;
        }

        public POS(string portName) : this(portName, 9600) { }

        public POS(string portName, int baudRate)
        {
            _serial = new SerialPort(portName);
            _serial.BaudRate = baudRate;
            _serial.DataBits = 8;
            _serial.Parity = Parity.None;
            _serial.StopBits = StopBits.One;
            _serial.Handshake = Handshake.None;
            _serial.DtrEnable = true;
            _serial.Encoding = Encoding.ASCII;
            _serial.NewLine = "\r\n";
            _serial.ReadBufferSize = 1024;
            _serial.WriteBufferSize = 1024;
            _serial.ReadTimeout = 5000;
            _serial.WriteTimeout = 5000;
        }

        ~POS() { if (_task) { POS_AutoTaskStop();  } Close(); }

        public void Open()
        {
            if (_serial?.IsOpen == true) { return; }
            try { _serial.Open(); datBF.Clear(); }
            catch (UnauthorizedAccessException) 
            { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            catch (Exception e) 
            { MessageBox.Show("Serial port open Error.\n{" + e.ToString() + "}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
        }

        public void Close() { if (_task) { POS_AutoTaskStop(); } if (_serial?.IsOpen == true) { _serial.Close(); } }

        public bool POS_AutoTaskStart()
        {
            if (_serial?.IsOpen != true) { return false; }
            _task = true;
            _loopTask = Task.Factory.StartNew(() => { return POS_Task(); });
            return true;
        }

        private int POS_Task()
        {
            while (_task)
            {
                try
                {
                    byte[] byBF = new byte[_serial.ReadBufferSize];
                    int count = _serial.Read(byBF,0, byBF.Length);
                    if (count > 0) { datBF.AddRange(byBF.Skip(0).Take(count).ToList()); }
                    int posi1 = SearchList.SearchBytesSundayLastIndexOf(datBF, new byte[] { 0x00, 0x96 });
                    if (posi1 >= 0 && datBF.Count >= posi1 + 40)
                    { OnReadDAT?.Invoke(this, new POSEventArgs(datBF.GetRange(posi1, 40), "GatDAT", null)); datBF.RemoveRange(0, posi1 + 40); }
                    else
                    {
                        int posi2 = SearchList.SearchBytesSundayLastIndexOf(datBF, new byte[] { 0x00, 0x96 }, posi1 - 1);
                        if (posi2 >= 0 && datBF.Count >= posi2 + 40)
                        { OnReadDAT?.Invoke(this, new POSEventArgs(datBF.GetRange(posi2, 40), "GatDAT", null)); datBF.RemoveRange(0, posi2 + 40); }
                    }
                    if(datBF.Count > 1024 * 1024) { datBF.Clear(); }
                }
                catch(TimeoutException ex)
                {
                    OnTimeoutError?.Invoke(this, new POSEventArgs(null, "ERROR > Timeout Exception", ex));
                    OnReadDAT?.Invoke(this, new POSEventArgs(null, "Timeout Exception", ex));
                }
                catch(Exception ex)
                {
                    OnTaskError?.Invoke(this,new POSEventArgs(null,"ERROR > Exception", ex));
                }
            }
            return 0;
        }

        public void POS_AutoTaskStop()
        {
            _task = false;
            _loopTask?.ConfigureAwait(false);
            _loopTask?.Wait();
        }

        public class POSEventArgs : EventArgs
        {
            private string msg;
            private List<byte> dat;
            private Exception exception;

            public POSEventArgs(List<byte> ReceiveDAT, string Message, Exception Exception) { dat = ReceiveDAT; msg = Message; exception = Exception; }

            public string Message { get { return msg; } }

            public List<byte> ReceiveDAT { get { return dat; } }

            public Exception Exception { get { return exception; } }
        }

        public class PAST2
        {
            public double TIME { get { return BitConverter.ToDouble(DATA, 2); } }
            public float ROLL { get { return BitConverter.ToInt16(DATA, 10) * 0.01f; } }
            public float PITCH { get { return BitConverter.ToInt16(DATA, 12) * 0.01f; } }
            public float HEADING { get { return BitConverter.ToUInt16(DATA, 14) * 0.01f; } }
            public float LATITUDE { get { return BitConverter.ToInt32(DATA, 16) * 0.001f; } }
            public float LONGITUDE { get { return BitConverter.ToInt32(DATA, 20) * 0.001f; } }
            public float ALTITUDE { get { return BitConverter.ToInt32(DATA, 24) * 0.01f; } }
            public float SPEED { get { return BitConverter.ToUInt16(DATA, 28) * 0.01f; } }
            public float TRACK { get { return BitConverter.ToUInt16(DATA, 30) * 0.01f; } }
            public float LONG_ACCEL { get { return BitConverter.ToInt16(DATA, 32) * 0.0005f; } }
            public float TRAN_ACCEL { get { return BitConverter.ToInt16(DATA, 34) * 0.0005f; } }
            public float DOWN_ACCEL { get { return BitConverter.ToInt16(DATA, 36) * 0.0005f; } }

            public byte[] TIME_BINARY { get { return DATA.Skip(2).Take(8).ToArray(); } }
            public byte[] ROLL_BINARY { get { return DATA.Skip(10).Take(2).ToArray(); } }
            public byte[] PITCH_BINARY { get { return DATA.Skip(12).Take(2).ToArray(); } }
            public byte[] HEADING_BINARY { get { return DATA.Skip(14).Take(2).ToArray(); } }
            public byte[] LATITUDE_BINARY { get { return DATA.Skip(16).Take(4).ToArray(); } }
            public byte[] LONGITUDE_BINARY { get { return DATA.Skip(20).Take(4).ToArray(); } }
            public byte[] ALTITUDE_BINARY { get { return DATA.Skip(24).Take(4).ToArray(); } }
            public byte[] SPEED_BINARY { get { return DATA.Skip(28).Take(2).ToArray(); } }
            public byte[] TRACK_BINARY { get { return DATA.Skip(30).Take(2).ToArray(); } }
            public byte[] LONG_ACCEL_BINARY { get { return DATA.Skip(32).Take(2).ToArray(); } }
            public byte[] TRAN_ACCELBINARY { get { return DATA.Skip(34).Take(2).ToArray(); } }
            public byte[] DOWN_ACCEL_BINARY { get { return DATA.Skip(36).Take(2).ToArray(); } }

            public byte[] DATA { get; private set; } = new byte[40];
            public PAST2(IEnumerable<byte> dat)
            {
                if (dat.Count() != 40) { throw new ArgumentOutOfRangeException("The parameter index cannot be anything other than 40bytes."); }
                DATA = dat.ToArray();
                if (DATA[0] != 0x00 || DATA[1] != 0x96) { throw new FormatException("Not in PAST2 message format."); }
                UInt16 sum = 0;
                for (uint count = 2; count < DATA.Length - 2; count++)
                {
                    sum += DATA[count];
                }
                if(sum != (DATA[38] + DATA[39] * 0x0100))
                { throw new Exception("Checksum Error."); }
            }
        }


    }
}
