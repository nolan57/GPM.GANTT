using Xunit;
using System;
using System.Linq;
using GPM.Gantt.Models;

namespace GPM.Gantt.Tests
{
    /// <summary>
    /// Unit tests for the GanttTask class.
    /// </summary>
    public class GanttTaskTests
    {
        [Fact]
        public void GanttTask_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var task = new GanttTask();
            
            // Assert
            Assert.NotEqual(Guid.Empty, task.Id);
            Assert.Equal(1, task.RowIndex);
            Assert.Equal(string.Empty, task.Title);
            Assert.Equal(0, task.Progress);
            Assert.Equal(Models.TaskPriority.Normal, task.Priority);
            Assert.Equal(Models.TaskStatus.NotStarted, task.Status);
        }
        
        [Fact]
        public void GanttTask_ValidTask_IsValidReturnsTrue()
        {
            // Arrange
            var task = new GanttTask
            {
                Title = "Test Task",
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1),
                RowIndex = 1
            };
            
            // Act
            var result = task.IsValid();
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public void GanttTask_EndBeforeStart_IsValidReturnsFalse()
        {
            // Arrange
            var task = new GanttTask
            {
                Title = "Test Task",
                Start = DateTime.Today.AddDays(1),
                End = DateTime.Today,
                RowIndex = 1
            };
            
            // Act
            var result = task.IsValid();
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void GanttTask_EmptyTitle_IsValidReturnsFalse()
        {
            // Arrange
            var task = new GanttTask
            {
                Title = "",
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1),
                RowIndex = 1
            };
            
            // Act
            var result = task.IsValid();
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void GanttTask_GetValidationErrors_ReturnsCorrectErrors()
        {
            // Arrange
            var task = new GanttTask
            {
                Title = "",
                Start = DateTime.Today.AddDays(1),
                End = DateTime.Today,
                RowIndex = -1,
                Progress = 150
            };
            
            // Act
            var errors = task.GetValidationErrors();
            
            // Assert
            Assert.True(errors.Count > 0);
            Assert.Contains(errors, e => e.Contains("Start date must be before"));
            Assert.Contains(errors, e => e.Contains("title is required"));
            Assert.Contains(errors, e => e.Contains("Row index must be greater"));
            Assert.Contains(errors, e => e.Contains("Progress must be between"));
        }
        
        [Fact]
        public void GanttTask_Duration_CalculatesCorrectly()
        {
            // Arrange
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(5);
            var task = new GanttTask
            {
                Start = start,
                End = end
            };
            
            // Act
            var duration = task.Duration;
            
            // Assert
            Assert.Equal(TimeSpan.FromDays(5), duration);
        }
        
        [Fact]
        public void GanttTask_OverlapsWith_DetectsOverlap()
        {
            // Arrange
            var task1 = new GanttTask
            {
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(3)
            };
            
            var task2 = new GanttTask
            {
                Start = DateTime.Today.AddDays(2),
                End = DateTime.Today.AddDays(5)
            };
            
            // Act
            var overlaps = task1.OverlapsWith(task2);
            
            // Assert
            Assert.True(overlaps);
        }
        
        [Fact]
        public void GanttTask_OverlapsWith_NoOverlap_ReturnsFalse()
        {
            // Arrange
            var task1 = new GanttTask
            {
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1)
            };
            
            var task2 = new GanttTask
            {
                Start = DateTime.Today.AddDays(2),
                End = DateTime.Today.AddDays(3)
            };
            
            // Act
            var overlaps = task1.OverlapsWith(task2);
            
            // Assert
            Assert.False(overlaps);
        }
        
        [Fact]
        public void GanttTask_Clone_CreatesNewInstance()
        {
            // Arrange
            var original = new GanttTask
            {
                Title = "Original Task",
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1),
                Progress = 50,
                Priority = Models.TaskPriority.High
            };
            
            // Act
            var clone = original.Clone();
            
            // Assert
            Assert.NotEqual(original.Id, clone.Id);
            Assert.Equal(original.Title, clone.Title);
            Assert.Equal(original.Start, clone.Start);
            Assert.Equal(original.End, clone.End);
            Assert.Equal(original.Progress, clone.Progress);
            Assert.Equal(original.Priority, clone.Priority);
        }
    }
}