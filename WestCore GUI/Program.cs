using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WestCore_GUI.Charts;
using static WestCore_GUI.Form1;

namespace WestCore_GUI
{
    static class Program
    {
        public static LiveChart motors;
        public static bool setup = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine("Creating Named Pipe...");

            // Create new pipe server
            // Parameters:
            // "west-pros-pipe" - name of pipe
            // PipeDirection.InOut - Allows data to be sent and received
            // 1 - Only 1 client will be able to connect at a time  
            // PipeTransmissionMode.Byte - Type of data being transfered
            var namedPipeServer = new NamedPipeServerStream("west-pros-pipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte);

            Console.WriteLine("west-core-pipe successfully initialized");

            Console.WriteLine("Waiting for connection...");



            // Wait until C++ client connects
            namedPipeServer.WaitForConnection();

            // The stream reader will be what reads the contents from the Named Pipe
            var streamReader = new BinaryReader(namedPipeServer);

            Stopwatch watch = new Stopwatch(); // The Stopwatch is used to calculate the interval between each sent. Used for timing diagnostics and making sure intervals are placed correctly

            Console.WriteLine("Waiting for line...");

            Task.Run(() =>
            {
                bool chartBuilt = false;

                int currentFrame = 0;
                int frameIncrement = 20;

                while (true)
                {
                    if (pidChart == null) continue;

                    try
                    {
                        watch.Start();

                        var len = (int)streamReader.ReadUInt32();
                        var li = new string(streamReader.ReadChars(len));

                        Console.WriteLine("Found " + li);



                        if (!li.StartsWith("GUI_DATA_8378"))
                        {
                            WestDebug.Log(Level.Info, li);
                            continue;
                        }

                        li = li.Substring(13);

                        if (li.Length <= 1) continue;
                        if (li.StartsWith("|")) li = li.Substring(1); // TODO: Make this not as makeshift
                        //li += ",";

                        string[] split = li.Split(',');

                        for (int i = 0; i < split.Length; i++)
                        {

                            Console.WriteLine($"Split: {split[i]}");

                            if (split[i].Length <= 1) continue;

                            string[] variableSplit = split[i].Split('|');


                            for (int j = 0; j < variableSplit.Length; j++)
                            {
                                if (variableSplit[j].Length <= 1) continue;

                                string[] valueSplit = variableSplit[j].Split('=');

                                if (!chartBuilt)
                                {
                                    motorsBuilder.AddSeries(variableSplit[0].Split('=')[1]);
                                }
                                else
                                {
                                    lock (motors.model.SyncRoot)
                                    {
                                        motors.AddPoint(i, frameIncrement, currentFrame, (int)double.Parse(valueSplit[1]));
                                    }
                                }
                            }
                        }

                        if (!chartBuilt)
                        {
                            motors = motorsBuilder.Build();
                            chartBuilt = true;


                        }

                        //Form1.pidChart.AddPoint(0, x, a);
                        //Form1.pidChart.AddPoint(1, x, b);
                        //Form1.pidChart.AddPoint(2, x, c);
                        //Form1.pidChart.AddPoint(3, x, d);

                        //x++;

                        watch.Stop();

                        Console.WriteLine("Took " + watch.ElapsedTicks);

                        watch.Restart(); // Reset watch, start counting again

                    }
                    catch (EndOfStreamException)
                    {
                        Console.WriteLine("Reached end of stream, ending...");
                        break;
                    }



                    currentFrame += frameIncrement;
                    //Console.WriteLine($"Read from pipe client: {streamReader.ReadLine()}");
                }

                // Lost connection, dispose of any ongoing streams and exit the program
                streamReader.Close();
                streamReader.Dispose();

                Application.Exit();
            });

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                Console.WriteLine(e.Data.ToString());
        }
    }
}
