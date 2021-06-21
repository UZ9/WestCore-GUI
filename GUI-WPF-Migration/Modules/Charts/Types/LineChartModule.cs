using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GUI_WPF_Migration.Modules.Util;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.SkiaSharp.Wpf;
using OxyPlot.Wpf;

namespace GUI_WPF_Migration.Modules.Charts.Types
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

        private double maxYFound;

        public LineChartModule(Border moduleContainer) : base(moduleContainer) { }

        public override void Initialize(string title, Dictionary<string, object> configMap)
        {
            base.Initialize(title, configMap);

            scroll = Convert.ToBoolean(configMap["scroll"]);

            refreshTime = Convert.ToInt32(configMap["refresh-rate"]);

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
                Background = new SolidColorBrush(ColorPallete.PanelColor),
                Name = "plot" + ModuleContainer.Name.Last(),
                Model = Model
            };


            plotView.KeyDown += HandleHandledKeyDown;

            ModuleContainer.Child = plotView;
        }

        public void HandleHandledKeyDown(object sender, RoutedEventArgs e)
        {
            if (!(e is KeyEventArgs ke) || ke.Key != Key.Space) return;

            foreach (var module in MainWindow.Instance.ChartManager.Modules)
            {
                if (module.Value is LineChartModule value && module.Key != Title)
                {
                    value.Sync(this);
                }
            }

            Console.WriteLine(Title);

            ke.Handled = true;
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
                MajorGridlineColor = ColorPallete.OxyChartGridLineColor,
                MinorGridlineColor = ColorPallete.OxyChartGridLineColor,
                TickStyle = TickStyle.None,
                MajorGridlineStyle = LineStyle.Solid,
            };

            xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Maximum = 300,
                Minimum = 1,
                MajorGridlineColor = ColorPallete.OxyChartGridLineColor,
                MinorGridlineColor = ColorPallete.OxyChartGridLineColor,
                TickStyle = TickStyle.None,
                MajorGridlineStyle = LineStyle.Solid,
                AbsoluteMinimum = 0
            };

            model.PlotAreaBorderColor = ColorPallete.OxyChartGridLineColor;

            // Add created axes
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            // Set default color for line graph
            model.DefaultColors[0] = ColorPallete.OxyDefaultColors[0];
            model.DefaultColors[1] = ColorPallete.OxyDefaultColors[1];
            model.DefaultColors[2] = ColorPallete.OxyDefaultColors[2];
            model.DefaultColors[3] = ColorPallete.OxyDefaultColors[3];

            // Set fonts
            model.DefaultFont = "Roboto";
            model.DefaultFontSize = 10;
            model.TitleFontWeight = OxyPlot.FontWeights.Bold;
            model.TextColor = OxyColor.FromRgb(255, 255, 255);

            xAxis.FontWeight = OxyPlot.FontWeights.Bold;
            yAxis.FontWeight = OxyPlot.FontWeights.Bold;

            // Add legend
            var legend = new OxyPlot.Legends.Legend
            {
                LegendBackground = new OxyColor(),
                LegendPosition = LegendPosition.BottomLeft,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Horizontal
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

                xAxis.Maximum += deltaX;
                yAxis.Maximum = lineSeriesDict.First().Value.MaxY;

                //xAxis.Pan(result);
            }

            // Apply refresh rate
            if (currentFrame % refreshTime == 0)
                Model.InvalidatePlot(false);
        }

        private void AddSeries(string name)
        {
            var newSeries = new LineSeries
            {
                LineStyle = LineStyle.Solid,
                //InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,// Interpolates between points to make a spline
                Title = name,
                TrackerFormatString = "{0}\nX={2},\nY={4}",
                //EdgeRenderingMode = EdgeRenderingMode.PreferSpeed
            };


            //newSeries.MarkerFill = mainColor;
            //newSeries.MarkerType = MarkerType.Circle;

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

        /// <summary>
        /// Synchronizes the chart rendering pane to another module
        /// </summary>
        /// <param name="other"></param>
        public void Sync(LineChartModule other)
        {
            yAxis.Minimum = other.yAxis.Minimum;
            xAxis.Minimum = other.xAxis.Minimum;
            yAxis.Maximum = other.yAxis.Maximum;
            xAxis.Maximum = other.xAxis.Maximum;

            xAxis.Zoom(other.xAxis.ActualMinimum, other.xAxis.ActualMaximum);
            yAxis.Zoom(other.yAxis.ActualMinimum, other.yAxis.ActualMaximum);

            //yAxis.ActualMajorStep = other.yAxis.ActualMaximum;
            //xAxis.ActualMaximum = other.xAxis.ActualMaximum;

            Model.InvalidatePlot(false);
        }
    }
}
