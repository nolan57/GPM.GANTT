using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GPM.Gantt.Models;

namespace GPM.Gantt.Rendering.Shapes
{
    /// <summary>
    /// Renders task bars with diamond-shaped ends.
    /// </summary>
    public class DiamondEndTaskBarRenderer : ITaskBarShapeRenderer
    {
        public TaskBarShape ShapeType => TaskBarShape.DiamondEnds;

        public Geometry CreateGeometry(Rect bounds, double cornerRadius = 0, ShapeRenderingParameters? parameters = null)
        {
            var param = parameters ?? ShapeRenderingParameters.CreateDefault(ShapeType);
            var diamondWidth = Math.Min(param.DiamondEndWidth, bounds.Width / 4);
            var diamondHeight = bounds.Height * param.DiamondEndHeight;
            var centerY = bounds.Y + bounds.Height / 2;
            
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            
            // Start from left diamond point
            figure.StartPoint = new Point(bounds.Left, centerY);
            
            // Left diamond top
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Left + diamondWidth / 2, bounds.Y + (bounds.Height - diamondHeight) / 2) 
            });
            
            // Top edge to right diamond
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Right - diamondWidth / 2, bounds.Y + (bounds.Height - diamondHeight) / 2) 
            });
            
            // Right diamond top
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Right, centerY) 
            });
            
            // Right diamond bottom
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Right - diamondWidth / 2, bounds.Y + (bounds.Height + diamondHeight) / 2) 
            });
            
            // Bottom edge to left diamond
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Left + diamondWidth / 2, bounds.Y + (bounds.Height + diamondHeight) / 2) 
            });
            
            // Left diamond bottom, close the path
            figure.Segments.Add(new LineSegment 
            { 
                Point = new Point(bounds.Left, centerY) 
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
            // Use the same geometry for hit testing
            return CreateGeometry(bounds, 0, parameters);
        }
    }
}