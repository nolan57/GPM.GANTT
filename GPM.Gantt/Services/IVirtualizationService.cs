using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service interface for managing UI virtualization in large datasets.
    /// Provides functionality for calculating visible ranges and managing viewport optimization.
    /// </summary>
    public interface IVirtualizationService
    {
        /// <summary>
        /// Gets the current viewport information.
        /// </summary>
        VirtualizationViewport Viewport { get; }

        /// <summary>
        /// Calculates the visible range of items based on the viewport size and scroll position.
        /// </summary>
        /// <param name="totalItems">Total number of items in the collection.</param>
        /// <param name="viewportSize">Size of the visible viewport.</param>
        /// <param name="scrollOffset">Current scroll offset.</param>
        /// <param name="itemSize">Average size of each item.</param>
        /// <returns>The visible range with buffer for smooth scrolling.</returns>
        VirtualizationRange CalculateVisibleRange(int totalItems, double viewportSize, double scrollOffset, double itemSize);

        /// <summary>
        /// Determines if virtualization should be enabled based on the dataset size and configuration.
        /// </summary>
        /// <param name="itemCount">Number of items in the dataset.</param>
        /// <param name="maxVisibleItems">Maximum items before enabling virtualization.</param>
        /// <returns>True if virtualization should be enabled.</returns>
        bool ShouldVirtualize(int itemCount, int maxVisibleItems);

        /// <summary>
        /// Updates the viewport information for the virtualization context.
        /// </summary>
        /// <param name="viewport">The new viewport information.</param>
        void UpdateViewport(VirtualizationViewport viewport);

        /// <summary>
        /// Calculates the optimal buffer size for smooth scrolling.
        /// </summary>
        /// <param name="visibleCount">Number of visible items.</param>
        /// <returns>Recommended buffer size.</returns>
        int CalculateBufferSize(int visibleCount);
    }

    /// <summary>
    /// Represents a range of items for virtualization.
    /// </summary>
    public struct VirtualizationRange
    {
        /// <summary>
        /// Gets the start index of the visible range.
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// Gets the end index of the visible range.
        /// </summary>
        public int EndIndex { get; }

        /// <summary>
        /// Gets the number of items in the visible range.
        /// </summary>
        public int Count => Math.Max(0, EndIndex - StartIndex + 1);

        /// <summary>
        /// Initializes a new instance of the VirtualizationRange struct.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        public VirtualizationRange(int startIndex, int endIndex)
        {
            StartIndex = Math.Max(0, startIndex);
            EndIndex = Math.Max(StartIndex, endIndex);
        }

        /// <summary>
        /// Checks if this range contains the specified index.
        /// </summary>
        /// <param name="index">The index to check.</param>
        /// <returns>True if the index is within the range.</returns>
        public readonly bool Contains(int index) => index >= StartIndex && index <= EndIndex;

        /// <summary>
        /// Creates an intersection of this range with another range.
        /// </summary>
        /// <param name="other">The other range.</param>
        /// <returns>The intersected range.</returns>
        public readonly VirtualizationRange Intersect(VirtualizationRange other)
        {
            var start = Math.Max(StartIndex, other.StartIndex);
            var end = Math.Min(EndIndex, other.EndIndex);
            return start <= end ? new VirtualizationRange(start, end) : new VirtualizationRange(0, -1);
        }
    }

    /// <summary>
    /// Represents viewport information for virtualization.
    /// </summary>
    public struct VirtualizationViewport
    {
        /// <summary>
        /// Gets the width of the viewport.
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Gets the height of the viewport.
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Gets the horizontal scroll offset.
        /// </summary>
        public double HorizontalOffset { get; }

        /// <summary>
        /// Gets the vertical scroll offset.
        /// </summary>
        public double VerticalOffset { get; }

        /// <summary>
        /// Initializes a new instance of the VirtualizationViewport struct.
        /// </summary>
        /// <param name="width">The viewport width.</param>
        /// <param name="height">The viewport height.</param>
        /// <param name="horizontalOffset">The horizontal scroll offset.</param>
        /// <param name="verticalOffset">The vertical scroll offset.</param>
        public VirtualizationViewport(double width, double height, double horizontalOffset, double verticalOffset)
        {
            Width = Math.Max(0, width);
            Height = Math.Max(0, height);
            HorizontalOffset = Math.Max(0, horizontalOffset);
            VerticalOffset = Math.Max(0, verticalOffset);
        }
    }
}