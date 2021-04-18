
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WestCore_GUI.Charts
{
    public class LiveChart
    {
        private Builder builder;

        public PlotModel model;

        public LiveChart(Builder builder)
        {
            this.builder = builder;

            model = builder.model;
        }

        public void AddPoint(int seriesIndex, double xPoint, double yPoint)
        {
            (model.Series[seriesIndex] as LineSeries).Points.Add(new DataPoint(xPoint, yPoint));

            // Marks for reconstruction
            model.InvalidatePlot(true);

            // Auto scrolling
            // If statements (in order):
            // - Only pan if seriesIndex 0 is being added, as we don't want to pan for every single series
            // - Only pan if scroll is enabled
            // - Only pan when the max point amount has exceeded
            if (seriesIndex == 0 && (builder.scroll) && (xPoint > builder.xAxis.Maximum))
            {
                // Finds the viewport offset and adds a -1 transform to it
                double result = builder.xAxis.Transform(-1 + builder.xAxis.Offset);

                builder.xAxis.Pan(result);

                // Marks for reconstruction
                model.InvalidatePlot(false);
            }
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

            public Builder(string title, bool scroll)
            {
                this.title = title;
                this.scroll = scroll;

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
                    AbsoluteMaximum = 200,
                    AbsoluteMinimum = -5,
                    IsZoomEnabled = false
                };

                xAxis = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    // TODO: These will have to be changed// TODO: These will have to be changed
                    Maximum = 100,
                    Minimum = 1,
                    AbsoluteMaximum = 50000,
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

                model.LegendBackground = new OxyColor();

                xAxis.FontWeight = FontWeights.Bold;
                yAxis.FontWeight = FontWeights.Bold;

                return model;

            }

            public Builder AddSeries(string name)
            {
                return AddSeries(name, null);
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

                return this;
            }

            public LiveChart Build()
            {
                return new LiveChart(this);
            }
        }
    }
}

