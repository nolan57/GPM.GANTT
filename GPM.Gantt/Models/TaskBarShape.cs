namespace GPM.Gantt.Models
{
    /// <summary>
    /// Defines the available task bar shapes for rendering.
    /// </summary>
    public enum TaskBarShape
    {
        /// <summary>
        /// Traditional rectangular task bar.
        /// </summary>
        Rectangle = 0,
        
        /// <summary>
        /// Task bar with diamond-shaped ends.
        /// </summary>
        DiamondEnds = 1,
        
        /// <summary>
        /// Rounded rectangular task bar.
        /// </summary>
        RoundedRectangle = 2,
        
        /// <summary>
        /// Chevron/arrow-shaped task bar.
        /// </summary>
        Chevron = 3,
        
        /// <summary>
        /// Milestone marker (diamond shape).
        /// </summary>
        Milestone = 4,
        
        /// <summary>
        /// Custom shape defined by user path geometry.
        /// </summary>
        Custom = 99
    }
}