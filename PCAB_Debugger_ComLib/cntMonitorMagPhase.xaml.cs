using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static PCAB_Debugger_ComLib.cntConfigPorts;

namespace PCAB_Debugger_ComLib
{
    /// <summary>
    /// cntMonitorMagPhase.xaml の相互作用ロジック
    /// </summary>
    public partial class cntMonitorMagPhase : UserControl
    {
        private ROTATE angle = ROTATE.MATRIX;
        private NormalizedColorChart colorChart = new NormalizedColorChart(-180, 180);
        private List<Grid> ports = new List<Grid>();
        private List<float> _values = new List<float>();
        private List<float> _offset = new List<float>();
        private List<float> _offsetVal = new List<float>();
        public double ChartMINvalue { get { return colorChart.MinValue; } set { colorChart.MinValue = value; ReloadView(); } }
        public double ChartMAXvalue { get { return colorChart.MaxValue; } set { colorChart.MaxValue = value; ReloadView(); } }

        public List<float> VALUEs
        {
            get { return _values; }
            set
            {
                if(_values.Count - 1 == value.Count) {
                    for (int i = 1; i < _values.Count; i++) { _values[i] = value[i-1]; }
                    ReloadView(); }
            }
        }

        public List<float> OFFSETs
        {
            get { return _offset; }
            set
            {

                if (_offset.Count - 1 == value.Count)
                {
                    for (int i = 1; i < _values.Count; i++) { _offset[i] = value[i - 1]; }
                    ReloadView();
                }
            }
        }

        public List<float> OFFSET_VALUEs { get { return _offsetVal; } }

        public void SetOffsetNow()
        {
            _offset = new List<float>(_values);
            ReloadView();
        }

        private void ReloadView()
        {
            for (int i = 1; i < ports.Count; i++)
            {
                _offsetVal[i] = _values[i] - _offset[i];
                _offsetVal[i] = NormalizedValue(_offsetVal[i]);
                ports[i].Background = new SolidColorBrush(colorChart.getColor(_offsetVal[i]));
                ((Label)((Viewbox)(ports[i].Children[1])).Child).Content = _offsetVal[i].ToString("0.000");
            }
        }

