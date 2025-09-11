using System;
using GPM.Gantt.Configuration;
using GPM.Gantt.Models;
using GPM.Gantt.Services;
using Xunit;

namespace GPM.Gantt.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void RenderingConfiguration_HighPerformance_ReturnsCorrectConfiguration()
        {
            // Act
            var config = RenderingConfiguration.HighPerformance();

            // Assert
            Assert.True(config.EnableVirtualization);
            Assert.Equal(500, config.MaxVisibleTasks);
            Assert.True(config.EnableCaching);
            Assert.Equal(GridRenderingMode.Lines, config.GridMode);
            Assert.False(config.UseEnhancedShapeRendering);
            Assert.Equal(PerformanceLevel.Performance, config.PerformanceLevel);
            Assert.False(config.EnableGpuAcceleration);
            Assert.True(config.EnableBatchRendering);
            Assert.Equal(200, config.LayoutDebounceDelay);
        }

        [Fact]
        public void RenderingConfiguration_HighQuality_ReturnsCorrectConfiguration()
        {
            // Act
            var config = RenderingConfiguration.HighQuality();

            // Assert
            Assert.True(config.EnableVirtualization);
            Assert.Equal(1000, config.MaxVisibleTasks);
            Assert.True(config.EnableCaching);
            Assert.Equal(GridRenderingMode.Rectangles, config.GridMode);
            Assert.True(config.UseEnhancedShapeRendering);
            Assert.Equal(PerformanceLevel.Quality, config.PerformanceLevel);
            Assert.True(config.EnableGpuAcceleration);
            Assert.True(config.EnableBatchRendering);
            Assert.Equal(100, config.LayoutDebounceDelay);
        }

        [Fact]
        public void RenderingConfiguration_Balanced_ReturnsCorrectConfiguration()
        {
            // Act
            var config = RenderingConfiguration.Balanced();

            // Assert
            Assert.True(config.EnableVirtualization);
            Assert.Equal(1000, config.MaxVisibleTasks);
            Assert.True(config.EnableCaching);
            Assert.Equal(GridRenderingMode.Rectangles, config.GridMode);
            Assert.True(config.UseEnhancedShapeRendering);
            Assert.Equal(PerformanceLevel.Balanced, config.PerformanceLevel);
            Assert.False(config.EnableGpuAcceleration);
            Assert.True(config.EnableBatchRendering);
            Assert.Equal(150, config.LayoutDebounceDelay);
        }

        [Fact]
        public void GanttConfiguration_ForLargeDatasets_ReturnsCorrectConfiguration()
        {
            // Act
            var config = GanttConfiguration.ForLargeDatasets(500);

            // Assert
            Assert.Equal(500, config.Rendering.MaxVisibleTasks);
            Assert.Equal(TimeUnit.Week, config.TimeScale.DefaultTimeUnit);
            Assert.True(config.TimeScale.HighlightWeekends);
            Assert.True(config.TimeScale.HighlightToday);
        }

        [Fact]
        public void GanttConfiguration_ForDetailedPlanning_ReturnsCorrectConfiguration()
        {
            // Act
            var config = GanttConfiguration.ForDetailedPlanning();

            // Assert
            Assert.Equal(1000, config.Rendering.MaxVisibleTasks);
            Assert.Equal(TimeUnit.Day, config.TimeScale.DefaultTimeUnit);
            Assert.True(config.TimeScale.HighlightWeekends);
            Assert.True(config.TimeScale.HighlightToday);
        }

        [Fact]
        public void GanttConfiguration_Validate_ThrowsException_ForInvalidMaxVisibleTasks()
        {
            // Arrange
            var config = new GanttConfiguration
            {
                Rendering = new RenderingConfiguration
                {
                    MaxVisibleTasks = 0
                }
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Equal("MaxVisibleTasks must be greater than zero.", exception.Message);
        }

        [Fact]
        public void GanttConfiguration_Validate_ThrowsException_ForNegativeLayoutDebounceDelay()
        {
            // Arrange
            var config = new GanttConfiguration
            {
                Rendering = new RenderingConfiguration
                {
                    MaxVisibleTasks = 1000,
                    LayoutDebounceDelay = -1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Equal("LayoutDebounceDelay cannot be negative.", exception.Message);
        }

        [Fact]
        public void GanttConfiguration_Validate_ThrowsException_ForGpuAccelerationWithoutTechnology()
        {
            // Arrange
            var config = new GanttConfiguration
            {
                Rendering = new RenderingConfiguration
                {
                    MaxVisibleTasks = 1000,
                    EnableGpuAcceleration = true,
                    GpuRenderingTechnology = GpuRenderingTechnology.Default
                }
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => config.Validate());
            Assert.Equal("GpuRenderingTechnology must be specified when GPU acceleration is enabled. Use Direct2D, DirectX, OpenGL, or Vulkan.", exception.Message);
        }

        [Fact]
        public void GanttConfiguration_ValidateWithResult_ReturnsInvalidResult_ForInvalidMaxVisibleTasks()
        {
            // Arrange
            var config = new GanttConfiguration
            {
                Rendering = new RenderingConfiguration
                {
                    MaxVisibleTasks = 0
                }
            };

            // Act
            var result = config.ValidateWithResult();

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("MaxVisibleTasks must be greater than zero.", result.Errors);
        }

        [Fact]
        public void GanttConfiguration_Builder_Pattern_Works_Correctly()
        {
            // Act
            var config = GanttConfiguration.CreateBuilder()
                .ForLargeDatasets(300)
                .WithTimeScale(ts => {
                    ts.DefaultTimeUnit = TimeUnit.Month;
                    ts.HighlightWeekends = false;
                })
                .Build();

            // Assert
            Assert.Equal(300, config.Rendering.MaxVisibleTasks);
            Assert.Equal(TimeUnit.Month, config.TimeScale.DefaultTimeUnit);
            Assert.False(config.TimeScale.HighlightWeekends);
        }
    }
}