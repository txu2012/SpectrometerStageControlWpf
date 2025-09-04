using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrometerStageControl
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
    }
}
