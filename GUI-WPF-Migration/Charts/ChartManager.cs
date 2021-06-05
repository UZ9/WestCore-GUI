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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Charts
{
    public class ChartManager
    {
        /// <summary>
        /// Represents the main states of the <see cref="ChartManager"/> at any given time. 
        /// </summary>
        public enum CmStatus
        {
            Stopped, // The NamedPipe is not currently running
            Loading, // The NamedPipe is being created
            AwaitingConnection, // The NamedPipe is waiting for a client (PROS CLI) to connect
            AwaitingConfiguration, // The ChartManager is awaiting a configuration header from the named pipe
            Operational, // The ChartManager has been successfully loaded and configured and is now listening for any data coming through the named pipe
            Stopping // The ChartManager is in the process of shutting down and switching back to STOPPED
        }

        // Headers are to differentiate normal cout logs from ones sending data to the GUI. Only strings starting with one of these prefixes will be parsed.
        private const string DataHeader = "GUI_DATA_8378";
        private const string ConfigHeader = "GUI_DATA_CONF_8378"; // TODO: Move this to an automatically generated value based off of DATA_HEADER

        // An issue encountered when sending huge amounts of configuration data (~1000 characters or more) was the buffer size. The configuration string was being chopped off as it reached the CLI,
        // meaning the JSON would freak out and break. To solve this, data is sent in a group of configuration strings until the CONFIG_END_HEADER is reached, where it is then processed.
        private const string ConfigEndHeader = "GUI_DATA_CONF_3434_END";

        /// <summary>
        /// The amount of milleseconds elapsed between data packets being sent by the PROS CLI. Used for properly differentiating time on the GUI chart time elements.
        /// </summary>
        private const int FrameIncrement = 20;

        /// <summary>
        /// Each module is designated with a given module ID. The module ID allows parsed data to go to the appropriate module.
        /// </summary>
        private readonly Dictionary<string, Module> modules;

        /// <summary>
        /// The NamedPipe used to transfer information from the PROS CLI to the GUI application
        /// </summary>
        private NamedPipeServerStream pipeStream;

        /// <summary>
        /// Allows bytes to be read from the <see cref="NamedPipeServerStream"/>
        /// </summary>
        private BinaryReader streamReader;

        /// <summary>
        /// The current CmStatus of the <see cref="ChartManager"/>
        /// </summary>
        public CmStatus Status;

        /// <summary>
        /// Reference to main window
        /// </summary>
        private readonly MainWindow window;

        /// <summary>
        /// Stores the token source for the chartLoop task
        /// </summary>
        private CancellationTokenSource cancelSource = new CancellationTokenSource();

        /// <summary>
        /// Stores the <see cref="CancellationToken"/> of the chartLoop. Used for cancelling the task in <see cref="Dispose"/>
        /// </summary>
        private CancellationToken chartLoopToken;

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
        /// Creates the NamedPipeServerStream labeled "west-pros-pipe"
        /// </summary>
        public void HostPipeServer()
        {
            Status = CmStatus.Loading;

            Console.WriteLine("Creating Named Pipe...");

            // Create new pipe server
            // Parameters:
            // "west-pros-pipe" - name of pipe
            // PipeDirection.InOut - Allows data to be sent and received
            // 1 - Only 1 client will be able to connect at a time  
            // PipeTransmissionMode.Byte - Type of data being transferred
            pipeStream = new NamedPipeServerStream("west-pros-pipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte);

            Console.WriteLine("west-core-pipe successfully initialized");
        }

        /// <summary>
        /// Blocks the main thread until a successful connection to the <see cref="NamedPipeServerStream"/>.
        /// </summary>
        public void AwaitPipeConnection()
        {
            Console.WriteLine("Waiting for connection...");

            Status = CmStatus.AwaitingConnection;

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
            //  The named pipe has been created, we now need to wait until the ConfigHeader is received
            Status = CmStatus.AwaitingConfiguration;

            var configString = "";

            chartLoopToken = cancelSource.Token;

            Task.Run(() =>
            {
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

                        var (header, data) = received.Value;

                        try
                        {

                            switch (Status)
                            {
                                // TODO: Change to JSON formatting
                                case CmStatus.AwaitingConfiguration:
                                    if (header == ConfigHeader)
                                    {
                                        if (data.TrimEnd().EndsWith(ConfigEndHeader))
                                        {
                                            Console.WriteLine("Found config end header");

                                            // Keep appending to the config string until all data has been received (see comments on CONFIG_END_HEADER)
                                            configString += data.Substring(0, data.Length - ConfigEndHeader.Length - 1);

                                            Console.WriteLine("End config string:");
                                            Console.WriteLine(configString);

                                            var json = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(configString);

                                            foreach (var modulePair in json)
                                            {
                                                // modulePair: 
                                                // Key: Module name
                                                // Value: Config map for module
                                                string type = modulePair.Value["module-type"] as string;

                                                var newModule = CreateModule(type);

                                                modules.Add(modulePair.Key, newModule);

                                                Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                    Console.WriteLine("Initializing " + modulePair.Key);

                                                    try
                                                    {
                                                        // Send config data to module to initialize
                                                        newModule.Initialize(modulePair.Key, modulePair.Value);
                                                    }
                                                    catch (KeyNotFoundException e)
                                                    {
                                                        Console.WriteLine(e);
                                                    }

                                                    Console.WriteLine("Done");
                                                });
                                            }

                                            // All modules have now been configured, switch to operational
                                            Status = CmStatus.Operational;
                                        }
                                        else
                                        {
                                            // Remove new lines from data
                                            configString += Regex.Replace(data, @"\n", "");
                                        }

                                    }
                                    break;
                                case CmStatus.Operational:
                                    // Connections have been fully established, fetch data and update information
                                    if (header == DataHeader)
                                    {

                                        var json = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(data);

                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            foreach (var modulePair in json)
                                            {
                                                // Update module's data
                                                modules[modulePair.Key].varMap = modulePair.Value;


                                                // Call update event for module
                                                modules[modulePair.Key].Update();

                                            }
                                        });
                                    }
                                    break;
                                default:
                                    break;

                            }
                        }
                        catch (JsonReaderException e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        Console.WriteLine("Reached end of stream, ending...");
                        Status = CmStatus.Stopping;
                        break;
                    }
                }

                // Lost connection, dispose of any ongoing streams and exit the program
                streamReader.Close();
                streamReader.Dispose();

                Status = CmStatus.Stopped;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }, chartLoopToken);
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

        /// <summary>
        /// Parses a string into tis header and data components
        /// </summary>
        /// <param name="rawString">A JSON string along with a heading prefix</param>
        /// <returns>Both the JSON data string and header string</returns>
        public (string, string)? ParseReceivedData(string rawString)
        {
            var rawStringArr = rawString.Split('|');

            // We need to have at least 2 elements when splitting off of '|'. 
            if (rawStringArr.Length != 2)
            {
                // TODO: Redo WestDebug to work with WPF
                //WestDebug.Log(Level.Info, li); // Prints to the GUI logger
                return null;
            }

            // Split between the header and the actual JSON data
            var header = rawStringArr[0];
            var data = rawStringArr[1];

            // Make sure every segment of the file header isn't empty
            if (rawStringArr.Any(s => s.Length == 0)) return null;

            return (header, data);
        }

        /// <summary>
        /// Closes all ongoing streams connected to the <see cref="ChartManager"/>
        /// </summary>
        public void Dispose()
        {
            // Cancel the task loop
            if (cancelSource != null)
            {
                cancelSource.Cancel();
                cancelSource = null;
            }

            // Shut down the streamReader connected to the pipeStream
            if (streamReader != null)
            {
                streamReader.Close();
                streamReader.Dispose();
            }

            // Shut down the pipe stream
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
