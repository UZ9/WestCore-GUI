using Charts;
using Newtonsoft.Json;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI_WPF_Migration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static MainWindow instance;

        private ChartManager chartManager;

        /// <summary>
        /// Queue for the <see cref="ChartManager"/> to pull out of for module creation.
        /// </summary>
        public Queue<Border> ModuleSlots { get; set; }

        public MainWindow()
        {
            instance = this;


            InitializeComponent();

            List<Border> currentBorders = new List<Border> { slot1, slot2, slot3, slot4, slot5, slot6, slot7, slot8, slot9 };

            ModuleSlots = new Queue<Border>(currentBorders);

            chartManager = new ChartManager(this);
            try
            {

                chartManager.HostPipeServer();
                chartManager.AwaitPipeConnection();
                chartManager.StartChartLoop();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            // Main shadow border

            //DrawOdomModule();


            //Task.Run(Loop);
        }

        /// <summary>
        /// When the application is signalled it's about to be closed, make sure everything has been disposed in the <see cref="ChartManager"/>
        /// </summary>
        public void Window_Close(object sender, CancelEventArgs e)
        {
            if (chartManager != null)
            {
                chartManager.Dispose();
            }
        }



    }
}
