using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows;

namespace PCAB_Debugger_ACS
{
    public class clsPOS
    {
        public event EventHandler<POSEventArgs> OnReadDAT;
        public event EventHandler<POSEventArgs> OnTaskError;
        public event EventHandler<POSEventArgs> OnTimeoutError;
        private SerialPort _serial;
        private bool _task = false;
        private List<byte> datBF = new List<byte>();

        public clsPOS(SerialPort _serialPort)
        {
            _serial = _serialPort;
        }

        public clsPOS(string portName)
        {
            _serial = new SerialPort(portName);
            _serial.PortName = "COM";
            _serial.BaudRate = 115200;
            _serial.DataBits = 8;
            _serial.Parity = Parity.None;
            _serial.StopBits = StopBits.One;
            _serial.Handshake = Handshake.None;
            _serial.DtrEnable = true;
            _serial.Encoding = Encoding.ASCII;
            _serial.NewLine = "\r\n";
            _serial.ReadBufferSize = 1024;
            _serial.WriteBufferSize = 1024;
            _serial.ReadTimeout = 1000;
            _serial.WriteTimeout = 1000;
        }

        ~clsPOS() { if (_serial?.IsOpen == true) { Close(); } }

        public void Open()
        {
            if (_serial?.IsOpen == true) { return; }
            try { _serial.Open(); datBF.Clear(); }
            catch (UnauthorizedAccessException) 
            { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            catch (Exception e) 
            { MessageBox.Show("Serial port open Error.\n{" + e.ToString() + "}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
        }

        public void Close() { if (_serial?.IsOpen == true) { _serial.Close(); } }

        public bool POS_AutoTaskStart()
        {
            if (_serial?.IsOpen != true) { return false; }
            _task = true;
            Task.Factory.StartNew(() => { POS_Task(); });
            return true;
        }

        public void POS_Task()
        {
            while (_task)
            {
                try
                {
                    byte[] byBF = new byte[_serial.ReadBufferSize];
                    int count = _serial.Read(byBF,0, byBF.Length);
                    if (count > 0) { datBF.AddRange(byBF.Skip(0).Take(count).ToList()); }
                    int posi = clsSearchList.SearchBytesSunday(datBF, new byte[] { 0x00, 0x96 });
                    while (!(posi < 0))
                    {
                        if (datBF.Count >= posi + 40)
                        {
                            OnReadDAT.Invoke(this, new POSEventArgs(datBF.GetRange(posi, 40), "GatDAT", null));
                            datBF.RemoveRange(0, posi + 40);
                            posi = clsSearchList.SearchBytesSunday(datBF, new byte[] { 0x00, 0x96 });
                        }
                    }
                }
                catch(TimeoutException ex)
                {
                    OnTimeoutError?.Invoke(this, new POSEventArgs(null, "ERROR > Timeout Exception", ex));
                }
                catch(Exception ex)
                {
                    OnTaskError?.Invoke(this,new POSEventArgs(null,"ERROR > Exception", ex));
                }
            }
        }

        public void POS_AutoTaskStop(){ _task = false; }

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



    }
}
