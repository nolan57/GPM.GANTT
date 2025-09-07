namespace GPM.Gantt.Models
{
    /// <summary>
    /// Defines time scale units for the Gantt chart timeline.
    /// Used for timeline axis generation and header text calculation.
    /// </summary>
    public enum TimeUnit
    {
        /// <summary>
        /// Hourly time scale - displays timeline in hour intervals.
        /// </summary>
        Hour = 0,
        
        /// <summary>
        /// Daily time scale - displays timeline in day intervals.
        /// </summary>
        Day = 1,
        
        /// <summary>
        /// Weekly time scale - displays timeline in week intervals.
        /// </summary>
        Week = 2,
        
        /// <summary>
        /// Monthly time scale - displays timeline in month intervals.
        /// </summary>
        Month = 3,
        
        /// <summary>
        /// Yearly time scale - displays timeline in year intervals.
        /// </summary>
        Year = 4
    }
}