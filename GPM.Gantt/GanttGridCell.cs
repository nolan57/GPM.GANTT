using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt
{
    /// <summary>
    /// Represents a single cell in the Gantt chart grid.
    /// </summary>
    public class GanttGridCell : GanttShapeBase
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets the row index of the cell.
        /// </summary>
        public static readonly DependencyProperty RowIndexProperty = DependencyProperty.Register(
            nameof(RowIndex), typeof(int), typeof(GanttGridCell), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the time index of the cell.
        /// </summary>
        public static readonly DependencyProperty TimeIndexProperty = DependencyProperty.Register(
            nameof(TimeIndex), typeof(int), typeof(GanttGridCell), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets whether this cell represents a weekend.
        /// </summary>
        public static readonly DependencyProperty IsWeekendProperty = DependencyProperty.Register(
            nameof(IsWeekend), typeof(bool), typeof(GanttGridCell), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether this cell represents today.
        /// </summary>
        public static readonly DependencyProperty IsTodayProperty = DependencyProperty.Register(
            nameof(IsToday), typeof(bool), typeof(GanttGridCell), new PropertyMetadata(false));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the row index of the cell.
        /// </summary>
        public int RowIndex
        {
            get => (int)GetValue(RowIndexProperty);
            set => SetValue(RowIndexProperty, value);
        }

        /// <summary>
        /// Gets or sets the time index of the cell.
        /// </summary>
        public int TimeIndex
        {
            get => (int)GetValue(TimeIndexProperty);
            set => SetValue(TimeIndexProperty, value);
        }

        /// <summary>
        /// Gets or sets whether this cell represents a weekend.
        /// </summary>
        public bool IsWeekend
        {
            get => (bool)GetValue(IsWeekendProperty);
            set => SetValue(IsWeekendProperty, value);
        }

        /// <summary>
        /// Gets or sets whether this cell represents today.
        /// </summary>
        public bool IsToday
        {
            get => (bool)GetValue(IsTodayProperty);
            set => SetValue(IsTodayProperty, value);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the GanttGridCell class.
        /// </summary>
        public GanttGridCell()
        {
            // Enable GPU rendering by default
            EnableGpuRendering = true;
        }

        #endregion

        #region Rendering Override

        /// <summary>
        /// Render grid cell using traditional WPF method
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        protected override void RenderWithWpf(DrawingContext drawingContext)
        {
            // Use base class rendering method
            base.RenderWithWpf(drawingContext);
        }

        #endregion
    }
}