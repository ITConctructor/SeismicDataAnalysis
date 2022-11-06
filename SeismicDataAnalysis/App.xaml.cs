using SeismicDataAnalysis.Model;
using SeismicDataAnalysis.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SeismicDataAnalysis
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            App app = new App();
            MainWindow window = new MainWindow();
            ViewModelBase viewModel = new ViewModelBase(window);
            window.DataContext = viewModel;
            app.Run(window);
        }
        
    }
}
