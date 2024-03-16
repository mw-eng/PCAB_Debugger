using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
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
        }

        ~PCAB() { if (_mod?.IsOpen == true) { try { _mod.Close(); } catch { } }_mod = null; }

        public bool PCAB_AutoTaskStart(UInt32 waiteTime,bool? initialize)
        {
            if (_mod?.IsOpen != true)
            {
                try { _mod.Open(); }
                catch (UnauthorizedAccessException) { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
                catch (Exception) { MessageBox.Show("Serial port open Error.\n{e.ToString()}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
            }
            _task = true;
            _mod.DiscardInBuffer();
            if (initialize == true)
            {
                try
                {
                    _mod.WriteLine("RST");
                    _mod.WriteLine("CUI 1");
                    Thread.Sleep(2000);
                    _mod.DiscardInBuffer();
                }
                catch { OnError?.Invoke(this, new PCABEventArgs(null, "Serial Connect Error.")); }
            }
            Task.Factory.StartNew(() => { PCAB_Task(waiteTime); });
            return true;
        }

        public void PCAB_AutoTaskStop() { _task = false; }

        public string PCAB_CMD(string cmd,int readLine)
        {
            _task = null;
            if (_mod?.IsOpen != true)
            {
                try { _mod.Open(); }
                catch (UnauthorizedAccessException) { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return null; }
                catch (Exception) { MessageBox.Show("Serial port open Error.\n{e.ToString()}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return null; }
            }
            _mod.WriteLine(cmd);
            string ret = "";
            for (int i = 0; i < readLine; i++) { ret += _mod.ReadLine() + "\n"; }
            _task = true;
            return ret;
        }

        private void PCAB_Task(UInt32 waiteTime)
        {
            do
            {
                Thread.Sleep((int)waiteTime);
                if (_task == true)
                {
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
                }
            }while(_task != false);
            _mod?.Close();
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
