using System.Windows.Media;

namespace GPM.Gantt
{
    // Grid row component: Spans across all time columns
    public class GanttGridRow : GanttRectangle
    {
        public GanttGridRow()
        {
            BorderBrush = Brushes.Silver;
        }
    }
}