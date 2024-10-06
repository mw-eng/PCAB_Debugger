using PCAB_Debugger_ACS.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace PCAB_Debugger_ACS
{
    /// <summary>
    /// winPCAB_PhaseMonitor.xaml の相互作用ロジック
    /// </summary>
    public partial class winPCAB_PhaseMonitor : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        private const int SC_CLOSE = 0xf060;
        private const int MF_BYCOMMAND = 0x0000;

        public bool closeFLG = false;
        public winPCAB_PhaseMonitor()
        {
            InitializeComponent();
            if (Settings.Default.winPhaseMonitorTop >= SystemParameters.VirtualScreenTop &&
                (Settings.Default.winPhaseMonitorTop + Settings.Default.winPhaseMonitorHeight) <
                SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
            {
                this.Top = Settings.Default.winPhaseMonitorTop;
            }
            if (Settings.Default.winPhaseMonitorLeft >= SystemParameters.VirtualScreenLeft &&
                (Settings.Default.winPhaseMonitorLeft + Settings.Default.winPhaseMonitorWidth) <
                SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
            {
                this.Left = Settings.Default.winPhaseMonitorLeft;
            }
            if (Settings.Default.winPhaseMonitorWidth > 0 &&
                Settings.Default.winPhaseMonitorWidth < SystemParameters.WorkArea.Width)
            {
                this.Width = Settings.Default.winPhaseMonitorWidth;
            }
            if (Settings.Default.winPhaseMonitorHeight > 0 &&
                Settings.Default.winPhaseMonitorHeight < SystemParameters.WorkArea.Height)
            {
                this.Height = Settings.Default.winPhaseMonitorHeight;
            }
            if (Settings.Default.winPhaseMonitorMaximized)
            { Loaded += (o, e) => this.WindowState = WindowState.Maximized; }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper((Window)sender).Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, false);
            RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!closeFLG) { e.Cancel = true; }
        }

        public void WindowClose()
        {
            Settings.Default.winPhaseMonitorTop = this.Top;
            Settings.Default.winPhaseMonitorLeft = this.Left;
            Settings.Default.winPhaseMonitorHeight = this.Height;
            Settings.Default.winPhaseMonitorWidth = this.Width;
            Settings.Default.winPhaseMonitorMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            closeFLG = true;
            Close();
        }

    }
}
