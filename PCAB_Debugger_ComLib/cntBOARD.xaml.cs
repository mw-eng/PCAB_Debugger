using System.Windows.Controls;
using static PCAB_Debugger_ComLib.cntConfigPorts;

namespace PCAB_Debugger_ComLib
{
    /// <summary>
    /// cntBOARD.xaml の相互作用ロジック
    /// </summary>
    public partial class cntBOARD : UserControl
    {
        public string SerialNumber { get;private set; }
        public cntConfig CONFIG { get; private set; }
        public cntAUTO AUTO { get; private set; }
        public cntBOARD():this("SN", 0) { }
        public cntBOARD(string _serialNumber, ROTATE _rotate)
        {
            InitializeComponent();
            TAB_CONTROL.Items.Clear();
            CONFIG = new cntConfig(_serialNumber, _rotate);
            AUTO = new cntAUTO(_serialNumber);
            SerialNumber = _serialNumber;
            TabItem control = new TabItem();
            TabItem auto = new TabItem();
            control.Header = "CONTROL";
            auto.Header = "ATUO";
            control.Name = "CONFIG";
            auto.Name = "CONFIG";
            control.Content = CONFIG;
            auto.Content = AUTO;
            TAB_CONTROL.Items.Add(control);
            TAB_CONTROL.Items.Add(auto);
        }
    }
}
