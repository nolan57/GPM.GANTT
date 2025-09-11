# Drawing Special Shapes Based on GanttShapeBase

## Overview

The [GanttShapeBase](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L11-L16) class is the foundation for creating custom shapes in the GPM Gantt framework. It provides GPU acceleration support, dependency properties for visual customization, and a flexible rendering system that can be extended to create any kind of special shape.

This guide explains how to create custom shapes by extending [GanttShapeBase](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L11-L16) directly or by implementing custom task bar shape renderers.

## GanttShapeBase Fundamentals

### Key Features

1. **GPU Acceleration Support**: Through the [EnableGpuRendering](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L89-L94) property
2. **Dependency Properties**: For common visual properties (Fill, Stroke, etc.)
3. **Flexible Rendering**: Override [RenderWithWpf](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L175-L227) for custom rendering or [RenderWithGpu](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L157-L173) for GPU-accelerated rendering
4. **Built-in Properties**: Background, Fill, Stroke, StrokeThickness, CornerRadius, BorderBrush, BorderThickness

### Core Properties

| Property | Type | Description |
|----------|------|-------------|
| [Background](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L97-L102) | Brush | Background brush for the shape |
| [Fill](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L107-L112) | Brush | Fill brush for the shape |
| [Stroke](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L117-L122) | Brush | Stroke brush for the outline |
| [StrokeThickness](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L127-L132) | double | Thickness of the outline |
| [CornerRadius](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L137-L142) | double | Corner radius for rounded shapes |
| [EnableGpuRendering](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L89-L94) | bool | Enable GPU-accelerated rendering |

## Approach 1: Extending GanttShapeBase Directly

This approach is best for creating standalone shape controls that can be used directly in XAML.

### Basic Implementation

```csharp
using System;
using System.Windows;
using System.Windows.Media;
using GPM.Gantt;

namespace GPM.Gantt.CustomShapes
{
    /// <summary>
    /// A custom triangle shape control based on GanttShapeBase
    /// </summary>
    public class TriangleShape : GanttShapeBase
    {
        protected override void RenderWithWpf(DrawingContext drawingContext)
        {
            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            
            // Draw background first
            if (Background != null && !Background.Equals(Brushes.Transparent))
            {
                drawingContext.DrawRectangle(Background, null, bounds);
            }
            
            // Create triangle geometry
            var geometry = CreateTriangleGeometry(bounds);
            
            // Draw fill
            if (Fill != null && !Fill.Equals(Brushes.Transparent))
            {
                drawingContext.DrawGeometry(Fill, null, geometry);
            }
            
            // Draw stroke
            if (Stroke != null && StrokeThickness > 0)
            {
                var pen = new Pen(Stroke, StrokeThickness);
                drawingContext.DrawGeometry(null, pen, geometry);
            }
        }
        
        private Geometry CreateTriangleGeometry(Rect bounds)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            
            // Triangle points (pointing right)
            var leftPoint = new Point(bounds.Left, bounds.Top + bounds.Height / 2);
            var topPoint = new Point(bounds.Right, bounds.Top);
            var bottomPoint = new Point(bounds.Right, bounds.Bottom);
            
            figure.StartPoint = leftPoint;
            figure.Segments.Add(new LineSegment { Point = topPoint });
            figure.Segments.Add(new LineSegment { Point = bottomPoint });
            figure.IsClosed = true;
            
            geometry.Figures.Add(figure);
            return geometry;
        }
    }
}
```

### Usage in XAML

```xml
<Window x:Class="GPM.Gantt.Demo.CustomShapesDemo"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:GPM.Gantt.CustomShapes">
    
    <Grid>
        <local:TriangleShape 
            Width="100" 
            Height="100" 
            Fill="Blue" 
            Stroke="DarkBlue" 
            StrokeThickness="2" 
            EnableGpuRendering="True" />
    </Grid>
</Window>
```

## Approach 2: Creating Custom Task Bar Shapes

