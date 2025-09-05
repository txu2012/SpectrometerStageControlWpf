using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;

namespace SpectrometerStageControlWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainView
    {
        private const bool enableDebug = true;

        #region Class Members
        private MainPresenter presenter;
        private bool updatingDisplay = false;
        private SpectrometerChart formChart;

        private DispatcherTimer tmrMain;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            this.presenter = new MainPresenter();

            this.presenter.AddMainView(this);

            tmrMain = new DispatcherTimer();
            tmrMain.Tick += tmrMain_Tick;
            tmrMain.Interval = new TimeSpan(0, 0, 0, 0, 50);

            setHandlers();
            RefreshStage();
            RefreshSpectrometer();
            UpdateDisplay();
            this.Closing += mainWindow_Closing;
        }

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (formChart != null && formChart.IsLoaded)
                formChart.Close();
        }

        private void setHandlers()
        {
            btnStageConnect.Click += btnStageConnect_Click;
            btnStageDisconnect.Click += btnStageDisconnect_Click;
            btnStageRefresh.Click += btnStageRefresh_Click;
            btnSpecConnect.Click += btnSpecConnect_Click;
            btnSpecDisconnect.Click += btnSpecDisconnect_Click;
            btnSpecRefresh.Click += btnSpecRefresh_Click;

            btnHome.Click += btnHome_Click;
            btnContBack.Click += btnContBack_Click;
            btnContFwd.Click += btnContFwd_Click;
            btnStop.Click += btnStop_Click;
            btnMoveByNeg.Click += btnMoveByNeg_Click;
            btnMoveByPos.Click += btnMoveByPos_Click;
            btnSetMm.Click += btnSetMm_Click;
            btnSetMmRange.Click += btnSetMmRange_Click;

            nudTimeFs.ValueChanged += nudTimeFs_ValueChanged;
            nudTimeRangeFs.ValueChanged += nudTimeRangeFs_ValueChanged;
            nudStageMoveBy.ValueChanged += nudStageMoveBy_ValueChanged;
            nudStageRange.ValueChanged += nudStageRange_ValueChanged;

            nudCenterWave.ValueChanged += nudCenterWave_ValueChanged;
            nudWaveRange.ValueChanged += nudWaveRange_ValueChanged;
            nudIntegrationUs.ValueChanged += nudIntegrationUs_ValueChanged;
            nudWaveInc.ValueChanged += nudWaveInc_ValueChanged;

            btnChart.Click += btnChart_Click;
            btnSpectrumRange.Click += btnSpectrumRange_Click;
            btnSpectrumFull.Click += btnSpectrumFull_Click;

            btnRun.Click += btnRun_Click;
            btnTestCsv.Click += btnTestCsv_Click;
        }

        #region Interface functions
        public void UpdateDisplay() 
        {
            updatingDisplay = true;

            if (presenter.StageConnected)
            {
                btnStageConnect.IsEnabled = false;
                btnStageRefresh.IsEnabled = false;
                btnStageDisconnect.IsEnabled = true;
                cbStage.IsEnabled = false;
            }
            else
            {
                btnStageConnect.IsEnabled = true;
                btnStageRefresh.IsEnabled = true;
                btnStageDisconnect.IsEnabled = false;
                cbStage.IsEnabled = true;
            }

            if (presenter.SpectrometerConnected)
            {
                btnSpecConnect.IsEnabled = false;
                btnSpecRefresh.IsEnabled = false;
                btnSpecDisconnect.IsEnabled = true;
                cbSpectrometer.IsEnabled = false;
            }
            else
            {
                btnSpecConnect.IsEnabled = true;
                btnSpecRefresh.IsEnabled = true;
                btnSpecDisconnect.IsEnabled = false;
                cbSpectrometer.IsEnabled = true;
            }

            lblStageConn.Content = (presenter.StageConnected) ? "Connected" : "Disconnected";
            lblSpectrometerConn.Content = (presenter.SpectrometerConnected) ? "Connected" : "Disconnected";

            if (!enableDebug)
            {
                gbStage.IsEnabled = presenter.StageConnected;
                gbSpectrometer.IsEnabled = presenter.SpectrometerConnected;

                if (gbStage.IsEnabled)
                    UpdateStageDisplay();

                if (gbSpectrometer.IsEnabled)
                    UpdateSpectrometerDisplay();

                btnTestCsv.IsEnabled = false;
                btnTestCsv.Visibility = Visibility.Hidden;
            }


            updatingDisplay = false;
        }

        private void UpdateStageDisplay() 
        {
            if (presenter.Stage.StageState != MotorState.Stopped)
            {
                btnHome.IsEnabled = false;
                btnContBack.IsEnabled = false;
                btnContFwd.IsEnabled = false;
                nudTimeFs.IsEnabled = false;
                nudTimeRangeFs.IsEnabled = false;
                nudStageMoveBy.IsEnabled = false;
                nudStageRange.IsEnabled = false;
                btnMoveByNeg.IsEnabled = false;
                btnMoveByPos.IsEnabled = false;
                btnSetMm.IsEnabled = false;
                btnSetMmRange.IsEnabled = false;
            }
            else
            {
                btnHome.IsEnabled = true;
                btnContBack.IsEnabled = true;
                btnContFwd.IsEnabled = true;
                nudTimeFs.IsEnabled = true;
                nudTimeRangeFs.IsEnabled = true;
                nudStageMoveBy.IsEnabled = true;
                nudStageRange.IsEnabled = true;
                btnMoveByNeg.IsEnabled = true;
                btnMoveByPos.IsEnabled = true;
                btnSetMm.IsEnabled = true;
                btnSetMmRange.IsEnabled = true;
            }
            txtHomed.Text = (presenter.Stage.IsHomed) ? "Yes" : "No";
            txtPosition.Text = presenter.Stage.CurrentPosition.ToString();
            txtState.Text = presenter.Stage.StageState.ToString();

            nudStageMoveBy.Value = (double)presenter.MoveBy_mm;
            nudStageRange.Value = (double)presenter.MoveRange_mm;
            nudTimeFs.Value = (double)presenter.TimeMove_fs;
            nudTimeRangeFs.Value = (double)presenter.TimeRange_fs;
        }

        private void UpdateSpectrometerDisplay() 
        {
            nudCenterWave.Value = (double)presenter.CenterWavelength;
            nudWaveRange.Value = (double)presenter.WavelengthRange;
            nudIntegrationUs.Value = (double)presenter.IntegrationTime_us;
        }

        public void Log(string msg) 
        {
            rtbLog.AppendText(msg + "\r\n");
            rtbLog.ScrollToEnd();
        }
        #endregion

        private void RefreshStage() 
        {
            cbStage.ItemsSource = presenter.StageDevices;
        }

        private void RefreshSpectrometer() 
        {
            presenter.RefreshSpectrometerDevices();
            if (presenter.SpectrometerDevices.Count > 0)
                cbSpectrometer.ItemsSource = presenter.SpectrometerDevices.Select(x => x.Id + "" + x.Name).ToList();
        }

        #region Connection
        private void btnStageConnect_Click(object sender, RoutedEventArgs e)
        {
            if (cbStage.SelectedItem != null)
                presenter.ConnectStage((string)cbStage.SelectedItem);
            UpdateDisplay();
        }

        private void btnStageDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (presenter.StageConnected)
                presenter.DisconnectStage();
            UpdateDisplay();
        }

        private void btnStageRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshStage();
            UpdateDisplay();
        }

        private void btnSpecConnect_Click(object sender, RoutedEventArgs e)
        {
            if (cbSpectrometer.SelectedItem != null)
                presenter.ConnectSpectrometer(cbSpectrometer.SelectedIndex);
            UpdateDisplay();
        }

        private void btnSpecDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (presenter.SpectrometerConnected)
                presenter.DisconnectSpectrometer();
            UpdateDisplay();
        }

        private void btnSpecRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshSpectrometer();
            UpdateDisplay();
        }
        #endregion

        #region Stage Control
        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            presenter.StageHome();
            tmrMain.IsEnabled = true;
        }

        private void btnContBack_Click(object sender, RoutedEventArgs e)
        {
            presenter.StageMoveContinuous(false);
            tmrMain.IsEnabled = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            presenter.StageStop();
            tmrMain.IsEnabled = true;
        }

        private void btnContFwd_Click(object sender, RoutedEventArgs e)
        {
            presenter.StageMoveContinuous(true);
            tmrMain.IsEnabled = true;
        }

        private void btnMoveByNeg_Click(object sender, RoutedEventArgs e)
        {
            presenter.StageMoveRelative(false, (decimal)nudStageMoveBy.Value);
            tmrMain.IsEnabled = true;
        }

        private void btnMoveByPos_Click(object sender, RoutedEventArgs e)
        {
            presenter.StageMoveRelative(true, (decimal)nudStageMoveBy.Value);
            tmrMain.IsEnabled = true;
        }

        private void nudStageMoveBy_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (updatingDisplay) return;
            presenter.MoveBy_mm = (decimal)nudStageMoveBy.Value;
            presenter.TimeMove_fs = presenter.MmToFemtosecond((decimal)nudStageMoveBy.Value);

            UpdateDisplay();
        }

        private void nudStageRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (updatingDisplay) return;
            presenter.MoveRange_mm = (decimal)nudStageRange.Value;
            presenter.TimeRange_fs = presenter.MmToFemtosecond((decimal)nudStageRange.Value);

            UpdateDisplay();
        }

        private void setMmFromFs()
        {
            if (updatingDisplay) return;

            presenter.MoveBy_mm = presenter.FemtosecondToMm((decimal)nudTimeFs.Value);
            presenter.TimeMove_fs = (decimal)nudTimeFs.Value;

            presenter.MoveRange_mm = presenter.FemtosecondToMm((decimal)nudTimeRangeFs.Value);
            presenter.TimeRange_fs = (decimal)nudTimeRangeFs.Value;

            UpdateDisplay();
        }

        private void btnSetMm_Click(object sender, RoutedEventArgs e)
        {
            setMmFromFs();
        }

        private void btnSetMmRange_Click(object sender, RoutedEventArgs e)
        {
            setMmFromFs();
        }

        private void nudTimeFs_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            setMmFromFs();
        }

        private void nudTimeRangeFs_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            setMmFromFs();
        }
        #endregion

        #region Spectrometer Control
        private void btnChart_Click(object sender, RoutedEventArgs e)
        {
            if (formChart == null || !formChart.IsLoaded)
            formChart = new SpectrometerChart(presenter);
            formChart.Show();
        }

        private void btnSpectrumFull_Click(object sender, RoutedEventArgs e)
        {
            presenter.GetFullSpectrum();
        }

        private void btnSpectrumRange_Click(object sender, RoutedEventArgs e)
        {
            presenter.GetSpectrumAtRange();
        }

        private void nudCenterWave_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (updatingDisplay) return;
            presenter.CenterWavelength = (double)nudCenterWave.Value;
            UpdateDisplay();
        }

        private void nudWaveRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (updatingDisplay) return;
            presenter.WavelengthRange = (double)nudWaveRange.Value;
            UpdateDisplay();
        }

        private void nudIntegrationUs_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (updatingDisplay) return;
            presenter.IntegrationTime_us = (long)nudIntegrationUs.Value;
            UpdateDisplay();
        }

        private void nudWaveInc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // NOP
        }
        #endregion

        private void tmrMain_Tick(object sender, EventArgs e)
        {
            if (presenter.Stage.StageState != MotorState.Stopped)
            {
                UpdateStageDisplay();
            }
            else
            {
                UpdateStageDisplay();
                tmrMain.IsEnabled = false;
            }
        }

        private void btnTestCsv_Click(object sender, RoutedEventArgs e)
        {
            presenter.TestWriteToFile();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            presenter.TestWriteToFile();
        }
    }
}
