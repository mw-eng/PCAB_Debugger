using PCAB_Debugger_ACS.Properties;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace PCAB_Debugger_ACS
{
    /// <summary>
    /// winPCAB_SensorMonitor.xaml の相互作用ロジック
    /// </summary>
    public partial class winPCAB_SensorMonitor : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        private const int SC_CLOSE = 0xf060;
        private const int MF_BYCOMMAND = 0x0000;

        public bool closeFLG = false;
        public winPCAB_SensorMonitor()
        {
            InitializeComponent();
            if (Settings.Default.winSnsMonitorTop >= SystemParameters.VirtualScreenTop &&
                (Settings.Default.winSnsMonitorTop + Settings.Default.winSnsMonitorHeight) <
                SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
            {
                this.Top = Settings.Default.winSnsMonitorTop;
            }
            if (Settings.Default.winSnsMonitorLeft >= SystemParameters.VirtualScreenLeft &&
                (Settings.Default.winSnsMonitorLeft + Settings.Default.winSnsMonitorWidth) <
                SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
            {
                this.Left = Settings.Default.winSnsMonitorLeft;
            }
            if (Settings.Default.winSnsMonitorWidth > 0 &&
                Settings.Default.winSnsMonitorWidth < SystemParameters.WorkArea.Width)
            {
                this.Width = Settings.Default.winSnsMonitorWidth;
            }
            if (Settings.Default.winSnsMonitorHeight > 0 &&
                Settings.Default.winSnsMonitorHeight < SystemParameters.WorkArea.Height)
            {
                this.Height = Settings.Default.winSnsMonitorHeight;
            }
            if (Settings.Default.winSnsMonitorMaximized)
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
            Settings.Default.winSnsMonitorTop = this.Top;
            Settings.Default.winSnsMonitorLeft = this.Left;
            Settings.Default.winSnsMonitorHeight = this.Height;
            Settings.Default.winSnsMonitorWidth = this.Width;
            Settings.Default.winSnsMonitorMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            closeFLG = true;
            Close();
        }



    }
}
