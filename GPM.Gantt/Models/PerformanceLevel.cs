namespace GPM.Gantt.Models
{
    /// <summary>
    /// Defines performance optimization levels for the Gantt chart rendering.
    /// </summary>
    public enum PerformanceLevel
    {
        /// <summary>
        /// Maximum visual quality with minimal performance optimizations.
        /// Best for small datasets and when visual fidelity is most important.
        /// </summary>
        Quality = 0,

        /// <summary>
        /// Balanced approach between visual quality and performance.
        /// Suitable for most use cases with moderate dataset sizes.
        /// </summary>
        Balanced = 1,

        /// <summary>
        /// Maximum performance optimizations with reduced visual quality.
        /// Best for large datasets and when responsiveness is critical.
        /// </summary>
        Performance = 2
    }
}