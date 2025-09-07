using System.Windows;
using System.Windows.Media;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Represents a visual dependency line between two tasks
    /// </summary>
    public class DependencyLine
    {
        /// <summary>
        /// The dependency relationship this line represents
        /// </summary>
        public TaskDependency Dependency { get; set; } = new();
        
        /// <summary>
        /// Starting point of the dependency line
        /// </summary>
        public Point StartPoint { get; set; }
        
        /// <summary>
        /// Ending point of the dependency line
        /// </summary>
        public Point EndPoint { get; set; }
        
        /// <summary>
        /// Control points for creating curved or angled lines
        /// </summary>
        public Point[] ControlPoints { get; set; } = new Point[0];
        
        /// <summary>
        /// Whether this line should be highlighted
        /// </summary>
        public bool IsHighlighted { get; set; }
        
        /// <summary>
        /// Color of the dependency line
        /// </summary>
        public Brush LineColor { get; set; } = Brushes.DarkBlue;
        
        /// <summary>
        /// Thickness of the dependency line
        /// </summary>
        public double LineThickness { get; set; } = 1.0;
        
        /// <summary>
        /// Whether to show an arrow at the end of the line
        /// </summary>
        public bool ShowArrow { get; set; } = true;
        
        /// <summary>
        /// Size of the arrow head
        /// </summary>
        public double ArrowSize { get; set; } = 6.0;
        
        /// <summary>
        /// Whether to show lag time label
        /// </summary>
        public bool ShowLagLabel { get; set; } = true;
    }
}