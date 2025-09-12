using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrometerStageControlWpf
{
    public struct SpectrumData
    {
        private double[] wavelengths;
        private double[] intensities;
        public double[] Wavelengths => this.wavelengths;
        public double[] WavelengthsNormalized => normalize(this.wavelengths);
        public double WavelengthMin => this.wavelengths.Min();
        public double WavelengthMax => this.wavelengths.Max();

        public double[] Intensities => this.intensities;
        public double[] IntensitiesNormalized => normalize(this.intensities);
        public double IntensityMin => this.intensities.Min();
        public double IntensityMax => this.intensities.Max();



        private double[] normalize(double[] data)
        {
            var max = data.Max();
            return data.Select(d => d / max).ToArray();
        }

        public SpectrumData(double[] wave, double[] intensities)
        {
            this.wavelengths = wave;
            this.intensities = intensities;

        }
        public SpectrumData DeepCopy()
        {
            return new SpectrumData(this.Wavelengths, this.Intensities);
        }

    }

    public struct FrogHeaderData 
    {
        public int NumDelayPoints;
        public int NumWavelengthPoints;
        public double DelayIncrements; // fs per pixel
        public double WavelengthIncrements_nm; // nm per pixel
        public double WavelengthCenter; // wavelength of center pixel
    }

    public struct FrogData
    {
        public readonly int Delay_fs;
        public readonly SpectrumData SpectrumData;
        public readonly double MinWavelength;
        public readonly double MaxWavelength;
        public readonly double MinIntensity;
        public readonly double MaxIntensity;

        public FrogData(int delay, SpectrumData data)
        {
            Delay_fs = delay;
            SpectrumData = data;
            MinWavelength = data.Wavelengths.Min();
            MaxWavelength = data.Wavelengths.Max();
            MinIntensity = data.Intensities.Min();
            MaxIntensity = data.Intensities.Max();
        }
    }
}
