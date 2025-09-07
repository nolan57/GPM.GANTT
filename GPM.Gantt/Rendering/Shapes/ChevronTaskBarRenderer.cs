using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GPM.Gantt.Models;

namespace GPM.Gantt.Rendering.Shapes
{
    /// <summary>
    /// Renders chevron/arrow-shaped task bars.
    /// </summary>
    public class ChevronTaskBarRenderer : ITaskBarShapeRenderer
    {
        public TaskBarShape ShapeType => TaskBarShape.Chevron;

        public Geometry CreateGeometry(Rect bounds, double cornerRadius = 0, ShapeRenderingParameters? parameters = null)
        {
            var param = parameters ?? ShapeRenderingParameters.CreateDefault(ShapeType);
            var angle = Math.PI * param.ChevronAngle / 180.0; // Convert to radians
            var arrowWidth = bounds.Height * Math.Tan(angle);
            
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            
            // Start from top-left
            figure.StartPoint = new Point(bounds.Left, bounds.Top);
            
            // Top edge to arrow point
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Right - arrowWidth, bounds.Top) 
            });
            
            // Arrow point
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Right, bounds.Y + bounds.Height / 2) 
            });
            
            // Bottom edge from arrow point
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Right - arrowWidth, bounds.Bottom) 
            });
            
            // Bottom edge to left
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Left, bounds.Bottom) 
            });
            
            figure.IsClosed = true;
            geometry.Figures.Add(figure);
            
            return geometry;
        }

        public FrameworkElement CreateVisualElement(Rect bounds, Brush brush, Brush stroke, double strokeThickness, ShapeRenderingParameters? parameters = null)
        {
            var geometry = CreateGeometry(bounds, 0, parameters);
            
            var path = new Path
            {
                Data = geometry,
                Fill = brush,
                Stroke = stroke,
                StrokeThickness = strokeThickness,
                Width = bounds.Width,
                Height = bounds.Height
            };
            
            Canvas.SetLeft(path, bounds.Left);
            Canvas.SetTop(path, bounds.Top);
            
            return path;
        }

        public Geometry GetHitTestGeometry(Rect bounds, ShapeRenderingParameters? parameters = null)
        {
            return CreateGeometry(bounds, 0, parameters);
        }
    }
}