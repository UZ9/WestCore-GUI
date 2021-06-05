using Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace GUI_WPF_Migration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public static MainWindow Instance;

        private readonly ChartManager chartManager;

        /// <summary>
        /// Queue for the <see cref="ChartManager"/> to pull out of for module creation.
        /// </summary>
        public Queue<Border> ModuleSlots { get; set; }

        public MainWindow()
        {
            Instance = this;


            InitializeComponent();

            var currentBorders = new List<Border> { slot1, slot2, slot3, slot4, slot5, slot6, slot7, slot8, slot9 };

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
        /// When the application is signaled it's about to be closed, make sure everything has been disposed in the <see cref="ChartManager"/>
        /// </summary>
        public void Window_Close(object sender, CancelEventArgs e)
        {
            chartManager?.Dispose();
        }



    }
}
