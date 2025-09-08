using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Services;
using GPM.Gantt.Utilities;

namespace GPM.Gantt.Layout
{
    /// <summary>
    /// Manages viewport operations for virtualization in the Gantt chart.
    /// </summary>
    public class ViewportManager
    {
        /// <summary>
        /// Finds the parent ScrollViewer for viewport calculations.
        /// </summary>
        public static ScrollViewer FindParentScrollViewer(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is ScrollViewer scrollViewer)
                    return scrollViewer;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
        
        /// <summary>
        /// Calculates visible row range based on viewport.
        /// </summary>
        public static (int startRow, int endRow) CalculateVisibleRange(int totalRows, Rect currentViewport, 
            double taskRowHeight, GridLength taskRowHeightGridLength)
        {
            if (currentViewport.IsEmpty)
                return (1, totalRows);
            
            var rowHeight = GetEstimatedRowHeight(taskRowHeight, taskRowHeightGridLength);
            var startRow = Math.Max(1, (int)(currentViewport.Y / rowHeight));
            var endRow = Math.Min(totalRows, startRow + (int)(currentViewport.Height / rowHeight) + 2); // +2 for buffer
            
            return (startRow, endRow);
        }
        
        /// <summary>
        /// Gets estimated row height for viewport calculations.
        /// </summary>
        private static double GetEstimatedRowHeight(double taskRowHeight, GridLength taskRowHeightGridLength)
        {
            const double defaultHeight = 40.0;
            
            if (taskRowHeightGridLength.GridUnitType == GridUnitType.Pixel)
                return taskRowHeightGridLength.Value;
            
            // Use cached value if available
            // In a real implementation, this would use the row height cache
            return taskRowHeight > 0 ? taskRowHeight : defaultHeight;
        }
        
        /// <summary>
        /// Determines if viewport change requires layout rebuild.
        /// </summary>
        public static bool IsSignificantViewportChange(ScrollChangedEventArgs e)
        {
            const double threshold = 50.0; // pixels
            return Math.Abs(e.VerticalChange) > threshold || Math.Abs(e.HorizontalChange) > threshold;
        }
    }
}