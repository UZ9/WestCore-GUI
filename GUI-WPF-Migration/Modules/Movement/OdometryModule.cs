using GUI_WPF_Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Modules
{
    public class OdometryModule : Module
    {
        private double robotHeading;
        private double robotX;
        private double robotY;

        private Grid odomGrid;

        private Border odomGridBorder;

        private Ellipse robotIcon;
        private Border arrowBorder;

        private Run positionText;
        private Run angleText;
        private Run leftEncoderText;
        private Run rightEncoderText;
        private Run midEncoderText;

        Random random;

        public OdometryModule(Border moduleContainer) : base(moduleContainer) { }

        public override void Initialize(string title, Dictionary<string, object> configMap)
        {
            random = new Random();

            // Create grid & text visualization
            CreateOdomGrid();
            CreateOdomText();

            // Create robot visualization
            CreateRobot();

        }

        public override void Update()
        {
            // Update robot position with new position values
            robotX = Convert.ToDouble(varMap["x"]);
            robotY = Convert.ToDouble(varMap["y"]);
            robotHeading = Convert.ToDouble(varMap["heading"]);

            Console.WriteLine($"Receiving {robotX}, {robotY}, {robotHeading}");

            UpdateRobotPosition();
        }

        private void CreateOdomGrid()
        {
            odomGrid = new Grid()
            {
                Width = 300,
                Height = 300,
                Margin = new Thickness(-280, 0, 0, 0)
            };


            // Align to container's grid
            odomGrid.SetValue(Grid.RowProperty, moduleContainer.GetValue(Grid.RowProperty));
            odomGrid.SetValue(Grid.ColumnProperty, moduleContainer.GetValue(Grid.ColumnProperty));

            // 6 Tiles in a field
            for (int i = 0; i < 6; i++)
            {
                odomGrid.ColumnDefinitions.Add(new ColumnDefinition());
                odomGrid.RowDefinitions.Add(new RowDefinition());
            }

            moduleContainer.Child = odomGrid;

            odomGridBorder = new Border()
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(39, 43, 77)),
                Margin = new Thickness(1),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2)
            };

            odomGridBorder.SetValue(Grid.RowSpanProperty, 6);
            odomGridBorder.SetValue(Grid.ColumnSpanProperty, 6);

            // Add shadow
            odomGridBorder.Effect = new DropShadowEffect()
            {
                BlurRadius = 15,
                Direction = 320,
                RenderingBias = RenderingBias.Quality,
                ShadowDepth = 3,
                Opacity = 0.6
            };

            odomGrid.Children.Add(odomGridBorder);

            // Generate odometry grid
            for (int x = 0; x < odomGrid.RowDefinitions.Count; x++)
            {
                for (int y = 0; y < odomGrid.ColumnDefinitions.Count; y++)
                {
                    odomGrid.Children.Add(GenerateGridTile(x, y));
                }
            }
        }

        private void CreateOdomText()
        {
            TextBlock location = ModuleTextFactory.GetTextBuilderTemplate(ModuleTextFactory.TextType.Subtitle)
                .WithMargin(367, 50, 41, 10)
                .WithColor("#ededed")
                .WithFont(28.5)
                .WithGridLocation((int)moduleContainer.GetValue(Grid.RowProperty), (int)moduleContainer.GetValue(Grid.ColumnProperty))
                .WithRenderTransformOrigin(1.13, 0.722)
                .WithText("Location")
                .Build();
            TextBlock positionContainer = ModuleTextFactory.GetTextBuilderTemplate(ModuleTextFactory.TextType.Paragraph)
                .WithMargin(367, 93, 41, 355)
                .WithGridLocation((int)moduleContainer.GetValue(Grid.RowProperty), (int)moduleContainer.GetValue(Grid.ColumnProperty))
                .WithGridSpan(2, 1)
                .Build();

            positionText = new Run() { Foreground = Brushes.IndianRed, Text = "Position: (0, 0)\n" };
            angleText = new Run() { Foreground = Brushes.IndianRed, Text = "Angle: 0°" };

            positionContainer.Inlines.Add(positionText);
            positionContainer.Inlines.Add(angleText);

            TextBlock encoderValues = ModuleTextFactory.GetTextBuilderTemplate(ModuleTextFactory.TextType.Subtitle)
                .WithMargin(367, 195, 41, 328)
                .WithFont(28.5)
                .WithGridLocation((int)moduleContainer.GetValue(Grid.RowProperty), (int)moduleContainer.GetValue(Grid.ColumnProperty))
                .WithGridSpan(2, 1)
                .WithText("Encoder Values")
                .Build();

            TextBlock encoderContainer = ModuleTextFactory.GetTextBuilderTemplate(ModuleTextFactory.TextType.Paragraph)
                .WithGridLocation((int)moduleContainer.GetValue(Grid.RowProperty), (int)moduleContainer.GetValue(Grid.ColumnProperty))
                .WithGridSpan(2, 1)
                .WithMargin(367, 238, 41, 319)
                .Build();

            leftEncoderText = new Run() { Foreground = Brushes.IndianRed, Text = "Left: 0.000000\n" };
            rightEncoderText = new Run() { Foreground = Brushes.IndianRed, Text = "Right: 0.000000\n" };
            midEncoderText = new Run() { Foreground = Brushes.IndianRed, Text = "Mid: 0.000000" };

            encoderContainer.Inlines.Add(leftEncoderText);
            encoderContainer.Inlines.Add(rightEncoderText);
            encoderContainer.Inlines.Add(midEncoderText);

            Grid moduleGrid = MainWindow.instance.moduleGrid;

            moduleGrid.Children.Add(location);
            moduleGrid.Children.Add(positionContainer);
            moduleGrid.Children.Add(encoderValues);
            moduleGrid.Children.Add(encoderContainer);
        }

        private void CreateRobot()
        {
            Border robotBorder = new Border();
            //robotBorder.Margin = new Thickness(1);
            robotBorder.SetValue(Grid.RowSpanProperty, 6);
            robotBorder.SetValue(Grid.ColumnSpanProperty, 6);

            odomGrid.Children.Add(robotBorder);

            robotIcon = new Ellipse();

            //robotIcon.Margin = new Thickness(odomGrid.Width / 2.0 - 8, odomGrid.Height / 2.0 - 8, 0, 0);
            robotIcon.Height = 15;
            robotIcon.Width = 15;
            robotIcon.Stroke = new SolidColorBrush(Color.FromRgb(39, 43, 77)); //new SolidColorBrush(Color.FromRgb(189, 189, 189));
            robotIcon.StrokeThickness = 1;
            robotIcon.Fill = new SolidColorBrush(Color.FromRgb(186, 86, 90));
            robotIcon.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            robotIcon.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            robotX = 4;
            robotY = 4;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////

            robotBorder.Child = robotIcon;

            arrowBorder = new Border();
            //robotBorder.Margin = new Thickness(1);
            arrowBorder.SetValue(Grid.RowSpanProperty, 6);
            arrowBorder.SetValue(Grid.ColumnSpanProperty, 6);


            odomGrid.Children.Add(arrowBorder);

            //MouseDown += Window_MouseDown;

            UpdateRobotPosition();
        }

        private void UpdateRobotPosition()
        {
            (double guiX, double guiY) = PosToGuiMargin(robotX, robotY);

            positionText.Text = $"Position: ({Math.Round(robotX, 1)}, {Math.Round(robotY, 1)})\n";
            angleText.Text = $"Angle: {Math.Round(robotHeading, 1)}°";

            // Adjust margin
            robotIcon.Margin = new Thickness(guiX, guiY, 0, 0);

            /////////////////////////////////////////////////////////////////


            (double currentX, double currentY) = PosToGuiMargin(robotX, robotY);

            // Convert heading to radians
            double radianHeading = ((robotHeading - 90) * Math.PI) / 180.0;

            double xOffset = Math.Cos(radianHeading) * 30;
            double yOffset = Math.Sin(radianHeading) * 30; // Because coordinates are starting from the top left, switch the y coordinate

            var points = CreateLineWithArrowPointCollection(new Point(currentX + 7.5, currentY + 7.5), new Point(currentX + xOffset + (xOffset * 0.7) + 7.5, currentY + yOffset + (yOffset * 0.7) + 7.5), 2);



            var polygon = new Polygon();
            polygon.Points = points;
            polygon.Fill = new SolidColorBrush(Color.FromRgb(186, 86, 90));

            arrowBorder.Child = polygon;
        }

        private Color GetGridRectangleColor()
        {
            // Original color
            // 125, 132, 124
            Color color = Color.FromRgb(58 * 2, 64 * 2, 112 * 2);

            double randomCoefficient = random.NextDouble() * 0.05 + 0.9;

            // Add random bit of brightness modification
            return Color.FromRgb((byte)(color.R * randomCoefficient), (byte)(color.G * randomCoefficient), (byte)(color.B * randomCoefficient));
        }

        private Border GenerateGridTile(int row, int column)
        {
            Border border = new Border();

            border.SetValue(Grid.RowProperty, row);
            border.SetValue(Grid.ColumnProperty, column);

            border.BorderThickness = new Thickness(1);

            border.BorderBrush = new SolidColorBrush(Color.FromRgb(39, 43, 77));//new SolidColorBrush(Color.FromRgb(189, 189, 189));//Color.FromRgb(93, 99, 149));

            border.Child = GenerateGridRectangle(row, column);

            return border;
        }

        private Rectangle GenerateGridRectangle(int row, int column)
        {
            Rectangle rectangle = new Rectangle();

            rectangle.Fill = new SolidColorBrush(GetGridRectangleColor());

            rectangle.SetValue(Grid.RowProperty, row);
            rectangle.SetValue(Grid.ColumnProperty, column);

            rectangle.Opacity = 0.1;

            return rectangle;
        }

        //private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        //{

        //    Point gridPos = e.GetPosition(odomGridBorder);

        //    if (gridPos.X < 0 || gridPos.Y < 0 || gridPos.X > 300 || gridPos.Y > 300) return;



        //    double targetRobotX = gridPos.X / (odomGrid.Width / 6.0);
        //    double targetRobotY = gridPos.Y / (odomGrid.Width / 6.0);

        //    robotHeading += new Random().Next(1, 60);

        //    if (robotHeading > 360) robotHeading -= 360;

        //    robotX = targetRobotX;
        //    robotY = targetRobotY;

        //    UpdateRobotPosition();

        //    //if (e.ChangedButton == MouseButton.Left)
        //    //{
        //    //    DragMove();
        //    //}
        //}

        /// <summary>
        /// Converts an x,y field position to GUI margin coordinates
        /// </summary>
        /// <param name="x">The x coordinate of the field, ranging from 0 to 6</param>
        /// <param name="y">The y coordinate of the field, ranging from 0 to 6</param>
        /// <returns>The translated GUI margin coordinates</returns>
        public (double, double) PosToGuiMargin(double x, double y)
        {
            // Full width: odomGrid.Width - 8 (the 8 is the border)
            // Because the grid is a square, we don't have to worry about the grid size being different for each axis
            // Field: 6 tiles in length/width
            double conversionFactor = odomGrid.Width / 6.0;

            return (x * conversionFactor - 7.5, y * conversionFactor - 7.5);
        }

        private const double _maxArrowLengthPercent = 0.3; // factor that determines how the arrow is shortened for very short lines
        private const double _lineArrowLengthFactor = 3.73205081; // 15 degrees arrow:  = 1 / Math.Tan(15 * Math.PI / 180); 

        public static PointCollection CreateLineWithArrowPointCollection(Point startPoint, Point endPoint, double lineWidth)
        {
            Vector direction = endPoint - startPoint;

            Vector normalizedDirection = direction;
            normalizedDirection.Normalize();

            Vector normalizedlineWidenVector = new Vector(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            Vector lineWidenVector = normalizedlineWidenVector * lineWidth * 0.5;

            double lineLength = direction.Length;

            double defaultArrowLength = lineWidth * _lineArrowLengthFactor;

            // Prepare usedArrowLength
            // if the length is bigger than 1/3 (_maxArrowLengthPercent) of the line length adjust the arrow length to 1/3 of line length

            double usedArrowLength;
            if (lineLength * _maxArrowLengthPercent < defaultArrowLength)
                usedArrowLength = lineLength * _maxArrowLengthPercent;
            else
                usedArrowLength = defaultArrowLength;

            // Adjust arrow thickness for very thick lines
            double arrowWidthFactor;
            if (lineWidth <= 1.5)
                arrowWidthFactor = 3;
            else if (lineWidth <= 2.66)
                arrowWidthFactor = 4;
            else
                arrowWidthFactor = 1.5 * lineWidth;

            Vector arrowWidthVector = normalizedlineWidenVector * arrowWidthFactor;


            // Now we have all the vectors so we can create the arrow shape positions
            var pointCollection = new PointCollection(7);

            Point endArrowCenterPosition = endPoint - (normalizedDirection * usedArrowLength);

            pointCollection.Add(endPoint); // Start with tip of the arrow
            pointCollection.Add(endArrowCenterPosition + arrowWidthVector);
            pointCollection.Add(endArrowCenterPosition + lineWidenVector);
            pointCollection.Add(startPoint + lineWidenVector);
            pointCollection.Add(startPoint - lineWidenVector);
            pointCollection.Add(endArrowCenterPosition - lineWidenVector);
            pointCollection.Add(endArrowCenterPosition - arrowWidthVector);

            return pointCollection;
        }
    }
}
