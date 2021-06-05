using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using GUI_WPF_Migration.Modules.Util;

namespace GUI_WPF_Migration.Modules.Movement
{
    public class OdometryModule : Module
    {
        private double robotHeading;
        private double robotX;
        private double robotY;

        private Grid odomGrid;

        private Border odomGridBorder;

        private Polygon robotArrow;
        private Ellipse robotIcon;
        private Border arrowBorder;

        private Run positionText;
        private Run angleText;
        private Run leftEncoderText;
        private Run rightEncoderText;
        private Run midEncoderText;

        private Random random;

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
            robotX = Convert.ToDouble(VarMap["x"]);
            robotY = Convert.ToDouble(VarMap["y"]);
            robotHeading = Convert.ToDouble(VarMap["heading"]);

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
            odomGrid.SetValue(Grid.RowProperty, ModuleContainer.GetValue(Grid.RowProperty));
            odomGrid.SetValue(Grid.ColumnProperty, ModuleContainer.GetValue(Grid.ColumnProperty));

            // 6 Tiles in a field
            for (var i = 0; i < 6; i++)
            {
                odomGrid.ColumnDefinitions.Add(new ColumnDefinition());
                odomGrid.RowDefinitions.Add(new RowDefinition());
            }

            ModuleContainer.Child = odomGrid;

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
            for (var x = 0; x < odomGrid.RowDefinitions.Count; x++)
            {
                for (var y = 0; y < odomGrid.ColumnDefinitions.Count; y++)
                {
                    odomGrid.Children.Add(GenerateGridTile(x, y));
                }
            }
        }

        /// <summary>
        /// Creates and aligns all TextBlocks used in the <see cref="OdometryModule"/>
        /// </summary>
        private void CreateOdomText()
        {
            var location = ModuleTextFactory.GetTextBuilderTemplate(ModuleTextFactory.TextType.Subtitle)
                .WithMargin(367, 50, 41, 10)
                .WithColor("#ededed")
                .WithFont(28.5)
                .WithGridLocation((int)ModuleContainer.GetValue(Grid.RowProperty), (int)ModuleContainer.GetValue(Grid.ColumnProperty))
                .WithRenderTransformOrigin(1.13, 0.722)
                .WithText("Location")
                .Build();
            var positionContainer = ModuleTextFactory.GetTextBuilderTemplate(ModuleTextFactory.TextType.Paragraph)
                .WithMargin(367, 93, 41, 20)
                .WithGridLocation((int)ModuleContainer.GetValue(Grid.RowProperty), (int)ModuleContainer.GetValue(Grid.ColumnProperty))
                //.WithGridSpan(2, 1)
                .Build();

            positionText = new Run() { Foreground = Brushes.IndianRed, Text = "Position: (0, 0)\n" };
            angleText = new Run() { Foreground = Brushes.IndianRed, Text = "Angle: 0°" };

            positionContainer.Inlines.Add(positionText);
            positionContainer.Inlines.Add(angleText);

            var encoderValues = ModuleTextFactory.GetTextBuilderTemplate(ModuleTextFactory.TextType.Subtitle)
                .WithMargin(367, 195, 41, 20)
                .WithFont(28.5)
                .WithGridLocation((int)ModuleContainer.GetValue(Grid.RowProperty), (int)ModuleContainer.GetValue(Grid.ColumnProperty))
                //.WithGridSpan(2, 1)
                .WithText("Encoder Values")
                .Build();

            var encoderContainer = ModuleTextFactory.GetTextBuilderTemplate(ModuleTextFactory.TextType.Paragraph)
                .WithGridLocation((int)ModuleContainer.GetValue(Grid.RowProperty), (int)ModuleContainer.GetValue(Grid.ColumnProperty))
                //.WithGridSpan(2, 1)
                .WithMargin(367, 238, 41, 20)
                .Build();

            leftEncoderText = new Run() { Foreground = Brushes.IndianRed, Text = "Left: 0.000000\n" };
            rightEncoderText = new Run() { Foreground = Brushes.IndianRed, Text = "Right: 0.000000\n" };
            midEncoderText = new Run() { Foreground = Brushes.IndianRed, Text = "Mid: 0.000000" };

            encoderContainer.Inlines.Add(leftEncoderText);
            encoderContainer.Inlines.Add(rightEncoderText);
            encoderContainer.Inlines.Add(midEncoderText);

            var moduleGrid = MainWindow.Instance.moduleGrid;

            moduleGrid.Children.Add(location);
            moduleGrid.Children.Add(positionContainer);
            moduleGrid.Children.Add(encoderValues);
            moduleGrid.Children.Add(encoderContainer);
        }

