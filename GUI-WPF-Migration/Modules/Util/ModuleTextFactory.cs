using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI_WPF_Migration.Modules.Util
{
    internal static class ModuleTextFactory
    {
        public enum TextType
        {
            Title,
            Subtitle,
            Paragraph
        }

        public static ModuleTextBuilder GetTextBuilderTemplate(TextType type)
        {
            TextBlock textBlock = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("Roboto"),
                FontWeight = FontWeights.Normal,
                RenderTransformOrigin = new Point(1.13, 0.722) // When I created this in the XAML view, this one was one the properties. I'm not exactly sure how it got here 
            };

            switch (type)
            {
                case TextType.Title:
                    textBlock.Foreground = Brushes.White;
                    textBlock.FontSize = 30;
                    break;
                case TextType.Subtitle:
                    textBlock.FontSize = 28.5;
                    textBlock.Foreground = new SolidColorBrush(Color.FromRgb(237, 237, 237));
                    break;
                case TextType.Paragraph:
                    textBlock.FontSize = 24;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return new ModuleTextBuilder(textBlock);
        }

    }
}
