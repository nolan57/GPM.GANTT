using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Models;
using GPM.Gantt.Rendering;

namespace GPM.Gantt
{
    /// <summary>
    /// Enhanced task bar with support for different shapes and advanced rendering capabilities.
    /// </summary>
    public class EnhancedGanttTaskBar : GanttTaskBar
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets the shape of the task bar.
        /// </summary>
        public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(
            nameof(Shape), typeof(TaskBarShape), typeof(EnhancedGanttTaskBar),
            new FrameworkPropertyMetadata(TaskBarShape.Rectangle, OnShapeChanged));

        public TaskBarShape Shape
        {
            get => (TaskBarShape)GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        /// <summary>
        /// Gets or sets the shape rendering parameters.
        /// </summary>
        public static readonly DependencyProperty ShapeParametersProperty = DependencyProperty.Register(
            nameof(ShapeParameters), typeof(ShapeRenderingParameters), typeof(EnhancedGanttTaskBar),
            new FrameworkPropertyMetadata(null, OnShapeParametersChanged));

        public ShapeRenderingParameters? ShapeParameters
        {
            get => (ShapeRenderingParameters?)GetValue(ShapeParametersProperty);
            set => SetValue(ShapeParametersProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to use the legacy rectangle rendering.
        /// </summary>
        public static readonly DependencyProperty UseLegacyRenderingProperty = DependencyProperty.Register(
            nameof(UseLegacyRendering), typeof(bool), typeof(EnhancedGanttTaskBar),
            new FrameworkPropertyMetadata(false, OnRenderingModeChanged));

        public bool UseLegacyRendering
        {
            get => (bool)GetValue(UseLegacyRenderingProperty);
            set => SetValue(UseLegacyRenderingProperty, value);
        }

        #endregion

        #region Private Fields

        private ITaskBarShapeRenderer? _currentRenderer;
        private FrameworkElement? _shapeElement;
        private Canvas? _shapeContainer;

        #endregion

        static EnhancedGanttTaskBar()
        {
            // Override the default style key
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EnhancedGanttTaskBar), 
                new FrameworkPropertyMetadata(typeof(EnhancedGanttTaskBar)));
        }

        public EnhancedGanttTaskBar()
        {
            // Initialize with default parameters
            ShapeParameters = ShapeRenderingParameters.CreateDefault(TaskBarShape.Rectangle);
            
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        #region Event Handlers

        private static void OnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EnhancedGanttTaskBar taskBar)
            {
                taskBar.UpdateShapeParameters();
                taskBar.RenderShape();
            }
        }

        private static void OnShapeParametersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EnhancedGanttTaskBar taskBar)
            {
                taskBar.RenderShape();
            }
        }

        private static void OnRenderingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EnhancedGanttTaskBar taskBar)
            {
                taskBar.RenderShape();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeShapeRendering();
            RenderShape();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded && !UseLegacyRendering)
            {
                RenderShape();
            }
        }

        #endregion

        #region Shape Rendering

        private void InitializeShapeRendering()
        {
            if (UseLegacyRendering)
            {
                // Use the existing GanttTaskBar rendering
                return;
            }

            // Create shape container if it doesn't exist
            if (_shapeContainer == null)
            {
                _shapeContainer = new Canvas
                {
                    Background = Brushes.Transparent,
                    IsHitTestVisible = true
                };

                // Replace the existing child with our canvas
                if (Child != null)
                {
                    var existingChild = Child;
                    Child = _shapeContainer;
                    
                    // Add the existing child to our canvas
                    _shapeContainer.Children.Add(existingChild);
                }
                else
                {
                    Child = _shapeContainer;
                }
            }
        }

        private void UpdateShapeParameters()
        {
            if (ShapeParameters == null || ShapeParameters.GetType() != typeof(ShapeRenderingParameters))
            {
                ShapeParameters = ShapeRenderingParameters.CreateDefault(Shape);
            }
        }

        private void RenderShape()
        {
            if (!IsLoaded || ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }

            if (UseLegacyRendering)
            {
                // Remove shape container and revert to legacy rendering
                if (_shapeContainer != null && Child == _shapeContainer)
                {
                    Child = null;
                    _shapeContainer = null;
                    _shapeElement = null;
                }
                return;
            }

            InitializeShapeRendering();

            try
            {
                _currentRenderer = TaskBarShapeRendererFactory.GetRenderer(Shape);
                
                var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
                var fill = Background ?? Brushes.LightBlue;
                var stroke = BorderBrush ?? Brushes.Gray;
                var strokeThickness = BorderThickness.Left;

                // Remove existing shape element
                if (_shapeElement != null && _shapeContainer?.Children.Contains(_shapeElement) == true)
                {
                    _shapeContainer.Children.Remove(_shapeElement);
                }

                // Create new shape element
                _shapeElement = _currentRenderer.CreateVisualElement(bounds, fill, stroke, strokeThickness, ShapeParameters);
                
                if (_shapeContainer != null)
                {
                    // Insert at the beginning so it appears behind other content
                    _shapeContainer.Children.Insert(0, _shapeElement);
                }
            }
            catch (System.NotSupportedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shape rendering failed: {ex.Message}. Falling back to legacy rendering.");
                UseLegacyRendering = true;
            }
        }

        #endregion

        #region Hit Testing Override

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            if (UseLegacyRendering || _currentRenderer == null)
            {
                return base.HitTestCore(hitTestParameters);
            }

            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            var hitGeometry = _currentRenderer.GetHitTestGeometry(bounds, ShapeParameters);
            
            if (hitGeometry.FillContains(hitTestParameters.HitPoint))
            {
                return new PointHitTestResult(this, hitTestParameters.HitPoint);
            }

            return base.HitTestCore(hitTestParameters);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Forces a re-render of the task bar shape.
        /// </summary>
        public void RefreshShape()
        {
            RenderShape();
        }

        /// <summary>
        /// Sets a custom shape with specific parameters.
        /// </summary>
        /// <param name="shape">The shape type to use</param>
        /// <param name="parameters">Custom parameters for the shape</param>
        public void SetCustomShape(TaskBarShape shape, ShapeRenderingParameters parameters)
        {
            ShapeParameters = parameters;
            Shape = shape;
        }

        #endregion
    }
}