        /// <summary>
        /// Creates the visualization for the robot icon
        /// </summary>
        private void CreateRobot()
        {
            var robotBorder = new Border();
            //robotBorder.Margin = new Thickness(1);
            robotBorder.SetValue(Grid.RowSpanProperty, 6);
            robotBorder.SetValue(Grid.ColumnSpanProperty, 6);

            odomGrid.Children.Add(robotBorder);

            robotIcon = new Ellipse
            {
                Height = 15,
                Width = 15,
                Stroke = new SolidColorBrush(Color.FromRgb(39, 43, 77)),
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromRgb(186, 86, 90)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            robotX = 4;
            robotY = 4;

            robotBorder.Child = robotIcon;

            arrowBorder = new Border();

            arrowBorder.SetValue(Grid.RowSpanProperty, 6);
            arrowBorder.SetValue(Grid.ColumnSpanProperty, 6);

            odomGrid.Children.Add(arrowBorder);

            // Create arrow polygon
            robotArrow = new Polygon { Fill = new SolidColorBrush(Color.FromRgb(186, 86, 90)) };

            arrowBorder.Child = robotArrow;

            UpdateRobotPosition();
        }


        /// <summary>
        /// Updates the robot's position based on <see cref="robotX"/>, <see cref="robotY"/>, and <see cref="robotHeading"/>.
        /// </summary>
        private void UpdateRobotPosition()
        {
            var (guiX, guiY) = PosToGuiMargin(robotX, robotY);

            positionText.Text = $"Position: ({Math.Round(robotX, 1):0.0}, {Math.Round(robotY, 1):0.0})\n";
            angleText.Text = $"Angle: {Math.Round(robotHeading, 1)}°";

            // Adjust margin
            robotIcon.Margin = new Thickness(guiX, guiY, 0, 0);

            var (currentX, currentY) = PosToGuiMargin(robotX, robotY);

            // Convert heading to radians
            var radianHeading = ((robotHeading - 90) * Math.PI) / 180.0;

            var xOffset = Math.Cos(radianHeading) * 30;
            var yOffset = Math.Sin(radianHeading) * 30; // Because coordinates are starting from the top left, switch the y coordinate

            var points = CreateLineWithArrowPointCollection(new Point(currentX + 7.5, currentY + 7.5), new Point(currentX + xOffset + (xOffset * 0.7) + 7.5, currentY + yOffset + (yOffset * 0.7) + 7.5), 2);

            robotArrow.Points = points;




        }

        /// <summary>
        /// Retrieves a color for one of the tiles. To introduce a slight bit of variety, the tiles are each assigned slightly different RGB values.
        /// </summary>
        /// <returns>A <see cref="Color"/> to be used in a tile</returns>
        private Color GetGridRectangleColor()
        {
            // Original color
            // 125, 132, 124
            var color = Color.FromRgb(58 * 2, 64 * 2, 112 * 2);

            var randomCoefficient = random.NextDouble() * 0.05 + 0.9;

            // Add random bit of brightness modification
            return Color.FromRgb((byte)(color.R * randomCoefficient), (byte)(color.G * randomCoefficient), (byte)(color.B * randomCoefficient));
        }

        /// <summary>
        /// Generates a Grid Tile at a given row and column
        /// </summary>
        /// <param name="row">The row of the grid tile</param>
        /// <param name="column">The column of the grid tile</param>
        /// <returns>A <see cref="Border"/> WPF element containing a colored rectangle and border</returns>
        private Border GenerateGridTile(int row, int column)
        {
            var border = new Border();

            border.SetValue(Grid.RowProperty, row);
            border.SetValue(Grid.ColumnProperty, column);

            border.BorderThickness = new Thickness(1);

            border.BorderBrush = new SolidColorBrush(Color.FromRgb(39, 43, 77));//new SolidColorBrush(Color.FromRgb(189, 189, 189));//Color.FromRgb(93, 99, 149));

            border.Child = GenerateGridRectangle(row, column);

            return border;
        }

        /// <summary>
        /// Generates a grid rectangle visualization given a row and column
        /// </summary>
        /// <param name="row">The row of the grid tile</param>
        /// <param name="column">The column of the grid tile</param>
        /// <returns>A <see cref="Rectangle"/> WPF element representing the tile's fill</returns>
        private Rectangle GenerateGridRectangle(int row, int column)
        {
            var rectangle = new Rectangle { Fill = new SolidColorBrush(GetGridRectangleColor()) };

            rectangle.SetValue(Grid.RowProperty, row);
            rectangle.SetValue(Grid.ColumnProperty, column);

            rectangle.Opacity = 0.1;

            return rectangle;
        }

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
            var conversionFactor = odomGrid.Width / 6.0;

            return (x * conversionFactor - 7.5, y * conversionFactor - 7.5);
        }

        private const double MaxArrowLengthPercent = 0.3; // factor that determines how the arrow is shortened for very short lines
        private const double LineArrowLengthFactor = 3.73205081; // 15 degrees arrow:  = 1 / Math.Tan(15 * Math.PI / 180); 

        /// <summary>
        /// Creates an arrow between two points
        /// </summary>
        /// <param name="startPoint">The origin to base the arrow off of</param>
        /// <param name="endPoint">The end point of the arrow. This will determine the direction the arrow is facing.</param>
        /// <param name="lineWidth">The width of the arrow</param>
        /// <returns></returns>
        public static PointCollection CreateLineWithArrowPointCollection(Point startPoint, Point endPoint, double lineWidth)
        {
            var direction = endPoint - startPoint;

            var normalizedDirection = direction;
            normalizedDirection.Normalize();

            var normalizedlineWidenVector = new Vector(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            var lineWidenVector = normalizedlineWidenVector * lineWidth * 0.5;

            var lineLength = direction.Length;

            var defaultArrowLength = lineWidth * LineArrowLengthFactor;

            // Prepare usedArrowLength
            // if the length is bigger than 1/3 (_maxArrowLengthPercent) of the line length adjust the arrow length to 1/3 of line length

            double usedArrowLength;
            if (lineLength * MaxArrowLengthPercent < defaultArrowLength)
                usedArrowLength = lineLength * MaxArrowLengthPercent;
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

            var arrowWidthVector = normalizedlineWidenVector * arrowWidthFactor;


            // Now we have all the vectors so we can create the arrow shape positions
            var pointCollection = new PointCollection(7);

            var endArrowCenterPosition = endPoint - (normalizedDirection * usedArrowLength);

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
