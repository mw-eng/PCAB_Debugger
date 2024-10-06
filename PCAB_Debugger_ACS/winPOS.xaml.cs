using PCAB_Debugger_ACS.Properties;
using PCAB_Debugger_ComLib;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using static PCAB_Debugger_ComLib.POS;

namespace PCAB_Debugger_ACS
{
    /// <summary>
    /// winPOS.xaml の相互作用ロジック
    /// </summary>
    public partial class winPOS : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        private const int SC_CLOSE = 0xf060;
        private const int MF_BYCOMMAND = 0x0000;

        public PAST2 DATA { get { return POS_VIEWER.DATA; } }

        public winPOS()
        {
            InitializeComponent();
            if (Settings.Default.winPosMonitorTop >= SystemParameters.VirtualScreenTop &&
                (Settings.Default.winPosMonitorTop + Settings.Default.winPosMonitorHeight) <
                SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
            {
                this.Top = Settings.Default.winPosMonitorTop;
            }
            if (Settings.Default.winPosMonitorLeft >= SystemParameters.VirtualScreenLeft &&
                (Settings.Default.winPosMonitorLeft + Settings.Default.winPosMonitorWidth) <
                SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
            {
                this.Left = Settings.Default.winPosMonitorLeft;
            }
            if (Settings.Default.winPosMonitorWidth > 0 &&
                Settings.Default.winPosMonitorWidth < SystemParameters.WorkArea.Width)
            {
                this.Width = Settings.Default.winPosMonitorWidth;
            }
            if (Settings.Default.winPosMonitorHeight > 0 &&
                Settings.Default.winPosMonitorHeight < SystemParameters.WorkArea.Height)
            {
                this.Height = Settings.Default.winPosMonitorHeight;
            }
            if (Settings.Default.winPosMonitorMaximized)
            { Loaded += (o, e) => this.WindowState = WindowState.Maximized; }
            startTIME = DateTime.Now;
        }

        private DateTime startTIME = new DateTime();
        private bool closeFLG = false;
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
            Settings.Default.winPosMonitorTop = this.Top;
            Settings.Default.winPosMonitorLeft = this.Left;
            Settings.Default.winPosMonitorHeight = this.Height;
            Settings.Default.winPosMonitorWidth = this.Width;
            Settings.Default.winPosMonitorMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            closeFLG = true;
            Close();
        }

        public void OnReadDAT(object sender, POSEventArgs e)
        {
            try
            {
                POS.PAST2 dat = new PAST2(e.ReceiveDAT);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    POS_VIEWER.DATA = dat;
                }));
                //if (DateTime.Now - startTIME > new TimeSpan(100 * 1000))
                //{
                //    startTIME = DateTime.Now;
                //    Dispatcher.BeginInvoke(new Action(() =>
                //    {
                //        POS_VIEWER.DATA = dat;
                //    }));
                //}
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    POS_VIEWER.DATA = null;
                }));
                //Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    LOG_TEXTBOX.Text += ex.Message + " > " + BitConverter.ToString(e.ReceiveDAT.ToArray()) + "\n";
                //    LOG_TEXTBOX.ScrollToEnd();
                //}));
            }
        }
    }
}
