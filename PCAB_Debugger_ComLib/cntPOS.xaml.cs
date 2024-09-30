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
                    int day = (int)Math.Truncate(_data.TIME / 24 / 60 / 60);
                    int hh = (int)Math.Truncate((_data.TIME - (day * 24 * 60 * 60)) / 60 / 60);
                    int mm = (int)Math.Truncate((_data.TIME - (day * 24 * 60 * 60) - (hh * 60 * 60)) / 60);
                    double ss = _data.TIME - day * 24 * 60 * 60 - hh * 60 * 60 - mm * 60;
                    TIME_LABEL.Content = day.ToString() + "day" +  hh.ToString("00") + ":" + mm.ToString("00")+ ":" + ss.ToString("00.000000");
                    ROLL_LABEL.Content = _data.ROLL.ToString("0.00").PadLeft(7, ' ');
                    PITCH_LABEL.Content = _data.PITCH.ToString("0.00").PadLeft(7, ' ');
                    HEADING_LABEL.Content = _data.HEADING.ToString("0.00").PadLeft(7, ' ');
                    hh = (int)Math.Truncate(_data.LATITUDE / 60 / 60);
                    mm = (int)Math.Truncate((_data.LATITUDE - (hh * 60 * 60)) / 60);
                    ss = _data.LATITUDE - hh * 60 * 60 - mm * 60;
                    LATITUDE_LABEL.Content = hh.ToString().PadLeft(3, ' ') + "°" + mm.ToString("00") + "'" + ss.ToString("00.000000") + "\"";
                    hh = (int)Math.Truncate(_data.LONGITUDE / 60 / 60);
                    mm = (int)Math.Truncate((_data.LONGITUDE - (hh * 60 * 60)) / 60);
                    ss = _data.LONGITUDE - hh * 60 * 60 - mm * 60;
                    LONGITUDE_LABEL.Content = hh.ToString().PadLeft(3, ' ') + "°" + mm.ToString("00") + "'" + ss.ToString("00.000000") + "\"";
                    ALTITUDE_LABEL.Content = _data.ALTITUDE.ToString("0.00").PadLeft(8, ' ');
                    SPEED_LABEL.Content = _data.SPEED.ToString("0.00").PadLeft(8, ' ');
                    TRACK_LABEL.Content = _data.TRACK.ToString("0.00").PadLeft(8, ' ');
                    LONG_ACCEL_LABEL.Content = _data.LONG_ACCEL.ToString("0.0000").PadLeft(8, ' ');
                    TRAN_ACCEL_LABEL.Content = _data.TRAN_ACCEL.ToString("0.0000").PadLeft(8, ' ');
                    DOWN_ACCEL_LABEL.Content = _data.DOWN_ACCEL.ToString("0.0000").PadLeft(8, ' ');
                }));
            }
        }
        public cntPOS()
        {
            InitializeComponent();
        }

    }
}
