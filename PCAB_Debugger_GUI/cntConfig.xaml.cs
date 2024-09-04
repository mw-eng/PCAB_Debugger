using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using static PCAB_Debugger_GUI.cntConfigSettings;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// cntConfig.xaml の相互作用ロジック
    /// </summary>
    public partial class cntConfig : UserControl
    {
        public enum ButtonCategory
        {
            LOADMEM,
            SAVEMEM,
            RESET,
            WRITEDSA,
            WRITEDPS,
            WRITE,
            NULL
        }
        public delegate void ButtonClickEventHandler(object sender, RoutedEventArgs e, ButtonCategory category);
        public event ButtonClickEventHandler ButtonClickEvent;
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
            ButtonClickEvent?.Invoke(sender, e, ButtonCategory.LOADMEM);
        }

        private void SAVEMEM_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(sender, e, ButtonCategory.SAVEMEM);
        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(sender, e, ButtonCategory.RESET);
        }

        private void WRITE_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = (Button)sender;
                switch (btn.Name)
                {
                    case "WRITEDSA":
                        ButtonClickEvent?.Invoke(sender, e, ButtonCategory.WRITEDSA);
                        break;
                    case "WRITEDPS":
                        ButtonClickEvent?.Invoke(sender, e, ButtonCategory.WRITEDPS);
                        break;
                    case "WRITE":
                        ButtonClickEvent?.Invoke(sender, e, ButtonCategory.WRITE);
                        break;
                }
            }
        }
    }
}
