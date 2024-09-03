using System;
using System.Windows;
using System.Windows.Controls;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// cntConfig.xaml の相互作用ロジック
    /// </summary>
    public partial class cntConfig : UserControl
    {
        public cntConfig()
        {
            InitializeComponent();
        }

        private void SAVEADDRESS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SAVEADDRESS_COMBOBOX.SelectedIndex < 0) { SAVEADDRESS_COMBOBOX.SelectedIndex = 0; }
        }

        private void LOADMEM_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SAVEMEM_Click(object sender, RoutedEventArgs e)
        {
        }

        private void WRITE_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
