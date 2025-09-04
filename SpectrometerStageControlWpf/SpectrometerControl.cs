using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetOceanDirect;

namespace SpectrometerStageControl
{
    public struct SpectrometerDevice
    {
        public int Id;
        public string Name;
        public int SpectrumLength;
    }

    public class SpectrometerControl : IDisposable
    {
        #region Class Members
        private OceanDirect instance_;
        private List<Devices> devicesList_;
        private Devices device_;
        private int pixels_;
        private int spectrumLength_;
        private bool connected;

        public double[] Wavelengths { get; private set; }
        public bool Connected 
        {
            get { return connected; }
            private set { connected = value; }
        }
        #endregion

        #region Dispose pattern
        ~SpectrometerControl()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            instance_.shutDown();
        }

        private bool disposed = false;
        #endregion

        public SpectrometerControl() 
        {
            instance_ = OceanDirect.getInstance();
            devicesList_ = new List<Devices>();
        }

        public void Connect(int deviceId)
        {
            int errCode = 0;
            var device = devicesList_.Find(d => d.Id == deviceId);

            if (device != null)
            {
                instance_.openDevice(device.Id, ref errCode);
                if (isError(errCode))
                    throw new InvalidOperationException($"Function openDevice failed with error code {errCode}");
           
                device_ = device;
                initialize();

                connected = true;
            }
        }

        public void Disconnect()
        {
            int errCode = 0;
            if (device_.InUse)
            {
                instance_.closeDevice(device_.Id, ref errCode);
                if (isError(errCode))
                    throw new InvalidOperationException($"Function closeDevice failed with error code {errCode}");
            }
            device_ = null;
            connected = false;
        }

        public void SetIntegrationTime(long timeUs)
        {
            int errCode = 0;
            instance_.setIntegrationTimeMicros(device_.Id, ref errCode, (uint)timeUs);

            if (isError(errCode))
                throw new InvalidOperationException($"Function setIntegrationTimeMicros failed with error code {errCode}");
        }

        public (double[], double[]) GetFullSpectrum()
        {
            (_, var intensities) = getSpectrumAndWavelengths();
            return (Wavelengths, intensities); 
        }

        public List<(double, double)> GetSpectrumAtRange(double lo, double hi, int increments = 0)
        {
            Console.WriteLine($"Wavelengths from approx. {lo} nm to {hi} nm");
            int errCode = 0;
            List<(double, double)> spectrum = new List<(double, double)>();
            List<int> indices = new List<int>();
            
            (var wavelengths, var intensities) = getSpectrumAndWavelengths();

            if (increments > 0)
                indices = getIndiciesByIncrementsInRange(ref wavelengths, lo, hi, increments).ToList();
            else
            {
                indices = instance_.getIndicesAtWavelengthRange(device_.Id, ref errCode, ref wavelengths, lo, hi).ToList();
                if (wavelengths.Length != indices.Count)
                    throw new InvalidOperationException("Number of indices and wavelengths do not match.");
                if (isError(errCode))
                    throw new InvalidOperationException($"Function getIndicesAtWavelengthRange failed with error code {errCode}");
            }
            for (int i = 0; i < indices.Count; i++)
                spectrum.Add((intensities[indices[i]], wavelengths[i]));

            return spectrum;
        }

        public List<SpectrometerDevice> GetDevices()
        {
            List<SpectrometerDevice> deviceList = new List<SpectrometerDevice>();
            devicesList_.Clear();

            var ocean = OceanDirect.getInstance();
            var devices = ocean.findDevices();
            Thread.Sleep(1000);

            foreach (var device in devices)
            {
                deviceList.Add(
                    new SpectrometerDevice()
                    {
                        Id = device.Id,
                        Name = device.Name,
                        SpectrumLength = device.SpectrumLen
                    });
                devicesList_.Add(device);
            }

            return deviceList;
        }

        #region Private Functions
        private (double[], double[]) getSpectrumAndWavelengths()
        {
            // Pixel = Intensity
            int errCode = 0;

            double[] intensities = instance_.getSpectrum(device_.Id, ref errCode);
            if (isError(errCode))
                throw new InvalidOperationException($"Function getSpectrum failed with error code {errCode}");

            double[] wavelengths = instance_.getWavelengths(device_.Id, ref errCode);
            if (isError(errCode))
                throw new InvalidOperationException($"Function getWaveLengths failed with error code {errCode}");

            return (wavelengths, intensities);
        }

        private int[] getIndiciesByIncrementsInRange(ref double[] wavelengths, double lo, double hi, int increments)
        {
            IEnumerable<double> GetValues(double start, double end, double increment)
            {
                for (double i = start; i <= end; i += increment)
                    yield return i;
            }
            int errCode = 0;
            wavelengths = GetValues(lo, hi, increments).ToArray();
            int[] indices = instance_.getIndicesAtAnyWavelength(device_.Id, ref errCode, ref wavelengths, wavelengths.Length);

            if (wavelengths.Length != indices.Length)
                throw new InvalidOperationException("Number of indices and wavelengths do not match.");
            if (isError(errCode))
                throw new InvalidOperationException($"Function getIndicesAtAnyWavelength failed with error code {errCode}");

            return indices;
        }
        private bool isError(int errCode)
        {
            if (errCode > 0 && errCode < 10001)
                return true;
            else
                return false;
        }
        private void initialize()
        {
            int errCode = 0;

            pixels_ = instance_.getNumberOfPixels(device_.Id, ref errCode);
            if (isError(errCode))
                throw new InvalidOperationException($"Function getNumberOfPixels failed with error code {errCode}");

            spectrumLength_ = instance_.findSpectrumLength(device_.Id, ref errCode);
            if (isError(errCode))
                throw new InvalidOperationException($"Function findSpectrumLength failed with error code {errCode}");

            Wavelengths = instance_.getWavelengths(device_.Id, ref errCode);
            if (isError(errCode))
                throw new InvalidOperationException($"Function getWaveLengths failed with error code {errCode}");
        }
        #endregion
    }
}
