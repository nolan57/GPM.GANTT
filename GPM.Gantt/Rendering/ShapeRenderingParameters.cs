using System.Collections.Generic;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt.Rendering
{
    /// <summary>
    /// Contains parameters for customizing task bar shape rendering.
    /// </summary>
    public class ShapeRenderingParameters
    {
        /// <summary>
        /// Corner radius for rounded shapes.
        /// </summary>
        public double CornerRadius { get; set; } = 4.0;
        
        /// <summary>
        /// Height of diamond ends relative to task bar height (0.0 to 1.0).
        /// </summary>
        public double DiamondEndHeight { get; set; } = 0.8;
        
        /// <summary>
        /// Width of diamond ends in pixels.
        /// </summary>
        public double DiamondEndWidth { get; set; } = 12.0;
        
        /// <summary>
        /// Chevron angle in degrees.
        /// </summary>
        public double ChevronAngle { get; set; } = 15.0;
        
        /// <summary>
        /// Custom path geometry for user-defined shapes.
        /// </summary>
        public Geometry? CustomGeometry { get; set; }
        
        /// <summary>
        /// Additional custom properties for extensibility.
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; set; } = new();
        
        /// <summary>
        /// Creates default parameters for a specific shape type.
        /// </summary>
        public static ShapeRenderingParameters CreateDefault(TaskBarShape shapeType)
        {
            return shapeType switch
            {
                TaskBarShape.Rectangle => new ShapeRenderingParameters { CornerRadius = 0 },
                TaskBarShape.RoundedRectangle => new ShapeRenderingParameters { CornerRadius = 6.0 },
                TaskBarShape.DiamondEnds => new ShapeRenderingParameters 
                { 
                    DiamondEndHeight = 0.8, 
                    DiamondEndWidth = 12.0,
                    CornerRadius = 2.0 
                },
                TaskBarShape.Chevron => new ShapeRenderingParameters 
                { 
                    ChevronAngle = 15.0,
                    CornerRadius = 2.0 
                },
                TaskBarShape.Milestone => new ShapeRenderingParameters(),
                _ => new ShapeRenderingParameters()
            };
        }
    }
}