using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GPM.Gantt.Utilities
{
    /// <summary>
    /// Manages pools of reusable UI elements to improve performance.
    /// </summary>
    public class ElementPool
    {
        private readonly Queue<GanttTimeCell> _timeCellPool = new();
        private readonly Queue<GanttGridRow> _gridRowPool = new();
        private readonly Queue<GanttGridCell> _gridCellPool = new();
        private readonly Queue<GanttTaskBar> _taskBarPool = new();
        
        private const int MaxPoolSize = 500; // Prevent unlimited growth
        
        /// <summary>
        /// Gets or creates a GanttTimeCell from the pool.
        /// </summary>
        /// <returns>A reusable GanttTimeCell instance.</returns>
        public GanttTimeCell GetOrCreateTimeCell()
        {
            if (_timeCellPool.Count > 0)
            {
                var cell = _timeCellPool.Dequeue();
                ResetTimeCell(cell);
                return cell;
            }
            
            return new GanttTimeCell();
        }
        
        /// <summary>
        /// Returns a GanttTimeCell to the pool for reuse.
        /// </summary>
        /// <param name="cell">The cell to return to the pool.</param>
        public void ReturnTimeCell(GanttTimeCell cell)
        {
            if (cell == null || _timeCellPool.Count >= MaxPoolSize)
                return;
                
            // Clear parent references
            if (cell.Parent is Panel parent)
            {
                parent.Children.Remove(cell);
            }
            
            _timeCellPool.Enqueue(cell);
        }
        
        /// <summary>
        /// Gets or creates a GanttGridRow from the pool.
        /// </summary>
        /// <returns>A reusable GanttGridRow instance.</returns>
        public GanttGridRow GetOrCreateGridRow()
        {
            if (_gridRowPool.Count > 0)
            {
                var row = _gridRowPool.Dequeue();
                ResetGridRow(row);
                return row;
            }
            
            return new GanttGridRow();
        }
        
        /// <summary>
        /// Returns a GanttGridRow to the pool for reuse.
        /// </summary>
        /// <param name="row">The row to return to the pool.</param>
        public void ReturnGridRow(GanttGridRow row)
        {
            if (row == null || _gridRowPool.Count >= MaxPoolSize)
                return;
                
            // Clear parent references
            if (row.Parent is Panel parent)
            {
                parent.Children.Remove(row);
            }
            
            _gridRowPool.Enqueue(row);
        }
        
        /// <summary>
        /// Gets or creates a GanttGridCell from the pool.
        /// </summary>
        /// <returns>A reusable GanttGridCell instance.</returns>
        public GanttGridCell GetOrCreateGridCell()
        {
            if (_gridCellPool.Count > 0)
            {
                var cell = _gridCellPool.Dequeue();
                ResetGridCell(cell);
                return cell;
            }
            
            return new GanttGridCell();
        }
        
        /// <summary>
        /// Returns a GanttGridCell to the pool for reuse.
        /// </summary>
        /// <param name="cell">The cell to return to the pool.</param>
        public void ReturnGridCell(GanttGridCell cell)
        {
            if (cell == null || _gridCellPool.Count >= MaxPoolSize)
                return;
                
            // Clear parent references
            if (cell.Parent is Panel parent)
            {
                parent.Children.Remove(cell);
            }
            
            _gridCellPool.Enqueue(cell);
        }
        
        /// <summary>
        /// Gets or creates a GanttTaskBar from the pool.
        /// </summary>
        /// <returns>A reusable GanttTaskBar instance.</returns>
        public GanttTaskBar GetOrCreateTaskBar()
        {
            if (_taskBarPool.Count > 0)
            {
                var taskBar = _taskBarPool.Dequeue();
                ResetTaskBar(taskBar);
                return taskBar;
            }
            
            return new GanttTaskBar();
        }
        
        /// <summary>
        /// Returns a GanttTaskBar to the pool for reuse.
        /// </summary>
        /// <param name="taskBar">The task bar to return to the pool.</param>
        public void ReturnTaskBar(GanttTaskBar taskBar)
        {
            if (taskBar == null || _taskBarPool.Count >= MaxPoolSize)
                return;
                
            // Clear parent references
            if (taskBar.Parent is Panel parent)
            {
                parent.Children.Remove(taskBar);
            }
            
            _taskBarPool.Enqueue(taskBar);
        }
        
        /// <summary>
        /// Clears all pools and releases pooled elements.
        /// </summary>
        public void Clear()
        {
            _timeCellPool.Clear();
            _gridRowPool.Clear();
            _gridCellPool.Clear();
            _taskBarPool.Clear();
        }
        
        /// <summary>
        /// Gets statistics about the current pool usage.
        /// </summary>
        /// <returns>Pool usage statistics.</returns>
        public PoolStatistics GetStatistics()
        {
            return new PoolStatistics
            {
                TimeCellPoolSize = _timeCellPool.Count,
                GridRowPoolSize = _gridRowPool.Count,
                GridCellPoolSize = _gridCellPool.Count,
                TaskBarPoolSize = _taskBarPool.Count,
                TotalPoolSize = _timeCellPool.Count + _gridRowPool.Count + _gridCellPool.Count + _taskBarPool.Count,
                MaxPoolSize = MaxPoolSize
            };
        }
        
        #region Reset Methods
        
        private static void ResetTimeCell(GanttTimeCell cell)
        {
            cell.TimeIndex = 0;
            cell.RowIndex = 0;
            cell.TimeText = string.Empty;
            cell.IsWeekend = false;
            cell.IsToday = false;
            
            // Reset layout properties
            Grid.SetRow(cell, 0);
            Grid.SetColumn(cell, 0);
            Grid.SetColumnSpan(cell, 1);
            Grid.SetRowSpan(cell, 1);
        }
        
        private static void ResetGridRow(GanttGridRow row)
        {
            row.RowIndex = 0;
            
            // Reset layout properties
            Grid.SetRow(row, 0);
            Grid.SetColumn(row, 0);
            Grid.SetColumnSpan(row, 1);
            Grid.SetRowSpan(row, 1);
        }
        
        private static void ResetGridCell(GanttGridCell cell)
        {
            cell.TimeIndex = 0;
            cell.RowIndex = 0;
            cell.IsWeekend = false;
            cell.IsToday = false;
            
            // Reset layout properties
            Grid.SetRow(cell, 0);
            Grid.SetColumn(cell, 0);
            Grid.SetColumnSpan(cell, 1);
            Grid.SetRowSpan(cell, 1);
        }
        
        private static void ResetTaskBar(GanttTaskBar taskBar)
        {
            taskBar.RowIndex = 0;
            taskBar.TimeIndex = 0;
            taskBar.CustomText = string.Empty;
            taskBar.Progress = 0;
            taskBar.Priority = GPM.Gantt.Models.TaskPriority.Normal;
            taskBar.Status = GPM.Gantt.Models.TaskStatus.NotStarted;
            taskBar.IsSelected = false;
            
            // Reset layout properties
            Grid.SetRow(taskBar, 0);
            Grid.SetColumn(taskBar, 0);
            Grid.SetColumnSpan(taskBar, 1);
            Grid.SetRowSpan(taskBar, 1);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Statistics about element pool usage.
    /// </summary>
    public class PoolStatistics
    {
        public int TimeCellPoolSize { get; set; }
        public int GridRowPoolSize { get; set; }
        public int GridCellPoolSize { get; set; }
        public int TaskBarPoolSize { get; set; }
        public int TotalPoolSize { get; set; }
        public int MaxPoolSize { get; set; }
    }
}