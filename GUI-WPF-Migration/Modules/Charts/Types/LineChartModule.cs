using GUI_WPF_Migration;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GUI_WPF_Migration.Modules.Charts;

namespace Modules
{

    public class LineChartModule : ChartModule
    {
        /// <summary>
        /// The fresh time for the chart to update (in milliseconds)
        /// </summary>
        private int refreshTime;

        private LinearAxis xAxis;
        private LinearAxis yAxis;

        /// <summary>
        /// Determines whether the chart should autoscroll or not
        /// </summary>
        private bool scroll;

        private Dictionary<string, LineSeries> lineSeriesDict;

        private int currentFrame = 0;

        public LineChartModule(Border moduleContainer) : base(moduleContainer) { }

        public override void Initialize(string title, Dictionary<string, object> configMap)
        {
            base.Initialize(title, configMap);

            scroll = Convert.ToBoolean(configMap["scroll"]);

            refreshTime = Convert.ToInt32(configMap["refresh-rate"]);

            Console.WriteLine("Registered a line chart module with a fresh rate of " + refreshTime);

            lineSeriesDict = new Dictionary<string, LineSeries>();

            // Retrieve a list of every series name
            var seriesNames = ((Newtonsoft.Json.Linq.JArray)configMap["series-names"]).ToObject<string[]>();

            // Initialize series list by adding each series name
            foreach (var series in seriesNames)
            {
                AddSeries(series);
            }


            // TODO: Make this automatic with a ChartModule?

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
            var model = new PlotModel
            {
                Title = Title,
                TitleToolTip = Title,
            };

            yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Maximum = MaxRange,
                Minimum = MinRange,
                //AbsoluteMaximum = MaxRange,
                //AbsoluteMinimum = MinRange,
                //IsZoomEnabled = false
            };

            xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Maximum = 300,
                Minimum = 1,
                AbsoluteMinimum = 0
            };

            // Set axis colors
            var mainColor = OxyColor.FromRgb(152, 147, 218); ;

            xAxis.AxislineColor = mainColor;
            xAxis.TicklineColor = mainColor;
            yAxis.AxislineColor = mainColor;
            yAxis.TicklineColor = mainColor;

            model.PlotAreaBorderColor = mainColor;

            // Add created axes
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

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

            xAxis.FontWeight = OxyPlot.FontWeights.Bold;
            yAxis.FontWeight = OxyPlot.FontWeights.Bold;

            // Add legend
            var legend = new OxyPlot.Legends.Legend
            {
                LegendBackground = new OxyColor()
            };

            model.Legends.Add(legend);

            return model;
        }

        private void AddPoint(string seriesName, int deltaX, double xPoint, double yPoint)
        {
            lineSeriesDict[seriesName].Points.Add(new DataPoint(xPoint, yPoint));

            // Auto scrolling
            // If statements (in order):
            // - Only pan if the first series is being added, as we don't want to pan for every single series
            // - Only pan if scroll is enabled
            // - Only pan when the max point amount has exceeded
            if (lineSeriesDict.First().Key == seriesName && scroll && (xPoint > xAxis.Maximum))
            {
                // Finds the viewport offset and adds a -20 transform to it
                var result = xAxis.Transform(-deltaX + xAxis.Offset);

                xAxis.Pan(result);
            }

            // Apply refresh rate
            if (currentFrame % refreshTime == 0)
                Model.InvalidatePlot(false);
        }

        private void AddSeries(string name)
        {
            LineSeries newSeries = new LineSeries
            {
                LineStyle = LineStyle.Solid,
                //InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,// Interpolates between points to make a spline
                Title = name,
                TrackerFormatString = "{0}\nX={2},\nY={4}"
            };


            // Add new series to native list
            Model.Series.Add(newSeries);

            lineSeriesDict[name] = newSeries;
        }

        public override void Update()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var variable in VarMap)
                {
                    AddPoint(variable.Key, 20, currentFrame, Convert.ToDouble(variable.Value));
                }
            });


            currentFrame += 20;
        }
    }
}
