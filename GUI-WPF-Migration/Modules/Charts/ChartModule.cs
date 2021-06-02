using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using OxyPlot;

namespace Modules
{
    /// <summary>
    /// Contains the main components of a Chart related <see cref="Module"/>.
    /// </summary>
    public abstract class ChartModule : Module
    {
        /// <summary>
        /// The attached <see cref="PlotModel"/> of the chart, containing all of the necessary style properties
        /// </summary>
        public PlotModel Model { get; private set; }

        /// <summary>
        /// The title shown above the chart 
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The minimum value the chart can display 
        /// </summary>
        public int MinRange { get; protected set; }

        /// <summary>
        /// The maximum value the chart can display
        /// </summary>
        public int MaxRange { get; protected set; }

        public ChartModule(Border moduleContainer) : base(moduleContainer) { }

        /// <summary>
        /// Initializes a ChartModule's core components
        /// </summary>
        /// <param name="configMap">The configuration data sent through the CONFIG_HEADER</param>
        public override void Initialize(string title, Dictionary<string, object> configMap)
        {
            Title = title;

            MinRange = Convert.ToInt32(configMap["min-range"]);
            MaxRange = Convert.ToInt32(configMap["max-range"]);

            // Create a model for the hcart
            Model = CreateModel();
        }

        /// <summary>
        /// Creates the <see cref="PlotModel"/> of the chart. As this differs depending on the type, this is left as abstract.
        /// </summary>
        /// <returns></returns>
        public abstract PlotModel CreateModel();
    }
}
