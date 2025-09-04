using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace SpectrometerStageControl
{
    public class DataFile
    {
        private static readonly DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"));
        
        public static bool WriteToFile(FrogHeaderData header, List<FrogData> data, bool normalized = false)
        {
            try
            {
                // Check if directory exists
                if (!dirInfo.Exists)
                    Directory.CreateDirectory(dirInfo.FullName);
                string filename = Path.Combine(dirInfo.FullName, DateTime.Now.ToString("yyyyMMddHHmmss") + ".12H");

                StringBuilder sb = new StringBuilder();

                // Header Data
                sb.Append($"{header.NumDelayPoints} {header.NumWavelengthPoints} {header.DelayIncrements} {header.WavelengthIncrements_nm} {header.WavelengthCenter}\r\n");

                // Main Data
                // for loop for faster time than LINQ
                for (int i = 0; i < data.Count; ++i)
                    sb.Append(string.Join("\t", normalized ? data[i].SpectrumData.IntensitiesNormalized : data[i].SpectrumData.Intensities) + "\r\n");

                // Simplier code (LINQ)
                /*sb.Append(string.Concat(
                    data.Select(d => 
                        string.Join("\t", normalized ? d.SpectrumData.IntensitiesNormalized : d.SpectrumData.Intensities) + "\r\n")));*/
                
                File.WriteAllText(filename, sb.ToString());                
                Console.WriteLine(sb.ToString());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to file. {ex.StackTrace}");
                return false;
            }
        }

        public static void TestWriteFile()
        {
            FrogHeaderData header = new FrogHeaderData();
            header.NumDelayPoints = 20;
            header.NumWavelengthPoints = 10;
            header.DelayIncrements = 1.0;
            header.WavelengthIncrements_nm = 5.0;
            header.WavelengthCenter = 30.0;

            List<FrogData> data = TesterExtension.GenerateTestFrogDataSet();

            WriteToFile(header, data);
        }
    }
}
