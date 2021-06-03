using GUI_WPF_Migration;
using Modules;
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

        // Headers are to differentiate normal cout logs from ones sending data to the GUI. Only strings starting with one of these prefixes will be parsed.
        private const string DATA_HEADER = "GUI_DATA_8378";
        private const string CONFIG_HEADER = "GUI_DATA_CONF_8378"; // TODO: Move this to an automatically generated value based off of DATA_HEADER

        /// <summary>
        /// The amount of milleseconds elapsed between data packets being sent by the PROS CLI. Used for properly differentiating time on the GUI chart time elements.
        /// </summary>
        private const int frameIncrement = 20;

        /// <summary>
        /// Each module is designated with a given module ID. The module ID allows parsed data to go to the appropriate module.
        /// </summary>
        private Dictionary<string, Module> modules;

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

            modules = new Dictionary<string, Module>();
        }

        ~ChartManager()
        {
            Dispose();
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
        }

        public void AwaitPipeConnection()
        {
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
                    try
                    {
                        // Example string received from the stream reader:
                        // CONFIG_HEADER|JSON_DATA
                        var len = (int)streamReader.ReadUInt32();
                        var rawString = new string(streamReader.ReadChars(len));

                        var received = ParseReceivedData(rawString);

                        // If no header/data was found, continue
                        if (!received.HasValue) continue;

                        (string header, string data) = received.Value;

                        switch (status)
                        {
                            // TODO: Change to JSON formatting
                            case Status.AwaitingConfiguration:
                                if (header == CONFIG_HEADER)
                                {
                                    var json = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(data);

                                    foreach (var modulePair in json)
                                    {
                                        // modulePair: 
                                        // Key: Module name
                                        // Value: Config map for module
                                        string type = modulePair.Value["module-type"] as string;


                                        Module newModule = CreateModule(type);


                                        modules.Add(modulePair.Key, newModule);


                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            // Send config data to module to initialize
                                            newModule.Initialize(modulePair.Key, modulePair.Value);
                                        });
                                    }

                                    // All modules have now been configured, switch to operational
                                    status = Status.Operational;
                                }
                                break;
                            case Status.Operational:
                                // Connections have been fully established, fetch data and update information
                                if (header == DATA_HEADER)
                                {
                                    var json = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(data);

                                    foreach (var modulePair in json)
                                    {
                                        // Update module's data
                                        modules[modulePair.Key].varMap = modulePair.Value;

                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            // Call update event for module
                                            modules[modulePair.Key].Update();
                                        });
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

        /// <summary>
        /// Creates a new module based on a given name
        /// </summary>
        /// <param name="name">A moduletype name provided by the C++ program</param>
        /// <returns>A new <see cref="Module"/> based on the given input</returns>
        public Module CreateModule(string name)
        {
            Module newModule = null;

            switch (name.ToLower())
            {
                case "linechart":
                    newModule = new LineChartModule(window.ModuleSlots.Dequeue());
                    break;
                case "odometry":
                    newModule = new OdometryModule(window.ModuleSlots.Dequeue());
                    break;
            }

            return newModule;
        }

        public (string, string)? ParseReceivedData(string rawString)
        {
            string[] rawStringArr = rawString.Split('|');

            // We need to have at least 2 elements when splitting off of '|'. 
            if (rawStringArr.Length != 2)
            {
                // TODO: Redo WestDebug to work with WPF
                //WestDebug.Log(Level.Info, li); // Prints to the GUI logger
                return null;
            }

            // Split between the header and the actual JSON data
            string header = rawStringArr[0];
            string data = rawStringArr[1];

            // Make sure every segment of the file header isn't empty
            if (rawStringArr.Where(s => s.Length == 0).Any()) return null;

            return (header, data);
        }

        /// <summary>
        /// Closes all ongoing streams connected to the <see cref="ChartManager"/>
        /// </summary>
        public void Dispose()
        {
            if (streamReader != null)
            {
                streamReader.Close();
                streamReader.Dispose();
            }

            if (pipeStream != null)
            {
                if (pipeStream.IsConnected)
                    pipeStream.Disconnect();
                pipeStream.Close();
                pipeStream.Dispose();
            }
        }

    }
}
