using System.Windows.Controls;

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// cntBOARD.xaml の相互作用ロジック
    /// </summary>
    public partial class cntBOARD : UserControl
    {
        public string SerialNumber { get;private set; }
        public cntConfig CONFIG { get; private set; }
        public cntAUTO AUTO { get; private set; }
        public cntBOARD():this("SN") { }
        public cntBOARD(string SN)
        {
            InitializeComponent();
            TAB_CONTROL.Items.Clear();
            CONFIG = new cntConfig(SN);
            AUTO = new cntAUTO(SN);
            SerialNumber = SN;
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
