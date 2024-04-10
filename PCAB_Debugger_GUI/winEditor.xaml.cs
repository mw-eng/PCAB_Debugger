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
using System.ComponentModel.Design;
using System.Data;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// winEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class winEditor : Window
    {
        private struct binaryROW
        {
            public string addr { get; set; }
            public string dat00 { get; set; }
            public string dat01 { get; set; }
            public string dat02 { get; set; }
            public string dat03 { get; set; }
            public string dat04 { get; set; }
            public string dat05 { get; set; }
            public string dat06 { get; set; }
            public string dat07 { get; set; }
            public string dat08 { get; set; }
            public string dat09 { get; set; }
            public string dat0A { get; set; }
            public string dat0B { get; set; }
            public string dat0C { get; set; }
            public string dat0D { get; set; }
            public string dat0E { get; set; }
            public string dat0F { get; set; }
            public string datSTR { get; set; }

            public binaryROW(UInt16 line, byte _dat00, byte _dat01, byte _dat02, byte _dat03, byte _dat04, byte _dat05, byte _dat06, byte _dat07, byte _dat08, byte _dat09, byte _dat0A, byte _dat0B, byte _dat0C, byte _dat0D, byte _dat0E, byte _dat0F)
            {
                addr = (16 * line).ToString("X8");
                dat00 = _dat00.ToString("X2");
                dat01 = _dat01.ToString("X2");
                dat02 = _dat02.ToString("X2");
                dat03 = _dat03.ToString("X2");
                dat04 = _dat04.ToString("X2");
                dat05 = _dat05.ToString("X2");
                dat06 = _dat06.ToString("X2");
                dat07 = _dat07.ToString("X2");
                dat08 = _dat08.ToString("X2");
                dat09 = _dat09.ToString("X2");
                dat0A = _dat0A.ToString("X2");
                dat0B = _dat0B.ToString("X2");
                dat0C = _dat0C.ToString("X2");
                dat0D = _dat0D.ToString("X2");
                dat0E = _dat0E.ToString("X2");
                dat0F = _dat0F.ToString("X2");
                datSTR = "";
                if (32 <= _dat00 && _dat00 <= 126) { datSTR += (char)_dat00; } else { datSTR += "."; }
                if (32 <= _dat01 && _dat01 <= 126) { datSTR += (char)_dat01; } else { datSTR += "."; }
                if (32 <= _dat02 && _dat02 <= 126) { datSTR += (char)_dat02; } else { datSTR += "."; }
                if (32 <= _dat03 && _dat03 <= 126) { datSTR += (char)_dat03; } else { datSTR += "."; }
                if (32 <= _dat04 && _dat04 <= 126) { datSTR += (char)_dat04; } else { datSTR += "."; }
                if (32 <= _dat05 && _dat05 <= 126) { datSTR += (char)_dat05; } else { datSTR += "."; }
                if (32 <= _dat06 && _dat06 <= 126) { datSTR += (char)_dat06; } else { datSTR += "."; }
                if (32 <= _dat07 && _dat07 <= 126) { datSTR += (char)_dat07; } else { datSTR += "."; }
                if (32 <= _dat08 && _dat08 <= 126) { datSTR += (char)_dat08; } else { datSTR += "."; }
                if (32 <= _dat09 && _dat09 <= 126) { datSTR += (char)_dat09; } else { datSTR += "."; }
                if (32 <= _dat0A && _dat0A <= 126) { datSTR += (char)_dat0A; } else { datSTR += "."; }
                if (32 <= _dat0B && _dat0B <= 126) { datSTR += (char)_dat0B; } else { datSTR += "."; }
                if (32 <= _dat0C && _dat0C <= 126) { datSTR += (char)_dat0C; } else { datSTR += "."; }
                if (32 <= _dat0D && _dat0D <= 126) { datSTR += (char)_dat0D; } else { datSTR += "."; }
                if (32 <= _dat0E && _dat0E <= 126) { datSTR += (char)_dat0E; } else { datSTR += "."; }
                if (32 <= _dat0F && _dat0F <= 126) { datSTR += (char)_dat0F; } else { datSTR += "."; }
            }
        }
        private List<binaryROW> dataTable = new List<binaryROW>();

        public winEditor()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //dataTable.Columns.Clear();
            //for(int i = 0;i< BINARY_DATAGRID.Columns.Count;i++)
            //{
            //    dataTable.Columns.Add(BINARY_DATAGRID.Columns[i].Header.ToString());
            //}
            //dataTable.Rows.Clear();
            //DataRow row = dataTable.NewRow();
            //for (int i = 0; i < BINARY_DATAGRID.Columns.Count; i++)
            //{ row[i] = i.ToString(); }
            //dataTable.Rows.Add(row);
            //BINARY_DATAGRID.Items.Clear();
            //BINARY_DATAGRID.Items.Add();
            //for (int i = 0; i < BINARY_DATAGRID.Columns.Count; i++)
            //{
            //    dataTable.Columns.Add(BINARY_DATAGRID.Columns[i].Header.ToString());
            //}
            dataTable.Clear();
            dataTable.Add(new binaryROW(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            dataTable.Add(new binaryROW(0, 0, 16, 0, 0, 0, 0, 0, 58, 0, 0, 0, 0, 245, 0, 0, 0));
            BINARY_DATAGRID.ItemsSource = dataTable;
            foreach(DataGridColumn col in BINARY_DATAGRID.Columns) { col.MinWidth = col.ActualWidth; }
            
        }
    }
}
