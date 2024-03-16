using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PCAB_Debugger_GUI
{
    internal class PCAB
    {
        private bool? _task;    //true:run / false:stop / null:Interrupt
        private STATUS _state;
        private SerialPort _mod;
        public event EventHandler<EventArgs> OnUpdateDAT;
        public event EventHandler<EventArgs> OnError;

        public PCAB(string PortName)
        {
            _state = STATUS.NOERROR;
            _mod = new SerialPort();
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

        public bool PCAB_AutoTaskStart()
        {
            if (_mod?.IsOpen != true)
            {
                try { _mod.Open(); }
                catch (UnauthorizedAccessException) { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
                catch (Exception) { MessageBox.Show("Serial port open Error.\n{e.ToString()}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
            }
            _task = true;
            Task.Factory.StartNew(() => { PCAB_Task(); });
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
            for (int i = 0; i < readLine; i++) { ret += _mod.ReadLine(); }
            _task = true;
            return cmd;
        }

        private void PCAB_Task()
        {
            do
            {
                if(_task == true)
                {

                }
            }while(_task != false);
        }

        

        private enum STATUS
        {
            SUCCESS,
            ERROR,
            NOERROR
        }
    }
}
