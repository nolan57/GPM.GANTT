using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Default WPF rendering service implementation
    /// </summary>
    public class DefaultRenderingService : IGpuRenderingService, IDisposable
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _renderedElements;
        private long _startTimeTicks;
        
        public GpuRenderingTechnology Technology => GpuRenderingTechnology.Default;
        
        public bool Initialize()
        {
            // Default WPF rendering does not require special initialization
            return true;
        }
        
        public void RenderRectangle(
            DrawingContext drawingContext, 
            Rect bounds, 
            Brush fillBrush, 
            Brush borderBrush, 
            double borderThickness,
            double cornerRadius = 0)
        {
            if (drawingContext == null) return;
            
            _renderedElements++;
            
            if (cornerRadius > 0)
            {
                // Use rounded rectangle
                var geometry = new RectangleGeometry(bounds, cornerRadius, cornerRadius);
                if (fillBrush != null)
                {
                    drawingContext.DrawGeometry(fillBrush, null, geometry);
                }
                
                if (borderBrush != null && borderThickness > 0)
                {
                    var pen = new Pen(borderBrush, borderThickness);
                    drawingContext.DrawGeometry(null, pen, geometry);
                }
            }
            else
            {
                // Use regular rectangle
                if (fillBrush != null)
                {
                    drawingContext.DrawRectangle(fillBrush, null, bounds);
                }
                
                if (borderBrush != null && borderThickness > 0)
                {
                    var pen = new Pen(borderBrush, borderThickness);
                    drawingContext.DrawRectangle(null, pen, bounds);
                }
            }
        }
        
        public void RenderLine(
            DrawingContext drawingContext,
            Point startPoint,
            Point endPoint,
            Brush strokeBrush,
            double strokeThickness)
        {
            if (drawingContext == null || strokeBrush == null) return;
            
            _renderedElements++;
            var pen = new Pen(strokeBrush, strokeThickness);
            drawingContext.DrawLine(pen, startPoint, endPoint);
        }
        
        public void RenderText(
            DrawingContext drawingContext,
            string text,
            FontFamily fontFamily,
            double fontSize,
            Brush foreground,
            Point origin)
        {
            if (drawingContext == null || string.IsNullOrEmpty(text) || foreground == null) return;
            
            _renderedElements++;
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize,
                foreground);
                
            drawingContext.DrawText(formattedText, origin);
        }
        
        public void RenderGeometry(
            DrawingContext drawingContext,
            Geometry geometry,
            Brush fillBrush,
            Brush strokeBrush,
            double strokeThickness)
        {
            if (drawingContext == null || geometry == null) return;
            
            _renderedElements++;
            Pen pen = null;
            if (strokeBrush != null && strokeThickness > 0)
            {
                pen = new Pen(strokeBrush, strokeThickness);
            }
            
            drawingContext.DrawGeometry(fillBrush, pen, geometry);
        }
        
        public void BeginBatchRender()
        {
            _renderedElements = 0;
            _startTimeTicks = Stopwatch.GetTimestamp();
            _stopwatch.Restart();
        }
        
        public void EndBatchRender()
        {
            _stopwatch.Stop();
        }
        
        public GpuRenderingMetrics GetPerformanceMetrics()
        {
            var elapsedMs = _stopwatch.ElapsedMilliseconds;
            var fps = elapsedMs > 0 ? 1000.0 / elapsedMs : 0;
            
            return new GpuRenderingMetrics
            {
                RenderedElements = _renderedElements,
                RenderTimeMs = elapsedMs,
                GpuMemoryUsage = 0, // Default implementation does not track GPU memory
                Fps = fps
            };
        }
        
        public void Dispose()
        {
            // Default implementation does not require special cleanup
        }
    }
}