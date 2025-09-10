using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpectrometerStageControlWpf
{
    public partial class SpectrometerModel : ObservableObject
    {
        public SpectrometerModel(MainPresenter presenter) 
        {
            this.presenter = presenter;
            UpdateData();
        }

        public void InitializeChart(double xMin, double xMax, double yMin, double yMax)
        {
            Series = new ISeries[]
            {
                new LineSeries<ObservablePoint>()
                {
                    Values = data,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 1 },
                    GeometryStroke = null,
                    GeometryFill = null,
                    LineSmoothness = 0
                }
            };

            Title = new LabelVisual
            {
                Text = "Spectrometer",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15)
            };

            XAxes = new Axis[]
            {
                new Axis()
                {
                    Name = "Wavelength",
                    MinLimit = xMin,
                    MaxLimit = xMax,
                    UnitWidth = 100
                }
            };

            YAxes = new Axis[]
            {
                new Axis()
                {
                    Name = "Intensity",
                    MinLimit = yMin,
                    MaxLimit = yMax,
                    UnitWidth = 1
                }
            };
        }

        public void SetData(ObservablePoint[] d1)
        {
            lock (Sync)
            {
                data.Clear();
                foreach (var point in d1)
                {
                    data.Add(point);
                }
            }
            OnPropertyChanged();
        }

        public void UpdateData(bool normalized = false)
        {
            lock (Sync)
            {
                data.Clear();
                var intensities = normalized ? presenter.SpectrumData.IntensitiesNormalized : presenter.SpectrumData.Intensities;
                var wavelengths = presenter.SpectrumData.Wavelengths;

                for (int i = 0; i < wavelengths.Length; i++)
                {
                    data.Add(new ObservablePoint(wavelengths[i], intensities[i]));
                }
            }
            OnPropertyChanged();
        }

        public void UpdateChartAxes(double xMin, double xMax, double xUnitWidth, double yMin, double yMax, double yUnitWidth)
        {
            XAxes[0].MinLimit = xMin;
            XAxes[0].MaxLimit = xMax;
            XAxes[0].UnitWidth = xUnitWidth;

            YAxes[0].MinLimit = yMin;
            YAxes[0].MaxLimit = yMax;
            YAxes[0].UnitWidth = yUnitWidth;
        }

        public void UpdateChartAxes(SpectrumData spectrumData, bool normalize)
        {
            if (normalize)
            {
                YAxes[0].MinLimit = 0;
                YAxes[0].MaxLimit = 1;
                YAxes[0].UnitWidth = 0.10;
            }
            else
            {
                double min = spectrumData.Intensities.Min();
                double max = spectrumData.Intensities.Max();
                double interval = (max - min) / 10;

                YAxes[0].MinLimit = min;
                YAxes[0].MaxLimit = max;
                YAxes[0].UnitWidth = interval;
            }
            UpdateData(normalize);
            OnPropertyChanged();
        }

        private MainPresenter presenter;
        private readonly ObservableCollection<ObservablePoint> data = new ObservableCollection<ObservablePoint>();
        public ISeries[] Series { get; set; }
        public object Sync { get; } = new object();

        public Axis[] XAxes { get; set; } 
        public Axis[] YAxes { get; set; }

        public LabelVisual Title;

        public Func<float, float> EasingFunction = null;
        
    }
}
