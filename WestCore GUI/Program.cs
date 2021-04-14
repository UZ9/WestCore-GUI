using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WestCore_GUI
{
    static class Program
    {
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

            // The stream reader will be what reads the contents from the Named Pipe
            var streamReader = new StreamReader(namedPipeServer);

            // Wait until C++ client connects
            namedPipeServer.WaitForConnection();

            Console.WriteLine("Waiting for line...");

            Task.Run(() =>
            {
                int x = 0;
                int a = 0;
                int b = 1;
                int c = 2;
                int d = 3;
                while (true)
                {
                    if (Form1.pidChart == null) continue;



                    Form1.pidChart.AddPoint(0, x, a);
                    Form1.pidChart.AddPoint(1, x, b);
                    Form1.pidChart.AddPoint(2, x, c);
                    Form1.pidChart.AddPoint(3, x, d);



                    x++;
                    streamReader.ReadLine();
                    //Console.WriteLine($"Read from pipe client: {streamReader.ReadLine()}");
                }
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
