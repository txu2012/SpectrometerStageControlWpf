using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SpectrometerStageControlWpf
{
    
    public interface IView
    {
        void UpdateDisplay();
    }
    public interface IMainView : IView
    {
        void Log(string msg);
    }

    public interface IChartView : IView { }

    public interface ISurfacePlotView: IView { }

    

    public class MainPresenter
    {
        #region Class Members
        public StageControl Stage;
        public SpectrometerControl Spectrometer;
        private IMainView mainView;
        private IChartView chartView;

        public List<string> StageDevices { get { return Stage.GetDevices(); } }

        private List<SpectrometerDevice> spectrometerDevices;
        public List<SpectrometerDevice> SpectrometerDevices { get { return spectrometerDevices; } }
        public bool SpectrometerConnected { get { return Spectrometer.Connected; } }
        public bool StageConnected { get { return Stage.IsConnected; } }

        private SpectrumData spectrumData;
        public SpectrumData SpectrumData { get { return spectrumData; } }
        public double CenterWavelength { get; set; } = 400;
        public double WavelengthRange { get; set; } = 100;
        public long IntegrationTime_us { get; set; } = 10000;

        public decimal MoveBy_mm { get; set; } = 0.0008m;
        public decimal MoveRange_mm { get; set; } = 1.00000m;
        public decimal TimeMove_fs { get; set; } = 2.67m;
        public decimal TimeRange_fs { get; set; } = 3335.64m;

        private double wavelengthIncrements = 0.2;

        public FrogDataManager FrogDataManager;
        #endregion

        public MainPresenter() 
        {
            Spectrometer = new SpectrometerControl();
            Stage = new StageControl();

            spectrumData = new SpectrumData()
            {
                Wavelengths = new double[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 },
                Intensities = new double[] { 10, 20, 30, 40, 32, 31, 22, 6, 2, 1 }
            };

            spectrometerDevices = new List<SpectrometerDevice>();
            FrogDataManager = new FrogDataManager();
        }

        #region Connection
        public void ConnectStage(string serialNumber)
        {
            try
            {
                Stage.Connect(serialNumber);

                mainView.Log($"Connected to stage {serialNumber}");
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to connect to stage {serialNumber}. {ex.Message}");
            }
        }

        public void DisconnectStage()
        {
            try
            {
                Stage.Disconnect();
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to disconnect from stage. {ex.Message}");
            }
        }

        public void ConnectSpectrometer(int index)
        {
            try
            {
                Spectrometer.Connect(index);

                mainView.Log($"Connected to spectrometer {SpectrometerDevices[index].Id} {SpectrometerDevices[index].Name}");
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to connect to spectrometer {index}. {ex.Message}");
            }
        }

        public void DisconnectSpectrometer()
        {
            try
            {
                Spectrometer.Disconnect();
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to disconnect from spectrometer. {ex.Message}");
            }
        }
        #endregion

        public void AddMainView(IMainView view)
        {
            mainView = view;
        }

        public void AddChartView(IChartView view)
        {
            chartView = view;
        }

        public void RemoveChartView()
        {
            chartView = null;
        }

        public void RunLength(
            decimal moveBy_mm,
            decimal moveRange_mm,
            double centerWl_nm,
            double wlRange_nm,
            long integration_us)
        {
            try
            {
                if (!StageConnected || !SpectrometerConnected)
                {
                    mainView.Log($"Devices not connected. Stopping.");
                    return;
                }

                mainView.Log($"Starting set.");

                FrogDataManager.Clear();

                decimal initialPosition = -moveRange_mm;
                decimal finalPosition = moveRange_mm;
                CenterWavelength = centerWl_nm;
                WavelengthRange = wlRange_nm;
                int step = 0;

                mainView.Log($"Setting integration time to {integration_us}.");
                // Set integration time
                //SetIntegrationTime(integration_us);

                // Move to intial position and wait until finished
                mainView.Log($"Moving to initial home position.");
                StageHome();
                WaitForStage();

                // Get first set of data
                FrogDataManager.Append(step, GetSpectrumAtRangeInternal());
                mainView.Log($"Starting from first position.");

                while (Stage.CurrentPosition_mm < finalPosition)
                {
                    // Move to next position
                    StageMoveRelative(true, moveBy_mm);
                    Thread.Sleep(500);
                    WaitForStage();
                    mainView.Log($"Moved stage {moveBy_mm} mm Forward. Current position: {Stage.CurrentPosition_mm}");
                    mainView.UpdateDisplay();

                    // Increment time/step
                    step++;

                    // Acquire Spectrum range
                    FrogDataManager.Append(step, GetSpectrumAtRangeInternal());
                }
                mainView.Log($"Finished running set.");

                mainView.Log($"Exporting data to file.");
                FrogDataManager.SetHeader(new FrogHeaderData()
                {
                    NumDelayPoints = FrogDataManager.FrogData.Count(),
                    NumWavelengthPoints = Spectrometer.Wavelengths.Length,
                    DelayIncrements = (double)TimeMove_fs,
                    WavelengthIncrements_nm = wavelengthIncrements,
                    WavelengthCenter = CenterWavelength
                });
                FrogDataManager.ExportToFile();
            }
            catch (Exception ex)
            {
                StageStop();
                mainView.Log($"Error occured while running set. Stopping. {ex.Message}");
            }
        }

        private void WaitForStage()
        { 
            while(Stage.StageState != MotorState.Stopped)
            {
                mainView.UpdateDisplay();
                Thread.Sleep(100);
            }
        }

        #region Spectrometer
        public void RefreshSpectrometerDevices()
        {
            try
            {
                spectrometerDevices = Spectrometer.GetDevices();
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to refresh spectrometer list. {ex.Message}");
            }
        }

        public SpectrumData GetFullSpectrumInternal()
        {
            try
            {
                if (!SpectrometerConnected) throw new InvalidOperationException("Spectrometer Not Connected.");

                var (wavelengths, spectrum) = Spectrometer.GetFullSpectrum();
                return new SpectrumData(wavelengths, spectrum);
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to acquire full spectrum from spectrometer. {ex.Message}");
                return new SpectrumData() { Wavelengths = new double[] { }, Intensities = new double[] { } };
            }
        }
        public void GetFullSpectrum()
        {
            spectrumData = GetFullSpectrumInternal();

            if (chartView != null) chartView.UpdateDisplay();
        }

        public SpectrumData GetSpectrumAtRangeInternal()
        {
            try
            {
                double lo = CenterWavelength - WavelengthRange;
                double hi = CenterWavelength + WavelengthRange;
                if (!SpectrometerConnected)
                    throw new InvalidOperationException("Spectrometer Not Connected.");
                if ((lo < Spectrometer.Wavelengths[0]) ||
                    (hi > Spectrometer.Wavelengths[Spectrometer.Wavelengths.Length - 1]))
                    throw new InvalidOperationException("Wavelength range not within range of spectrometer.");

                var spec = Spectrometer.GetSpectrumAtRange(lo, hi);
                var wavelengths = spec.Select(s => s.Item1).ToArray();
                var spectrum = spec.Select(s => s.Item2).ToArray();

                return new SpectrumData(wavelengths, spectrum);
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to acquire spectrum range from spectrometer. {ex.Message}");
                return new SpectrumData() { Wavelengths = new double[] { }, Intensities = new double[] { } };
            }
        }
        public void GetSpectrumAtRange()
        {
            spectrumData = GetSpectrumAtRangeInternal();

            if (chartView != null) chartView.UpdateDisplay();
        }

        public void SetIntegrationTime(long timeUs)
        {
            try
            {
                if (!SpectrometerConnected) throw new InvalidOperationException("Spectrometer Not Connected.");

                Spectrometer.SetIntegrationTime(timeUs);
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to acquire spectrum range from spectrometer. {ex.Message}");
            }
        }
        #endregion

        #region Stage
        public void StageHome()
        {
            try
            {
                if (!StageConnected) throw new InvalidOperationException("Stage Not Connected.");
                mainView.Log($"Homing Stage.");
                Stage.Home();
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to home stage. {ex.Message}");
            }
        }

        public void StageMoveRelative(bool direction, decimal moveBy_mm)
        {
            try
            {
                if (!StageConnected) throw new InvalidOperationException("Stage Not Connected.");
                mainView.Log($"Moving stage {moveBy_mm} mm {(direction ? "Forward" : "Back")}");
                Stage.MoveRelative(direction, moveBy_mm);
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to move stage {moveBy_mm} mm {((direction) ? "Forward" : "Backward")}. {ex.Message}");
            }
        }

        public void StageMoveContinuous(bool direction)
        {
            try
            {
                if (!StageConnected) throw new InvalidOperationException("Stage Not Connected.");
                mainView.Log($"Moving stage {(direction ? "Forward" : "Back")}");
                Stage.MoveContiuous(direction);
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to move stage continuously {((direction) ? "Forward" : "Backward")}. {ex.Message}");
            }
        }

        public void StageStop()
        {
            try
            {
                if (!StageConnected) throw new InvalidOperationException("Stage Not Connected.");
                mainView.Log($"Stopping stage");
                Stage.Stop();
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to stop stage. {ex.Message}");
            }
        }

        public void StageMoveTo(decimal targetPos_mm)
        {
            try
            {
                if (!StageConnected) throw new InvalidOperationException("Stage Not Connected.");
                mainView.Log($"Moving stage to target position {targetPos_mm}");
                Stage.MoveTo(targetPos_mm);
            }
            catch (Exception ex)
            {
                mainView.Log($"Failed to move stage to position {targetPos_mm} mm. {ex.Message}");
            }
        }
        #endregion

        public decimal FemtosecondToMm(decimal timeFs)
        {
            // d = v * t
            // Mm / s
            long lightSpeed_mps = 299792458000;
            double timeS = (double)timeFs / 1_000_000_000_000_000.0;
            return (decimal)(lightSpeed_mps * (timeS));
        }

        public decimal MmToFemtosecond(decimal mm)
        {
            // t = d / v
            long lightSpeed_mps = 299792458000;
            double time = (double)(mm / lightSpeed_mps);
            return (decimal)(time * 1_000_000_000_000_000.0);
        }

        public void TestWriteToFile()
        {
            DataFile.TestWriteFile();
        }
    }
}
