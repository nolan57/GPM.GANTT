using System.Threading.Tasks;
using System.Windows;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Supported export formats for Gantt chart
    /// </summary>
    public enum ExportFormat
    {
        PNG,
        PDF,
        SVG,
        JPEG,
        BMP
    }

    /// <summary>
    /// Export options for configuring the export process
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// Export format
        /// </summary>
        public ExportFormat Format { get; set; } = ExportFormat.PNG;
        
        /// <summary>
        /// DPI for image exports
        /// </summary>
        public double DPI { get; set; } = 96;
        
        /// <summary>
        /// Width of the exported image (0 = use actual width)
        /// </summary>
        public int Width { get; set; } = 0;
        
        /// <summary>
        /// Height of the exported image (0 = use actual height)
        /// </summary>
        public int Height { get; set; } = 0;
        
        /// <summary>
        /// Whether to include background in export
        /// </summary>
        public bool IncludeBackground { get; set; } = true;
        
        /// <summary>
        /// Whether to include dependency lines in export
        /// </summary>
        public bool IncludeDependencies { get; set; } = true;
        
        /// <summary>
        /// Title for PDF metadata
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Author for PDF metadata
        /// </summary>
        public string Author { get; set; } = string.Empty;
        
        /// <summary>
        /// Subject for PDF metadata
        /// </summary>
        public string Subject { get; set; } = string.Empty;
        
        /// <summary>
        /// Quality level for JPEG export (0-100)
        /// </summary>
        public int Quality { get; set; } = 95;
        
        /// <summary>
        /// Whether to fit content to page for PDF export
        /// </summary>
        public bool FitToPage { get; set; } = true;
        
        /// <summary>
        /// Page orientation for PDF export
        /// </summary>
        public PageOrientation Orientation { get; set; } = PageOrientation.Landscape;
    }

    /// <summary>
    /// Page orientation for PDF export
    /// </summary>
    public enum PageOrientation
    {
        Portrait,
        Landscape
    }

    /// <summary>
    /// Service interface for exporting Gantt charts to various formats
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Exports a framework element to a file
        /// </summary>
        /// <param name="element">The element to export</param>
        /// <param name="filePath">Path where to save the file</param>
        /// <param name="options">Export options</param>
        /// <returns>True if export was successful</returns>
        Task<bool> ExportAsync(FrameworkElement element, string filePath, ExportOptions options);
        
        /// <summary>
        /// Exports a framework element to byte array
        /// </summary>
        /// <param name="element">The element to export</param>
        /// <param name="options">Export options</param>
        /// <returns>Byte array containing the exported data</returns>
        Task<byte[]> ExportToBytesAsync(FrameworkElement element, ExportOptions options);
        
        /// <summary>
        /// Exports a Gantt container specifically
        /// </summary>
        /// <param name="ganttContainer">The Gantt container to export</param>
        /// <param name="filePath">Path where to save the file</param>
        /// <param name="options">Export options</param>
        /// <returns>True if export was successful</returns>
        Task<bool> ExportGanttChartAsync(GanttContainer ganttContainer, string filePath, ExportOptions options);
        
        /// <summary>
        /// Gets default filename for the specified format
        /// </summary>
        /// <param name="format">Export format</param>
        /// <returns>Default filename with timestamp</returns>
        string GetDefaultFileName(ExportFormat format);
        
        /// <summary>
        /// Gets list of supported export formats
        /// </summary>
        /// <returns>Array of supported format names</returns>
        string[] GetSupportedFormats();
        
        /// <summary>
        /// Validates export options
        /// </summary>
        /// <param name="options">Options to validate</param>
        /// <returns>True if options are valid</returns>
        bool ValidateExportOptions(ExportOptions options);
        
        /// <summary>
        /// Gets the file filter string for save dialogs
        /// </summary>
        /// <returns>File filter string</returns>
        string GetFileFilter();
    }
}