using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

namespace GPM.Gantt.Interaction
{
    /// <summary>
    /// Manages visual feedback for interactive operations like drag-and-drop and resizing.
    /// </summary>
    public class VisualFeedbackManager
    {
        #region Private Fields

        private Panel? _parentPanel;
        private Canvas? _feedbackCanvas;
        private Rectangle? _dragPreview;
        private Rectangle? _resizePreview;
        private Rectangle? _dropIndicator;
        private Storyboard? _pulseAnimation;

        #endregion

        #region Drag Preview Methods

        /// <summary>
        /// Shows a visual preview for the drag operation.
        /// </summary>
        /// <param name="taskBar">The task bar being dragged.</param>
        public void ShowDragPreview(GanttTaskBar taskBar)
        {
            if (taskBar?.Parent is not Panel parent) return;

            _parentPanel = parent;
            EnsureFeedbackCanvas();

            _dragPreview = new Rectangle
            {
                Width = taskBar.ActualWidth,
                Height = taskBar.ActualHeight,
                Fill = new SolidColorBrush(Color.FromArgb(128, 33, 150, 243)), // Semi-transparent blue
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 2,
                RadiusX = 4,
                RadiusY = 4,
                IsHitTestVisible = false,
                RenderTransform = new ScaleTransform(),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var position = taskBar.TranslatePoint(new Point(0, 0), _feedbackCanvas);
            Canvas.SetLeft(_dragPreview, position.X);
            Canvas.SetTop(_dragPreview, position.Y);
            Canvas.SetZIndex(_dragPreview, 998);

            _feedbackCanvas?.Children.Add(_dragPreview);

            // Add subtle animation (duration and easing from tokens)
            _pulseAnimation = CreatePulseAnimation();
            _pulseAnimation.Begin(_dragPreview);
        }

        /// <summary>
        /// Updates the drag preview position and validity indication.
        /// </summary>
        /// <param name="taskBar">The task bar being dragged.</param>
        /// <param name="currentPosition">Current mouse position.</param>
        /// <param name="isValidDrop">Whether the current position is a valid drop target.</param>
        public void UpdateDragPreview(GanttTaskBar taskBar, Point currentPosition, bool isValidDrop)
        {
            if (_dragPreview == null || _feedbackCanvas == null) return;

            // Update position
            Canvas.SetLeft(_dragPreview, currentPosition.X - _dragPreview.Width / 2);
            Canvas.SetTop(_dragPreview, currentPosition.Y - _dragPreview.Height / 2);

            // Update visual state based on validity
            if (isValidDrop)
            {
                _dragPreview.Fill = new SolidColorBrush(Color.FromArgb(128, 76, 175, 80)); // Semi-transparent green
                _dragPreview.Stroke = Brushes.LimeGreen;
                ShowDropIndicator(currentPosition);
            }
            else
            {
                _dragPreview.Fill = new SolidColorBrush(Color.FromArgb(128, 244, 67, 54)); // Semi-transparent red
                _dragPreview.Stroke = Brushes.Red;
                HideDropIndicator();
            }
        }

        /// <summary>
        /// Hides the drag preview.
        /// </summary>
        public void HideDragPreview()
        {
            if (_dragPreview != null && _feedbackCanvas != null)
            {
                _pulseAnimation?.Stop();

                // Animate fade out using tokens
                var animTokens = ThemeManager.Tokens.GetAnimationTokens();
                var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(animTokens.DragPreviewFadeOutMs))
                {
                    EasingFunction = GetEasing(animTokens.FadeEasing)
                };
                fadeOut.Completed += (s, e) =>
                {
                    _feedbackCanvas.Children.Remove(_dragPreview);
                    _dragPreview = null;
                };
                _dragPreview.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }

            HideDropIndicator();
        }

        #endregion

        #region Resize Preview Methods

        /// <summary>
        /// Shows a preview for the resize operation.
        /// </summary>
        /// <param name="taskBar">The task bar being resized.</param>
        /// <param name="direction">The resize direction.</param>
        public void ShowResizePreview(GanttTaskBar taskBar, ResizeDirection direction)
        {
            if (taskBar?.Parent is not Panel parent) return;

            _parentPanel = parent;
            EnsureFeedbackCanvas();

            _resizePreview = new Rectangle
            {
                Width = taskBar.ActualWidth,
                Height = taskBar.ActualHeight,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Orange,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 5, 5 },
                IsHitTestVisible = false
            };

            var position = taskBar.TranslatePoint(new Point(0, 0), _feedbackCanvas);
            Canvas.SetLeft(_resizePreview, position.X);
            Canvas.SetTop(_resizePreview, position.Y);
            Canvas.SetZIndex(_resizePreview, 999);

            _feedbackCanvas?.Children.Add(_resizePreview);
        }

