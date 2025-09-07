using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GPM.Gantt.Models;

namespace GPM.Gantt.Rendering
{
    /// <summary>
    /// Renderer for dependency lines between Gantt chart tasks
    /// </summary>
    public class DependencyLineRenderer
    {
        private readonly double _arrowSize = 6.0;
        private readonly double _cornerRadius = 10.0;
        private readonly double _minimumLineSpacing = 20.0;

        /// <summary>
        /// Renders all dependency lines for the given dependencies and task positions
        /// </summary>
        /// <param name="dependencyLines">List of dependency lines to render</param>
        /// <param name="taskPositions">Dictionary mapping task IDs to their screen positions</param>
        /// <returns>List of UI elements representing the dependency lines</returns>
        public List<UIElement> RenderDependencyLines(List<DependencyLine> dependencyLines, 
            Dictionary<string, Rect> taskPositions)
        {
            var elements = new List<UIElement>();

            foreach (var depLine in dependencyLines)
            {
                var lineElements = CreateDependencyLine(depLine, taskPositions);
                elements.AddRange(lineElements);
            }

            return elements;
        }

        /// <summary>
        /// Creates UI elements for a single dependency line
        /// </summary>
        private List<UIElement> CreateDependencyLine(DependencyLine dependencyLine, 
            Dictionary<string, Rect> taskPositions)
        {
            var elements = new List<UIElement>();
            var dependency = dependencyLine.Dependency;

            if (!taskPositions.ContainsKey(dependency.PredecessorTaskId) || 
                !taskPositions.ContainsKey(dependency.SuccessorTaskId))
                return elements;

            var predRect = taskPositions[dependency.PredecessorTaskId];
            var succRect = taskPositions[dependency.SuccessorTaskId];

            var (startPoint, endPoint) = CalculateConnectionPoints(predRect, succRect, dependency.Type);
            var pathPoints = CalculatePathPoints(startPoint, endPoint);

            // Create the dependency line path
            var path = CreateDependencyPath(pathPoints, dependencyLine);
            elements.Add(path);

            // Create arrow head
            if (dependencyLine.ShowArrow)
            {
                var arrow = CreateArrowHead(pathPoints.Last(), pathPoints[pathPoints.Length - 2], dependencyLine);
                elements.Add(arrow);
            }

            // Add lag indicator if needed
            if (dependency.Lag != TimeSpan.Zero && dependencyLine.ShowLagLabel)
            {
                var lagIndicator = CreateLagIndicator(pathPoints, dependency.Lag, dependencyLine);
                elements.Add(lagIndicator);
            }

            // Add dependency type indicator
            var typeIndicator = CreateDependencyTypeIndicator(startPoint, dependency.Type, dependencyLine);
            if (typeIndicator != null)
                elements.Add(typeIndicator);

            return elements;
        }

        /// <summary>
        /// Calculates the connection points based on dependency type
        /// </summary>
        private (Point start, Point end) CalculateConnectionPoints(Rect predRect, Rect succRect, DependencyType type)
        {
            Point start, end;

            switch (type)
            {
                case DependencyType.FinishToStart:
                    start = new Point(predRect.Right, predRect.Top + predRect.Height / 2);
                    end = new Point(succRect.Left, succRect.Top + succRect.Height / 2);
                    break;
                case DependencyType.StartToStart:
                    start = new Point(predRect.Left, predRect.Top + predRect.Height / 2);
                    end = new Point(succRect.Left, succRect.Top + succRect.Height / 2);
                    break;
                case DependencyType.FinishToFinish:
                    start = new Point(predRect.Right, predRect.Top + predRect.Height / 2);
                    end = new Point(succRect.Right, succRect.Top + succRect.Height / 2);
                    break;
                case DependencyType.StartToFinish:
                    start = new Point(predRect.Left, predRect.Top + predRect.Height / 2);
                    end = new Point(succRect.Right, succRect.Top + succRect.Height / 2);
                    break;
                default:
                    start = new Point(predRect.Right, predRect.Top + predRect.Height / 2);
                    end = new Point(succRect.Left, succRect.Top + succRect.Height / 2);
                    break;
            }

            return (start, end);
        }

        /// <summary>
        /// Calculates the path points for smart routing around obstacles
        /// </summary>
        private Point[] CalculatePathPoints(Point start, Point end)
        {
            var points = new List<Point> { start };

            var deltaX = end.X - start.X;
            var deltaY = end.Y - start.Y;

            // Create smart routing with right angles
            if (Math.Abs(deltaY) < 5) // Nearly horizontal
            {
                points.Add(end);
            }
            else if (deltaX > _minimumLineSpacing) // Standard left-to-right
            {
                var midX = start.X + deltaX / 2;
                points.Add(new Point(midX, start.Y));
                points.Add(new Point(midX, end.Y));
                points.Add(end);
            }
            else // Complex routing (overlapping or right-to-left)
            {
                var offsetX = Math.Max(_minimumLineSpacing, -deltaX / 2);
                var midX1 = start.X + offsetX;
                var midX2 = end.X - offsetX;
                var midY = start.Y + deltaY / 2;

                points.Add(new Point(midX1, start.Y));
                points.Add(new Point(midX1, midY));
                points.Add(new Point(midX2, midY));
                points.Add(new Point(midX2, end.Y));
                points.Add(end);
            }

            return points.ToArray();
        }

        /// <summary>
        /// Creates the main dependency path
        /// </summary>
        private Path CreateDependencyPath(Point[] points, DependencyLine dependencyLine)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = points[0] };

