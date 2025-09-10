using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SpectrometerStageControlWpf
{
    public class FrogDataManager
    {
        private FrogHeaderData header;
        private List<FrogData> frogData;
        public List<FrogData> FrogData => frogData;

        public FrogDataManager() { frogData = new List<FrogData>(); }
        public FrogDataManager(FrogHeaderData header)
        {
            this.header = header;
            frogData = new List<FrogData>();
        }

        public void SetHeader(FrogHeaderData header)
        {
            this.header = header;
        }

        public void Append(FrogData data)
        {
            frogData.Add(data);
        }
        public void Append(int delayIndex, SpectrumData data)
        {
            frogData.Add(new FrogData(delayIndex, data));
        }

        public void Clear()
        {
            frogData.Clear();
        }

        public List<FrogData> GetAll()
        {
            return frogData;
        }

        public FrogData Get(int delayIndex)
        {
            FrogData data = new FrogData();

            var idx = frogData.FindIndex(d => d.Delay_fs == delayIndex);
            if (idx != -1)
                data = frogData.FirstOrDefault(d => d.Delay_fs == delayIndex);

            return data;
        }

        public void ExportToFile()
        {
            DataFile.WriteToFile(header, frogData);
        }

        public List<List<float>> ToPlotFormat(
            out float xMin, out float xMax, out int xCount,
            out float yMin, out float yMax, out int yCount,
            out float zMin, out float zMax, out int zCount,
            bool normalize = false)
        {
            xMax = xMin = xCount = 0;
            yMax = yMin = yCount = 0;
            zMax = zMin = zCount = 0;

            if (frogData.Count <= 0)
                return new List<List<float>>();

            xCount = frogData.Count;
            yCount = frogData[0].SpectrumData.Wavelengths.Length;
            zCount = frogData[0].SpectrumData.Intensities.Length;

            List<List<float>> srcData = new List<List<float>>();
            for (int i = 0; i < xCount; ++i)
            {
                var data = frogData[i];
                List<float> list = new List<float>();
                srcData.Add(list);

                xMin = (data.Delay_fs < xMin) ? data.Delay_fs : xMin;
                xMax = (data.Delay_fs > xMax) ? data.Delay_fs : xMax;
                yMin = (float)data.MinWavelength;
                yMax = (float)data.MaxWavelength;
                zMin = (float)data.MinIntensity;
                zMax = (float)data.MaxIntensity;

                for (int j = 0; j < yCount; ++j)
                {
                    list.Add(normalize 
                        ? (float)data.SpectrumData.Intensities[j] 
                        : (float)data.SpectrumData.IntensitiesNormalized[j]);
                }
            }

            return srcData;
        }

        public (float, float) GetMinMaxIntensity()
        {
            var min = (float)frogData.Select(x => x.SpectrumData.Intensities.Min()).Min();
            var max = (float)frogData.Select(x => x.SpectrumData.Intensities.Max()).Max();

            return (min, max);
        }

        #region Testing functions
        public void GenerateTestData()
        {
            Random random = new Random();

            double[] wavelengthsTest = { 10, 15, 20, 25, 30, 35, 40, 45, 50, 55 };

            header = new FrogHeaderData();
            header.NumDelayPoints = 20;
            header.NumWavelengthPoints = 10;
            header.DelayIncrements = 1.0;
            header.WavelengthIncrements_nm = 5.0;
            header.WavelengthCenter = 30.0;

            for (int i = 0; i < 20; ++i)
            {
                // Generate different intensities for different delays
                double[] intensitiesTest = Enumerable
                    .Repeat(0, 10)
                    .Select(j => random.NextDouble())
                    .ToArray();

                frogData.Add(new FrogData(i, new SpectrumData(wavelengthsTest, intensitiesTest)));
            }
            
        }

        public void GenerateTestPlotPoints()
        {
            Random random = new Random();

            double[] wavelengthsTest = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };

            for (int i = 0; i < 1; ++i)
            {
                // Generate different intensities for different delays
                double[] intensitiesTest = Enumerable
                    .Repeat(0, 10)
                    //.Select(j => random.NextDouble())
                    .Select(j => (double)random.Next(1, 4))
                    .ToArray();

                frogData.Add(new FrogData(i, new SpectrumData(wavelengthsTest, intensitiesTest)));
            }
        }
        public void AppendRandomSpectrumData()
        {
            Random random = new Random();

            double[] wavelengthsTest = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };

            // Generate different intensities for different delays
            double[] intensitiesTest = Enumerable
                .Repeat(0, 10)
                //.Select(j => random.NextDouble())
                .Select(j => (double)random.Next(1, 4))
                .ToArray();

            frogData.Add(new FrogData(frogData.Count, new SpectrumData(wavelengthsTest, intensitiesTest)));
        }
        #endregion
    }
}
