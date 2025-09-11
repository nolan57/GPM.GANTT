using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Services;

namespace GPM.Gantt
{
    /// <summary>
    /// Unified shape base class with GPU acceleration support
    /// </summary>
    public abstract class GanttShapeBase : FrameworkElement
    {
        #region Dependency Properties
        
        /// <summary>
        /// Background brush dependency property
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background), typeof(Brush), typeof(GanttShapeBase), 
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));
        
        /// <summary>
        /// Fill brush dependency property
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill), typeof(Brush), typeof(GanttShapeBase), 
            new FrameworkPropertyMetadata(Brushes.LightBlue, FrameworkPropertyMetadataOptions.AffectsRender));
            
        /// <summary>
        /// Stroke brush dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            nameof(Stroke), typeof(Brush), typeof(GanttShapeBase), 
            new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));
            
        /// <summary>
        /// Stroke thickness dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness), typeof(double), typeof(GanttShapeBase), 
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
            
        /// <summary>
        /// Corner radius dependency property
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(double), typeof(GanttShapeBase), 
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
            
        /// <summary>
        /// Border brush dependency property
        /// </summary>
        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            nameof(BorderBrush), typeof(Brush), typeof(GanttShapeBase), 
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));
            
        /// <summary>
        /// Border thickness dependency property
        /// </summary>
        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
            nameof(BorderThickness), typeof(Thickness), typeof(GanttShapeBase), 
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsRender));
            
        /// <summary>
        /// Enable GPU rendering dependency property
        /// </summary>
        public static readonly DependencyProperty EnableGpuRenderingProperty = DependencyProperty.Register(
            nameof(EnableGpuRendering), typeof(bool), typeof(GanttShapeBase), 
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion
        
        #region Property Accessors
        
        /// <summary>
        /// Gets or sets the background brush
        /// </summary>
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the fill brush
        /// </summary>
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the stroke brush
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the stroke thickness
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the corner radius
        /// </summary>
        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the border brush
        /// </summary>
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets the border thickness
        /// </summary>
        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets whether GPU rendering is enabled
        /// </summary>
        public bool EnableGpuRendering
        {
            get { return (bool)GetValue(EnableGpuRenderingProperty); }
            set { SetValue(EnableGpuRenderingProperty, value); }
        }
        
        #endregion
        
        #region Private Fields
        
        private IGpuRenderingService _gpuRenderingService;
        private bool _isGpuRenderingInitialized;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Initializes a new instance of the GanttShapeBase class
        /// </summary>
        protected GanttShapeBase()
        {
            // Enable GPU rendering by default
            EnableGpuRendering = true;
        }
        
        #endregion
        
        #region Rendering Methods
        
        /// <summary>
        /// Override OnRender method to support GPU rendering
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (EnableGpuRendering && InitializeGpuRendering())
            {
                // Use GPU rendering
                RenderWithGpu(drawingContext);
            }
            else
            {
                // Use traditional WPF rendering
                RenderWithWpf(drawingContext);
            }
        }
        
        /// <summary>
        /// Initialize GPU rendering service
        /// </summary>
        /// <returns>Whether initialization was successful</returns>
        private bool InitializeGpuRendering()
        {
            if (_isGpuRenderingInitialized)
                return true;
                
            try
            {
                // Get rendering technology type from configuration
                // Temporarily use default rendering service as an example
                _gpuRenderingService = GpuRenderingServiceFactory.CreateService(GpuRenderingTechnology.Default);
                _isGpuRenderingInitialized = _gpuRenderingService != null;
                return _isGpuRenderingInitialized;
            }
            catch
            {
                // Initialization failed, fall back to WPF rendering
                _isGpuRenderingInitialized = false;
                return false;
            }
        }
        
        /// <summary>
        /// Render shape using GPU
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        protected virtual void RenderWithGpu(DrawingContext drawingContext)
        {
            // Default implementation uses rectangle rendering
            if (_gpuRenderingService != null)
            {
                var padding = 0.0;
                if (Effect is System.Windows.Media.Effects.DropShadowEffect dse && dse.Opacity > 0)
                {
                    // Provide room for blur to appear inside the element bounds
                    padding = Math.Min(8.0, Math.Max(3.0, dse.BlurRadius * 0.5));
                    padding = Math.Min(padding, Math.Min(ActualWidth / 4.0, ActualHeight / 4.0));
                }

                var bounds = new Rect(padding, padding, Math.Max(0, ActualWidth - 2 * padding), Math.Max(0, ActualHeight - 2 * padding));
                _gpuRenderingService.RenderRectangle(
                    drawingContext, 
                    bounds, 
                    Fill, 
                    Stroke, 
                    StrokeThickness, 
                    CornerRadius);
            }
            else
            {
                // If GPU rendering service is not available, fall back to WPF rendering
                RenderWithWpf(drawingContext);
            }
        }
        
        /// <summary>
        /// Render shape using traditional WPF method
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        protected virtual void RenderWithWpf(DrawingContext drawingContext)
        {
            var padding = 0.0;
            if (Effect is System.Windows.Media.Effects.DropShadowEffect dse && dse.Opacity > 0)
            {
                // Provide room for blur to appear inside the element bounds
                padding = Math.Min(8.0, Math.Max(3.0, dse.BlurRadius * 0.5));
                padding = Math.Min(padding, Math.Min(ActualWidth / 4.0, ActualHeight / 4.0));
            }

            var bounds = new Rect(padding, padding, Math.Max(0, ActualWidth - 2 * padding), Math.Max(0, ActualHeight - 2 * padding));
            
            // Draw background first
            if (Background != null && !Background.Equals(Brushes.Transparent))
            {
                drawingContext.DrawRectangle(Background, null, bounds);
            }
            
            // Draw border if border properties are set
            if (BorderBrush != null && !BorderBrush.Equals(Brushes.Transparent) && BorderThickness != new Thickness(0))
            {
                var pen = new Pen(BorderBrush, Math.Max(BorderThickness.Left, Math.Max(BorderThickness.Top, Math.Max(BorderThickness.Right, BorderThickness.Bottom))));
                drawingContext.DrawRectangle(null, pen, bounds);
            }
            
            // Draw fill if specified
            if (Fill != null && !Fill.Equals(Brushes.Transparent))
            {
                if (CornerRadius > 0)
                {
                    // Use rounded rectangle
                    var geometry = new RectangleGeometry(bounds, CornerRadius, CornerRadius);
                    drawingContext.DrawGeometry(Fill, null, geometry);
                }
                else
                {
                    // Use regular rectangle
                    drawingContext.DrawRectangle(Fill, null, bounds);
                }
            }
            
            // Draw stroke if specified
            if (Stroke != null && StrokeThickness > 0)
            {
                var pen = new Pen(Stroke, StrokeThickness);
                if (CornerRadius > 0)
                {
                    // Use rounded rectangle
                    var geometry = new RectangleGeometry(bounds, CornerRadius, CornerRadius);
                    drawingContext.DrawGeometry(null, pen, geometry);
                }
                else
                {
                    // Use regular rectangle
                    drawingContext.DrawRectangle(null, pen, bounds);
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Force re-rendering
        /// </summary>
        public void Refresh()
        {
            InvalidateVisual();
        }
        
        #endregion
    }
}