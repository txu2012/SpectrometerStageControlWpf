using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;

namespace SpectrometerStageControlWpf
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class SpectrometerView : Window, IChartView
    {
        public bool enableDebug = false;

        private DispatcherTimer tmrUpdate;
        private MainPresenter MainPresenter;
        private ObservableCollection<ObservablePoint> data = new ObservableCollection<ObservablePoint>();

        public object Sync { get; } = new object();
        private bool range;
        private SpectrometerModel viewModel;

        public SpectrometerView(MainPresenter presenter, bool range = false)
        {
            InitializeComponent();
            this.MainPresenter = presenter;
            this.range = range;
            MainPresenter.AddChartView(this);

            viewModel = new SpectrometerModel(this.MainPresenter);

            if (enableDebug && (chkTestData.IsChecked ?? false))
                viewModel.InitializeChart(0, 800, 0, 800);
            else
                viewModel.InitializeChart(
                    MainPresenter.SpectrumData.Wavelengths.Min(),
                    MainPresenter.SpectrumData.Wavelengths.Max(),
                    MainPresenter.SpectrumData.Intensities.Min(),
                    MainPresenter.SpectrumData.Intensities.Max());

            lvcSpectrometerChart.DataContext = viewModel;

            btnAcquire.Click += btnAcquire_Click;
            chkNormalized.Checked += chkNormalized_ToggleChecked;
            chkNormalized.Unchecked += chkNormalized_ToggleChecked;
            chkTestData.Checked += chkTestData_ToggleChecked;
            chkTestData.Unchecked += chkTestData_ToggleChecked;

            tmrUpdate = new DispatcherTimer();
            tmrUpdate.Tick += tmrUpdate_Tick;
            tmrUpdate.Interval = new TimeSpan(0, 0, 0, 0, 100);

            UpdateDisplay();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainPresenter.RemoveChartView(this);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                enableDebug = !enableDebug;
                UpdateDisplay();
            }
        }

        private void btnAcquire_Click(object sender, RoutedEventArgs e)
        {
            if (tmrUpdate.IsEnabled)
                tmrUpdate.IsEnabled = false;
            else
                tmrUpdate.IsEnabled = true;
        }

        private void chkNormalized_ToggleChecked(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateChartAxes(MainPresenter.SpectrumData, chkNormalized.IsChecked ?? false);
        }

        public void UpdateDisplay()
        {
            viewModel.UpdateData(chkNormalized.IsChecked ?? false);

            if (enableDebug)
            {
                chkTestData.Visibility = Visibility.Visible;
                chkTestData.IsEnabled = true;
            }
            else
            {
                chkTestData.Visibility = Visibility.Hidden;
                chkTestData.IsEnabled = false;
            }

            btnAcquire.IsEnabled = MainPresenter.SpectrometerConnected || (enableDebug && (chkTestData.IsChecked ?? false));
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (enableDebug && (chkTestData.IsChecked ?? false))
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

        private void chkTestData_ToggleChecked(object sender, RoutedEventArgs e)
        {
            if (enableDebug && (chkTestData.IsChecked ?? false))
            {
                viewModel.UpdateChartAxes(0, 800, 100, 0, 800, 100);
                UpdateData();
            }
            else
            {
                viewModel.UpdateChartAxes(MainPresenter.SpectrumData, chkNormalized.IsChecked ?? false);
            }

            btnAcquire.IsEnabled = MainPresenter.SpectrometerConnected || (enableDebug && (chkTestData.IsChecked ?? false));
        }
        #endregion

        
    }
}
