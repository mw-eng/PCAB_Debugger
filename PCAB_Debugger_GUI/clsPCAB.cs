using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PCAB_Debugger_GUI
{
    internal class PCAB
    {
        private bool? _task;    //true:run / false:stop / null:Interrupt
        private SerialPort _mod;
        private bool _state;
        public event EventHandler<PCABEventArgs> OnUpdateDAT;
        public event EventHandler<PCABEventArgs> OnError;
        public string Vd_now { get; set; }
        public string Id_now { get; set; }
        public string TEMP_now { get; set; }

        public PCAB(string PortName)
        {
            _mod = new SerialPort(PortName);
            _mod.BaudRate = 9600;
            _mod.DataBits = 8;
            _mod.Parity = Parity.None;
            _mod.StopBits = StopBits.One;
            _mod.Handshake = Handshake.None;
            _mod.DtrEnable = false;
            _mod.Encoding = Encoding.ASCII;
            _mod.NewLine = "\n";
            _mod.ReadBufferSize = 2048;
            _mod.WriteTimeout = 1000;
            _mod.ReadTimeout = 1000;
        }

        ~PCAB() { if (_mod?.IsOpen == true) { try { _mod.Close(); } catch { } }_mod = null; }

        public bool PCAB_AutoTaskStart(UInt32 waiteTime, bool? initialize)
        {
            if (!autoOpen()) { return false; }
            _mod.DiscardInBuffer();
            try
            {
                if (initialize == true)
                {
                    _mod.WriteLine("RST");
                }
                _mod.WriteLine("CUI 1");
                Thread.Sleep(2000);
                _mod.DiscardInBuffer();
                _mod.WriteLine("GetIDN");
                string[] arrBf = _mod.ReadLine().Split(',');
                if (arrBf[0] != "PCAB") { return false; }
            }
            catch { OnError?.Invoke(this, new PCABEventArgs(null, "Serial Connect Error.")); }

            _task = true;
            Task.Factory.StartNew(() => { PCAB_Task(waiteTime); });
            return true;
        }

        public void PCAB_AutoTaskStop() { _task = false; }

        public string PCAB_CMD(string cmd,int readLine)
        {
            if (!autoOpen()) { return "ERR"; }
            _task = null;
            while (_state) {Thread.Sleep(59);}
            _state = true;
            try
            {
                _mod.WriteLine(cmd);
                string ret = "";
                for (int i = 0; i < readLine; i++) { ret += _mod.ReadLine() + "\n"; }
                _state = false;
                _task = true;
                return ret;
            }
            catch(Exception e)
            {
                OnError?.Invoke(this, new PCABEventArgs(null, e.Message));
                return "ERR\n";
            }
        }

        public void DiscardInBuffer(){ _mod.DiscardInBuffer(); }

        private void PCAB_Task(UInt32 waiteTime)
        {
            do
            {
                Thread.Sleep((int)waiteTime);
                if (_task == true)
                {
                    while (_state) { Thread.Sleep(53); }
                    _state = true;
                    _mod.DiscardInBuffer();
                    _mod.WriteLine("GetId");
                    string id = _mod.ReadLine();
                    _mod.WriteLine("GetVd");
                    string vd = _mod.ReadLine();
                    _mod.WriteLine("GetTMP0");
                    string temp = _mod.ReadLine();
                    if(Id_now != id || Vd_now != vd || TEMP_now != temp)
                    {
                        string[] dat = { id, vd, temp };
                        OnUpdateDAT?.Invoke(this, new PCABEventArgs(dat, null));
                    }
                    _state = false;
                }
            }while(_task != false);
            _mod?.Close();
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
            private string[] dat;
            public PCABEventArgs(string[] ReceiveDAT, string Message) { dat = ReceiveDAT; msg = Message; }

            public string Message { get { return msg; } }

            public string[] ReceiveDAT { get { return dat; } }

        }
    }
}
