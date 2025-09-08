using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPM.Gantt
{
    /// <summary>
    /// Represents a time cell in the Gantt chart header.
    /// </summary>
    public class GanttTimeCell : GanttShapeBase
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets the row index of the time cell.
        /// </summary>
        public static readonly DependencyProperty RowIndexProperty = DependencyProperty.Register(
            nameof(RowIndex), typeof(int), typeof(GanttTimeCell), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the time index of the cell.
        /// </summary>
        public static readonly DependencyProperty TimeIndexProperty = DependencyProperty.Register(
            nameof(TimeIndex), typeof(int), typeof(GanttTimeCell), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the text to display in the time cell.
        /// </summary>
        public static readonly DependencyProperty TimeTextProperty = DependencyProperty.Register(
            nameof(TimeText), typeof(string), typeof(GanttTimeCell), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets whether this cell represents a weekend.
        /// </summary>
        public static readonly DependencyProperty IsWeekendProperty = DependencyProperty.Register(
            nameof(IsWeekend), typeof(bool), typeof(GanttTimeCell), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether this cell represents today.
        /// </summary>
        public static readonly DependencyProperty IsTodayProperty = DependencyProperty.Register(
            nameof(IsToday), typeof(bool), typeof(GanttTimeCell), new PropertyMetadata(false));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the row index of the time cell.
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
        /// Gets or sets the text to display in the time cell.
        /// </summary>
        public string TimeText
        {
            get => (string)GetValue(TimeTextProperty);
            set => SetValue(TimeTextProperty, value);
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

        #region Private Fields

        private TextBlock? _textBlock;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the child element of the time cell.
        /// </summary>
        public UIElement? Child => _textBlock;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the GanttTimeCell class.
        /// </summary>
        public GanttTimeCell()
        {
            // Enable GPU rendering by default
            EnableGpuRendering = true;
            
            // Create and configure the text block
            _textBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };
            
            // Add the text block as a child
            if (_textBlock != null)
            {
                AddVisualChild(_textBlock);
                AddLogicalChild(_textBlock);
            }
        }

        #endregion

        #region Visual and Logical Children

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        protected override int VisualChildrenCount => _textBlock != null ? 1 : 0;

        /// <summary>
        /// Overrides System.Windows.Media.Visual.GetVisualChild(System.Int32),
        /// and returns a child at the specified index from a collection of child elements.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0 && _textBlock != null)
                return _textBlock;
            throw new System.ArgumentOutOfRangeException(nameof(index));
        }

        #endregion

        #region Layout Override

        /// <summary>
        /// Measures the size in layout required for child elements.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            if (_textBlock != null)
            {
                _textBlock.Text = TimeText;
                _textBlock.Measure(constraint);
                return _textBlock.DesiredSize;
            }
            return new Size(0, 0);
        }

        /// <summary>
        /// Positions child elements and determines a size for a System.Windows.FrameworkElement.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_textBlock != null)
            {
                _textBlock.Text = TimeText;
                _textBlock.Arrange(new Rect(finalSize));
            }
            return finalSize;
        }

        #endregion

        #region Rendering Override

        /// <summary>
        /// Render time cell using traditional WPF method
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        protected override void RenderWithWpf(DrawingContext drawingContext)
        {
            // Use base class rendering method to render background
            base.RenderWithWpf(drawingContext);
        }

        #endregion
    }
}