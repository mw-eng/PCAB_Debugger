using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PCAB_Debugger_GUI.PCAB;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// winEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class winEditor : Window
    {
        public class BindableBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) { return false; }

                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
        }
        private class binaryROW : BindableBase
        {
            private string _addr;
            private string _dat00;
            private string _dat01;
            private string _dat02;
            private string _dat03;
            private string _dat04;
            private string _dat05;
            private string _dat06;
            private string _dat07;
            private string _dat08;
            private string _dat09;
            private string _dat0A;
            private string _dat0B;
            private string _dat0C;
            private string _dat0D;
            private string _dat0E;
            private string _dat0F;
            private string _datSTR;


            public string addr { get => _addr;set=> SetProperty(ref _addr, value);}
            public string dat00  {get=>_dat00;set=>SetProperty(ref _dat00,value);}
            public string dat01  {get=>_dat01;set=>SetProperty(ref _dat01,value);}
            public string dat02  {get=>_dat02;set=>SetProperty(ref _dat02,value);}
            public string dat03  {get=>_dat03;set=>SetProperty(ref _dat03,value);}
            public string dat04  {get=>_dat04;set=>SetProperty(ref _dat04,value);}
            public string dat05  {get=>_dat05;set=>SetProperty(ref _dat05,value);}
            public string dat06  {get=>_dat06;set=>SetProperty(ref _dat06,value);}
            public string dat07  {get=>_dat07;set=>SetProperty(ref _dat07,value);}
            public string dat08  {get=>_dat08;set=>SetProperty(ref _dat08,value);}
            public string dat09  {get=>_dat09;set=>SetProperty(ref _dat09,value);}
            public string dat0A  {get=>_dat0A;set=>SetProperty(ref _dat0A,value);}
            public string dat0B  {get=>_dat0B;set=>SetProperty(ref _dat0B,value);}
            public string dat0C  {get=>_dat0C;set=>SetProperty(ref _dat0C,value);}
            public string dat0D  {get=>_dat0D;set=>SetProperty(ref _dat0D,value);}
            public string dat0E  {get=>_dat0E;set=>SetProperty(ref _dat0E,value);}
            public string dat0F  {get=>_dat0F;set=>SetProperty(ref _dat0F, value);}
            public string datSTR {get=> _datSTR; set=>SetProperty(ref _datSTR, value);}
            public string datLINE { get => _dat00 + _dat01 + _dat02 + _dat03 + _dat04 + _dat05 + _dat06 + _dat07 + _dat08 + _dat09 + _dat0A + _dat0B + _dat0C + _dat0D + _dat0E + _dat0F; }

            public binaryROW(UInt16 line, byte dat00, byte dat01, byte dat02, byte dat03, byte dat04, byte dat05, byte dat06, byte dat07, byte dat08, byte dat09, byte dat0A, byte dat0B, byte dat0C, byte dat0D, byte dat0E, byte dat0F)
            {
                _addr = (16 * line).ToString("X8");
                _dat00 = dat00.ToString("X2");
                _dat01 = dat01.ToString("X2");
                _dat02 = dat02.ToString("X2");
                _dat03 = dat03.ToString("X2");
                _dat04 = dat04.ToString("X2");
                _dat05 = dat05.ToString("X2");
                _dat06 = dat06.ToString("X2");
                _dat07 = dat07.ToString("X2");
                _dat08 = dat08.ToString("X2");
                _dat09 = dat09.ToString("X2");
                _dat0A = dat0A.ToString("X2");
                _dat0B = dat0B.ToString("X2");
                _dat0C = dat0C.ToString("X2");
                _dat0D = dat0D.ToString("X2");
                _dat0E = dat0E.ToString("X2");
                _dat0F = dat0F.ToString("X2");
                _datSTR = "";
                if (32 <= dat00 && dat00 <= 126) { datSTR += (char)dat00; } else { datSTR += "."; }
                if (32 <= dat01 && dat01 <= 126) { datSTR += (char)dat01; } else { datSTR += "."; }
                if (32 <= dat02 && dat02 <= 126) { datSTR += (char)dat02; } else { datSTR += "."; }
                if (32 <= dat03 && dat03 <= 126) { datSTR += (char)dat03; } else { datSTR += "."; }
                if (32 <= dat04 && dat04 <= 126) { datSTR += (char)dat04; } else { datSTR += "."; }
                if (32 <= dat05 && dat05 <= 126) { datSTR += (char)dat05; } else { datSTR += "."; }
                if (32 <= dat06 && dat06 <= 126) { datSTR += (char)dat06; } else { datSTR += "."; }
                if (32 <= dat07 && dat07 <= 126) { datSTR += (char)dat07; } else { datSTR += "."; }
                if (32 <= dat08 && dat08 <= 126) { datSTR += (char)dat08; } else { datSTR += "."; }
                if (32 <= dat09 && dat09 <= 126) { datSTR += (char)dat09; } else { datSTR += "."; }
                if (32 <= dat0A && dat0A <= 126) { datSTR += (char)dat0A; } else { datSTR += "."; }
                if (32 <= dat0B && dat0B <= 126) { datSTR += (char)dat0B; } else { datSTR += "."; }
                if (32 <= dat0C && dat0C <= 126) { datSTR += (char)dat0C; } else { datSTR += "."; }
                if (32 <= dat0D && dat0D <= 126) { datSTR += (char)dat0D; } else { datSTR += "."; }
                if (32 <= dat0E && dat0E <= 126) { datSTR += (char)dat0E; } else { datSTR += "."; }
                if (32 <= dat0F && dat0F <= 126) { datSTR += (char)dat0F; } else { datSTR += "."; }
            }
            public binaryROW Copy()
            {
                return (binaryROW)MemberwiseClone();
            }
        }
        private ObservableCollection<binaryROW> dataTableNOW = new ObservableCollection<binaryROW>();
        private ObservableCollection<binaryROW> dataTable = new ObservableCollection<binaryROW>();
        private PCAB _mod;

        public winEditor(PCAB mod)
        {
            InitializeComponent();
            _mod = mod;
            _mod.OnError += OnError;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string strBF;
            int block = 0;
            BLOCK_COMBOBOX.Items.Clear();
            strBF = _mod.PCAB_CMD("*", "RROM " + block.ToString("X") + "-00", 1);
            while (strBF.Substring(0, 3) != "ERR")
            {
                BLOCK_COMBOBOX.Items.Add(block.ToString("X4"));
                block++;
                strBF = _mod.PCAB_CMD("*", "RROM " + block.ToString("X") + "-00", 1);
            }
            SECTOR_COMBOBOX.Items.Clear();
            for(uint i = 0; i < 0x10u; i++) { SECTOR_COMBOBOX.Items.Add((i * 0x10u).ToString("X2")); }
            BLOCK_COMBOBOX.SelectedIndex = BLOCK_COMBOBOX.Items.Count - 1;
            SECTOR_COMBOBOX.SelectedIndex = SECTOR_COMBOBOX.Items.Count - 1;
            reload();
        }

        private void BINARY_DATAGRID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(TextBox))
            {
                int cnt = (((TextBox)e.OriginalSource).Text + e.Text).Length - ((TextBox)e.OriginalSource).SelectionLength;
                if (2 >= cnt && new Regex("[0-9a-fA-F]").IsMatch(e.Text)) { e.Handled = false; ((TextBox)e.OriginalSource).CharacterCasing = CharacterCasing.Upper; }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void BINARY_DATAGRID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(TextBox))
            {
                if (((TextBox)e.OriginalSource).Text.Length == 0) { ((TextBox)e.OriginalSource).Text = "00"; }
                else if (((TextBox)e.OriginalSource).Text.Length == 1) { ((TextBox)e.OriginalSource).Text = "0" + ((TextBox)e.OriginalSource).Text; }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _mod.OnError -= OnError;
        }

        private void OnError(object sender, PCABEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Close();
            }));
        }

        private void RELOAD_Click(object sender, RoutedEventArgs e)
        {
            reload();
            MessageBox.Show("Rom reload completed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void WRITE_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to overwrite the ROM data at Address:" + BLOCK_COMBOBOX.Text + "-" + SECTOR_COMBOBOX.Text + "?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) { return; }
            string strBF;
            uint sect = uint.Parse(SECTOR_COMBOBOX.Text, System.Globalization.NumberStyles.HexNumber);
            for (int i = 0; i < dataTable.Count; i += 0x10)
            {
                strBF = "";
                for (int j = 0; j < 0x10u; j++)
                {
                    strBF += dataTable[i + j].datLINE;
                }
                if (_mod.PCAB_CMD("*", "OROM " + BLOCK_COMBOBOX.Text + "-" + (sect + i / 0x10).ToString("X") + " " + strBF, 1).Substring(0, 3) == "ERR")
                {
                    MessageBox.Show("Write rom error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; 
                }
            }
            MessageBox.Show("Success write rom.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {
            dataTable.Clear();
            foreach (binaryROW row in dataTableNOW) { dataTable.Add(row.Copy()); }
            BINARY_DATAGRID.Items.Refresh();
        }

        private void BLOCK_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {

        }

        private void SECTOR_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {

        }

        private void reload()
        {
            dataTable.Clear();
            dataTableNOW.Clear();
            for(int page = 0; page < 0x10u; page++)
            {
                string strBF = _mod.PCAB_CMD("*", "RROM " + BLOCK_COMBOBOX.Text + "-" + 
                    (uint.Parse(SECTOR_COMBOBOX.Text, System.Globalization.NumberStyles.HexNumber) + page).ToString("X"), 1);
                for (int i = 0; i < strBF.Length; i += 32)
                {
                    if (strBF.Length < i + 32) { break; }
                    dataTable.Add(new binaryROW((UInt16)(page * 0x10u + (uint)i / 32u),
                        Convert.ToByte(strBF.Substring(i + 0, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 2, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 4, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 6, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 8, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 10, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 12, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 14, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 16, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 18, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 20, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 22, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 24, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 26, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 28, 2), 16),
                        Convert.ToByte(strBF.Substring(i + 30, 2), 16)));
                }
            }
            foreach (binaryROW row in dataTable) { dataTableNOW.Add(row.Copy()); }
            BINARY_DATAGRID.ItemsSource = dataTable;
            foreach (DataGridColumn col in BINARY_DATAGRID.Columns) { col.MinWidth = col.ActualWidth; }
        }

    }
}
