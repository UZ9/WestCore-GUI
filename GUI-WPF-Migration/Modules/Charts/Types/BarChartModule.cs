using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp.Wpf;

namespace GUI_WPF_Migration.Modules.Charts.Types
{
    class BarChartModule : ChartModule
    {
        /// <summary>
        /// The fresh time for the chart to update (in milliseconds)
        /// </summary>
        private int refreshTime;

        private BarSeries barSeries;
        private CategoryAxis axis;

        private readonly Dictionary<string, BarItem> barItems = new Dictionary<string, BarItem>();

        private int currentFrame = 0;

        public BarChartModule(Border moduleContainer) : base(moduleContainer) { }

        public override void Initialize(string title, Dictionary<string, object> configMap)
        {
            base.Initialize(title, configMap);

            // Retrieve a list of every series name
            var seriesNames = ((Newtonsoft.Json.Linq.JArray)configMap["series-names"]).ToObject<string[]>();

            refreshTime = Convert.ToInt32(configMap["refresh-rate"]);

            foreach (var series in seriesNames)
            {
                barItems[series] = new BarItem { Value = 3 };
            }

            AttachBarItems();

            // Add PlotView to window & set properties
            var plotView = new PlotView
            {
                Margin = new Thickness(5, 10, 20, 10),
                Background = new SolidColorBrush(Color.FromRgb(39, 44, 77)),
                Name = "plot" + ModuleContainer.Name.Last(),
                Model = Model
            };

            ModuleContainer.Child = plotView;


        }

        public override PlotModel CreateModel()
        {
            var model = new PlotModel()
            {
                Title = Title,
                TitleToolTip = Title
            };



            // Set main color
            var mainColor = OxyColor.FromRgb(152, 147, 218); ;

            // Set default color for line graph
            model.DefaultColors[0] = mainColor;
            model.DefaultColors[1] = OxyColor.FromRgb(231, 76, 60);
            model.DefaultColors[2] = OxyColor.FromRgb(41, 128, 185);
            model.DefaultColors[3] = OxyColor.FromRgb(39, 174, 96);

            // Set fonts
            model.DefaultFont = "Nirmala UI";
            model.DefaultFontSize = 10;
            model.TitleFontWeight = OxyPlot.FontWeights.Bold;
            model.TextColor = OxyColor.FromRgb(255, 255, 255);

            // Add legend
            var legend = new OxyPlot.Legends.Legend
            {
                LegendBackground = new OxyColor()
            };

            model.Legends.Add(legend);

            model.PlotAreaBorderColor = mainColor;

            barSeries = new BarSeries()
            {

                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}"
            };

            axis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                AxislineColor = mainColor,
                TicklineColor = mainColor,
                IsZoomEnabled = false
            };




            return model;


        }

        public override void Update()
        {
            foreach (var variable in VarMap)
            {
                barItems[variable.Key].Value = Convert.ToDouble(variable.Value);
            }

            currentFrame += 20;

            if (currentFrame % refreshTime == 0)
            {
                Model.InvalidatePlot(false);
            }
        }

        private void AttachBarItems()
        {
            barSeries.ItemsSource = barItems.Values.ToArray();

            axis.ItemsSource = barItems.Keys.ToArray();

            Model.Series.Add(barSeries);
            Model.Axes.Add(axis);

            var mainColor = OxyColor.FromRgb(152, 147, 218); ;

            Model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                AxislineColor = mainColor,
                TicklineColor = mainColor,
                Minimum = MinRange,
                Maximum = MaxRange
            });
        }
    }
}
