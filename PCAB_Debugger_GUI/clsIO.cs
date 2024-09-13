using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
using static PCAB_Debugger_GUI.PCAB_TASK;

namespace PCAB_Debugger_GUI
{
    public class clsIO
    {
        public List<clsSerialIO> serialIO { get; private set; }
        public clsIO(List<clsSerialIO> serial) { serialIO = serial; }
    }

    public class clsSerialIO
    {
        private PCAB_TASK _task;
        public List<cntBOARD> PCAB_Boards {  get; private set; }
        public List<cntMonitor> PCAB_Monitors { get; private set; }
        public clsSerialIO(string SerialPortName, string[] SerialNumber,uint MonitorIntervalTime)
        {
            _task = new PCAB_TASK(SerialPortName);
            if (_task.PCAB_AutoTaskStart(MonitorIntervalTime, SerialNumber))
            {
                foreach(PCAB_SerialInterface.PCAB_UnitInterface unit in _task.UNITs)
                {

                }
            }
        }

        private void OnError(object sender, PCABEventArgs e)
        {
            _task.PCAB_AutoTaskStop();
            _task.Close();
            MessageBox.Show(e.Message.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _task = null;
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //}));
        }

        private void OnUpdateDAT(object sender, PCABEventArgs e)
        {
            //Dispatcher.BeginInvoke(new Action(() =>
            //{

            //}));
        }
    }
}
