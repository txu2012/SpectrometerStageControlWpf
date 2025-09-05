using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace SpectrometerStageControlWpf
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class SpectrometerChart : Window, IChartView
    {
        private DispatcherTimer tmrUpdate;
        private MainPresenter MainPresenter;
        private ObservableCollection<ObservablePoint> data = new ObservableCollection<ObservablePoint>();
        public object Sync { get; } = new object();
        private bool range;

        public SpectrometerChart(MainPresenter presenter, bool range = false)
        {
            InitializeComponent();
            this.MainPresenter = presenter;
            this.range = range;

            initializeChart(
                MainPresenter.SpectrumData.Wavelengths.Min(),
                MainPresenter.SpectrumData.Wavelengths.Max(),
                MainPresenter.SpectrumData.Intensities.Min(),
                MainPresenter.SpectrumData.Intensities.Max());
            btnUpdate.Click += btnUpdate_Click;
            chkNormalized.Checked += chkNormalized_ToggleChecked;
            chkNormalized.Unchecked += chkNormalized_ToggleChecked;

            tmrUpdate = new DispatcherTimer();
            tmrUpdate.Tick += tmrUpdate_Tick;
            tmrUpdate.Interval = new TimeSpan(0, 0, 0, 0, 100);

            UpdateDisplay();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (tmrUpdate.IsEnabled)
                tmrUpdate.IsEnabled = false;
            else
                tmrUpdate.IsEnabled = true;
            /*await Task.Run(() => 
            { 
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Console.WriteLine($"{i}, {j}");
                        UpdateData();
                        Thread.Sleep(50);
                    }
                }
            });*/
        }

        private void chkNormalized_ToggleChecked(object sender, RoutedEventArgs e)
        {
            updateChartAxes(chkNormalized.IsChecked ?? false);
        }

        private void initializeChart(double xMin, double xMax, double yMin, double yMax)
        {
            lvcSpectrometerChart.XAxes = new Axis[]
            {
                new Axis()
                {
                    Name = "Wavelength",
                    MinLimit = xMin,
                    MaxLimit = xMax,
                    UnitWidth = 100
                }
            };

            lvcSpectrometerChart.YAxes = new Axis[]
            {
                new Axis()
                {
                    Name = "Intensity",
                    MinLimit = yMin,
                    MaxLimit = yMax,
                    UnitWidth = 1
                }
            };

            lvcSpectrometerChart.Title = new LabelVisual
            {
                Text = "Spectrometer",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15)
            };

            lvcSpectrometerChart.Series = new ISeries[]
            {
                new LineSeries<ObservablePoint>()
                {
                    Values = data,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 1 },
                    GeometryStroke = null,
                    GeometryFill = null
                }
            };

            lvcSpectrometerChart.EasingFunction = null;
            var d = new ObservablePoint(100, 222);
        }

        public void UpdateDisplay()
        {
            var points = generateChartPoints(MainPresenter.SpectrumData, chkNormalized.IsChecked ?? false);
            lock (Sync)
            {
                data.Clear();
                foreach (var point in points)
                {
                    data.Add(point);
                }
            }
        }

        private List<ObservablePoint> generateChartPoints(SpectrumData spectrumData, bool normalized)
        {
            List<ObservablePoint> dataPoints = new List<ObservablePoint>();
            var intensities = normalized ? spectrumData.IntensitiesNormalized : spectrumData.Intensities;
            var wavelengths = spectrumData.Wavelengths;
            
            for (int i = 0; i < wavelengths.Length; i++)
            {
                dataPoints.Add(new ObservablePoint(wavelengths[i], intensities[i]));
            }

            return dataPoints;
        }

        private void updateChartAxes(bool normalize)
        {
            if (normalize)
            {
                lvcSpectrometerChart.YAxes = new Axis[]
                {
                    new Axis()
                    {
                        Name = "Intensity",
                        MinLimit = 0,
                        MaxLimit = 1,
                        UnitWidth = 0.10
                    }
                };
            }
            else
            {
                double min = MainPresenter.SpectrumData.Intensities.Min();
                double max = MainPresenter.SpectrumData.Intensities.Max();
                double interval = (max - min) / 10;

                lvcSpectrometerChart.YAxes = new Axis[]
                {
                    new Axis()
                    {
                        Name = "Intensity",
                        MinLimit = min,
                        MaxLimit = max,
                        UnitWidth = interval
                    }
                };
            }
            UpdateDisplay();
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (range)
                MainPresenter.GetSpectrumAtRange();
            else
                MainPresenter.GetFullSpectrum();

            UpdateDisplay();
        }

        /*private int index = 0;
        private List<ObservablePoint[]> testData = new List<ObservablePoint[]>()
        {
            new ObservablePoint[]
            {
                new ObservablePoint(100, 222),
                new ObservablePoint(200, 333),
                new ObservablePoint(300, 444),
                new ObservablePoint(400, 555),
                new ObservablePoint(500, 777)
            },
            new ObservablePoint[]
            {
                new ObservablePoint(100, 777),
                new ObservablePoint(200, 555),
                new ObservablePoint(300, 444),
                new ObservablePoint(400, 333),
                new ObservablePoint(500, 222)
            },
            new ObservablePoint[]
            {
                new ObservablePoint(100, 222),
                new ObservablePoint(200, 777),
                new ObservablePoint(300, 333),
                new ObservablePoint(400, 555),
                new ObservablePoint(500, 444)
            }
        };
        */
    }

    /*public class ViewModel
    {
        public ISeries[] Series { get; set; } =
        {
            new LineSeries<ObservablePoint>
            {
                Values = new ObservablePoint[]
                {
                    new ObservablePoint(100, 222),
                    new ObservablePoint(200, 333),
                    new ObservablePoint(300, 444),
                    new ObservablePoint(400, 555),
                    new ObservablePoint(500, 666)
                }
            }
        };

        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis()
            {
                MinLimit = 40,
                MaxLimit = 900,
                UnitWidth = 100
            }
        };

        public Axis[] YAxes { get; set; } = new Axis[]
        {
            new Axis()
            {
                MinLimit = 100,
                MaxLimit = 900,
                UnitWidth = 100
            }
        };

        public LabelVisual Title = new LabelVisual()
        {
            Text = "My chart title",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };
    }*/
}
