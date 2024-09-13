using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// winMonitor.xaml の相互作用ロジック
    /// </summary>
    public partial class winMonitor : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        private const int SC_CLOSE = 0xf060;
        private const int MF_BYCOMMAND = 0x0000;

        public bool closeFLG = false;
        public winMonitor()
        {
            InitializeComponent();
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
            closeFLG = true;
            Close();
        }
    }
}
