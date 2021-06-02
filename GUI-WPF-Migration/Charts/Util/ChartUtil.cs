using OxyPlot.Series;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Charts
{
    class ChartUtil
    {
        /// <summary>
        /// Generates a fake data point every 20ms until it reaches its numberOfPoints limit
        /// 
        /// Used for simulation of realtime chart data
        /// </summary>
        /// <param name="chart">The chart the information will be added to</param>
        /// <param name="numberOfPoints">The number of points to be added over time</param>
        /// <param name="seriesIndex">Indicates the series the data will be placed on within the chart</param>
        public static async void GenerateRandomData(LiveChart chart, int numberOfPoints = 250, int seriesIndex = 0)
        {
            Random random = new Random();

            LineSeries series = (chart.model.Series[seriesIndex] as LineSeries);

            for (int i = 0; i < numberOfPoints; i++)
            {
                double x = series.Points.Count > 0 ? series.Points[series.Points.Count - 1].X + 1 : 0;
                //if (series.Points.Count >= 200)
                //    series.Points.RemoveAt(0);
                double y = 0;

                y = 0.5 * (-3.2 * Math.Sin(-1.3 * x) - 1.2 * Math.Sin(-1.7 * Math.E * x) + 1.9 * Math.Sin(1.3 * Math.PI * x)) * random.NextDouble() + random.Next(0, 5);
                chart.AddPoint(seriesIndex, 1, x, y);

                await Task.Delay(TimeSpan.FromMilliseconds(20));
            }
        }
    }
}
