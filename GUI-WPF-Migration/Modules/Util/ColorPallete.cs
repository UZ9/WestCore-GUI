using System.Windows.Media;
using OxyPlot;

namespace GUI_WPF_Migration.Modules.Util
{
    public static class ColorPallete
    {
        public static readonly Color BackgroundColor = Color.FromRgb(16, 17, 22);
        public static readonly Color PanelColor = Color.FromRgb(24, 27, 31);
        public static readonly Color ChartGridLineColor = Color.FromRgb(53, 57, 60);

        public static readonly OxyColor OxyBackgroundColor = OxyColor.FromRgb(16, 17, 22);
        public static readonly OxyColor OxyPanelColor = OxyColor.FromRgb(24, 27, 31);
        public static readonly OxyColor OxyChartGridLineColor = OxyColor.FromRgb(53, 57, 60);

        public static readonly OxyColor[] OxyDefaultColors = {
            OxyColor.FromRgb(226,77,66),
            OxyColor.FromRgb(103,197,227),
            OxyColor.FromRgb(48,186,13),
            OxyColor.FromRgb(254,253,152),
        };

    }
}