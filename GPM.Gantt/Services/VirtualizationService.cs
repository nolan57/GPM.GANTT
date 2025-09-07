using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Implementation of the virtualization service for optimizing large dataset performance.
    /// Provides efficient viewport calculations and range management for UI virtualization.
    /// </summary>
    public class VirtualizationService : IVirtualizationService
    {
        private VirtualizationViewport _viewport;

        /// <summary>
        /// Gets the current viewport information.
        /// </summary>
        public VirtualizationViewport Viewport => _viewport;

        /// <summary>
        /// Calculates the visible range of items based on the viewport size and scroll position.
        /// </summary>
        /// <param name="totalItems">Total number of items in the collection.</param>
        /// <param name="viewportSize">Size of the visible viewport.</param>
        /// <param name="scrollOffset">Current scroll offset.</param>
        /// <param name="itemSize">Average size of each item.</param>
        /// <returns>The visible range with buffer for smooth scrolling.</returns>
        public VirtualizationRange CalculateVisibleRange(int totalItems, double viewportSize, double scrollOffset, double itemSize)
        {
            if (totalItems <= 0 || itemSize <= 0 || viewportSize <= 0)
                return new VirtualizationRange(0, Math.Max(0, totalItems - 1));

            // Calculate visible items without buffer
            var startIndex = (int)(scrollOffset / itemSize);
            var visibleCount = (int)Math.Ceiling(viewportSize / itemSize);
            var endIndex = startIndex + visibleCount - 1;

            // Add buffer for smooth scrolling
            var bufferSize = CalculateBufferSize(visibleCount);
            var bufferedStart = Math.Max(0, startIndex - bufferSize);
            var bufferedEnd = Math.Min(totalItems - 1, endIndex + bufferSize);

            return new VirtualizationRange(bufferedStart, bufferedEnd);
        }

        /// <summary>
        /// Determines if virtualization should be enabled based on the dataset size and configuration.
        /// </summary>
        /// <param name="itemCount">Number of items in the dataset.</param>
        /// <param name="maxVisibleItems">Maximum items before enabling virtualization.</param>
        /// <returns>True if virtualization should be enabled.</returns>
        public bool ShouldVirtualize(int itemCount, int maxVisibleItems)
        {
            return itemCount > maxVisibleItems && maxVisibleItems > 0;
        }

        /// <summary>
        /// Updates the viewport information for the virtualization context.
        /// </summary>
        /// <param name="viewport">The new viewport information.</param>
        public void UpdateViewport(VirtualizationViewport viewport)
        {
            _viewport = viewport;
        }

        /// <summary>
        /// Calculates the optimal buffer size for smooth scrolling.
        /// </summary>
        /// <param name="visibleCount">Number of visible items.</param>
        /// <returns>Recommended buffer size.</returns>
        public int CalculateBufferSize(int visibleCount)
        {
            // Use 50% of visible count as buffer, minimum 2, maximum 10
            return Math.Max(2, Math.Min(10, visibleCount / 2));
        }
    }
}