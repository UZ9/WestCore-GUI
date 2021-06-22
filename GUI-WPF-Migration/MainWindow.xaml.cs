using Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using GUI_WPF_Migration.Logging;

namespace GUI_WPF_Migration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public static MainWindow Instance;

        public readonly ChartManager ChartManager;

        /// <summary>
        /// Queue for the <see cref="Charts.ChartManager"/> to pull out of for module creation.
        /// </summary>
        public Queue<Border> ModuleSlots { get; }

        public MainWindow()
        {
            Instance = this;


            InitializeComponent();

            // Note: The logging panel is separate from the module system and is required, preventing any module being created in slot5.
            var currentBorders = new List<Border> { slot1, slot2, slot3, slot4, slot6, slot7, slot8, slot9 };

            ModuleSlots = new Queue<Border>(currentBorders);

            ChartManager = new ChartManager(this);

            try
            {

                ChartManager.HostPipeServer();
                ChartManager.AwaitPipeConnections();
                ChartManager.StartChartLoop();

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
        /// When the application is signaled it's about to be closed, make sure everything has been disposed in the <see cref="Charts.ChartManager"/>
        /// </summary>
        public void Window_Close(object sender, CancelEventArgs e)
        {
            ChartManager?.Dispose();
        }



    }
}
