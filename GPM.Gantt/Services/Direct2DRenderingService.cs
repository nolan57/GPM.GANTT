using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Direct2D rendering service implementation example
    /// Note: This is just an example implementation, actual use requires referencing Direct2D related NuGet packages
    /// </summary>
    public class Direct2DRenderingService : IGpuRenderingService, IDisposable
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _renderedElements;
        private long _startTimeTicks;
        
        // In actual implementation, Direct2D related objects need to be declared here
        // private SharpDX.Direct2D1.Factory _d2dFactory;
        // private SharpDX.Direct2D1.RenderTarget _renderTarget;
        
        public GpuRenderingTechnology Technology => GpuRenderingTechnology.Direct2D;
        
        public bool Initialize()
        {
            try
            {
                // In actual implementation, Direct2D factory and render target need to be initialized here
                // _d2dFactory = new SharpDX.Direct2D1.Factory();
                // _renderTarget = CreateRenderTarget();
                
                return true;
            }
            catch
            {
                // Initialization failed, fall back to default rendering
                return false;
            }
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
            
            // In actual implementation, Direct2D API would be used for rendering
            // Since this is an example, we still use WPF's drawing API
            // But in an actual Direct2D implementation, SharpDX or other Direct2D wrapper libraries would be used
            
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
                GpuMemoryUsage = 0, // Need to track GPU memory usage in actual implementation
                Fps = fps
            };
        }
        
        public void Dispose()
        {
            // In actual implementation, Direct2D resources need to be released here
            // _renderTarget?.Dispose();
            // _d2dFactory?.Dispose();
        }
    }
}