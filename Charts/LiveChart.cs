using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;

namespace Charts
{
    public class LiveChart
    {
        private Builder builder;

        public PlotModel model { get; private set; }

        /// <summary>
        /// List of all the <see cref="LineSeries"/> elements present in the LiveChart. As the list within the plot is of type <see cref="Series"/>, 
        /// this is added to avoid casting every <see cref="AddPoint(int, int, double, double)"/>.
        /// </summary>
        private List<LineSeries> lineSeries;

        public struct LineChartConfiguration
        {
            // Chart title
            public string chartTitle;

            // Chart Properties
            public double minY;
            public double maxY;
        }

        public LiveChart(Builder builder)
        {
            this.lineSeries = builder.lineSeries;

            this.builder = builder;

            model = builder.model;
        }

        public void AddPoint(int seriesIndex, int deltaX, double xPoint, double yPoint)
        {
            lineSeries[seriesIndex].Points.Add(new DataPoint(xPoint, yPoint));

            // Auto scrolling
            // If statements (in order):
            // - Only pan if seriesIndex 0 is being added, as we don't want to pan for every single series
            // - Only pan if scroll is enabled
            // - Only pan when the max point amount has exceeded
            if (seriesIndex == 0 && builder.scroll && (xPoint > builder.xAxis.Maximum))
            {
                // Finds the viewport offset and adds a -20 transform to it
                double result = builder.xAxis.Transform(-deltaX + builder.xAxis.Offset);

                builder.xAxis.Pan(result);
            }


            // Marks for reconstruction
            model.InvalidatePlot(true);


        }


        public class Builder
        {
            // A reference to the plot model
            public PlotModel model;

            // The title of the chart
            public string title;

            // Determines whether to auto scroll or not
            public bool scroll;

            // Represents the two axes of the graph
            public LinearAxis xAxis;
            public LinearAxis yAxis;

            public List<LineSeries> lineSeries { get; private set; }



            public Builder(string title, bool scroll)
            {
                this.title = title;
                this.scroll = scroll;
                this.lineSeries = new List<LineSeries>();

                model = CreatePlotModel();
            }

            private PlotModel CreatePlotModel()
            {
                PlotModel model = new PlotModel
                {
                    Title = title,
                    TitleToolTip = title,
                };

                yAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    //AbsoluteMinimum = -2,
                    //AbsoluteMaximum = 2,
                    // TODO: These will have to be changed
                    Maximum = 200,
                    Minimum = -200,
                    AbsoluteMaximum = 200,
                    AbsoluteMinimum = -200,
                    //IsZoomEnabled = false
                };

                xAxis = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    // TODO: These will have to be changed// TODO: These will have to be changed
                    Maximum = 100,
                    Minimum = 1,
                    //AbsoluteMaximum = 50000,
                    AbsoluteMinimum = 0
                };

                OxyColor mainColor = OxyColor.FromRgb(152, 147, 218); ;

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
                model.TitleFontWeight = FontWeights.Bold;
                model.TextColor = OxyColor.FromRgb(255, 255, 255);

                OxyPlot.Legends.Legend legend = new OxyPlot.Legends.Legend();

                legend.LegendBackground = new OxyColor();

                model.Legends.Add(legend);

                xAxis.FontWeight = FontWeights.Bold;
                yAxis.FontWeight = FontWeights.Bold;

                return model;

            }

            public Builder AddSeries(string name)
            {
                return AddSeries(name, null);
            }

            public Builder WithConfiguration(LineChartConfiguration configuration)
            {
                (model.Axes[1] as LinearAxis).AbsoluteMinimum = configuration.minY;
                (model.Axes[1] as LinearAxis).AbsoluteMaximum = configuration.maxY;

                model.Title = configuration.chartTitle;

                return this;
            }

            public Builder AddSeries(string name, IEnumerable<DataPoint> data)
            {
                LineSeries newSeries = new LineSeries
                {
                    LineStyle = LineStyle.Solid,
                    // Interpolates between points to make a spline
                    InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,
                    Title = name
                };

                if (data != null)
                    newSeries.Points.AddRange(data);

                newSeries.TrackerFormatString = "{0}\nX={2},\nY={4}";

                // Add new series to native list
                model.Series.Add(newSeries);

                lineSeries.Add(newSeries);

                return this;
            }

            public LiveChart Build()
            {
                return new LiveChart(this);
            }
        }
    }
}