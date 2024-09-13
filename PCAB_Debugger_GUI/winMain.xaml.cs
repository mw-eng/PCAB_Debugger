using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PCAB_Debugger_GUI.ShowSerialPortName;
using static PCAB_Debugger_GUI.PCAB_TASK;
using static PCAB_Debugger_GUI.cntConfig;
using MWComLibCS.ExternalControl;
using static PCAB_Debugger_GUI.cntConfigSettings;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class winMain : Window
    {
        SerialPortTable[] ports;
        public clsSerialIO _io;
        private winMonitor monitor;
        private int visa32Resource;

        public winMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title += " Ver," + System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
#if DEBUG
            //Settings.Default.Reset();
            this.Title += "_DEBUG MODE";
            BOARD_GRID.IsEnabled = true;
#endif
            visa32Resource = VisaControlNI.NewResourceManager();
        }

        #region Serial EVENT

        private void SERIAL_PORTS_COMBOBOX_RELOAD()
        {
            SERIAL_PORTS_COMBOBOX.Items.Clear();
            ports = GetDeviceNames();
            if (ports != null)
            {
                foreach (SerialPortTable port in ports)
                {
                    SERIAL_PORTS_COMBOBOX.Items.Add(port.Caption);
                }
            }
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownOpened(object sender, EventArgs e)
        {
            SERIAL_PORTS_COMBOBOX_RELOAD();
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if(SERIAL_PORTS_COMBOBOX.SelectedIndex < 0) { CONNECT_BUTTON.IsEnabled = false; }
            else { CONNECT_BUTTON.IsEnabled = true; }
        }

        private void CONNECT_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (_io?.isOpen == true)
            {
                OnError(null, null);
            }
            else
            {
                string[] sn = SERIAL_NUMBERS_TEXTBOX.Text.Replace(" ", "").Split(',');
                _io = new clsSerialIO(ports[SERIAL_PORTS_COMBOBOX.SelectedIndex].Name);
                _io.OnError += OnError;
                try
                {
                    _io.Open(sn, uint.Parse(WAITE_TIME_TEXTBOX.Text));
                }
                catch
                {
                    _io.Close();
                    SERIAL_PORTS_COMBOBOX_RELOAD();
                    SERIAL_PORTS_COMBOBOX_DropDownClosed(null, null);
                    return;
                }
                if (_io?.isOpen == true)
                {
                    monitor = new winMonitor();
                    monitor.MONITOR_GRID.Children.Clear();
                    monitor.MONITOR_GRID.RowDefinitions.Clear();
                    monitor.MONITOR_GRID.ColumnDefinitions.Clear();
                    foreach (cntMonitor mon in _io.PCAB_Monitors)
                    {
                        monitor.MONITOR_GRID.ColumnDefinitions.Add(new ColumnDefinition());
                        mon.SetValue(Grid.ColumnProperty, 0);
                        monitor.MONITOR_GRID.Children.Add(mon);
                    }
                    monitor.Show();
                    BOARD_GRID.Children.Clear();
                    if (_io.PCAB_Boards.Count == 1)
                    {
                        BOARD_GRID.Children.Add(_io.PCAB_Boards[0]);
                        ((cntBOARD)BOARD_GRID.Children[0]).AUTO.setResourceManager = visa32Resource;
                        ((cntBOARD)BOARD_GRID.Children[0]).AUTO.ButtonClickEvent += AUTO_ButtonClickEvent;
                        ((cntBOARD)BOARD_GRID.Children[0]).CONFIG.ButtonClickEvent += CONFIG_ButtonClickEvent;
                        ((cntBOARD)BOARD_GRID.Children[0]).CONFIG.CONFIG_SETTINGS.CheckboxClickEvent += CONFIG_CONFIG_SETTINGS_CheckboxClickEventHandler;
                    }
                    else
                    {
                        BOARD_GRID.Children.Add(new TabControl());
                        for (int i = 0; i < _io.PCAB_Boards.Count; i++)
                        {
                            ((TabControl)BOARD_GRID.Children[0]).FontSize = 24;
                            ((TabControl)BOARD_GRID.Children[0]).Margin = new Thickness(5);
                            ((TabControl)BOARD_GRID.Children[0]).Items.Add(new TabItem());
                            ((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Header = "S/N, " + _io.PCAB_Boards[i].SerialNumber;
                            ((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content = _io.PCAB_Boards[i];
                            ((cntBOARD)((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content).AUTO.setResourceManager = visa32Resource;
                            ((cntBOARD)((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content).AUTO.ButtonClickEvent += AUTO_ButtonClickEvent;
                            ((cntBOARD)((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content).CONFIG.ButtonClickEvent += CONFIG_ButtonClickEvent;
                            ((cntBOARD)((TabItem)((TabControl)BOARD_GRID.Children[0]).Items[i]).Content).CONFIG.CONFIG_SETTINGS.CheckboxClickEvent += CONFIG_CONFIG_SETTINGS_CheckboxClickEventHandler;

                        }
                    }
                    SERIAL_PORTS_COMBOBOX.IsEnabled = false;
                    SERIAL_CONFIG_GRID.IsEnabled = false;
                    BOARD_GRID.IsEnabled = true;
                    CONNECT_BUTTON_CONTENT.Text = "Disconnect";
                }
                else
                {
                    _io.Close();
                    MessageBox.Show("No valid PCAB found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Other EVENT

        private void DEC_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9のみ
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void DEC_TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼付け場合
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9]|[ ]").IsMatch(strTXT[cnt].ToString()))
                    {
                        // 処理済み
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        private void DEC_TextBox_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                uint uintVal = Convert.ToUInt32(((TextBox)sender).Text);
                if (0 <= uintVal && uintVal <= 65535) { return; }
                MessageBox.Show("Enter in the range 0 to 65535");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("Enter in the range 0 to 65535");
                e.Handled = true;
            }
        }

        private void SN_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9およびa-f/A-Fのみ
            e.Handled = !new Regex("[0-9|a-z|A-Z|,| |*]").IsMatch(e.Text);
        }

        private void SN_TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼付け場合
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9|a-z|A-Z|,]|[ ]").IsMatch(strTXT[cnt].ToString()))
                    {
                        // 処理済み
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        private void SN_TextBox_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                string strBF = ((TextBox)sender).Text.Replace(" ", "");
                if(0 <= strBF.IndexOf("*") && strBF.Length != 1)
                {
                    MessageBox.Show("Multiple \"*\" specifications cannot be specified.");
                    e.Handled = true;
                    return;
                }
                string[] arrBF = strBF.Split(',');
                if (arrBF.Length > 0)
                {
                    foreach (string str in arrBF)
                    {
                        if (str.Length < 0 || 15 < str.Length) { throw new Exception(); }
                    }
                }
                else { throw new Exception(); }
                return;
            }
            catch
            {
                MessageBox.Show("Enter the serial number between 1 and 15 characters, without spaces, separated by commas.");
                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_io?.isOpen == true)
            {
                OnError(null, null);
            }
        }
        #endregion

        #region Sub Function EVENT
        private void CONFIG_ButtonClickEvent(object sender, RoutedEventArgs e, ButtonCategory category)
        {
            string strSN = ((cntConfig)sender).SerialNumber;
        }
        private void AUTO_ButtonClickEvent(object sender, RoutedEventArgs e, string dirPath)
        {
            string strSN = ((cntAUTO)sender).SerialNumber;
        }
        private void CONFIG_CONFIG_SETTINGS_CheckboxClickEventHandler(object sender, RoutedEventArgs e, CheckBoxCategory cat, bool? isChecked)
        {
            string strSN = ((cntConfigSettings)sender).SerialNumber;
        }

        private void OnError(object sender, PCABEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                monitor.WindowClose();
                _io.Close();
                BOARD_GRID.Children.Clear();
                BOARD_GRID.Children.Add(new cntBOARD());
                BOARD_GRID.IsEnabled = false;
                SERIAL_PORTS_COMBOBOX.IsEnabled = true;
                SERIAL_CONFIG_GRID.IsEnabled = true;
                CONNECT_BUTTON_CONTENT.Text = "Connect";
            }));
        }
        #endregion
    }
}
