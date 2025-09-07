using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using GPM.Gantt.Models;

namespace GPM.Gantt.Tests
{
    /// <summary>
    /// Unit tests for the GanttTask class.
    /// </summary>
    [TestClass]
    public class GanttTaskTests
    {
        [TestMethod]
        public void GanttTask_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var task = new GanttTask();
            
            // Assert
            Assert.AreNotEqual(Guid.Empty, task.Id);
            Assert.AreEqual(1, task.RowIndex);
            Assert.AreEqual(string.Empty, task.Title);
            Assert.AreEqual(0, task.Progress);
            Assert.AreEqual(Models.TaskPriority.Normal, task.Priority);
            Assert.AreEqual(Models.TaskStatus.NotStarted, task.Status);
        }
        
        [TestMethod]
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
            Assert.IsTrue(result);
        }
        
        [TestMethod]
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
            Assert.IsFalse(result);
        }
        
        [TestMethod]
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
            Assert.IsFalse(result);
        }
        
        [TestMethod]
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
            Assert.IsTrue(errors.Count > 0);
            Assert.IsTrue(errors.Any(e => e.Contains("Start date must be before")));
            Assert.IsTrue(errors.Any(e => e.Contains("title is required")));
            Assert.IsTrue(errors.Any(e => e.Contains("Row index must be greater")));
            Assert.IsTrue(errors.Any(e => e.Contains("Progress must be between")));
        }
        
        [TestMethod]
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
            Assert.AreEqual(TimeSpan.FromDays(5), duration);
        }
        
        [TestMethod]
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
            Assert.IsTrue(overlaps);
        }
        
        [TestMethod]
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
            Assert.IsFalse(overlaps);
        }
        
        [TestMethod]
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
            Assert.AreNotEqual(original.Id, clone.Id);
            Assert.AreEqual(original.Title, clone.Title);
            Assert.AreEqual(original.Start, clone.Start);
            Assert.AreEqual(original.End, clone.End);
            Assert.AreEqual(original.Progress, clone.Progress);
            Assert.AreEqual(original.Priority, clone.Priority);
        }
    }
}