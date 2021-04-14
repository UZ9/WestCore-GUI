using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Media;

using Color = System.Drawing.Color;
using Point = System.Windows.Point;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Windows.Threading;
using WestCore_GUI.Charts;
using System.Threading;

namespace WestCore_GUI
{


    public partial class Form1 : Form
    {
        Random random = new Random();

        public static ListBoxLog listBoxLog;
        public static LiveChart pidChart;



        public Form1()
        {
            InitializeComponent();

            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            listBoxLog = new ListBoxLog(Debug);


            pidChart = new LiveChart.Builder("PID Results", true)
                .AddSeries("Target") // Add series of data
                .AddSeries("P")
                .AddSeries("I")
                .AddSeries("D")
                .Build();


            plotView1.Model = pidChart.model; // Set model


            int numPoints = 250;

            //GenerateRandomData(pidChart, numPoints, 1); // Generate random data
            //GenerateRandomData(pidChart, numPoints, 2); // Generate random data
            //GenerateRandomData(pidChart, numPoints, 3); // Generate random data

            LineSeries series = (pidChart.model.Series[0] as LineSeries);

            // Solid line creation
            //for (int i = 0; i < numPoints; i++)
            //{
            //    DelayAction(20 * i, () =>
            //    {
            //        double x = series.Points.Count > 0 ? series.Points[series.Points.Count - 1].X + 1 : 0;
            //        if (series.Points.Count >= 200)
            //            series.Points.RemoveAt(0);
            //        double y = 0;

            //        y = 1;
            //        chart.AddPoint(0, x, y);
            //    });
            //}


            // Create sideways bar chart
            var model = new PlotModel { Title = "Motors" };

            var barSeries = new BarSeries
            {
                ItemsSource = new List<BarItem>(new[] {
                    new BarItem { Value = 0 },
                    new BarItem { Value = 0 },
                    new BarItem { Value = 0 },
                    new BarItem { Value = 0 }
                }),
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0}",
                LabelMargin = 3,
                FontSize = 16

            };

            model.Series.Add(barSeries);

