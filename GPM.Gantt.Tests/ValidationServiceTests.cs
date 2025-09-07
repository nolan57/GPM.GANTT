using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPM.Gantt.Services;
using GPM.Gantt.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPM.Gantt.Tests
{
    /// <summary>
    /// Unit tests for the ValidationService class.
    /// </summary>
    [TestClass]
    public class ValidationServiceTests
    {
        private ValidationService _validationService = null!;
        
        [TestInitialize]
        public void Setup()
        {
            _validationService = new ValidationService();
        }
        
        [TestMethod]
        public void ValidateTask_ValidTask_ReturnsSuccess()
        {
            // Arrange
            var task = new GanttTask
            {
                Title = "Valid Task",
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1),
                RowIndex = 1
            };
            
            // Act
            var result = _validationService.ValidateTask(task);
            
            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }
        
        [TestMethod]
        public void ValidateTask_NullTask_ReturnsError()
        {
            // Act
            var result = _validationService.ValidateTask(null!);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("cannot be null")));
        }
        
        [TestMethod]
        public void ValidateTimeRange_ValidRange_ReturnsSuccess()
        {
            // Arrange
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(7);
            
            // Act
            var result = _validationService.ValidateTimeRange(start, end);
            
            // Assert
            Assert.IsTrue(result.IsValid);
        }
        
        [TestMethod]
        public void ValidateTimeRange_EndBeforeStart_ReturnsError()
        {
            // Arrange
            var start = DateTime.Today.AddDays(1);
            var end = DateTime.Today;
            
            // Act
            var result = _validationService.ValidateTimeRange(start, end);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("Start time must be before")));
        }
        
        [TestMethod]
        public void ValidateTaskCollection_ValidTasks_ReturnsSuccess()
        {
            // Arrange
            var tasks = new List<GanttTask>
            {
                new() { Id = Guid.NewGuid(), Title = "Task 1", Start = DateTime.Today, End = DateTime.Today.AddDays(1), RowIndex = 1 },
                new() { Id = Guid.NewGuid(), Title = "Task 2", Start = DateTime.Today.AddDays(2), End = DateTime.Today.AddDays(3), RowIndex = 2 }
            };
            
            // Act
            var result = _validationService.ValidateTaskCollection(tasks);
            
            // Assert
            Assert.IsTrue(result.IsValid);
        }
        
        [TestMethod]
        public void ValidateTaskCollection_OverlappingTasks_ReturnsError()
        {
            // Arrange
            var tasks = new List<GanttTask>
            {
                new() { Id = Guid.NewGuid(), Title = "Task 1", Start = DateTime.Today, End = DateTime.Today.AddDays(2), RowIndex = 1 },
                new() { Id = Guid.NewGuid(), Title = "Task 2", Start = DateTime.Today.AddDays(1), End = DateTime.Today.AddDays(3), RowIndex = 1 }
            };
            
            // Act
            var result = _validationService.ValidateTaskCollection(tasks);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("overlap")));
        }
        
        [TestMethod]
        public void ValidateTaskDependencies_CircularDependency_ReturnsError()
        {
            // Arrange
            var task1Id = Guid.NewGuid();
            var task2Id = Guid.NewGuid();
            
            var task1 = new GanttTask
            {
                Id = task1Id,
                Title = "Task 1",
                Dependencies = new List<Guid> { task2Id }
            };
            
            var task2 = new GanttTask
            {
                Id = task2Id,
                Title = "Task 2",
                Dependencies = new List<Guid> { task1Id }
            };
            
            var allTasks = new List<GanttTask> { task1, task2 };
            
            // Act
            var result = _validationService.ValidateTaskDependencies(task1, allTasks);
            
            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("Circular dependency")));
        }
    }
}