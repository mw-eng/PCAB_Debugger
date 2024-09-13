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
        public enum ButtonCategory
        {
            LOADMEM,
            SAVEMEM,
            RESET,
            WRITEDSA,
            WRITEDPS,
            WRITE,
            READ,
            NULL
        }
        public delegate void ButtonClickEventHandler(object sender, RoutedEventArgs e, ButtonCategory category);
        public event ButtonClickEventHandler ButtonClickEvent;
        public string SerialNumber { get; private set; }
        public cntConfigSettings CONFIG_SETTINGS { get; private set; }

        public cntConfig() : this("SN") { }
        public cntConfig(string SN)
        {
            InitializeComponent();
            SerialNumber = SN;
            CS_GRID.Children.Clear();
            CONFIG_SETTINGS = new cntConfigSettings(SN);
            CS_GRID.Children.Add(CONFIG_SETTINGS);

        }

        private void SAVEADDRESS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SAVEADDRESS_COMBOBOX.SelectedIndex < 0) { SAVEADDRESS_COMBOBOX.SelectedIndex = 0; }
        }

        private void LOADMEM_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(this, e, ButtonCategory.LOADMEM);
        }

        private void SAVEMEM_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(this, e, ButtonCategory.SAVEMEM);
        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(this, e, ButtonCategory.RESET);
        }

        private void WRITE_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = (Button)sender;
                switch (btn.Name)
                {
                    case "WRITEDSA":
                        ButtonClickEvent?.Invoke(this, e, ButtonCategory.WRITEDSA);
                        break;
                    case "WRITEDPS":
                        ButtonClickEvent?.Invoke(this, e, ButtonCategory.WRITEDPS);
                        break;
                    case "WRITE":
                        ButtonClickEvent?.Invoke(this, e, ButtonCategory.WRITE);
                        break;
                }
            }
        }

        private void READ_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(this, e, ButtonCategory.READ);
        }
    }
}