            OxyColor mainColor = OxyColor.FromRgb(152, 147, 218);

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                ItemsSource = new[]
                {
                    "Top Left",
                    "Bottom Left",
                    "Top Right",
                    "Bottom Right"
                },
                AbsoluteMaximum = 12000,
                AbsoluteMinimum = -12000,
                TicklineColor = mainColor,
                IsZoomEnabled = false

            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = -12000,
                Maximum = 12000,
                AbsoluteMaximum = 12000,
                AbsoluteMinimum = -12000
            });

            plotView2.Model = model;

            // TODO: Move this back to library




            model.PlotAreaBorderColor = mainColor;

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

            for (int i = 0; i < 10000; i++)
            {

                var currentBarSeries = (model.Series[0] as BarSeries);



                DelayAction(10 * i, () =>
                        {
                            var currentData = barSeries.ItemsSource.Cast<BarItem>();

                            double currentLeft = CalculateNewVoltage((int)currentData.ElementAt(0).Value, true);
                            double currentRight = CalculateNewVoltage((int)currentData.ElementAt(3).Value, false);

                            //Console.WriteLine($"Updating with currentLeft: {currentLeft}, currentRight {currentRight}");

                            currentBarSeries.ItemsSource = new[] {
                    new BarItem{Value = currentLeft},
                    new BarItem{Value = currentLeft},
                    new BarItem{Value = currentRight},
                    new BarItem{Value = currentRight}
                            };//(seriesIndex, x, y);
                            model.InvalidatePlot(true);
                        });
            }




        }

        private int leftTarget = -12000, rightTarget = 12000;

        private int CalculateNewVoltage(int currentVoltage, bool isLeftTarget)
        {
            if (currentVoltage == 0)
            {
                WestDebug.Log(Level.Critical, "Critical test - reached 0");
            }

            if (isLeftTarget)
            {
                if (currentVoltage < leftTarget)
                {
                    currentVoltage += 20;
                }
                else if (currentVoltage > leftTarget)
                {
                    currentVoltage -= 20;
                }
                else
                {
                    WestDebug.Log(Level.Error, "Example error");
                    // Inverse target to go the other way
                    leftTarget *= -1;
                }
            }
            else
            {
                if (currentVoltage < rightTarget)
                {
                    currentVoltage += 20;
                }
                else if (currentVoltage > rightTarget)
                {
                    currentVoltage -= 20;
                }
                else
                {
                    WestDebug.Log(Level.Info, "Right Target reached its peak");

                    // Inverse target to go the other way
                    rightTarget *= -1;
                }
            }

            return currentVoltage;
        }

        public static void DelayAction(int millisecond, Action action)
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate

            {
                action.Invoke();
                timer.Stop();
            };

            timer.Interval = TimeSpan.FromMilliseconds(millisecond);
            timer.Start();
        }

        private void GenerateRandomData(LiveChart chart, int numberOfPoints = 250, int seriesIndex = 0)
        {
            LineSeries series = (chart.model.Series[seriesIndex] as LineSeries);

            for (int i = 0; i < numberOfPoints; i++)
            {
                DelayAction(20 * i, () =>
                {
                    double x = series.Points.Count > 0 ? series.Points[series.Points.Count - 1].X + 1 : 0;
                    if (series.Points.Count >= 200)
                        series.Points.RemoveAt(0);
                    double y = 0;

                    y = 0.5 * (-3.2 * Math.Sin(-1.3 * x) - 1.2 * Math.Sin(-1.7 * Math.E * x) + 1.9 * Math.Sin(1.3 * Math.PI * x)) * random.NextDouble() + random.Next(0, 5);
                    chart.AddPoint(seriesIndex, x, y);
                });


            }
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllise);

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void plotView1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public enum Level : int
        {
            Critical = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
            Verbose = 4,
            Debug = 5
        };

        public sealed class ListBoxLog : IDisposable
        {
            private const string DEFAULT_MESSAGE_FORMAT = "{0} [{5}] : {8}";
            private const int DEFAULT_MAX_LINES_IN_LISTBOX = 2000;

            private bool _disposed;
            private ListBox _listBox;
            private string _messageFormat;
            private int _maxEntriesInListBox;
            private bool _canAdd;
            private bool _paused;

            private void OnHandleCreated(object sender, EventArgs e)
            {
                _canAdd = true;
            }
            private void OnHandleDestroyed(object sender, EventArgs e)
            {
                _canAdd = false;
            }
            private void DrawItemHandler(object sender, DrawItemEventArgs e)
            {
                if (e.Index >= 0)
                {
                    e.DrawBackground();
                    e.DrawFocusRectangle();

                    LogEvent logEvent = ((ListBox)sender).Items[e.Index] as LogEvent;

                    // SafeGuard against wrong configuration of list box
                    if (logEvent == null)
                    {
                        logEvent = new LogEvent(Level.Critical, ((ListBox)sender).Items[e.Index].ToString());
                    }

                    Color color;
                    switch (logEvent.Level)
                    {
                        case Level.Critical:
                            color = Color.White;
                            break;
                        case Level.Error:
                            color = Color.Red;
                            break;
                        case Level.Warning:
                            color = Color.Goldenrod;
                            break;
                        case Level.Info:
                            color = Color.Green;
                            break;
                        case Level.Verbose:
                            color = Color.Blue;
                            break;
                        default:
                            color = Color.Black;
                            break;
                    }

                    if (logEvent.Level == Level.Critical)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.Red), e.Bounds);
                    }
                    e.Graphics.DrawString(FormatALogEventMessage(logEvent, _messageFormat), new Font("Nirmala UI", 9.25f, FontStyle.Bold), new SolidBrush(color), e.Bounds);
                }
            }
            private void KeyDownHandler(object sender, KeyEventArgs e)
            {
                if ((e.Modifiers == Keys.Control) && (e.KeyCode == Keys.C))
                {
                    CopyToClipboard();
                }
            }
            private void CopyMenuOnClickHandler(object sender, EventArgs e)
            {
                CopyToClipboard();
            }
            private void CopyMenuPopupHandler(object sender, EventArgs e)
            {
                ContextMenu menu = sender as ContextMenu;
                if (menu != null)
                {
                    menu.MenuItems[0].Enabled = (_listBox.SelectedItems.Count > 0);
                }
            }

            private class LogEvent
            {
                public LogEvent(Level level, string message)
                {
                    EventTime = DateTime.Now;
                    Level = level;
                    Message = message;
                }

                public readonly DateTime EventTime;

                public readonly Level Level;
                public readonly string Message;
            }
            private void WriteEvent(LogEvent logEvent)
            {
                if ((logEvent != null) && (_canAdd))
                {
                    _listBox.BeginInvoke(new AddALogEntryDelegate(AddALogEntry), logEvent);
                }
            }
            private delegate void AddALogEntryDelegate(object item);
            private void AddALogEntry(object item)
            {
                _listBox.Items.Add(item);

                if (_listBox.Items.Count > _maxEntriesInListBox)
                {
                    _listBox.Items.RemoveAt(0);
                }

                if (!_paused) _listBox.TopIndex = _listBox.Items.Count - 1;
            }
            private string LevelName(Level level)
            {
                switch (level)
                {
                    case Level.Critical: return "Critical";
                    case Level.Error: return "Error";
                    case Level.Warning: return "Warning";
                    case Level.Info: return "Info";
                    case Level.Verbose: return "Verbose";
                    case Level.Debug: return "Debug";
                    default: return string.Format("<value={0}>", (int)level);
                }
            }
            private string FormatALogEventMessage(LogEvent logEvent, string messageFormat)
            {
                string message = logEvent.Message;
                if (message == null) { message = "<NULL>"; }
                return string.Format(messageFormat,
                    /* {0} */ logEvent.EventTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    /* {1} */ logEvent.EventTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    /* {2} */ logEvent.EventTime.ToString("yyyy-MM-dd"),
                    /* {3} */ logEvent.EventTime.ToString("HH:mm:ss.fff"),
                    /* {4} */ logEvent.EventTime.ToString("HH:mm:ss"),

                    /* {5} */ LevelName(logEvent.Level)[0],
                    /* {6} */ LevelName(logEvent.Level),
                    /* {7} */ (int)logEvent.Level,

                    /* {8} */ message);
            }
            private void CopyToClipboard()
            {
                if (_listBox.SelectedItems.Count > 0)
                {
                    StringBuilder selectedItemsAsRTFText = new StringBuilder();
                    selectedItemsAsRTFText.AppendLine(@"{\rtf1\ansi\deff0{\fonttbl{\f0\fcharset0 Courier;}}");
                    selectedItemsAsRTFText.AppendLine(@"{\colortbl;\red255\green255\blue255;\red255\green0\blue0;\red218\green165\blue32;\red0\green128\blue0;\red0\green0\blue255;\red0\green0\blue0}");
                    foreach (LogEvent logEvent in _listBox.SelectedItems)
                    {
                        selectedItemsAsRTFText.AppendFormat(@"{{\f0\fs16\chshdng0\chcbpat{0}\cb{0}\cf{1} ", (logEvent.Level == Level.Critical) ? 2 : 1, (logEvent.Level == Level.Critical) ? 1 : ((int)logEvent.Level > 5) ? 6 : ((int)logEvent.Level) + 1);
                        selectedItemsAsRTFText.Append(FormatALogEventMessage(logEvent, _messageFormat));
                        selectedItemsAsRTFText.AppendLine(@"\par}");
                    }
                    selectedItemsAsRTFText.AppendLine(@"}");
                    System.Diagnostics.Debug.WriteLine(selectedItemsAsRTFText.ToString());
                    Clipboard.SetData(DataFormats.Rtf, selectedItemsAsRTFText.ToString());
                }

            }

            public ListBoxLog(ListBox listBox) : this(listBox, DEFAULT_MESSAGE_FORMAT, DEFAULT_MAX_LINES_IN_LISTBOX) { }
            public ListBoxLog(ListBox listBox, string messageFormat) : this(listBox, messageFormat, DEFAULT_MAX_LINES_IN_LISTBOX) { }
            public ListBoxLog(ListBox listBox, string messageFormat, int maxLinesInListbox)
            {
                _disposed = false;

                _listBox = listBox;
                _messageFormat = messageFormat;
                _maxEntriesInListBox = maxLinesInListbox;

                _paused = false;

                _canAdd = listBox.IsHandleCreated;

                _listBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;

                _listBox.HandleCreated += OnHandleCreated;
                _listBox.HandleDestroyed += OnHandleDestroyed;
                _listBox.DrawItem += DrawItemHandler;
                _listBox.KeyDown += KeyDownHandler;

                MenuItem[] menuItems = new MenuItem[] { new MenuItem("Copy", new EventHandler(CopyMenuOnClickHandler)) };
                _listBox.ContextMenu = new ContextMenu(menuItems);
                _listBox.ContextMenu.Popup += new EventHandler(CopyMenuPopupHandler);

                _listBox.DrawMode = DrawMode.OwnerDrawFixed;
            }

            public void Log(string message) { Log(Level.Debug, message); }
            public void Log(string format, params object[] args) { Log(Level.Debug, (format == null) ? null : string.Format(format, args)); }
            public void Log(Level level, string format, params object[] args) { Log(level, (format == null) ? null : string.Format(format, args)); }
            public void Log(Level level, string message)
            {
                WriteEvent(new LogEvent(level, message));
            }

            public bool Paused
            {
                get { return _paused; }
                set { _paused = value; }
            }

            ~ListBoxLog()
            {
                if (!_disposed)
                {
                    Dispose(false);
                    _disposed = true;
                }
            }
            public void Dispose()
            {
                if (!_disposed)
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                    _disposed = true;
                }
            }
            private void Dispose(bool disposing)
            {
                if (_listBox != null)
                {
                    _canAdd = false;

                    _listBox.HandleCreated -= OnHandleCreated;
                    _listBox.HandleCreated -= OnHandleDestroyed;
                    _listBox.DrawItem -= DrawItemHandler;
                    _listBox.KeyDown -= KeyDownHandler;

                    _listBox.ContextMenu.MenuItems.Clear();
                    _listBox.ContextMenu.Popup -= CopyMenuPopupHandler;
                    _listBox.ContextMenu = null;

                    _listBox.Items.Clear();
                    _listBox.DrawMode = DrawMode.Normal;
                    _listBox = null;
                }
            }
        }
    }

    public static class WestDebug
    {
        public static void Log(Form1.Level level, string message)
        {
            Form1.listBoxLog.Log(level, message);
        }
    }
}
