using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using OpenControls.Wpf.SurfacePlot.Model;

namespace SpectrometerStageControlWpf
{
    /// <summary>
    /// Interaction logic for SurfacePlotterView.xaml
    /// </summary>
    public partial class SurfacePlotterView : Window, ILabelFormatter, ISurfacePlotView
    {
        private const bool useTestData = false;
        private const bool useTestData2 = false;

        private SurfacePlotterModel viewModel;
        private MainPresenter presenter;
        public SurfacePlotterView(MainPresenter presenter)
        {
            InitializeComponent();

            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;
            btnUpdate.Click += btnUpdate_Click;
            this.presenter = presenter;

            if (useTestData)
                presenter.FrogDataManager.GenerateTestPlotPoints();

            disableControls(useTestData || useTestData2);
        }

        private void disableControls(bool toggle)
        {
            if (!toggle)
            {
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = false;
                btnUpdate.IsEnabled = false;
                cbSpeed.IsEnabled = false;

                btnStart.Visibility = Visibility.Hidden;
                btnStop.Visibility = Visibility.Hidden;
                btnUpdate.Visibility = Visibility.Hidden;
                lblSpeed.Visibility = Visibility.Hidden;
                cbSpeed.Visibility = Visibility.Hidden;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as SurfacePlotterModel).Save();
            Stop();

            base.OnClosing(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = new SurfacePlotterModel();
            DataContext = viewModel;

            viewModel.Load();
            _configurationControl.DataContext = new OpenControls.Wpf.SurfacePlot.ViewModel.ConfigurationControlViewModel(viewModel.IConfiguration);
            _surfacePlotControl.Initialise(viewModel.IConfiguration);
        }

        public void UpdateDisplay()
        {
            SetData();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (useTestData)
            {
                presenter.FrogDataManager.AppendRandomSpectrumData();
            }
            UpdateDisplay();
        }

        private void SetData()
        {
            var frogData = presenter.FrogDataManager.FrogData;
            if (frogData.Count <= 0) return;
            
            int XCount = frogData.Count;
            int YCount = frogData[0].SpectrumData.Wavelengths.Length;
            
            float xMax = frogData.Count;
            float xMin = 0;
            float yMax = (float)frogData[0].SpectrumData.Wavelengths.Max();
            float yMin = (float)frogData[0].SpectrumData.Wavelengths.Min();
            float zMax = (float)frogData[0].SpectrumData.Intensities.Max();
            float zMin = (float)frogData[0].SpectrumData.Intensities.Min();

            List<List<float>> srcData = new List<List<float>>();
            for (int i = 0; i < XCount; ++i)
            {
                var data = frogData[i].SpectrumData.Intensities;
                List<float> list = new List<float>();
                srcData.Add(list);
                for (int j = 0; j < YCount; ++j)
                {
                    list.Add((float)data[j]);
                }
            }

            _surfacePlotControl.SetData(srcData, xMin, xMax, XCount+1, yMin, yMax, 10, zMin, zMax, 10);
        }
                
        public void Start()
        {
            (DataContext as SurfacePlotterModel).IsRunning = true;

            if (useTestData2 && !useTestData)
            {
                runTestModel();
            }
            else
            {
                viewModel = (DataContext as SurfacePlotterModel);
                _surfacePlotControl.ILabelFormatter = this;
                _surfacePlotControl.XAxisTitle = "Delay fs (X)";
                _surfacePlotControl.YAxisTitle = "Wavelength (Y)";
                _surfacePlotControl.ZAxisTitle = "Intensity (Z)";
            
                UpdateDisplay();
            }
        }

        private void Stop()
        {
            (DataContext as SurfacePlotterModel).IsRunning = false;
            if (useTestData2)
                _task = null;
        }

        #region OpenControls.Wpf.SurfacePlot.Model.ILabelFormatter

        public string XLabel(float x)
        {
            return x.ToString("F1");
        }

        public string YLabel(float y)
        {
            return y.ToString("F1");
        }

        public string ZLabel(float z)
        {
            return z.ToString("E2");
        }

        #endregion OpenControls.Wpf.SurfacePlot.Model.ILabelFormatter

        #region Test Functions
        private void runTestModel2()
        {
            viewModel = (DataContext as SurfacePlotterModel);
            _surfacePlotControl.ILabelFormatter = this;

            /*const int XCount = 20;
            const int YCount = 10;
            float zMax = 1;
            float zMin = -1;
            float scale = 2f * (float)System.Math.PI / (float)XCount;

            Random random = new Random();

            List<List<float>> srcData = new List<List<float>>();
            for (int i = 0; i < XCount; ++i)
            {
                List<float> list = new List<float>();
                srcData.Add(list);
                for (int j = 0; j < YCount; ++j)
                {
                    list.Add((float)(zMax * System.Math.Sin(scale * i) * System.Math.Sin(scale * j)));
                    //list.Add((float)random.NextDouble());
                }
            }*/

            _surfacePlotControl.XAxisTitle = "Delay fs (X)";
            _surfacePlotControl.YAxisTitle = "Wavelength (Y)";
            _surfacePlotControl.ZAxisTitle = "Intensity (Z)";

            //_surfacePlotControl.SetData(srcData, 0, 20, 11, 100, 1000, 11, zMin, zMax, 10);

            UpdateDisplay();
        }

        System.Threading.Tasks.Task _task;
        private void runTestModel()
        {
            viewModel = (DataContext as SurfacePlotterModel);

            int algorithm = viewModel.SelectedSpeed;
            int sleepIntervalInMSecs = 1000 / viewModel.SelectedSpeed;

            viewModel.IsRunning = true;
            _task = new System.Threading.Tasks.Task(new System.Action(delegate
            {
                _surfacePlotControl.ILabelFormatter = this;

                const int YCount = 100;
                const int XCount = 100;
                int counter = 0;
                float zMax = 160;
                float zMin = -160;
                float scale = 2f * (float)System.Math.PI / (float)XCount;

                Random random = new Random();

                List<List<float>> srcData = new List<List<float>>();
                for (int i = 0; i < XCount; ++i)
                {
                    List<float> list = new List<float>();
                    srcData.Add(list);
                    for (int j = 0; j < YCount; ++j)
                    {
                        list.Add((float)(zMax * System.Math.Sin(scale * i) * System.Math.Sin(scale * j)));
                    }
                }

                _surfacePlotControl.XAxisTitle = "Number of Samples (X)";
                _surfacePlotControl.YAxisTitle = "Population Count (Y)";
                _surfacePlotControl.ZAxisTitle = "Adjusted Sigma Delta (Z)";

                while (viewModel.IsRunning == true)
                {
                    List<List<float>> drawData = new List<List<float>>();

                    for (int i = 0; i < XCount; ++i)
                    {
                        int offset = i + counter;
                        while (offset >= XCount)
                        {
                            offset -= XCount;
                        }
                        List<float> list = new List<float>();
                        drawData.Add(list);
                        for (int j = 0; j < YCount; ++j)
                        {
                            list.Add(srcData[offset][j]);
                        }
                    }

                    this.Dispatcher.Invoke(delegate
                    {
                        _surfacePlotControl.SetData(drawData, 0, 10, 11, 0, 100, 11, zMin, zMax, 11);
                    });

                    counter += 1;
                    if (counter >= XCount)
                    {
                        counter = 0;
                    }

                    System.Threading.Thread.Sleep(sleepIntervalInMSecs);
                }
            }));
            _task.Start();
        }
        #endregion
    }
}
