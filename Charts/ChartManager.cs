using GUI_WPF_Migration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Charts
{
    public class ChartManager
    {
        /// <summary>
        /// Represents the main states of the <see cref="ChartManager"/> at any given time. 
        /// </summary>
        public enum Status
        {
            Stopped, // The NamedPipe is not currently running
            Loading, // The NamedPipe is being created
            AwaitingConnection, // The NamedPipe is waiting for a client (PROS CLI) to connect
            AwaitingConfiguration, // The ChartManager is awaiting a configuration header from the named pipe
            Operational, // The ChartManager has been successfully loaded and configured and is now listening for any data coming through the named pipe
            Stopping // The ChartManager is in the process of shutting down and switching back to STOPPED
        }

        /// <summary>
        /// Used for storing values from the JSON parsing
        /// </summary>
        private class ChartSeriesObject
        {
            [JsonProperty(PropertyName = "min-y")]
            public double minY;

            [JsonProperty(PropertyName = "max-y")]
            public double maxY;

            [JsonProperty(PropertyName = "series-names")]
            public List<string> seriesNames;
        }

        // Headers are to differentiate normal cout logs from ones sending data to the GUI. Only strings starting with one of these prefixes will be parsed.
        private const string DATA_HEADER = "GUI_DATA_8378";
        private const string CONFIG_HEADER = "GUI_DATA_CONF_8378"; // TODO: Move this to an automatically generated value based off of DATA_HEADER

        /// <summary>
        /// The amount of milleseconds elapsed between data packets being sent by the PROS CLI. Used for properly differentiating time on the GUI chart time elements.
        /// </summary>
        private const int frameIncrement = 20;

        /// <summary>
        /// Each chart is designated with a given chart ID. The chart ID allows parsed data to go to the appropriate chart.
        /// </summary>
        private Dictionary<string, LiveChart> charts;

        /// <summary>
        /// The NamedPipe used to transfer information from the PROS CLI to the GUI application
        /// </summary>
        private NamedPipeServerStream pipeStream;

        /// <summary>
        /// Allows bytes to be read from the <see cref="NamedPipeServerStream"/>
        /// </summary>
        private BinaryReader streamReader;

        /// <summary>
        /// The current status of the <see cref="ChartManager"/>
        /// </summary>
        public Status status;

        /// <summary>
        /// Reference to main window
        /// </summary>
        private MainWindow window;

        public ChartManager(MainWindow window)
        {
            this.window = window;

            charts = new Dictionary<string, LiveChart>();
        }

        /// <summary>
        /// Creates the NamedPipeServerStream labelled "west-pros-pipe"
        /// </summary>
        public void HostPipeServer()
        {
            status = Status.Loading;

            Console.WriteLine("Creating Named Pipe...");

            // Create new pipe server
            // Parameters:
            // "west-pros-pipe" - name of pipe
            // PipeDirection.InOut - Allows data to be sent and received
            // 1 - Only 1 client will be able to connect at a time  
            // PipeTransmissionMode.Byte - Type of data being transfered
            pipeStream = new NamedPipeServerStream("west-pros-pipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte);

            Console.WriteLine("west-core-pipe successfully initialized");

            Console.WriteLine("Waiting for connection...");

            status = Status.AwaitingConnection;

            // Wait until C++ client connects
            pipeStream.WaitForConnection();

            // The stream reader will be what reads the contents from the Named Pipe
            streamReader = new BinaryReader(pipeStream);

            Console.WriteLine("Waiting for stdout input...");
        }

        /// <summary>
        /// Manages the data fetch loop for charts
        /// </summary>
        public void StartChartLoop()
        {
            //  The named pipe has been created, we now need to wait until the cONFIG_HEADER is received
            status = Status.AwaitingConfiguration;

            Task.Run(() =>
            {
                int currentFrame = 0;

                while (true) // Continuously pull data from piped server
                {

                    //if (pidChart == null) continue;

                    try
                    {
                        // Example string received from the stream reader:
                        // CONFIG_HEADER|JSON_DATA
                        var len = (int)streamReader.ReadUInt32();
                        var li = new string(streamReader.ReadChars(len));

                        string[] rawStringArr = li.Split('|');



                        // We need to have at least 2 elements when splitting off of '|'. 
                        if (rawStringArr.Length != 2)
                        {
                            // TODO: Redo WestDebug to work with WPF
                            //WestDebug.Log(Level.Info, li); // Prints to the GUI logger
                            continue;
                        }

                        // Split between the header and the actual JSON data
                        string header = rawStringArr[0];
                        string data = rawStringArr[1];

                        Console.WriteLine($"{header} | {data}");

                        // Make sure every segment of the file header isn't empty
                        if (rawStringArr.Where(s => s.Length == 0).Any()) continue;

                        switch (status)
                        {
                            // TODO: Change to JSON formatting
                            case Status.AwaitingConfiguration:
                                if (header == CONFIG_HEADER)
                                {
                                    // -----------------------------------------
                                    // CONFIGURATION FILE LOADING
                                    // -----------------------------------------
                                    // Values retrieved from configuration data:
                                    // - Chart Series data
                                    // - Chart title
                                    // - Chart min-y
                                    // - Chart max-y
                                    var json = JsonConvert.DeserializeObject<Dictionary<string, ChartSeriesObject>>(data);

                                    int i = 0;

                                    foreach (var chart in json)
                                    {
                                        ChartSeriesObject obj = chart.Value;

                                        LiveChart.LineChartConfiguration conf = new LiveChart.LineChartConfiguration
                                        {
                                            chartTitle = chart.Key,
                                            minY = obj.minY,
                                            maxY = obj.maxY
                                        };

                                        //RegisterChart(chart.Key, conf);

                                        LiveChart.Builder builder = new LiveChart.Builder(chart.Key, true)
                                            .WithConfiguration(conf);

                                        foreach (var seriesName in obj.seriesNames)
                                        {
                                            builder.AddSeries(seriesName);
                                        }

                                        charts.Add(chart.Key, builder.Build());




                                        //charts[chart.Key].model = ;

                                        Console.WriteLine("Successfully loaded " + chart.Key);
                                        i++;
                                    }

                                    // All charts have now been configured, switch to operational
                                    status = Status.Operational;

                                    var liveCharts = charts.Values.ToArray();

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        window.DataContext = new
                                        {
                                            charts = liveCharts
                                        };
                                    });


                                }
                                break;
                            case Status.Operational:
                                // Connections have been fully established, fetch data and update information
                                if (li.StartsWith(DATA_HEADER))
                                {
                                    // -----------------------------------------
                                    // PARSING CHART DATA
                                    // -----------------------------------------
                                    // The JSON format received from the PROS CLI more or less resembles this hierarchy:
                                    /* ----------------------------------------- /
                                            chart_1
                                            ├─ series_1 
                                            │  ├─ current_value
                                            ├─ series_2 
                                            │  ├─ current_value
                                            ├─ series_3 
                                            │  ├─ current_value
                                            chart_2
                                            ├─ series_1 
                                            │  ├─ current_value
                                            ├─ series_2 
                                            │  ├─ current_value
                                            ├─ series_3 
                                            │  ├─ current_value
                                         / ----------------------------------------- */
                                    var json = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(data);

                                    foreach (var chart in json)
                                    {
                                        int i = 0;

                                        foreach (var variable in chart.Value)
                                        {
                                            // motors.AddPoint(i, frameIncrement, currentFrame, (int)double.Parse(valueSplit[1]));
                                            // Update chart with new points
                                            charts[chart.Key].AddPoint(i++, frameIncrement, currentFrame, double.Parse(variable.Value));
                                        }
                                    }
                                }
                                break;
                            default:
                                break;

                        }
                    }
                    catch (EndOfStreamException)
                    {
                        Console.WriteLine("Reached end of stream, ending...");
                        status = Status.Stopping;
                        break;
                    }

                    currentFrame += frameIncrement;
                }

                // Lost connection, dispose of any ongoing streams and exit the program
                streamReader.Close();
                streamReader.Dispose();

                status = Status.Stopped;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            });



        }
    }




}
