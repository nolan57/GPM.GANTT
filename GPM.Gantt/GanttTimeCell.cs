using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GPM.Gantt
{
    // Time cell: Used for time display in the first row, inherits from GanttRectangle
    public class GanttTimeCell : GanttRectangle
    {
        public static readonly DependencyProperty TimeTextProperty = DependencyProperty.Register(
            nameof(TimeText), typeof(string), typeof(GanttTimeCell), new FrameworkPropertyMetadata(string.Empty));

        public string TimeText
        {
            get => (string)GetValue(TimeTextProperty);
            set => SetValue(TimeTextProperty, value);
        }

        public GanttTimeCell()
        {
            // Place text inside the border to display time
            var tb = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            tb.SetBinding(TextBlock.TextProperty, new Binding(nameof(TimeText)) { Source = this });
            Child = tb;
        }
    }
}