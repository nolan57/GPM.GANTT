using System;
using System.Windows;
using System.Windows.Controls;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

namespace GPM.Gantt.Controls
{
    /// <summary>
    /// Multi-level time scale control
    /// </summary>
    public class MultiLevelTimeScaleControl : UserControl
    {
        public static readonly DependencyProperty StartDateProperty =
            DependencyProperty.Register(nameof(StartDate), typeof(DateTime), typeof(MultiLevelTimeScaleControl),
                new PropertyMetadata(DateTime.Today, OnTimeRangeChanged));

        public static readonly DependencyProperty EndDateProperty =
            DependencyProperty.Register(nameof(EndDate), typeof(DateTime), typeof(MultiLevelTimeScaleControl),
                new PropertyMetadata(DateTime.Today.AddMonths(1), OnTimeRangeChanged));

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register(nameof(Configuration), typeof(MultiLevelTimeScaleConfiguration), typeof(MultiLevelTimeScaleControl),
                new PropertyMetadata(null, OnConfigurationChanged));

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(MultiLevelTimeScaleControl),
                new PropertyMetadata(1.0, OnZoomLevelChanged));

        // Events
        public event EventHandler<TimeSegmentExpandEventArgs>? TimeSegmentExpanded;
        public event EventHandler<TimeSegmentCollapseEventArgs>? TimeSegmentCollapsed;

        // Services and state
        private IMultiLevelTimeScaleRenderingService _renderingService;
        private FrameworkElement? _currentTimeScale;

        public DateTime StartDate
        {
            get => (DateTime)GetValue(StartDateProperty);
            set => SetValue(StartDateProperty, value);
        }

        public DateTime EndDate
        {
            get => (DateTime)GetValue(EndDateProperty);
            set => SetValue(EndDateProperty, value);
        }

        public MultiLevelTimeScaleConfiguration Configuration
        {
            get => (MultiLevelTimeScaleConfiguration)GetValue(ConfigurationProperty);
            set => SetValue(ConfigurationProperty, value);
        }