        /// <summary>
        /// Updates the resize preview.
        /// </summary>
        /// <param name="taskBar">The task bar being resized.</param>
        /// <param name="currentPosition">Current mouse position.</param>
        /// <param name="direction">The resize direction.</param>
        /// <param name="isValid">Whether the resize is valid.</param>
        public void UpdateResizePreview(GanttTaskBar taskBar, Point currentPosition, ResizeDirection direction, bool isValid)
        {
            if (_resizePreview == null || _feedbackCanvas == null) return;

            var originalPosition = taskBar.TranslatePoint(new Point(0, 0), _feedbackCanvas);
            var deltaX = currentPosition.X - originalPosition.X;

            if (direction == ResizeDirection.Left)
            {
                var newLeft = Math.Min(originalPosition.X + deltaX, originalPosition.X + taskBar.ActualWidth - 20);
                var newWidth = originalPosition.X + taskBar.ActualWidth - newLeft;
                
                Canvas.SetLeft(_resizePreview, newLeft);
                _resizePreview.Width = Math.Max(20, newWidth);
            }
            else if (direction == ResizeDirection.Right)
            {
                var newWidth = Math.Max(20, taskBar.ActualWidth + deltaX);
                _resizePreview.Width = newWidth;
            }

            _resizePreview.Stroke = isValid ? Brushes.Orange : Brushes.Red;
        }

        /// <summary>
        /// Hides the resize preview.
        /// </summary>
        public void HideResizePreview()
        {
            if (_resizePreview != null && _feedbackCanvas != null)
            {
                _feedbackCanvas.Children.Remove(_resizePreview);
                _resizePreview = null;
            }
        }

        #endregion

        #region Drop Indicator Methods

        private void ShowDropIndicator(Point position)
        {
            if (_feedbackCanvas == null) return;

            if (_dropIndicator == null)
            {
                _dropIndicator = new Rectangle
                {
                    Width = 2,
                    Height = 30,
                    Fill = Brushes.LimeGreen,
                    IsHitTestVisible = false
                };

                _feedbackCanvas.Children.Add(_dropIndicator);
            }

            Canvas.SetLeft(_dropIndicator, position.X - 1);
            Canvas.SetTop(_dropIndicator, position.Y - 15);
            Canvas.SetZIndex(_dropIndicator, 1001);
            _dropIndicator.Visibility = Visibility.Visible;
        }

        private void HideDropIndicator()
        {
            if (_dropIndicator != null)
            {
                _dropIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Helper Methods

        private void EnsureFeedbackCanvas()
        {
            if (_feedbackCanvas != null && _parentPanel != null && _parentPanel.Children.Contains(_feedbackCanvas))
                return;

            if (_parentPanel == null) return;

            _feedbackCanvas = new Canvas
            {
                IsHitTestVisible = false,
                Background = Brushes.Transparent
            };

            // Make canvas fill the entire parent
            if (_parentPanel is Grid grid)
            {
                Grid.SetRowSpan(_feedbackCanvas, grid.RowDefinitions.Count);
                Grid.SetColumnSpan(_feedbackCanvas, grid.ColumnDefinitions.Count);
            }

            Panel.SetZIndex(_feedbackCanvas, 1000);
            _parentPanel.Children.Add(_feedbackCanvas);
        }

        private Storyboard CreatePulseAnimation()
        {
            var tokens = ThemeManager.Tokens.GetAnimationTokens();

            var storyboard = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever
            };

            var easing = GetEasing(tokens.DragPreviewPulseEasing);

            var scaleXAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.05,
                Duration = TimeSpan.FromMilliseconds(tokens.DragPreviewPulseDurationMs),
                AutoReverse = true,
                EasingFunction = easing
            };

            var scaleYAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.05,
                Duration = TimeSpan.FromMilliseconds(tokens.DragPreviewPulseDurationMs),
                AutoReverse = true,
                EasingFunction = easing
            };

            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);

            return storyboard;
        }

        private static IEasingFunction GetEasing(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id)) return new QuadraticEase { EasingMode = EasingMode.EaseOut };
                var key = id.Trim();
                var lower = key.ToLowerInvariant();

                EasingMode mode = EasingMode.EaseOut;
                if (lower.EndsWith("easeinout")) mode = EasingMode.EaseInOut;
                else if (lower.EndsWith("easein")) mode = EasingMode.EaseIn;
                else if (lower.EndsWith("easeout")) mode = EasingMode.EaseOut;

                if (lower.StartsWith("cubic")) return new CubicEase { EasingMode = mode };
                if (lower.StartsWith("quadratic")) return new QuadraticEase { EasingMode = mode };
                if (lower.StartsWith("sine")) return new SineEase { EasingMode = mode };
                if (lower.StartsWith("quartic")) return new QuarticEase { EasingMode = mode };
                if (lower.StartsWith("quintic")) return new QuinticEase { EasingMode = mode };
                if (lower.StartsWith("back")) return new BackEase { EasingMode = mode };
                if (lower.StartsWith("bounce")) return new BounceEase { EasingMode = mode };
                if (lower.StartsWith("circle") || lower.StartsWith("circular")) return new CircleEase { EasingMode = mode };
                if (lower.StartsWith("exponential") || lower.StartsWith("expo")) return new ExponentialEase { EasingMode = mode };
                if (lower.StartsWith("power")) return new PowerEase { EasingMode = mode };
                if (lower.StartsWith("elastic")) return new ElasticEase { EasingMode = mode };

                return new QuadraticEase { EasingMode = EasingMode.EaseOut };
            }
            catch
            {
                return new QuadraticEase { EasingMode = EasingMode.EaseOut };
            }
        }

        public void Cleanup()
        {
            HideDragPreview();
            HideResizePreview();
            HideDropIndicator();

            if (_feedbackCanvas != null && _parentPanel != null)
            {
                _parentPanel.Children.Remove(_feedbackCanvas);
                _feedbackCanvas = null;
            }

            _pulseAnimation?.Stop();
        }

        #endregion
    }
}