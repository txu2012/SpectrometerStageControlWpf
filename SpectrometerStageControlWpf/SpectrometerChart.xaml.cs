using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Specialized;

namespace SpectrometerStageControlWpf
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class SpectrometerChart : Window, IChartView
    {
        private const bool UseTestData = false;

        private DispatcherTimer tmrUpdate;
        private MainPresenter MainPresenter;
        private ObservableCollection<ObservablePoint> data = new ObservableCollection<ObservablePoint>();

        public object Sync { get; } = new object();
        private bool range;
        private SpectrometerViewModel viewModel;

        public SpectrometerChart(MainPresenter presenter, bool range = false)
        {
            InitializeComponent();
            this.MainPresenter = presenter;
            this.range = range;

            viewModel = new SpectrometerViewModel(this.MainPresenter);

            if (UseTestData)
                viewModel.InitializeChart(0, 800, 0, 800);
            else
                viewModel.InitializeChart(
                    MainPresenter.SpectrumData.Wavelengths.Min(),
                    MainPresenter.SpectrumData.Wavelengths.Max(),
                    MainPresenter.SpectrumData.Intensities.Min(),
                    MainPresenter.SpectrumData.Intensities.Max());

            lvcSpectrometerChart.DataContext = viewModel;

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
        }

        private void chkNormalized_ToggleChecked(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateChartAxes(MainPresenter.SpectrumData, chkNormalized.IsChecked?? false);
        }

        public void UpdateDisplay()
        {
            viewModel.UpdateData(chkNormalized.IsChecked ?? false);
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (UseTestData)
            {
                UpdateData();
            }
            else
            {
                if (MainPresenter.SpectrometerConnected)
                {
                    if (range)
                        MainPresenter.GetSpectrumAtRange();
                    else
                        MainPresenter.GetFullSpectrum();

                    UpdateDisplay();
                }
            }
        }

        #region Debug Test
        private int index = 0;
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

        public void UpdateData()
        {
            viewModel.SetData(testData[index]);

            if (index == testData.Count - 1)
                index = 0;
            else
                index++;
        }
        #endregion
    }
}
