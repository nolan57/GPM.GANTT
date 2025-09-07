using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GPM.Gantt.Models;

namespace GPM.Gantt.Rendering.Shapes
{
    /// <summary>
    /// Renders rounded rectangular task bars.
    /// </summary>
    public class RoundedRectangleTaskBarRenderer : ITaskBarShapeRenderer
    {
        public TaskBarShape ShapeType => TaskBarShape.RoundedRectangle;

        public Geometry CreateGeometry(Rect bounds, double cornerRadius = 0, ShapeRenderingParameters? parameters = null)
        {
            var param = parameters ?? ShapeRenderingParameters.CreateDefault(ShapeType);
            var radius = cornerRadius > 0 ? cornerRadius : param.CornerRadius;
            
            return new RectangleGeometry(bounds, radius, radius);
        }

        public FrameworkElement CreateVisualElement(Rect bounds, Brush brush, Brush stroke, double strokeThickness, ShapeRenderingParameters? parameters = null)
        {
            var param = parameters ?? ShapeRenderingParameters.CreateDefault(ShapeType);
            
            var border = new Border
            {
                Background = brush,
                BorderBrush = stroke,
                BorderThickness = new Thickness(strokeThickness),
                CornerRadius = new CornerRadius(param.CornerRadius),
                Width = bounds.Width,
                Height = bounds.Height
            };
            
            Canvas.SetLeft(border, bounds.Left);
            Canvas.SetTop(border, bounds.Top);
            
            return border;
        }

        public Geometry GetHitTestGeometry(Rect bounds, ShapeRenderingParameters? parameters = null)
        {
            var param = parameters ?? ShapeRenderingParameters.CreateDefault(ShapeType);
            return CreateGeometry(bounds, param.CornerRadius, parameters);
        }
    }
}