        public double ZoomLevel
        {
            get => (double)GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, value);
        }

        public MultiLevelTimeScaleControl()
        {
            _renderingService = new MultiLevelTimeScaleRenderingService();
            
            // Set default configuration
            Configuration = MultiLevelTimeScaleConfiguration.CreateDefault();
            
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshTimeScale();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                RefreshTimeScale();
            }
        }

        private static void OnTimeRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiLevelTimeScaleControl control)
            {
                control.RefreshTimeScale();
            }
        }

        private static void OnConfigurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiLevelTimeScaleControl control)
            {
                control.RefreshTimeScale();
            }
        }

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiLevelTimeScaleControl control)
            {
                control.RefreshTimeScale();
            }
        }

        public void RefreshTimeScale()
        {
            if (Configuration == null || ActualWidth <= 0)
                return;

            var context = new TimeScaleContext
            {
                StartDate = StartDate,
                EndDate = EndDate,
                AvailableWidth = ActualWidth,
                AvailableHeight = ActualHeight,
                ZoomLevel = ZoomLevel,
                MaxVisibleLevels = Configuration.MaxVisibleLevels
            };

            if (_currentTimeScale == null)
            {
                _currentTimeScale = _renderingService.CreateMultiLevelTimeScale(Configuration, context);
                Content = _currentTimeScale;
            }
            else
            {
                // Temporarily unsubscribe from events to prevent issues during update
                UnsubscribeFromTickEvents(_currentTimeScale);
                
                _renderingService.UpdateTimeScale(_currentTimeScale, Configuration, context);
                
                // Re-subscribe to events after update
                SubscribeToTickEvents(_currentTimeScale);
            }

            // Subscribe to expand/collapse events from ticks
            SubscribeToTickEvents(_currentTimeScale);
        }

        private void SubscribeToTickEvents(FrameworkElement timeScale)
        {
            if (timeScale is Grid grid)
            {
                foreach (UIElement child in grid.Children)
                {
                    if (child is Canvas canvas)
                    {
                        foreach (UIElement canvasChild in canvas.Children)
                        {
                            if (canvasChild is MultiLevelTimeScaleTick tick)
                            {
                                // Properly manage event subscriptions to avoid duplicates
                                tick.ExpandRequested -= OnTickExpandRequested;
                                tick.CollapseRequested -= OnTickCollapseRequested;
                                tick.ExpandRequested += OnTickExpandRequested;
                                tick.CollapseRequested += OnTickCollapseRequested;
                            }
                        }
                    }
                }
            }
        }

        private void OnTickExpandRequested(object? sender, TimeSegmentExpandEventArgs e)
        {
            // Temporarily unsubscribe to prevent recursive calls during refresh
            if (_currentTimeScale != null)
            {
                UnsubscribeFromTickEvents(_currentTimeScale);
            }
            
            try
            {
                // Expand the time segment
                _renderingService.ExpandTimeSegment(_currentTimeScale, e.SegmentStart, e.ToLevel);
                
                // Update configuration
                Configuration.ExpandSegment(e.SegmentStart, e.FromLevel, e.ToLevel);
                
                // Refresh display
                RefreshTimeScale();
                
                // Notify external listeners
                TimeSegmentExpanded?.Invoke(this, e);
            }
            finally
            {
                // Re-subscribe to events
                if (_currentTimeScale != null)
                {
                    SubscribeToTickEvents(_currentTimeScale);
                }
            }
        }

        private void OnTickCollapseRequested(object? sender, TimeSegmentCollapseEventArgs e)
        {
            // Temporarily unsubscribe to prevent recursive calls during refresh
            if (_currentTimeScale != null)
            {
                UnsubscribeFromTickEvents(_currentTimeScale);
            }
            
            try
            {
                // Collapse the time segment
                _renderingService.CollapseTimeSegment(_currentTimeScale, e.SegmentStart);
                
                // Update configuration
                Configuration.CollapseSegment(e.SegmentStart);
                
                // Refresh display
                RefreshTimeScale();
                
                // Notify external listeners
                TimeSegmentCollapsed?.Invoke(this, e);
            }
            finally
            {
                // Re-subscribe to events
                if (_currentTimeScale != null)
                {
                    SubscribeToTickEvents(_currentTimeScale);
                }
            }
        }

        private void UnsubscribeFromTickEvents(FrameworkElement timeScale)
        {
            if (timeScale is Grid grid)
            {
                foreach (UIElement child in grid.Children)
                {
                    if (child is Canvas canvas)
                    {
                        foreach (UIElement canvasChild in canvas.Children)
                        {
                            if (canvasChild is MultiLevelTimeScaleTick tick)
                            {
                                tick.ExpandRequested -= OnTickExpandRequested;
                                tick.CollapseRequested -= OnTickCollapseRequested;
                            }
                        }
                    }
                }
            }
        }

        public void ExpandTimeSegment(DateTime segmentStart, ExtendedTimeUnit toLevel)
        {
            if (_currentTimeScale != null)
            {
                _renderingService.ExpandTimeSegment(_currentTimeScale, segmentStart, toLevel);
                Configuration.ExpandSegment(segmentStart, ExtendedTimeUnit.Week, toLevel);
                RefreshTimeScale();
            }
        }

        public void CollapseTimeSegment(DateTime segmentStart)
        {
            if (_currentTimeScale != null)
            {
                _renderingService.CollapseTimeSegment(_currentTimeScale, segmentStart);
                Configuration.CollapseSegment(segmentStart);
                RefreshTimeScale();
            }
        }

        public void SetTimeRange(DateTime start, DateTime end)
        {
            StartDate = start;
            EndDate = end;
        }

        public void ZoomIn()
        {
            ZoomLevel = Math.Min(ZoomLevel * 1.5, 10.0);
        }

        public void ZoomOut()
        {
            ZoomLevel = Math.Max(ZoomLevel / 1.5, 0.1);
        }

        public void ResetZoom()
        {
            ZoomLevel = 1.0;
        }
    }
}