using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GPM.Gantt.Models;

namespace GPM.Gantt.Rendering.Shapes
{
    /// <summary>
    /// Renders milestone markers as diamond shapes.
    /// </summary>
    public class MilestoneTaskBarRenderer : ITaskBarShapeRenderer
    {
        public TaskBarShape ShapeType => TaskBarShape.Milestone;

        public Geometry CreateGeometry(Rect bounds, double cornerRadius = 0, ShapeRenderingParameters? parameters = null)
        {
            var centerX = bounds.X + bounds.Width / 2;
            var centerY = bounds.Y + bounds.Height / 2;
            var halfWidth = bounds.Width / 2;
            var halfHeight = bounds.Height / 2;
            
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            
            // Diamond shape: top, right, bottom, left
            figure.StartPoint = new Point(centerX, bounds.Top);
            figure.Segments.Add(new LineSegment { Point = new Point(bounds.Right, centerY) });
            figure.Segments.Add(new LineSegment { Point = new Point(centerX, bounds.Bottom) });
            figure.Segments.Add(new LineSegment { Point = new Point(bounds.Left, centerY) });
            
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