using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using GUI_WPF_Migration.Modules;

namespace GUI_WPF_Migration.Logging
{
    /// <summary>
    /// Used for sending log updates to the logging moudle
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Represents the format for log messages.
        /// {0} - The severity type's prefix
        /// {1} - The message entered
        /// </summary>
        public static string LogFormat = "[{0}] - {1}";

        /// <summary>
        /// Determines the type of logging to be used 
        /// </summary>
        public class Level
        {
            public static readonly Level STDOUT = new Level("STDOUT", Brushes.Gray);
            public static readonly Level DEBUG = new Level("DEBUG", Brushes.LimeGreen);
            public static readonly Level INFO = new Level("INFO", Brushes.Cyan);
            public static readonly Level WARNING = new Level("WARNING", Brushes.Yellow);
            public static readonly Level ERROR = new Level("ERROR", Brushes.Red);
            public static readonly Level SEVERE = new Level("SEVERE", Brushes.White, Brushes.Red);

            public Brush Color { get; }
            public Brush Background { get; }
            public string Name { get; }

            private Level(string name, Brush color, Brush background = null)
            {
                this.Color = color;
                this.Background = background;
                this.Name = name;
            }
        }

        /// <summary>
        /// Logs a message to the Logging <see cref="Module"/> on the GUI pane
        /// </summary>
        /// <param name="level">The level of severity the message should be</param>
        /// <param name="message">The message to be sent</param>
        public static void Log(Level level, string message)
        {
            var logBox = MainWindow.Instance.LogTextBox;

            // Create new run
            var paragraph = new Paragraph { Margin = new Thickness(1) };
            var run = new Run(string.Format(LogFormat, level.Name, message.TrimEnd())) { Foreground = level.Color };

            paragraph.Inlines.Add(run);
            logBox.Document.Blocks.Add(paragraph);

            // Set colors
            if (level.Background != null) run.Background = level.Background;

            logBox.ScrollToEnd();
        }
    }
}