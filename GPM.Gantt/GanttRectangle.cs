using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace GPM.Gantt
{
    // Rectangle base class: Common base class for all time cells, grid rows, and grid cells
    // Since WPF's System.Windows.Shapes.Rectangle is sealed, we use Border as an inheritable rectangle appearance base class
    public class GanttRectangle : Border
    {
        public static readonly DependencyProperty TimeIndexProperty = DependencyProperty.Register(
            nameof(TimeIndex), typeof(int), typeof(GanttRectangle), new FrameworkPropertyMetadata(0));

        public int TimeIndex
        {
            get => (int)GetValue(TimeIndexProperty);
            set => SetValue(TimeIndexProperty, value);
        }

        public static readonly DependencyProperty RowIndexProperty = DependencyProperty.Register(
            nameof(RowIndex), typeof(int), typeof(GanttRectangle), new FrameworkPropertyMetadata(0));

        public int RowIndex
        {
            get => (int)GetValue(RowIndexProperty);
            set => SetValue(RowIndexProperty, value);
        }

        // --- Added common state properties ---
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(GanttRectangle), new FrameworkPropertyMetadata(false));
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsHoveredProperty = DependencyProperty.Register(
            nameof(IsHovered), typeof(bool), typeof(GanttRectangle), new FrameworkPropertyMetadata(false));
        public bool IsHovered
        {
            get => (bool)GetValue(IsHoveredProperty);
            set => SetValue(IsHoveredProperty, value);
        }

        public static readonly DependencyProperty IsTodayProperty = DependencyProperty.Register(
            nameof(IsToday), typeof(bool), typeof(GanttRectangle), new FrameworkPropertyMetadata(false));
        public bool IsToday
        {
            get => (bool)GetValue(IsTodayProperty);
            set => SetValue(IsTodayProperty, value);
        }

        public static readonly DependencyProperty IsWeekendProperty = DependencyProperty.Register(
            nameof(IsWeekend), typeof(bool), typeof(GanttRectangle), new FrameworkPropertyMetadata(false));
        public bool IsWeekend
        {
            get => (bool)GetValue(IsWeekendProperty);
            set => SetValue(IsWeekendProperty, value);
        }

        // ZIndex wrapper property, syncs to Panel.ZIndex when changed
        public static readonly DependencyProperty ZIndexProperty = DependencyProperty.Register(
            nameof(ZIndex), typeof(int), typeof(GanttRectangle), new FrameworkPropertyMetadata(0, OnZIndexChanged));
        public int ZIndex
        {
            get => (int)GetValue(ZIndexProperty);
            set => SetValue(ZIndexProperty, value);
        }
        private static void OnZIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttRectangle r)
            {
                Panel.SetZIndex(r, (int)e.NewValue);
            }
        }

        // Whether interactive: syncs to IsHitTestVisible
        public static readonly DependencyProperty IsInteractiveProperty = DependencyProperty.Register(
            nameof(IsInteractive), typeof(bool), typeof(GanttRectangle), new FrameworkPropertyMetadata(true, OnIsInteractiveChanged));
        public bool IsInteractive
        {
            get => (bool)GetValue(IsInteractiveProperty);
            set => SetValue(IsInteractiveProperty, value);
        }
        private static void OnIsInteractiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttRectangle r)
            {
                r.IsHitTestVisible = (bool)e.NewValue;
            }
        }

        // --- Appearance and Layout: background color, outline style, size, font, corner radius ---
        // Background color, corner radius, size (Width/Height/Min/Max) are directly inherited from Border/FrameworkElement, no need to redefine.
        // Here we provide outline style and font family properties (font properties do not directly affect Border, this class is a container, used by child elements or templates later).

        public static readonly DependencyProperty OutlineBrushProperty = DependencyProperty.Register(
            nameof(OutlineBrush), typeof(Brush), typeof(GanttRectangle), new FrameworkPropertyMetadata(Brushes.Gray, OnOutlineChanged));
        public Brush OutlineBrush
        {
            get => (Brush)GetValue(OutlineBrushProperty);
            set => SetValue(OutlineBrushProperty, value);
        }

        public static readonly DependencyProperty OutlineThicknessProperty = DependencyProperty.Register(
            nameof(OutlineThickness), typeof(Thickness), typeof(GanttRectangle), new FrameworkPropertyMetadata(new Thickness(1), OnOutlineChanged));
        public Thickness OutlineThickness
        {
            get => (Thickness)GetValue(OutlineThicknessProperty);
            set => SetValue(OutlineThicknessProperty, value);
        }

        private static void OnOutlineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttRectangle r)
            {
                r.BorderBrush = r.OutlineBrush;
                r.BorderThickness = r.OutlineThickness;
            }
        }

        public static readonly DependencyProperty ContentPaddingProperty = DependencyProperty.Register(
            nameof(ContentPadding), typeof(Thickness), typeof(GanttRectangle), new FrameworkPropertyMetadata(new Thickness(0), OnPaddingChanged));
        public Thickness ContentPadding
        {
            get => (Thickness)GetValue(ContentPaddingProperty);
            set => SetValue(ContentPaddingProperty, value);
        }
        private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttRectangle r)
            {
                r.Padding = (Thickness)e.NewValue;
            }
        }

        // Font properties: As inheritable dependency properties for child elements (like TextBlock), not directly applied to Border
        public static readonly DependencyProperty FontFamilyExProperty = DependencyProperty.Register(
            nameof(FontFamilyEx), typeof(FontFamily), typeof(GanttRectangle), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
        public FontFamily FontFamilyEx
        {
            get => (FontFamily)GetValue(FontFamilyExProperty);
            set => SetValue(FontFamilyExProperty, value);
        }
        public static readonly DependencyProperty FontSizeExProperty = DependencyProperty.Register(
            nameof(FontSizeEx), typeof(double), typeof(GanttRectangle), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
        public double FontSizeEx
        {
            get => (double)GetValue(FontSizeExProperty);
            set => SetValue(FontSizeExProperty, value);
        }
        public static readonly DependencyProperty FontWeightExProperty = DependencyProperty.Register(
            nameof(FontWeightEx), typeof(FontWeight), typeof(GanttRectangle), new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
        public FontWeight FontWeightEx
        {
            get => (FontWeight)GetValue(FontWeightExProperty);
            set => SetValue(FontWeightExProperty, value);
        }
        public static readonly DependencyProperty FontStyleExProperty = DependencyProperty.Register(
            nameof(FontStyleEx), typeof(FontStyle), typeof(GanttRectangle), new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
        public FontStyle FontStyleEx
        {
            get => (FontStyle)GetValue(FontStyleExProperty);
            set => SetValue(FontStyleExProperty, value);
        }

        public GanttRectangle()
        {
            BorderBrush = Brushes.Gray;
            BorderThickness = new Thickness(1);
            Background = Brushes.Transparent;

            // Automatically maintain hover state (visual is controlled by upper-level styles)
            MouseEnter += (_, __) => IsHovered = true;
            MouseLeave += (_, __) => IsHovered = false;

            // Demo interaction: Click to toggle selected state (can be disabled with IsInteractive)
            MouseLeftButtonDown += (_, __) =>
            {
                if (IsInteractive)
                {
                    IsSelected = !IsSelected;
                }
            };
        }
    }
}
