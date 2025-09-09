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
    public partial class SurfacePlotterView : Window, ILabelFormatter
    {
        //private SurfacePlotterModel viewModel;
        public SurfacePlotterView(MainPresenter presenter)
        {
            InitializeComponent();

            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as SurfacePlotterModel).Save();
            base.OnClosing(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Initialize()
        {
            SurfacePlotterModel viewModel = new SurfacePlotterModel();
            DataContext = viewModel;

            viewModel.Load();
            _configurationControl.DataContext = new OpenControls.Wpf.SurfacePlot.ViewModel.ConfigurationControlViewModel(viewModel.IConfiguration);
            _surfacePlotControl.Initialise(viewModel.IConfiguration);
        }


        
        private void Start()
        {
            runTestModel();
        }

        private void Stop()
        {
            (DataContext as SurfacePlotterModel).IsRunning = false;
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
        System.Threading.Tasks.Task _task;
        private void runTestModel()
        {
            SurfacePlotterModel viewModel = (DataContext as SurfacePlotterModel);

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
                        //list.Add((float)random.NextDouble());
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