For shapes that should be used as task bars in the Gantt chart, implement the [ITaskBarShapeRenderer](file:///D:/Documents/CS/CS615/GPM/Rendering/ITaskBarShapeRenderer.cs#L11-L58) interface.

### Implementation Steps

1. Define a new shape type in the [TaskBarShape](file:///D:/Documents/CS/CS615/GPM/Models/TaskBarShape.cs#L9-L16) enum
2. Implement the [ITaskBarShapeRenderer](file:///D:/Documents/CS/CS615/GPM/Rendering/ITaskBarShapeRenderer.cs#L11-L58) interface
3. Register your renderer with the [TaskBarShapeRendererFactory](file:///D:/Documents/CS/CS615/GPM/Rendering/TaskBarShapeRendererFactory.cs#L10-L47)

### Example 1: Star Shape Renderer

```csharp
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Models;
using GPM.Gantt.Rendering;

namespace GPM.Gantt.Rendering.Shapes
{
    /// <summary>
    /// Renders star-shaped task bars.
    /// </summary>
    public class StarTaskBarRenderer : ITaskBarShapeRenderer
    {
        public TaskBarShape ShapeType => TaskBarShape.Star;

        public Geometry CreateGeometry(Rect bounds, double cornerRadius = 0, ShapeRenderingParameters? parameters = null)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            
            // Calculate star points
            var centerX = bounds.X + bounds.Width / 2;
            var centerY = bounds.Y + bounds.Height / 2;
            var outerRadius = Math.Min(bounds.Width, bounds.Height) / 2;
            var innerRadius = outerRadius * 0.4;
            
            // Start at top point
            figure.StartPoint = new Point(centerX, centerY - outerRadius);
            
            // Create the 5 points of the star
            for (int i = 1; i < 10; i++)
            {
                var angle = Math.PI / 2 + i * Math.PI * 2 / 10; // 10 points (5 outer, 5 inner)
                var radius = i % 2 == 0 ? outerRadius : innerRadius;
                var x = centerX + radius * Math.Cos(angle);
                var y = centerY - radius * Math.Sin(angle); // Negative because Y increases downward
                figure.Segments.Add(new LineSegment { Point = new Point(x, y) });
            }
            
            figure.IsClosed = true;
            geometry.Figures.Add(figure);
            
            return geometry;
        }

        public FrameworkElement CreateVisualElement(Rect bounds, Brush brush, Brush stroke, double strokeThickness, ShapeRenderingParameters? parameters = null)
        {
            var geometry = CreateGeometry(bounds, 0, parameters);
            
            var path = new Path
            {
                Data = geometry,
                Fill = brush,
                Stroke = stroke,
                StrokeThickness = strokeThickness
            };
            
            Canvas.SetLeft(path, bounds.Left);
            Canvas.SetTop(path, bounds.Top);
            
            return path;
        }

        public Geometry GetHitTestGeometry(Rect bounds, ShapeRenderingParameters? parameters = null)
        {
            return CreateGeometry(bounds, 0, parameters);
        }
    }
}
```

### Example 2: Hexagon Shape Renderer

```csharp
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Models;
using GPM.Gantt.Rendering;

namespace GPM.Gantt.Rendering.Shapes
{
    /// <summary>
    /// Renders hexagon-shaped task bars.
    /// </summary>
    public class HexagonTaskBarRenderer : ITaskBarShapeRenderer
    {
        public TaskBarShape ShapeType => TaskBarShape.Hexagon;

        public Geometry CreateGeometry(Rect bounds, double cornerRadius = 0, ShapeRenderingParameters? parameters = null)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            
            // Calculate hexagon points
            var centerX = bounds.X + bounds.Width / 2;
            var centerY = bounds.Y + bounds.Height / 2;
            var radius = Math.Min(bounds.Width, bounds.Height) / 2;
            
            // Create 6 points of the hexagon
            var points = new Point[6];
            for (int i = 0; i < 6; i++)
            {
                var angle = Math.PI / 2 + i * Math.PI * 2 / 6; // 6 points
                var x = centerX + radius * Math.Cos(angle);
                var y = centerY - radius * Math.Sin(angle); // Negative because Y increases downward
                points[i] = new Point(x, y);
            }
            
            // Start at first point
            figure.StartPoint = points[0];
            
            // Connect all points
            for (int i = 1; i < 6; i++)
            {
                figure.Segments.Add(new LineSegment { Point = points[i] });
            }
            
            figure.IsClosed = true;
            geometry.Figures.Add(figure);
            
            return geometry;
        }

        public FrameworkElement CreateVisualElement(Rect bounds, Brush brush, Brush stroke, double strokeThickness, ShapeRenderingParameters? parameters = null)
        {
            var geometry = CreateGeometry(bounds, 0, parameters);
            
            var path = new Path
            {
                Data = geometry,
                Fill = brush,
                Stroke = stroke,
                StrokeThickness = strokeThickness
            };
            
            Canvas.SetLeft(path, bounds.Left);
            Canvas.SetTop(path, bounds.Top);
            
            return path;
        }

        public Geometry GetHitTestGeometry(Rect bounds, ShapeRenderingParameters? parameters = null)
        {
            return CreateGeometry(bounds, 0, parameters);
        }
    }
}
```

### Registration and Usage

1. Add new shape types to the [TaskBarShape](file:///D:/Documents/CS/CS615/GPM/Models/TaskBarShape.cs#L9-L16) enum:
```csharp
public enum TaskBarShape
{
    // ... existing shapes ...
    Star = 5,
    Hexagon = 6
}
```

2. Register your renderers in the [TaskBarShapeRendererFactory](file:///D:/Documents/CS/CS615/GPM/Rendering/TaskBarShapeRendererFactory.cs#L10-L47):
```csharp
// In TaskBarShapeRendererFactory.cs
private static readonly Dictionary<TaskBarShape, ITaskBarShapeRenderer> _renderers = new()
{
    // ... existing renderers ...
    { TaskBarShape.Star, new StarTaskBarRenderer() },
    { TaskBarShape.Hexagon, new HexagonTaskBarRenderer() }
};
```

3. Use in your Gantt tasks:
```csharp
// Star-shaped task
var starTask = new GanttTask
{
    Title = "Star Task",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(5),
    RowIndex = 1,
    Shape = TaskBarShape.Star,
    ShapeParameters = new ShapeRenderingParameters()
};

// Hexagon-shaped task
var hexagonTask = new GanttTask
{
    Title = "Hexagon Task",
    Start = DateTime.Today.AddDays(2),
    End = DateTime.Today.AddDays(8),
    RowIndex = 2,
    Shape = TaskBarShape.Hexagon,
    ShapeParameters = new ShapeRenderingParameters()
};
```

## Advanced Customization

### Custom Shape Parameters

Extend the [ShapeRenderingParameters](file:///D:/Documents/CS/CS615/GPM/Rendering/ShapeRenderingParameters.cs#L10-L110) class to support custom properties:

```csharp
public class ExtendedShapeRenderingParameters : ShapeRenderingParameters
{
    /// <summary>
    /// Number of points for star shapes.
    /// </summary>
    public int StarPoints { get; set; } = 5;
    
    /// <summary>
    /// Inner radius ratio for star shapes.
    /// </summary>
    public double InnerRadiusRatio { get; set; } = 0.4;
    
    /// <summary>
    /// Creates default parameters for extended shapes.
    /// </summary>
    public static ExtendedShapeRenderingParameters CreateDefault(TaskBarShape shapeType)
    {
        var baseParams = ShapeRenderingParameters.CreateDefault(shapeType);
        var extendedParams = new ExtendedShapeRenderingParameters
        {
            CornerRadius = baseParams.CornerRadius,
            DiamondEndHeight = baseParams.DiamondEndHeight,
            DiamondEndWidth = baseParams.DiamondEndWidth,
            ChevronAngle = baseParams.ChevronAngle,
            CustomGeometry = baseParams.CustomGeometry,
            CustomProperties = baseParams.CustomProperties
        };
        
        if (shapeType == TaskBarShape.Star)
        {
            extendedParams.StarPoints = 5;
            extendedParams.InnerRadiusRatio = 0.4;
        }
        
        return extendedParams;
    }
}
```

### GPU-Accelerated Rendering

To implement GPU-accelerated rendering for your custom shapes, override the [RenderWithGpu](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L157-L173) method:

```csharp
public class CustomGpuShape : GanttShapeBase
{
    protected override void RenderWithGpu(DrawingContext drawingContext)
    {
        if (_gpuRenderingService != null)
        {
            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            
            // Custom GPU rendering implementation
            _gpuRenderingService.RenderCustomShape(
                drawingContext, 
                bounds, 
                Fill, 
                Stroke, 
                StrokeThickness, 
                CornerRadius);
        }
        else
        {
            // Fallback to WPF rendering
            RenderWithWpf(drawingContext);
        }
    }
}
```

## Best Practices

### 1. Performance Optimization

- Reuse geometry objects when possible
- Implement proper hit testing geometry
- Use simplified rendering for small shapes
- Consider virtualization for large datasets

### 2. Error Handling

- Validate input parameters
- Provide fallback rendering methods
- Handle exceptions gracefully
- Log errors appropriately

### 3. Accessibility

- Ensure adequate contrast between fill and stroke
- Maintain minimum sizes for visibility
- Test with different DPI settings
- Consider colorblind-friendly palettes

### 4. Memory Management

- Dispose of resources properly
- Avoid creating unnecessary objects in rendering loops
- Use object pooling for frequently created geometries
- Monitor memory usage during development

## Integration Examples

### Using Custom Shapes in GanttContainer

```csharp
var ganttContainer = new GanttContainer
{
    Configuration = new GanttConfiguration
    {
        Rendering = new RenderingConfiguration
        {
            UseEnhancedShapeRendering = true,
            DefaultTaskBarShape = TaskBarShape.Star
        }
    }
};

// Add tasks with custom shapes
ganttContainer.Tasks.Add(new GanttTask
{
    Title = "Custom Star Task",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(5),
    RowIndex = 1,
    Shape = TaskBarShape.Star,
    ShapeParameters = new ShapeRenderingParameters()
});
```

### Creating a Shape Gallery

```csharp
public class ShapeGallery : GanttShapeBase
{
    public static readonly DependencyProperty ShapeTypeProperty = DependencyProperty.Register(
        nameof(ShapeType), typeof(TaskBarShape), typeof(ShapeGallery), 
        new FrameworkPropertyMetadata(TaskBarShape.Rectangle, FrameworkPropertyMetadataOptions.AffectsRender));

    public TaskBarShape ShapeType
    {
        get { return (TaskBarShape)GetValue(ShapeTypeProperty); }
        set { SetValue(ShapeTypeProperty, value); }
    }

    protected override void RenderWithWpf(DrawingContext drawingContext)
    {
        var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
        
        // Draw background
        if (Background != null && !Background.Equals(Brushes.Transparent))
        {
            drawingContext.DrawRectangle(Background, null, bounds);
        }
        
        // Get renderer for the specified shape type
        var renderer = TaskBarShapeRendererFactory.GetRenderer(ShapeType);
        if (renderer != null)
        {
            var geometry = renderer.CreateGeometry(bounds, CornerRadius);
            
            // Draw fill
            if (Fill != null && !Fill.Equals(Brushes.Transparent))
            {
                drawingContext.DrawGeometry(Fill, null, geometry);
            }
            
            // Draw stroke
            if (Stroke != null && StrokeThickness > 0)
            {
                var pen = new Pen(Stroke, StrokeThickness);
                drawingContext.DrawGeometry(null, pen, geometry);
            }
        }
        else
        {
            // Fallback to rectangle
            base.RenderWithWpf(drawingContext);
        }
    }
}
```

## Conclusion

Creating special shapes based on [GanttShapeBase](file:///D:/Documents/CS/CS615/GPM/GanttShapeBase.cs#L11-L16) provides a powerful way to extend the visual capabilities of the GPM Gantt framework. Whether you're creating standalone shape controls or integrating with the task bar rendering system, the flexible architecture allows for virtually any custom shape implementation.

Remember to:
1. Choose the appropriate approach based on your use case
2. Follow best practices for performance and accessibility
3. Properly register custom renderers when extending the task bar system
4. Test thoroughly across different scenarios and configurations

> Note on DropShadowEffect
> If a DropShadowEffect is applied to a shape derived from GanttShapeBase, the renderer insets the drawn geometry slightly (a few pixels) within the control bounds. This allows the blur to be visible within the element without relying on drawing outside its bounds (which may be clipped by ancestor visuals). This behavior only applies when the effect is active and does not change the control’s measured/arranged size.