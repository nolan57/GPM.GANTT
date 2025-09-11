using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt.Controls
{
    /// <summary>
    /// Multi-level time scale tick control with expand/collapse functionality
    /// </summary>
    public class MultiLevelTimeScaleTick : UserControl
    {
        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register(nameof(Date), typeof(DateTime), typeof(MultiLevelTimeScaleTick));

        public static readonly DependencyProperty LevelTypeProperty =
            DependencyProperty.Register(nameof(LevelType), typeof(ExtendedTimeUnit), typeof(MultiLevelTimeScaleTick));

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(MultiLevelTimeScaleTick));

        public static readonly DependencyProperty ShowExpandButtonProperty =
            DependencyProperty.Register(nameof(ShowExpandButton), typeof(bool), typeof(MultiLevelTimeScaleTick),
                new PropertyMetadata(false, OnShowExpandButtonChanged));

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(MultiLevelTimeScaleTick),
                new PropertyMetadata(false, OnIsExpandedChanged));

        // Events
        public event EventHandler<TimeSegmentExpandEventArgs>? ExpandRequested;
        public event EventHandler<TimeSegmentCollapseEventArgs>? CollapseRequested;

        // UI elements
        private Border? _mainBorder;
        private TextBlock? _displayTextBlock;
        private Button? _expandButton;
        private Grid? _contentGrid;
        private bool _isSubscribed = false;

        public DateTime Date
        {
            get => (DateTime)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public ExtendedTimeUnit LevelType
        {
            get => (ExtendedTimeUnit)GetValue(LevelTypeProperty);
            set => SetValue(LevelTypeProperty, value);
        }

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }

        public bool ShowExpandButton
        {
            get => (bool)GetValue(ShowExpandButtonProperty);
            set => SetValue(ShowExpandButtonProperty, value);
        }

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public MultiLevelTimeScaleTick()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Main container
            _contentGrid = new Grid();
            _contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Main border
            _mainBorder = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Background = Brushes.White
            };

            // Display text
            _displayTextBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };

            // Bind display text
            _displayTextBlock.SetBinding(TextBlock.TextProperty, 
                new System.Windows.Data.Binding(nameof(DisplayText)) { Source = this });

            Grid.SetColumn(_displayTextBlock, 0);
            _contentGrid.Children.Add(_displayTextBlock);

            // Expand button
            _expandButton = new Button
            {
                Content = "▼",
                Width = 16,
                Height = 16,
                Margin = new Thickness(2),
                FontSize = 8,
                Visibility = Visibility.Collapsed
            };

            _expandButton.Click += OnExpandButtonClick;
            Grid.SetColumn(_expandButton, 1);
            _contentGrid.Children.Add(_expandButton);

            _mainBorder.Child = _contentGrid;
            Content = _mainBorder;
        }

        // Method to properly subscribe to events
        public void SubscribeToEvents()
        {
            if (!_isSubscribed && _expandButton != null)
            {
                _expandButton.Click += OnExpandButtonClick;
                _isSubscribed = true;
            }
        }

        // Method to properly unsubscribe from events
        public void UnsubscribeFromEvents()
        {
            if (_isSubscribed && _expandButton != null)
            {
                _expandButton.Click -= OnExpandButtonClick;
                _isSubscribed = false;
            }
        }

        private static void OnShowExpandButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiLevelTimeScaleTick tick)
            {
                tick.OnShowExpandButtonChanged();
            }
        }

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiLevelTimeScaleTick tick)
            {
                tick.UpdateExpandButtonAppearance();
            }
        }

        private void OnShowExpandButtonChanged()
        {
            if (_expandButton != null)
            {
                _expandButton.Visibility = ShowExpandButton ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateExpandButtonAppearance()
        {
            if (_expandButton != null)
            {
                _expandButton.Content = IsExpanded ? "▲" : "▼";
                _expandButton.Background = IsExpanded ? Brushes.LightBlue : Brushes.LightGray;
            }
        }

        private void OnExpandButtonClick(object sender, RoutedEventArgs e)
        {
            // Ensure this runs on the UI thread
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => OnExpandButtonClick(sender, e)));
                return;
            }

            if (IsExpanded)
            {
                // Request collapse
                CollapseRequested?.Invoke(this, new TimeSegmentCollapseEventArgs(Date, LevelType));
            }
            else
            {
                // Request expand
                var targetLevel = GetNextDetailLevel(LevelType);
                ExpandRequested?.Invoke(this, new TimeSegmentExpandEventArgs(Date, LevelType, targetLevel));
            }
        }

        private ExtendedTimeUnit GetNextDetailLevel(ExtendedTimeUnit currentLevel)
        {
            return currentLevel switch
            {
                ExtendedTimeUnit.Year => ExtendedTimeUnit.Quarter,
                ExtendedTimeUnit.Quarter => ExtendedTimeUnit.Month,
                ExtendedTimeUnit.Month => ExtendedTimeUnit.Week,
                ExtendedTimeUnit.Week => ExtendedTimeUnit.Day,
                ExtendedTimeUnit.Day => ExtendedTimeUnit.Hour,
                _ => ExtendedTimeUnit.Day
            };
        }

        public void UpdateStyle(TimeLevelConfiguration config)
        {
            if (config == null) return;

            // Ensure all UI updates happen on the dispatcher thread
            if (Dispatcher.CheckAccess())
            {
                ApplyStyle(config);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => ApplyStyle(config)));
            }
        }

        private void ApplyStyle(TimeLevelConfiguration config)
        {
            Height = config.Height;
            
            if (_mainBorder != null)
                _mainBorder.Background = config.Background;
                
            if (_displayTextBlock != null)
            {
                _displayTextBlock.Foreground = config.Foreground;
                
                // Create FontFamily on UI thread
                if (!string.IsNullOrEmpty(config.FontFamily))
                    _displayTextBlock.FontFamily = new FontFamily(config.FontFamily);
                    
                _displayTextBlock.FontSize = config.FontSize;
            }
        }
    }

    /// <summary>
    /// Time segment expand event arguments
    /// </summary>
    public class TimeSegmentExpandEventArgs : EventArgs
    {
        public DateTime SegmentStart { get; }
        public ExtendedTimeUnit FromLevel { get; }
        public ExtendedTimeUnit ToLevel { get; }

        public TimeSegmentExpandEventArgs(DateTime segmentStart, ExtendedTimeUnit fromLevel, ExtendedTimeUnit toLevel)
        {
            SegmentStart = segmentStart;
            FromLevel = fromLevel;
            ToLevel = toLevel;
        }
    }

    /// <summary>
    /// Time segment collapse event arguments
    /// </summary>
    public class TimeSegmentCollapseEventArgs : EventArgs
    {
        public DateTime SegmentStart { get; }
        public ExtendedTimeUnit Level { get; }

        public TimeSegmentCollapseEventArgs(DateTime segmentStart, ExtendedTimeUnit level)
        {
            SegmentStart = segmentStart;
            Level = level;
        }
    }
}