            for (int i = 1; i < points.Length; i++)
            {
                figure.Segments.Add(new LineSegment(points[i], true));
            }

            geometry.Figures.Add(figure);

            var path = new Path
            {
                Data = geometry,
                Stroke = dependencyLine.LineColor,
                StrokeThickness = dependencyLine.LineThickness,
                StrokeDashArray = dependencyLine.IsHighlighted ? new DoubleCollection { 4, 2 } : null,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            // Add tooltip with dependency information
            path.ToolTip = CreateDependencyTooltip(dependencyLine.Dependency);

            return path;
        }

        /// <summary>
        /// Creates an arrow head at the end of the dependency line
        /// </summary>
        private Polygon CreateArrowHead(Point tip, Point fromPoint, DependencyLine dependencyLine)
        {
            var angle = Math.Atan2(tip.Y - fromPoint.Y, tip.X - fromPoint.X);
            var arrowAngle = Math.PI / 6; // 30 degrees

            var arrowPoint1 = new Point(
                tip.X - dependencyLine.ArrowSize * Math.Cos(angle - arrowAngle),
                tip.Y - dependencyLine.ArrowSize * Math.Sin(angle - arrowAngle));

            var arrowPoint2 = new Point(
                tip.X - dependencyLine.ArrowSize * Math.Cos(angle + arrowAngle),
                tip.Y - dependencyLine.ArrowSize * Math.Sin(angle + arrowAngle));

            var arrow = new Polygon
            {
                Points = new PointCollection { tip, arrowPoint1, arrowPoint2 },
                Fill = dependencyLine.LineColor,
                Stroke = dependencyLine.LineColor,
                StrokeThickness = 1
            };

            return arrow;
        }

        /// <summary>
        /// Creates a lag time indicator label
        /// </summary>
        private TextBlock CreateLagIndicator(Point[] points, TimeSpan lag, DependencyLine dependencyLine)
        {
            var midPoint = points[points.Length / 2];
            var lagText = FormatLagTime(lag);

            var textBlock = new TextBlock
            {
                Text = lagText,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = dependencyLine.LineColor,
                Background = Brushes.White,
                Padding = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Add border
            var border = new Border
            {
                Child = textBlock,
                BorderBrush = dependencyLine.LineColor,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Background = Brushes.White
            };

            Canvas.SetLeft(border, midPoint.X - 20);
            Canvas.SetTop(border, midPoint.Y - 10);

            return textBlock;
        }

        /// <summary>
        /// Creates a dependency type indicator
        /// </summary>
        private UIElement? CreateDependencyTypeIndicator(Point startPoint, DependencyType type, DependencyLine dependencyLine)
        {
            string typeText = type switch
            {
                DependencyType.FinishToStart => "FS",
                DependencyType.StartToStart => "SS",
                DependencyType.FinishToFinish => "FF",
                DependencyType.StartToFinish => "SF",
                _ => "FS"
            };

            var textBlock = new TextBlock
            {
                Text = typeText,
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                Foreground = dependencyLine.LineColor,
                Background = Brushes.White,
                Padding = new Thickness(1)
            };

            Canvas.SetLeft(textBlock, startPoint.X + 5);
            Canvas.SetTop(textBlock, startPoint.Y - 12);

            return textBlock;
        }

        /// <summary>
        /// Creates a tooltip for dependency information
        /// </summary>
        private ToolTip CreateDependencyTooltip(TaskDependency dependency)
        {
            var tooltip = new ToolTip();
            var stackPanel = new StackPanel();

            // Dependency type
            stackPanel.Children.Add(new TextBlock
            {
                Text = $"Type: {GetDependencyTypeDescription(dependency.Type)}",
                FontWeight = FontWeights.Bold
            });

            // Lag time
            if (dependency.Lag != TimeSpan.Zero)
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Lag: {FormatLagTime(dependency.Lag)}"
                });
            }

            // Description
            if (!string.IsNullOrEmpty(dependency.Description))
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = dependency.Description,
                    FontStyle = FontStyles.Italic,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 200
                });
            }

            // Critical path indicator
            if (dependency.IsCritical)
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = "★ Critical Path",
                    Foreground = Brushes.Red,
                    FontWeight = FontWeights.Bold
                });
            }

            tooltip.Content = stackPanel;
            return tooltip;
        }

        /// <summary>
        /// Formats lag time for display
        /// </summary>
        private string FormatLagTime(TimeSpan lag)
        {
            if (lag.TotalDays >= 1)
                return lag.TotalDays > 0 ? $"+{lag.TotalDays:F0}d" : $"{lag.TotalDays:F0}d";
            else if (lag.TotalHours >= 1)
                return lag.TotalHours > 0 ? $"+{lag.TotalHours:F0}h" : $"{lag.TotalHours:F0}h";
            else
                return lag.TotalMinutes > 0 ? $"+{lag.TotalMinutes:F0}m" : $"{lag.TotalMinutes:F0}m";
        }

        /// <summary>
        /// Gets human-readable description of dependency type
        /// </summary>
        private string GetDependencyTypeDescription(DependencyType type)
        {
            return type switch
            {
                DependencyType.FinishToStart => "Finish-to-Start",
                DependencyType.StartToStart => "Start-to-Start",
                DependencyType.FinishToFinish => "Finish-to-Finish",
                DependencyType.StartToFinish => "Start-to-Finish",
                _ => "Finish-to-Start"
            };
        }
    }
}