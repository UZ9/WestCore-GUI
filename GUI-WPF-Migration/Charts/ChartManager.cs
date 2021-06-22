using GUI_WPF_Migration;
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
using GUI_WPF_Migration.Logging;
using GUI_WPF_Migration.Modules;
using GUI_WPF_Migration.Modules.Charts.Types;
using GUI_WPF_Migration.Modules.Movement;

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
        private const string LogHeader = "LOG_HEADER_2399";

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
        public readonly Dictionary<string, Module> Modules;

        /// <summary>
        /// The NamedPipe used to transfer information from the PROS CLI to the GUI application
        /// </summary>
        private NamedPipeServerStream readPipeStream;

        /// <summary>
        /// The NamedPipe client used to transfer information from the GUI application to the PROS CLI
        /// </summary>
        private NamedPipeServerStream writePipeStream;

        /// <summary>
        /// Allows bytes to be read from the <see cref="NamedPipeServerStream"/>
        /// </summary>
        private BinaryReader streamReader;

        /// <summary>
        /// Allows text to be written to the CLI through a <see cref="NamedPipeServerStream"/>
        /// </summary>
        private StreamWriter streamWriter;

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

            Modules = new Dictionary<string, Module>();
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

            // Create new pipe servers
            // Parameters:
            // "west-pros-pipe" - name of pipe
            // PipeDirection.InOut - Allows data to be sent and received
            // 1 - Only 1 client will be able to connect at a time  
            // PipeTransmissionMode.Byte - Type of data being transferred
            // Pipe names are from the perspective of the CLI
            readPipeStream = new NamedPipeServerStream("pros-gui-writer-pipe", PipeDirection.InOut, 2, PipeTransmissionMode.Byte);

            writePipeStream = new NamedPipeServerStream("pros-gui-reader-pipe", PipeDirection.InOut, 2,
                PipeTransmissionMode.Message);

            Console.WriteLine("west-core-pipe successfully initialized");
        }

        /// <summary>
        /// Blocks the main thread until a successful connection to the <see cref="NamedPipeServerStream"/>.
        /// </summary>
        public void AwaitPipeConnections()
        {
            Console.WriteLine("Waiting for connection...");

            Status = CmStatus.AwaitingConnection;

            // Wait until C++ client connects
            readPipeStream.WaitForConnection();
            writePipeStream.WaitForConnection();

            // The stream reader will be what reads the contents from the Named Pipe
            streamReader = new BinaryReader(readPipeStream);
            streamWriter = new StreamWriter(writePipeStream);

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

            Task.Run(WriteTask, chartLoopToken);

            try
            {
                Task.Run(() =>
                {
                    while (true) // Continuously pull data from piped server
                    {
                        try
                        {
                            if (chartLoopToken.IsCancellationRequested || streamReader == null ||
                                !readPipeStream.IsConnected) break;

                            // Example string received from the stream reader:
                            // CONFIG_HEADER|JSON_DATA
                            var len = (int)streamReader.ReadUInt32();
                            var rawString = new string(streamReader.ReadChars(len));

                            var received = ParseReceivedData(rawString);

                            // If no header/data was found, continue
                            if (!received.HasValue)
                            {
                                // Because logging has 2 '|' characters, it will always fail the ParsedReceivedData method.
                                // Here we can check for the LOG_HEADER and send the appropriate Logger.Level.
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (rawString.StartsWith(LogHeader))
                                    {
                                        var split = rawString.Split('|');

                                        Logger.Level loggingLevel;

                                        switch (split[1])
                                        {
                                            case "DEBUG":
                                                loggingLevel = Logger.Level.DEBUG;
                                                break;
                                            case "WARNING":
                                                loggingLevel = Logger.Level.WARNING;
                                                break;
                                            case "ERROR":
                                                loggingLevel = Logger.Level.ERROR;
                                                break;
                                            case "SEVERE":
                                                loggingLevel = Logger.Level.SEVERE;
                                                break;
                                            case "INFO":
                                                loggingLevel = Logger.Level.INFO;
                                                break;
                                            default:
                                                loggingLevel = Logger.Level.INFO;
                                                break;
                                        }

                                        Logger.Log(loggingLevel, split[2]);
                                    }
                                    else
                                    {
                                        Logger.Log(Logger.Level.STDOUT, rawString);
                                    }

                                });

                                continue;
                            }

                            var (header, data) = received.Value;


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
                                            configString += data.Substring(0,
                                                data.Length - ConfigEndHeader.Length - 1);

                                            Console.WriteLine("End config string:");
                                            Console.WriteLine(configString);

                                            var json = JsonConvert
                                                .DeserializeObject<Dictionary<string, Dictionary<string, object>>>(
                                                    configString);

                                            foreach (var modulePair in json)
                                            {
                                                // modulePair: 
                                                // Key: Module name
                                                // Value: Config map for module
                                                var type = modulePair.Value["module-type"] as string;

                                                var newModule = CreateModule(type);

                                                Modules.Add(modulePair.Key, newModule);

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

                                        var json = JsonConvert
                                            .DeserializeObject<Dictionary<string, Dictionary<string, object>>>(
                                                data);

                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            foreach (var modulePair in json)
                                            {
                                                // Update module's data
                                                Modules[modulePair.Key].VarMap = modulePair.Value;


                                                // Call update event for module
                                                Modules[modulePair.Key].Update();

                                            }
                                        });
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException("An invalid CmStatus was found in the ChartManager loop.");
                            }

                        }
                        catch (EndOfStreamException) // Called when the pipe connection ends
                        {
                            Console.WriteLine("Reached end of stream, ending...");
                            break;
                        }

                    }

                    // Lost connection, dispose of any ongoing streams and exit the program
                    streamReader.Close();
                    streamReader.Dispose();

                    Status = CmStatus.Stopped;
                    Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                }, chartLoopToken);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Successfully cancelled the Read Task.");
            }
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
                case "barchart":
                    newModule = new BarChartModule(window.ModuleSlots.Dequeue());
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
            cancelSource?.Cancel();

            // Shut down the pipe stream
            if (readPipeStream != null)
            {
                if (readPipeStream.IsConnected)
                    readPipeStream.Disconnect();
                readPipeStream.Close();
                readPipeStream.Dispose();
            }
        }

        private async Task WriteTask()
        {
            while (true)
            {
                if (chartLoopToken.IsCancellationRequested) break;

                streamWriter.WriteLine("Hello from the GUI!");
                streamWriter.Flush();

                try
                {
                    await Task.Delay(1000, chartLoopToken);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Successfully closed the Write Task.");
                }

            }
        }

    }
}