        public ROTATE TURN
        {
            get { return angle; }
            set
            {
                angle = value;
                switch (angle)
                {
                    case ROTATE.RIGHT_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 3);
                        P02_GRID.SetValue(Grid.RowProperty, 3);
                        P03_GRID.SetValue(Grid.RowProperty, 3);
                        P04_GRID.SetValue(Grid.RowProperty, 3);
                        P05_GRID.SetValue(Grid.RowProperty, 2);
                        P06_GRID.SetValue(Grid.RowProperty, 2);
                        P07_GRID.SetValue(Grid.RowProperty, 2);
                        P08_GRID.SetValue(Grid.RowProperty, 2);
                        P09_GRID.SetValue(Grid.RowProperty, 1);
                        P10_GRID.SetValue(Grid.RowProperty, 1);
                        P11_GRID.SetValue(Grid.RowProperty, 1);
                        P12_GRID.SetValue(Grid.RowProperty, 1);
                        P13_GRID.SetValue(Grid.RowProperty, 0);
                        P14_GRID.SetValue(Grid.RowProperty, 0);
                        P15_GRID.SetValue(Grid.RowProperty, 0);
                        P16_GRID.SetValue(Grid.RowProperty, 0);
                        P01_GRID.SetValue(Grid.ColumnProperty, 0);
                        P02_GRID.SetValue(Grid.ColumnProperty, 1);
                        P03_GRID.SetValue(Grid.ColumnProperty, 2);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 3);
                        P06_GRID.SetValue(Grid.ColumnProperty, 2);
                        P07_GRID.SetValue(Grid.ColumnProperty, 1);
                        P08_GRID.SetValue(Grid.ColumnProperty, 0);
                        P09_GRID.SetValue(Grid.ColumnProperty, 0);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 2);
                        P12_GRID.SetValue(Grid.ColumnProperty, 3);
                        P13_GRID.SetValue(Grid.ColumnProperty, 3);
                        P14_GRID.SetValue(Grid.ColumnProperty, 2);
                        P15_GRID.SetValue(Grid.ColumnProperty, 1);
                        P16_GRID.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case ROTATE.LEFT_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 0);
                        P03_GRID.SetValue(Grid.RowProperty, 0);
                        P04_GRID.SetValue(Grid.RowProperty, 0);
                        P05_GRID.SetValue(Grid.RowProperty, 1);
                        P06_GRID.SetValue(Grid.RowProperty, 1);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 1);
                        P09_GRID.SetValue(Grid.RowProperty, 2);
                        P10_GRID.SetValue(Grid.RowProperty, 2);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 2);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 3);
                        P15_GRID.SetValue(Grid.RowProperty, 3);
                        P16_GRID.SetValue(Grid.RowProperty, 3);
                        P01_GRID.SetValue(Grid.ColumnProperty, 3);
                        P02_GRID.SetValue(Grid.ColumnProperty, 2);
                        P03_GRID.SetValue(Grid.ColumnProperty, 1);
                        P04_GRID.SetValue(Grid.ColumnProperty, 0);
                        P05_GRID.SetValue(Grid.ColumnProperty, 0);
                        P06_GRID.SetValue(Grid.ColumnProperty, 1);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 3);
                        P09_GRID.SetValue(Grid.ColumnProperty, 3);
                        P10_GRID.SetValue(Grid.ColumnProperty, 2);
                        P11_GRID.SetValue(Grid.ColumnProperty, 1);
                        P12_GRID.SetValue(Grid.ColumnProperty, 0);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 1);
                        P15_GRID.SetValue(Grid.ColumnProperty, 2);
                        P16_GRID.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.HALF_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 1);
                        P03_GRID.SetValue(Grid.RowProperty, 2);
                        P04_GRID.SetValue(Grid.RowProperty, 3);
                        P05_GRID.SetValue(Grid.RowProperty, 3);
                        P06_GRID.SetValue(Grid.RowProperty, 2);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 0);
                        P09_GRID.SetValue(Grid.RowProperty, 0);
                        P10_GRID.SetValue(Grid.RowProperty, 1);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 3);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 2);
                        P15_GRID.SetValue(Grid.RowProperty, 1);
                        P16_GRID.SetValue(Grid.RowProperty, 0);
                        P01_GRID.SetValue(Grid.ColumnProperty, 0);
                        P02_GRID.SetValue(Grid.ColumnProperty, 0);
                        P03_GRID.SetValue(Grid.ColumnProperty, 0);
                        P04_GRID.SetValue(Grid.ColumnProperty, 0);
                        P05_GRID.SetValue(Grid.ColumnProperty, 1);
                        P06_GRID.SetValue(Grid.ColumnProperty, 1);
                        P07_GRID.SetValue(Grid.ColumnProperty, 1);
                        P08_GRID.SetValue(Grid.ColumnProperty, 1);
                        P09_GRID.SetValue(Grid.ColumnProperty, 2);
                        P10_GRID.SetValue(Grid.ColumnProperty, 2);
                        P11_GRID.SetValue(Grid.ColumnProperty, 2);
                        P12_GRID.SetValue(Grid.ColumnProperty, 2);
                        P13_GRID.SetValue(Grid.ColumnProperty, 3);
                        P14_GRID.SetValue(Grid.ColumnProperty, 3);
                        P15_GRID.SetValue(Grid.ColumnProperty, 3);
                        P16_GRID.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.MIRROR_ZERO:
                        P01_GRID.SetValue(Grid.RowProperty, 3);
                        P02_GRID.SetValue(Grid.RowProperty, 2);
                        P03_GRID.SetValue(Grid.RowProperty, 1);
                        P04_GRID.SetValue(Grid.RowProperty, 0);
                        P05_GRID.SetValue(Grid.RowProperty, 0);
                        P06_GRID.SetValue(Grid.RowProperty, 1);
                        P07_GRID.SetValue(Grid.RowProperty, 2);
                        P08_GRID.SetValue(Grid.RowProperty, 3);
                        P09_GRID.SetValue(Grid.RowProperty, 3);
                        P10_GRID.SetValue(Grid.RowProperty, 2);
                        P11_GRID.SetValue(Grid.RowProperty, 1);
                        P12_GRID.SetValue(Grid.RowProperty, 0);
                        P13_GRID.SetValue(Grid.RowProperty, 0);
                        P14_GRID.SetValue(Grid.RowProperty, 1);
                        P15_GRID.SetValue(Grid.RowProperty, 2);
                        P16_GRID.SetValue(Grid.RowProperty, 3);
                        P01_GRID.SetValue(Grid.ColumnProperty, 0);
                        P02_GRID.SetValue(Grid.ColumnProperty, 0);
                        P03_GRID.SetValue(Grid.ColumnProperty, 0);
                        P04_GRID.SetValue(Grid.ColumnProperty, 0);
                        P05_GRID.SetValue(Grid.ColumnProperty, 1);
                        P06_GRID.SetValue(Grid.ColumnProperty, 1);
                        P07_GRID.SetValue(Grid.ColumnProperty, 1);
                        P08_GRID.SetValue(Grid.ColumnProperty, 1);
                        P09_GRID.SetValue(Grid.ColumnProperty, 2);
                        P10_GRID.SetValue(Grid.ColumnProperty, 2);
                        P11_GRID.SetValue(Grid.ColumnProperty, 2);
                        P12_GRID.SetValue(Grid.ColumnProperty, 2);
                        P13_GRID.SetValue(Grid.ColumnProperty, 3);
                        P14_GRID.SetValue(Grid.ColumnProperty, 3);
                        P15_GRID.SetValue(Grid.ColumnProperty, 3);
                        P16_GRID.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.MIRROR_RIGHT_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 3);
                        P02_GRID.SetValue(Grid.RowProperty, 3);
                        P03_GRID.SetValue(Grid.RowProperty, 3);
                        P04_GRID.SetValue(Grid.RowProperty, 3);
                        P05_GRID.SetValue(Grid.RowProperty, 2);
                        P06_GRID.SetValue(Grid.RowProperty, 2);
                        P07_GRID.SetValue(Grid.RowProperty, 2);
                        P08_GRID.SetValue(Grid.RowProperty, 2);
                        P09_GRID.SetValue(Grid.RowProperty, 1);
                        P10_GRID.SetValue(Grid.RowProperty, 1);
                        P11_GRID.SetValue(Grid.RowProperty, 1);
                        P12_GRID.SetValue(Grid.RowProperty, 1);
                        P13_GRID.SetValue(Grid.RowProperty, 0);
                        P14_GRID.SetValue(Grid.RowProperty, 0);
                        P15_GRID.SetValue(Grid.RowProperty, 0);
                        P16_GRID.SetValue(Grid.RowProperty, 0);
                        P01_GRID.SetValue(Grid.ColumnProperty, 3);
                        P02_GRID.SetValue(Grid.ColumnProperty, 2);
                        P03_GRID.SetValue(Grid.ColumnProperty, 1);
                        P04_GRID.SetValue(Grid.ColumnProperty, 0);
                        P05_GRID.SetValue(Grid.ColumnProperty, 0);
                        P06_GRID.SetValue(Grid.ColumnProperty, 1);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 3);
                        P09_GRID.SetValue(Grid.ColumnProperty, 3);
                        P10_GRID.SetValue(Grid.ColumnProperty, 2);
                        P11_GRID.SetValue(Grid.ColumnProperty, 1);
                        P12_GRID.SetValue(Grid.ColumnProperty, 0);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 1);
                        P15_GRID.SetValue(Grid.ColumnProperty, 2);
                        P16_GRID.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.MIRROR_LEFT_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 0);
                        P03_GRID.SetValue(Grid.RowProperty, 0);
                        P04_GRID.SetValue(Grid.RowProperty, 0);
                        P05_GRID.SetValue(Grid.RowProperty, 1);
                        P06_GRID.SetValue(Grid.RowProperty, 1);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 1);
                        P09_GRID.SetValue(Grid.RowProperty, 2);
                        P10_GRID.SetValue(Grid.RowProperty, 2);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 2);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 3);
                        P15_GRID.SetValue(Grid.RowProperty, 3);
                        P16_GRID.SetValue(Grid.RowProperty, 3);
                        P01_GRID.SetValue(Grid.ColumnProperty, 0);
                        P02_GRID.SetValue(Grid.ColumnProperty, 1);
                        P03_GRID.SetValue(Grid.ColumnProperty, 2);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 3);
                        P06_GRID.SetValue(Grid.ColumnProperty, 2);
                        P07_GRID.SetValue(Grid.ColumnProperty, 1);
                        P08_GRID.SetValue(Grid.ColumnProperty, 0);
                        P09_GRID.SetValue(Grid.ColumnProperty, 0);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 2);
                        P12_GRID.SetValue(Grid.ColumnProperty, 3);
                        P13_GRID.SetValue(Grid.ColumnProperty, 3);
                        P14_GRID.SetValue(Grid.ColumnProperty, 2);
                        P15_GRID.SetValue(Grid.ColumnProperty, 1);
                        P16_GRID.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case ROTATE.MIRROR_HALF_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 1);
                        P03_GRID.SetValue(Grid.RowProperty, 2);
                        P04_GRID.SetValue(Grid.RowProperty, 3);
                        P05_GRID.SetValue(Grid.RowProperty, 3);
                        P06_GRID.SetValue(Grid.RowProperty, 2);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 0);
                        P09_GRID.SetValue(Grid.RowProperty, 0);
                        P10_GRID.SetValue(Grid.RowProperty, 1);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 3);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 2);
                        P15_GRID.SetValue(Grid.RowProperty, 1);
                        P16_GRID.SetValue(Grid.RowProperty, 0);
                        P01_GRID.SetValue(Grid.ColumnProperty, 3);
                        P02_GRID.SetValue(Grid.ColumnProperty, 3);
                        P03_GRID.SetValue(Grid.ColumnProperty, 3);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 2);
                        P06_GRID.SetValue(Grid.ColumnProperty, 2);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 2);
                        P09_GRID.SetValue(Grid.ColumnProperty, 1);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 1);
                        P12_GRID.SetValue(Grid.ColumnProperty, 1);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 0);
                        P15_GRID.SetValue(Grid.ColumnProperty, 0);
                        P16_GRID.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case ROTATE.MATRIX:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 0);
                        P03_GRID.SetValue(Grid.RowProperty, 0);
                        P04_GRID.SetValue(Grid.RowProperty, 0);
                        P05_GRID.SetValue(Grid.RowProperty, 1);
                        P06_GRID.SetValue(Grid.RowProperty, 1);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 1);
                        P09_GRID.SetValue(Grid.RowProperty, 2);
                        P10_GRID.SetValue(Grid.RowProperty, 2);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 2);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 3);
                        P15_GRID.SetValue(Grid.RowProperty, 3);
                        P16_GRID.SetValue(Grid.RowProperty, 3);
                        P01_GRID.SetValue(Grid.ColumnProperty, 0);
                        P02_GRID.SetValue(Grid.ColumnProperty, 1);
                        P03_GRID.SetValue(Grid.ColumnProperty, 2);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 0);
                        P06_GRID.SetValue(Grid.ColumnProperty, 1);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 3);
                        P09_GRID.SetValue(Grid.ColumnProperty, 0);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 2);
                        P12_GRID.SetValue(Grid.ColumnProperty, 3);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 1);
                        P15_GRID.SetValue(Grid.ColumnProperty, 2);
                        P16_GRID.SetValue(Grid.ColumnProperty, 3);
                        break;
                    default:
                        P01_GRID.SetValue(Grid.RowProperty, 3);
                        P02_GRID.SetValue(Grid.RowProperty, 2);
                        P03_GRID.SetValue(Grid.RowProperty, 1);
                        P04_GRID.SetValue(Grid.RowProperty, 0);
                        P05_GRID.SetValue(Grid.RowProperty, 0);
                        P06_GRID.SetValue(Grid.RowProperty, 1);
                        P07_GRID.SetValue(Grid.RowProperty, 2);
                        P08_GRID.SetValue(Grid.RowProperty, 3);
                        P09_GRID.SetValue(Grid.RowProperty, 3);
                        P10_GRID.SetValue(Grid.RowProperty, 2);
                        P11_GRID.SetValue(Grid.RowProperty, 1);
                        P12_GRID.SetValue(Grid.RowProperty, 0);
                        P13_GRID.SetValue(Grid.RowProperty, 0);
                        P14_GRID.SetValue(Grid.RowProperty, 1);
                        P15_GRID.SetValue(Grid.RowProperty, 2);
                        P16_GRID.SetValue(Grid.RowProperty, 3);
                        P01_GRID.SetValue(Grid.ColumnProperty, 3);
                        P02_GRID.SetValue(Grid.ColumnProperty, 3);
                        P03_GRID.SetValue(Grid.ColumnProperty, 3);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 2);
                        P06_GRID.SetValue(Grid.ColumnProperty, 2);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 2);
                        P09_GRID.SetValue(Grid.ColumnProperty, 1);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 1);
                        P12_GRID.SetValue(Grid.ColumnProperty, 1);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 0);
                        P15_GRID.SetValue(Grid.ColumnProperty, 0);
                        P16_GRID.SetValue(Grid.ColumnProperty, 0);
                        break;
                }
            }
        }

        public cntMonitorMagPhase(ROTATE _turn, NormalizedColorChart colorChart)
        {
            InitializeComponent();
            TURN = _turn;
            this.colorChart = colorChart;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Grid gridBF = new Grid();
                    gridBF.SetValue(Grid.RowProperty, i);
                    gridBF.SetValue(Grid.ColumnProperty, j);
                    gridBF.Background = new SolidColorBrush(colorChart.getColor(0));
                    gridBF.Children.Clear();
                    gridBF.RowDefinitions.Add(new RowDefinition());
                    gridBF.RowDefinitions.Add(new RowDefinition());
                    gridBF.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                    gridBF.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                    gridBF.Children.Add(new Viewbox());
                    gridBF.Children.Add(new Viewbox());
                    gridBF.Children[0].SetValue(Grid.RowProperty, 0);
                    gridBF.Children[1].SetValue(Grid.RowProperty, 1);
                    ((Viewbox)(gridBF.Children[0])).Child = new Label();
                    ((Viewbox)(gridBF.Children[1])).Child = new Label();
                    ((Label)((Viewbox)(gridBF.Children[0])).Child).Content = "Port " + (i * 4 + j).ToString("0").PadLeft(2);
                    ((Label)((Viewbox)(gridBF.Children[1])).Child).Content = "0.000";
                    ports.Add(gridBF);
                    _values.Add(0.000f);
                    _offset.Add(0.000f);
                    _offsetVal.Add(0.000f);
                }
            }
            ports[0].Children.Clear();
            ports[0].Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            P01_GRID.Children.Clear();
            P02_GRID.Children.Clear();
            P03_GRID.Children.Clear();
            P04_GRID.Children.Clear();
            P05_GRID.Children.Clear();
            P06_GRID.Children.Clear();
            P07_GRID.Children.Clear();
            P08_GRID.Children.Clear();
            P09_GRID.Children.Clear();
            P10_GRID.Children.Clear();
            P11_GRID.Children.Clear();
            P12_GRID.Children.Clear();
            P13_GRID.Children.Clear();
            P14_GRID.Children.Clear();
            P15_GRID.Children.Clear();
            P16_GRID.Children.Clear();
            P01_GRID.Children.Add(ports[1]);
            P02_GRID.Children.Add(ports[2]);
            P03_GRID.Children.Add(ports[3]);
            P04_GRID.Children.Add(ports[4]);
            P05_GRID.Children.Add(ports[5]);
            P06_GRID.Children.Add(ports[6]);
            P07_GRID.Children.Add(ports[7]);
            P08_GRID.Children.Add(ports[8]);
            P09_GRID.Children.Add(ports[9]);
            P10_GRID.Children.Add(ports[10]);
            P11_GRID.Children.Add(ports[11]);
            P12_GRID.Children.Add(ports[12]);
            P13_GRID.Children.Add(ports[13]);
            P14_GRID.Children.Add(ports[14]);
            P15_GRID.Children.Add(ports[15]);
            P16_GRID.Children.Add(ports[0]);
        }

        private float NormalizedValue(float value)
        {
            float norm = (value + (float)colorChart.MaxValue) % (float)(colorChart.MaxValue - colorChart.MinValue) + (float)colorChart.MinValue;
            if (norm < (float)colorChart.MinValue) { norm += (float)(colorChart.MaxValue - colorChart.MinValue); }
            return norm;
        }
    }
}
