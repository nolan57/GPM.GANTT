using System;
using System.Windows;
using System.Windows.Media;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// GPU rendering technology types
    /// </summary>
    public enum GpuRenderingTechnology
    {
        /// <summary>
        /// Default WPF rendering (software rendering)
        /// </summary>
        Default,
        
        /// <summary>
        /// Direct2D rendering
        /// </summary>
        Direct2D,
        
        /// <summary>
        /// DirectX rendering
        /// </summary>
        DirectX,
        
        /// <summary>
        /// OpenGL rendering
        /// </summary>
        OpenGL,
        
        /// <summary>
        /// Vulkan rendering
        /// </summary>
        Vulkan
    }

    /// <summary>
    /// GPU rendering service interface
    /// </summary>
    public interface IGpuRenderingService
    {
        /// <summary>
        /// Gets the rendering technology type
        /// </summary>
        GpuRenderingTechnology Technology { get; }
        
        /// <summary>
        /// Initialize the rendering service
        /// </summary>
        /// <returns>Whether initialization was successful</returns>
        bool Initialize();
        
        /// <summary>
        /// Render a rectangular shape
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        /// <param name="bounds">Boundary rectangle</param>
        /// <param name="fillBrush">Fill brush</param>
        /// <param name="borderBrush">Border brush</param>
        /// <param name="borderThickness">Border thickness</param>
        /// <param name="cornerRadius">Corner radius</param>
        void RenderRectangle(
            DrawingContext drawingContext, 
            Rect bounds, 
            Brush fillBrush, 
            Brush borderBrush, 
            double borderThickness,
            double cornerRadius = 0);
        
        /// <summary>
        /// Render a line
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        /// <param name="startPoint">Start point</param>
        /// <param name="endPoint">End point</param>
        /// <param name="strokeBrush">Line brush</param>
        /// <param name="strokeThickness">Line thickness</param>
        void RenderLine(
            DrawingContext drawingContext,
            Point startPoint,
            Point endPoint,
            Brush strokeBrush,
            double strokeThickness);
        
        /// <summary>
        /// Render text
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        /// <param name="text">Text content</param>
        /// <param name="fontFamily">Font family</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="foreground">Foreground color</param>
        /// <param name="origin">Text starting position</param>
        void RenderText(
            DrawingContext drawingContext,
            string text,
            FontFamily fontFamily,
            double fontSize,
            Brush foreground,
            Point origin);
        
        /// <summary>
        /// Render complex path
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        /// <param name="geometry">Geometric path</param>
        /// <param name="fillBrush">Fill brush</param>
        /// <param name="strokeBrush">Border brush</param>
        /// <param name="strokeThickness">Border thickness</param>
        void RenderGeometry(
            DrawingContext drawingContext,
            Geometry geometry,
            Brush fillBrush,
            Brush strokeBrush,
            double strokeThickness);
        
        /// <summary>
        /// Begin batch rendering operation
        /// </summary>
        void BeginBatchRender();
        
        /// <summary>
        /// End batch rendering operation
        /// </summary>
        void EndBatchRender();
        
        /// <summary>
        /// Release resources
        /// </summary>
        void Dispose();
        
        /// <summary>
        /// Get rendering performance metrics
        /// </summary>
        /// <returns>Performance metrics</returns>
        GpuRenderingMetrics GetPerformanceMetrics();
    }
    
    /// <summary>
    /// GPU rendering performance metrics
    /// </summary>
    public class GpuRenderingMetrics
    {
        /// <summary>
        /// Number of rendered elements
        /// </summary>
        public int RenderedElements { get; set; }
        
        /// <summary>
        /// Rendering time (milliseconds)
        /// </summary>
        public double RenderTimeMs { get; set; }
        
        /// <summary>
        /// GPU memory usage (bytes)
        /// </summary>
        public long GpuMemoryUsage { get; set; }
        
        /// <summary>
        /// Frame rate
        /// </summary>
        public double Fps { get; set; }
    }
}