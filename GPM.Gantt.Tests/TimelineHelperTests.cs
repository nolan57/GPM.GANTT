using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPM.Gantt.Models;
using GPM.Gantt.Utilities;
using System;
using System.Collections.Generic;

namespace GPM.Gantt.Tests
{
    /// <summary>
    /// Unit tests for the TimelineHelper utility class.
    /// </summary>
    [TestClass]
    public class TimelineHelperTests
    {
        [TestMethod]
        public void CalculateOptimalTimeUnit_ShortDuration_ReturnsHour()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1, 9, 0, 0);
            var end = new DateTime(2024, 1, 1, 17, 0, 0);
            
            // Act
            var optimal = TimelineHelper.CalculateOptimalTimeUnit(start, end, 10);
            
            // Assert
            Assert.AreEqual(TimeUnit.Hour, optimal);
        }
        
        [TestMethod]
        public void CalculateOptimalTimeUnit_MediumDuration_ReturnsDay()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 1, 20);
            
            // Act
            var optimal = TimelineHelper.CalculateOptimalTimeUnit(start, end, 25);
            
            // Assert
            Assert.AreEqual(TimeUnit.Day, optimal);
        }
        
        [TestMethod]
        public void CalculateOptimalTimeUnit_LongDuration_ReturnsMonth()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 12, 31);
            
            // Act
            var optimal = TimelineHelper.CalculateOptimalTimeUnit(start, end, 15);
            
            // Assert
            Assert.AreEqual(TimeUnit.Month, optimal);
        }
        
        [TestMethod]
        public void FindTickIndex_ExactMatch_ReturnsCorrectIndex()
        {
            // Arrange
            var ticks = new List<DateTime>
            {
                new(2024, 1, 1),
                new(2024, 1, 2),
                new(2024, 1, 3),
                new(2024, 1, 4)
            };
            var target = new DateTime(2024, 1, 3);
            
            // Act
            var index = TimelineHelper.FindTickIndex(ticks, target, false);
            
            // Assert
            Assert.AreEqual(2, index);
        }
        
        [TestMethod]
        public void FindTickIndex_NoExactMatch_ReturnsClosestLowerIndex()
        {
            // Arrange
            var ticks = new List<DateTime>
            {
                new(2024, 1, 1),
                new(2024, 1, 3),
                new(2024, 1, 5),
                new(2024, 1, 7)
            };
            var target = new DateTime(2024, 1, 4);
            
            // Act
            var index = TimelineHelper.FindTickIndex(ticks, target, false);
            
            // Assert
            Assert.AreEqual(1, index); // Should return index of Jan 3rd
        }
        
        [TestMethod]
        public void CalculateTaskSpan_TaskSpansMultipleTicks_ReturnsCorrectSpan()
        {
            // Arrange
            var ticks = new List<DateTime>
            {
                new(2024, 1, 1),
                new(2024, 1, 2),
                new(2024, 1, 3),
                new(2024, 1, 4),
                new(2024, 1, 5)
            };
            var taskStart = new DateTime(2024, 1, 2, 10, 0, 0);
            var taskEnd = new DateTime(2024, 1, 4, 14, 0, 0);
            
            // Act
            var (startIndex, columnSpan) = TimelineHelper.CalculateTaskSpan(ticks, taskStart, taskEnd, TimeUnit.Day);
            
            // Assert
            Assert.AreEqual(1, startIndex); // Jan 2nd
            // The task spans from Jan 2nd to Jan 4th, and since the end time is after midnight of Jan 4th,
            // it should extend to include Jan 5th, so span should be 4 (Jan 2nd, 3rd, 4th, 5th)
            Assert.AreEqual(4, columnSpan);
        }
        
        [TestMethod]
        public void ValidateTimeRange_ValidRange_ReturnsTrue()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 1, 10);
            
            // Act
            var (isValid, message) = TimelineHelper.ValidateTimeRange(start, end, TimeUnit.Day);
            
            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual("Valid time range", message);
        }
        
        [TestMethod]
        public void ValidateTimeRange_StartAfterEnd_ReturnsFalse()
        {
            // Arrange
            var start = new DateTime(2024, 1, 10);
            var end = new DateTime(2024, 1, 1);
            
            // Act
            var (isValid, message) = TimelineHelper.ValidateTimeRange(start, end, TimeUnit.Day);
            
            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(message.Contains("Start time must be before"));
        }
        
        [TestMethod]
        public void ValidateTimeRange_TooLongForHourUnit_ReturnsFalse()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 2, 15); // More than 30 days
            
            // Act
            var (isValid, message) = TimelineHelper.ValidateTimeRange(start, end, TimeUnit.Hour);
            
            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(message.Contains("should not exceed"));
        }
        
        [TestMethod]
        public void ExpandToUnitBoundaries_Day_ReturnsAlignedBoundaries()
        {
            // Arrange
            var start = new DateTime(2024, 1, 15, 10, 30, 0);
            var end = new DateTime(2024, 1, 17, 14, 30, 0);
            
            // Act
            var (alignedStart, alignedEnd) = TimelineHelper.ExpandToUnitBoundaries(start, end, TimeUnit.Day);
            
            // Assert
            Assert.AreEqual(new DateTime(2024, 1, 15), alignedStart);
            Assert.AreEqual(new DateTime(2024, 1, 18), alignedEnd);
        }
        
        [TestMethod]
        public void CalculateWorkingDays_ExcludesWeekends_ReturnsCorrectCount()
        {
            // Arrange (Jan 1, 2024 is a Monday)
            var start = new DateTime(2024, 1, 1);  // Monday
            var end = new DateTime(2024, 1, 7);    // Sunday
            
            // Act
            var workingDays = TimelineHelper.CalculateWorkingDays(start, end);
            
            // Assert
            Assert.AreEqual(5, workingDays); // Monday to Friday
        }
        
        [TestMethod]
        public void CalculateWorkingDays_WithHolidays_ExcludesHolidays()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 1, 5);
            var holidays = new[] { new DateTime(2024, 1, 3) };
            
            // Act
            var workingDays = TimelineHelper.CalculateWorkingDays(start, end, holidays);
            
            // Assert
            Assert.AreEqual(4, workingDays); // 5 weekdays minus 1 holiday
        }
        
        [TestMethod]
        public void GetTimelineMetadata_ReturnsCompleteMetadata()
        {
            // Arrange
            var start = new DateTime(2024, 1, 15, 10, 30, 0);
            var end = new DateTime(2024, 1, 17, 14, 30, 0);
            
            // Act
            var metadata = TimelineHelper.GetTimelineMetadata(start, end, TimeUnit.Day);
            
            // Assert
            Assert.AreEqual(start, metadata.OriginalStart);
            Assert.AreEqual(end, metadata.OriginalEnd);
            Assert.AreEqual(TimeUnit.Day, metadata.TimeUnit);
            Assert.AreEqual(3, metadata.TickCount); // Jan 15, 16, 17
            Assert.IsTrue(metadata.TotalDuration.TotalDays > 2);
        }
    }
}