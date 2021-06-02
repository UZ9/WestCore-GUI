using Charts;
using Newtonsoft.Json;
using OxyPlot;
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
        Random random = new Random();



        public static MainWindow instance;

        /// <summary>
        /// Queue for the <see cref="ChartManager"/> to pull out of for module creation.
        /// </summary>
        public Queue<Border> ModuleSlots { get; set; }

        private double robotX;
        private double robotY;
        private double robotHeading;

        public MainWindow()
        {
            instance = this;


            InitializeComponent();

            List<Border> currentBorders = new List<Border> { slot1, slot2, slot3, slot4, slot5, slot6, slot7, slot8, slot9 };

            ModuleSlots = new Queue<Border>(currentBorders);

            ChartManager chartManager = new ChartManager(this);

            chartManager.HostPipeServer();
            chartManager.StartChartLoop();

            // Main shadow border

            //DrawOdomModule();


            //Task.Run(Loop);
        }

    }
}
