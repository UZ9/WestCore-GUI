using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modules
{
    class ModuleTextBuilder
    {
        private TextBlock block;

        public ModuleTextBuilder()
        {
            block = new TextBlock();
        }

        public ModuleTextBuilder(TextBlock block)
        {
            this.block = block;
        }

        public ModuleTextBuilder WithMargin(int margin)
        {
            block.Margin = new Thickness(margin);
            return this;
        }

        public ModuleTextBuilder WithMargin(int left, int top, int right, int bottom)
        {
            block.Margin = new Thickness(left, top, right, bottom);
            return this;
        }

        public ModuleTextBuilder WithAlignment(TextAlignment alignment)
        {
            block.TextAlignment = alignment;
            return this;
        }

        public ModuleTextBuilder WithRenderTransformOrigin(double x, double y)
        {
            block.RenderTransformOrigin = new Point(x, y);
            return this;
        }

        public ModuleTextBuilder WithText(string text)
        {
            block.Text = text;
            return this;
        }

        public ModuleTextBuilder AddText(string text)
        {
            block.Text += "&#x0a;" + text;
            return this;
        }

        public ModuleTextBuilder WithColor(byte r, byte g, byte b)
        {
            return WithColor(Color.FromRgb(r, g, b));
        }

        public ModuleTextBuilder WithColor(string hex)
        {
            return WithColor((Color)ColorConverter.ConvertFromString(hex));
        }

        public ModuleTextBuilder WithColor(Color color)
        {
            block.Foreground = new SolidColorBrush(color);
            return this;
        }

        public ModuleTextBuilder WithColor(Brush brush)
        {
            block.Foreground = brush;
            return this;
        }

        public ModuleTextBuilder WithFont(double fontSize)
        {
            block.FontSize = fontSize;
            return this;
        }

        public ModuleTextBuilder WithFont(string fontFamily, FontWeight weight, double fontSize)
        {
            block.FontFamily = new FontFamily(fontFamily);
            block.FontSize = fontSize;
            block.FontWeight = weight;

            return this;
        }

        public ModuleTextBuilder WithGridLocation(int gridRow, int gridColumn)
        {
            block.SetValue(Grid.RowProperty, gridRow);
            block.SetValue(Grid.ColumnProperty, gridColumn);

            return this;
        }

        public ModuleTextBuilder WithGridSpan(int rowSpan, int columnSpan)
        {
            block.SetValue(Grid.RowSpanProperty, rowSpan);
            block.SetValue(Grid.ColumnSpanProperty, columnSpan);

            return this;
        }

        public TextBlock Build()
        {
            return block;
        }

    }
}
