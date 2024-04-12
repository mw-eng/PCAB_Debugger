using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static PCAB_Debugger_GUI.ShowSerialPortName;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// winUpdater.xaml の相互作用ロジック
    /// </summary>
    public partial class winUpdater : Window
    {
        SerialPortTable[] ports;

        public winUpdater()
        {
            InitializeComponent();
        }

        private void UPDATE_BUTTON_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SERIAL_PORTS_COMBOBOX_RELOAD()
        {
            SERIAL_PORTS_COMBOBOX.Items.Clear();
            ports = GetDeviceNames();
            foreach (SerialPortTable port in ports)
            {
                SERIAL_PORTS_COMBOBOX.Items.Add(port.Caption);
            }
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownOpened(object sender, EventArgs e)
        {
            SERIAL_PORTS_COMBOBOX_RELOAD();
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SERIAL_PORTS_COMBOBOX.SelectedIndex < 0) { UPDATE_BUTTON.IsEnabled = false; }
            else { UPDATE_BUTTON.IsEnabled = true; }
        }
    }
}
