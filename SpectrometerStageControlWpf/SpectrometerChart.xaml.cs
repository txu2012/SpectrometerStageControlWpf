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
using System.Windows.Shapes;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace SpectrometerStageControlWpf
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class SpectrometerChart : Window
    {
        public SpectrometerChart()
        {
            InitializeComponent();

            setChart();
        }

        private void setChart()
        {
            lvcSpectrometerChart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 111.0, 222.0, 111.0, 333.0, 555.0 }
                }
            };

            lvcSpectrometerChart.XAxes = new List<Axis>()
            {
                new Axis()
                {
                    MinLimit = 40,
                    MaxLimit = 900,
                    UnitWidth = 100
                }
            };

            lvcSpectrometerChart.Title = new LabelVisual
            {
                Text = "My chart title",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15)
            };
        }
    }

    /*
     <UserControl.DataContext>
            <vms:SpectrometerChartViewModel/>
        </UserControl.DataContext>
     */
}
