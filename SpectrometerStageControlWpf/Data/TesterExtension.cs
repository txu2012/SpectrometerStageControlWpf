using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrometerStageControlWpf
{
    public class TesterExtension
    {

        public static List<FrogData> GenerateTestFrogDataSet()
        {
            Random random = new Random();

            double[] wavelengthsTest = { 10, 15, 20, 25, 30, 35, 40, 45, 50, 55 };

            FrogHeaderData header = new FrogHeaderData();
            header.NumDelayPoints = 20;
            header.NumWavelengthPoints = 10;
            header.DelayIncrements = 1.0;
            header.WavelengthIncrements_nm = 5.0;
            header.WavelengthCenter = 30.0;

            List<FrogData> data = new List<FrogData>();
            for (int i = 0; i < 20; ++i)
            {
                // Generate different intensities for different delays
                double[] intensitiesTest = Enumerable
                    .Repeat(0, 10)
                    .Select(j => random.NextDouble())
                    .ToArray();

                data.Add(new FrogData(i, new SpectrumData(wavelengthsTest, intensitiesTest)));
            }

            return data;
        }

        public static List<FrogData> GenerateTestPlotPoints()
        {
            Random random = new Random();

            double[] wavelengthsTest = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };

            List<FrogData> data = new List<FrogData>();
            for (int i = 0; i < 1; ++i)
            {
                // Generate different intensities for different delays
                double[] intensitiesTest = Enumerable
                    .Repeat(0, 10)
                    //.Select(j => random.NextDouble())
                    .Select(j => (double)random.Next(1, 4))
                    .ToArray();

                data.Add(new FrogData(i, new SpectrumData(wavelengthsTest, intensitiesTest)));
            }

            return data;
        }

        public static void AppendRandomSpectrumData(ref List<FrogData> src)
        {
            Random random = new Random();

            double[] wavelengthsTest = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };

            // Generate different intensities for different delays
            double[] intensitiesTest = Enumerable
                .Repeat(0, 10)
                //.Select(j => random.NextDouble())
                .Select(j => (double)random.Next(1, 4))
                .ToArray();

            src.Add(new FrogData(src.Count, new SpectrumData(wavelengthsTest, intensitiesTest)));
        }

        public static FrogData GenerateRandomFrogData(int nextIdx)
        {
            Random random = new Random();

            double[] wavelengthsTest = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };

            // Generate different intensities for different delays
            double[] intensitiesTest = Enumerable
                .Repeat(0, 10)
                //.Select(j => random.NextDouble())
                .Select(j => (double)random.Next(1, 4))
                .ToArray();

            return new FrogData(nextIdx, new SpectrumData(wavelengthsTest, intensitiesTest));
        }
    }
}
