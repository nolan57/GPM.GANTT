using System.Windows;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt.Rendering
{
    /// <summary>
    /// Defines the contract for task bar shape rendering strategies.
    /// </summary>
    public interface ITaskBarShapeRenderer
    {
        /// <summary>
        /// Gets the shape type this renderer handles.
        /// </summary>
        TaskBarShape ShapeType { get; }
        
        /// <summary>
        /// Creates the geometry for the task bar shape.
        /// </summary>
        /// <param name="bounds">The rectangular bounds for the task bar</param>
        /// <param name="cornerRadius">Corner radius for rounded shapes</param>
        /// <param name="parameters">Additional shape-specific parameters</param>
        /// <returns>The path geometry defining the shape</returns>
        Geometry CreateGeometry(Rect bounds, double cornerRadius = 0, ShapeRenderingParameters? parameters = null);
        
        /// <summary>
        /// Creates the visual element for the task bar.
        /// </summary>
        /// <param name="bounds">The rectangular bounds for the task bar</param>
        /// <param name="brush">Fill brush for the shape</param>
        /// <param name="stroke">Stroke brush for the outline</param>
        /// <param name="strokeThickness">Thickness of the outline</param>
        /// <param name="parameters">Additional shape-specific parameters</param>
        /// <returns>The visual element representing the task bar</returns>
        FrameworkElement CreateVisualElement(Rect bounds, Brush brush, Brush stroke, double strokeThickness, ShapeRenderingParameters? parameters = null);
        
        /// <summary>
        /// Gets the hit test geometry for interaction handling.
        /// </summary>
        /// <param name="bounds">The rectangular bounds for the task bar</param>
        /// <param name="parameters">Additional shape-specific parameters</param>
        /// <returns>The geometry used for hit testing</returns>
        Geometry GetHitTestGeometry(Rect bounds, ShapeRenderingParameters? parameters = null);
    }
}