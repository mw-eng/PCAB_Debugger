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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static PCAB_Debugger_ComLib.ShowSerialPortName;

namespace PCAB_Debugger_ComLib
{
    /// <summary>
    /// cntPOS.xaml の相互作用ロジック
    /// </summary>
    public partial class cntPOS : UserControl
    {
        private clsPOS.PAST2 _data;
        public clsPOS.PAST2 DATA
        {
            get { return _data; }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    _data = value;
                    



                }));
            }
        }
        public cntPOS()
        {
            InitializeComponent();
        }

    }
}
