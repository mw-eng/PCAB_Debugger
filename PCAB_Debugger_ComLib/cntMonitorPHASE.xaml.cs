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
using static PCAB_Debugger_ComLib.cntConfigSettings;

namespace PCAB_Debugger_ComLib
{
    /// <summary>
    /// cntMonitorPHASE.xaml の相互作用ロジック
    /// </summary>
    public partial class cntMonitorPHASE : UserControl
    {
        private ROTATE angle = ROTATE.MATRIX;
        private ColorChart colorChart = new ColorChart(-180, 180);
        private List<Grid> ports = new List<Grid>();

        public cntMonitorPHASE()
        {
            InitializeComponent();
            GRID_MAIN.Children.Clear();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j ++)
                {
                    Grid gridBF = new Grid();
                    gridBF.SetValue(Grid.RowProperty, i);
                    gridBF.SetValue(Grid.ColumnProperty, j);
                    gridBF.Background = new SolidColorBrush(colorChart.getColor(0));
                    gridBF.Children.Clear();
                    gridBF.Children.Add(new Viewbox());
                    gridBF.Children.Add(new Viewbox());
                    gridBF.Children[0].SetValue(Grid.RowProperty, 0);
                    gridBF.Children[1].SetValue(Grid.RowProperty, 1);
                    ((Viewbox)(gridBF.Children[0])).Child = new Label();
                    ((Viewbox)(gridBF.Children[0])).Child = new Label();
                    ((Label)((Viewbox)(gridBF.Children[0])).Child).Content = "Port " + (i * 4 + j + 1).ToString().PadLeft(2);
                    ((Label)((Viewbox)(gridBF.Children[1])).Child).Content = "   0.000 deg";
                }
            }
        }
    }
}
