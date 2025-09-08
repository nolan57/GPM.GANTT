using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GPM.Gantt.Models;
using GPM.Gantt.Services;
using GPM.Gantt.Utilities;

namespace GPM.Gantt.Layout
{
    /// <summary>
    /// Manages the layout of grid elements in the Gantt chart.
    /// </summary>
    public static class GridLayoutManager
    {
        /// <summary>
        /// Builds the complete grid structure with all rows and columns.
        /// </summary>
        public static void BuildFullGrid(Grid container, List<DateTime> ticks, int rows, int columns, bool showGridCells)
        {
            // Clear existing children
            container.Children.Clear();
            container.RowDefinitions.Clear();
            container.ColumnDefinitions.Clear();
            
            // Create row definitions
            for (int i = 0; i < rows; i++)
            {
                // Use the container's row height settings if available, otherwise use a default height
                GridLength rowHeight;
                if (container is GanttContainer ganttContainer)
                {
                    rowHeight = (i == 0) ? ganttContainer.HeaderRowHeight : ganttContainer.TaskRowHeight;
                }
                else
                {
                    // Default heights if not a GanttContainer
                    rowHeight = (i == 0) ? new GridLength(30) : new GridLength(30);
                }
                
                container.RowDefinitions.Add(new RowDefinition { Height = rowHeight });
            }
            
            // Create column definitions
            var columnWidth = new GridLength(1, GridUnitType.Star);
            for (int i = 0; i < columns; i++)
            {
                container.ColumnDefinitions.Add(new ColumnDefinition { Width = columnWidth });
            }
            
            // First row: time scale cells
            for (int c = 0; c < columns; c++)
            {
                var dt = ticks[c];
                var cell = new GanttTimeCell
                {
                    TimeIndex = c,
                    TimeText = TimelineCalculator.FormatTick(dt, TimeUnit.Day), // Default to day formatting
                    IsWeekend = TimelineCalculator.IsWeekend(dt),
                    IsToday = TimelineCalculator.IsToday(dt)
                };
                Grid.SetRow(cell, 0);
                Grid.SetColumn(cell, c);
                container.Children.Add(cell);
            }
            
            // Other rows: grid rows or grid cells
            for (int r = 1; r < rows; r++)
            {
                if (!showGridCells)
                {
                    var row = new GanttGridRow { RowIndex = r };
                    Grid.SetRow(row, r);
                    Grid.SetColumn(row, 0);
                    Grid.SetColumnSpan(row, columns);
                    container.Children.Add(row);
                }
                else
                {
                    for (int c = 0; c < columns; c++)
                    {
                        var dt = ticks[c];
                        var cell = new GanttGridCell
                        {
                            RowIndex = r,
                            TimeIndex = c,
                            IsWeekend = TimelineCalculator.IsWeekend(dt),
                            IsToday = TimelineCalculator.IsToday(dt)
                        };
                        Grid.SetRow(cell, r);
                        Grid.SetColumn(cell, c);
                        container.Children.Add(cell);
                    }
                }
            }
        }
        
        /// <summary>
        /// Builds virtualized grid elements for visible range only.
        /// </summary>
        public static void BuildVirtualizedGrid(Grid container, List<DateTime> ticks, int columns, (int startRow, int endRow) visibleRange, bool showGridCells, ElementPool elementPool, List<UIElement> visibleElements, IPerformanceService performanceService)
        {
            using var measurement = performanceService.BeginMeasurement("VirtualizedGridBuild");
            
            var (startRow, endRow) = visibleRange;
            
            for (int r = startRow; r <= endRow; r++)
            {
                if (!showGridCells)
                {
                    var row = elementPool.GetOrCreateGridRow();
                    row.RowIndex = r;
                    Grid.SetRow(row, r);
                    Grid.SetColumn(row, 0);
                    Grid.SetColumnSpan(row, columns);
                    container.Children.Add(row);
                    visibleElements.Add(row);
                }
                else
                {
                    for (int c = 0; c < columns; c++)
                    {
                        var dt = ticks[c];
                        var cell = elementPool.GetOrCreateGridCell();
                        cell.RowIndex = r;
                        cell.TimeIndex = c;
                        cell.IsWeekend = TimelineCalculator.IsWeekend(dt);
                        cell.IsToday = TimelineCalculator.IsToday(dt);
                        
                        Grid.SetRow(cell, r);
                        Grid.SetColumn(cell, c);
                        container.Children.Add(cell);
                        visibleElements.Add(cell);
                    }
                }
            }
        }
    }
}