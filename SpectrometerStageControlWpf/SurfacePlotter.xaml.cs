using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFSurfacePlot3D;
using System.Windows.Media.Media3D;

namespace SpectrometerStageControlWpf
{
    /// <summary>
    /// Interaction logic for SurfacePlotter.xaml
    /// </summary>
    public partial class SurfacePlotter : Window, ISurfacePlotView
    {
        private MainPresenter presenter;
        private SurfacePlotModel viewModel;
        private List<FrogData> data;

        public SurfacePlotter(MainPresenter presenter)
        {
            InitializeComponent();

            this.presenter = presenter;
            viewModel = new SurfacePlotModel();
            //propertyGrid.DataContext = viewModel;
            surfacePlotView.DataContext = viewModel;
            data = new List<FrogData>();

            btnRefresh.Click += btnRefresh_Click;
            initialize();
        }

        private void initialize()
        {
            //viewModel.PlotData(presenter.FrogDataManager.GenerateTestPlotPoints());

            viewModel.XAxisLabel = "Wavelengths";
            viewModel.XAxisTicks = new double[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500 };
            viewModel.YAxisLabel = "Delays (fs)";
            viewModel.YAxisTicks = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            presenter.FrogDataManager.GenerateTestPlotPoints();
            if (presenter.FrogDataManager.FrogData.Count >= 3)
            {
                viewModel.PlotData(presenter.FrogDataManager.FrogData);
            }
        }

        public void UpdateDisplay() 
        {
            if (presenter.FrogDataManager.FrogData.Count >= 3)
            {
                viewModel.PlotData(presenter.FrogDataManager.FrogData);
            }
        }

        public void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            presenter.FrogDataManager.AppendRandomSpectrumData();            
            UpdateDisplay();
        }
    }
}
