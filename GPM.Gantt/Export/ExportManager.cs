using System;
using System.Threading.Tasks;
using GPM.Gantt.Services;

namespace GPM.Gantt.Export
{
    /// <summary>
    /// Manages export operations for the Gantt chart.
    /// </summary>
    public class ExportManager
    {
        /// <summary>
        /// Exports the Gantt chart using the configured export service
        /// </summary>
        public static async Task<bool> ExportAsync(IExportService exportService, GanttContainer ganttContainer, string filePath, ExportOptions options)
        {
            if (exportService == null)
            {
                System.Diagnostics.Debug.WriteLine("Export service not configured");
                return false;
            }
            
            return await exportService.ExportGanttChartAsync(ganttContainer, filePath, options);
        }
    }
}