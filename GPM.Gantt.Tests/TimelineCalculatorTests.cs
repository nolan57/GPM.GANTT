using Xunit;
using GPM.Gantt.Models;
using GPM.Gantt.Utilities;
using System;
using System.Globalization;
using System.Linq;

namespace GPM.Gantt.Tests
{
    /// <summary>
    /// Unit tests for the TimelineCalculator utility class.
    /// </summary>
    public class TimelineCalculatorTests
    {
        [Fact]
        public void GenerateTicks_DailyUnit_ReturnsCorrectTicks()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 1, 5);
            
            // Act
            var ticks = TimelineCalculator.GenerateTicks(start, end, TimeUnit.Day);
            
            // Assert
            Assert.Equal(5, ticks.Count);
            Assert.Equal(new DateTime(2024, 1, 1), ticks[0]);
            Assert.Equal(new DateTime(2024, 1, 5), ticks[4]);
        }
        
        [Fact]
        public void GenerateTicks_HourlyUnit_ReturnsCorrectTicks()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1, 10, 0, 0);
            var end = new DateTime(2024, 1, 1, 14, 0, 0);
            
            // Act
            var ticks = TimelineCalculator.GenerateTicks(start, end, TimeUnit.Hour);
            
            // Assert
            Assert.Equal(5, ticks.Count);
            Assert.Equal(new DateTime(2024, 1, 1, 10, 0, 0), ticks[0]);
            Assert.Equal(new DateTime(2024, 1, 1, 14, 0, 0), ticks[4]);
        }
        
        [Fact]
        public void GenerateTicks_WeeklyUnit_ReturnsCorrectTicks()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1); // Monday
            var end = new DateTime(2024, 1, 21);
            
            // Act
            var ticks = TimelineCalculator.GenerateTicks(start, end, TimeUnit.Week);
            
            // Assert
            Assert.True(ticks.Count >= 3);
            // Check that all ticks are at the start of weeks (assuming culture uses Monday as first day)
            foreach (var tick in ticks)
            {
                var dayOfWeek = tick.DayOfWeek;
                // Allow for different cultures - could be Sunday or Monday
                Assert.True(dayOfWeek == DayOfWeek.Sunday || dayOfWeek == DayOfWeek.Monday, 
                    $"Expected week start day, got {dayOfWeek} for {tick:yyyy-MM-dd}");
            }
        }
        
        [Fact]
        public void GenerateTicks_MonthlyUnit_ReturnsCorrectTicks()
        {
            // Arrange
            var start = new DateTime(2024, 1, 15);
            var end = new DateTime(2024, 3, 10);
            
            // Act
            var ticks = TimelineCalculator.GenerateTicks(start, end, TimeUnit.Month);
            
            // Assert
            Assert.Equal(3, ticks.Count);
            Assert.Equal(new DateTime(2024, 1, 1), ticks[0]);
            Assert.Equal(new DateTime(2024, 2, 1), ticks[1]);
            Assert.Equal(new DateTime(2024, 3, 1), ticks[2]);
        }
        
        [Fact]
        public void AlignToUnitFloor_Day_AlignsToMidnight()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);
            
            // Act
            var aligned = TimelineCalculator.AlignToUnitFloor(dateTime, TimeUnit.Day);
            
            // Assert
            Assert.Equal(new DateTime(2024, 1, 15, 0, 0, 0), aligned);
        }
        
        [Fact]
        public void AlignToUnitFloor_Hour_AlignsToHourStart()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);
            
            // Act
            var aligned = TimelineCalculator.AlignToUnitFloor(dateTime, TimeUnit.Hour);
            
            // Assert
            Assert.Equal(new DateTime(2024, 1, 15, 14, 0, 0), aligned);
        }
        
        [Fact]
        public void AlignToUnitFloor_Month_AlignsToFirstDay()
        {
            // Arrange
            var dateTime = new DateTime(2024, 3, 15, 14, 30, 45);
            
            // Act
            var aligned = TimelineCalculator.AlignToUnitFloor(dateTime, TimeUnit.Month);
            
            // Assert
            Assert.Equal(new DateTime(2024, 3, 1, 0, 0, 0), aligned);
        }
        
        [Fact]
        public void AlignToUnitCeiling_Day_AlignsToNextDay()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);
            
            // Act
            var aligned = TimelineCalculator.AlignToUnitCeiling(dateTime, TimeUnit.Day);
            
            // Assert
            Assert.Equal(new DateTime(2024, 1, 16, 0, 0, 0), aligned);
        }
        
        [Fact]
        public void AlignToUnitCeiling_AlreadyAligned_ReturnsSameValue()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15, 0, 0, 0);
            
            // Act
            var aligned = TimelineCalculator.AlignToUnitCeiling(dateTime, TimeUnit.Day);
            
            // Assert
            Assert.Equal(dateTime, aligned);
        }
        
        [Fact]
        public void FormatTick_Day_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2024, 3, 15);
            
            // Act
            var formatted = TimelineCalculator.FormatTick(dateTime, TimeUnit.Day);
            
            // Assert
            Assert.Contains("Mar 15", formatted);
        }
        
        [Fact]
        public void FormatTick_Day_WithCustomDateFormat_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2024, 3, 15);
            var customDateFormat = "dd/MM/yyyy";
            
            // Act
            var formatted = TimelineCalculator.FormatTick(dateTime, TimeUnit.Day, customDateFormat, null);
            
            // Assert
            Assert.Equal("15/03/2024", formatted);
        }
        
        [Fact]
        public void FormatTick_Hour_WithCustomTimeFormat_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2024, 3, 15, 14, 30, 0);
            var customDateFormat = "dd MMM";
            var customTimeFormat = "hh:mm tt";
            
            // Act
            var formatted = TimelineCalculator.FormatTick(dateTime, TimeUnit.Hour, customDateFormat, customTimeFormat);
            
            // Assert
            Assert.Equal("15 Mar 02:30 PM", formatted);
        }
        
        [Fact]
        public void FormatTick_Month_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2024, 3, 15);
            
            // Act
            var formatted = TimelineCalculator.FormatTick(dateTime, TimeUnit.Month);
            
            // Assert
            Assert.Contains("2024 Mar", formatted);
        }
        
        [Fact]
        public void FormatTick_Month_WithCustomFormat_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2024, 3, 15);
            var customDateFormat = "MMM yyyy";
            
            // Act
            var formatted = TimelineCalculator.FormatTick(dateTime, TimeUnit.Month, customDateFormat, null);
            
            // Assert
            Assert.Equal("Mar 2024", formatted);
        }
        
        [Fact]
        public void FormatTick_WithNullCustomFormats_UsesDefaults()
        {
            // Arrange
            var dateTime = new DateTime(2024, 3, 15);
            
            // Act
            var formatted = TimelineCalculator.FormatTick(dateTime, TimeUnit.Day, null, null);
            
            // Assert
            Assert.Contains("Mar 15", formatted);
        }
        
        [Fact]
        public void FormatTick_WithEmptyCustomFormats_UsesDefaults()
        {
            // Arrange
            var dateTime = new DateTime(2024, 3, 15);
            
            // Act
            var formatted = TimelineCalculator.FormatTick(dateTime, TimeUnit.Day, "", "");
            
            // Assert
            Assert.Contains("Mar 15", formatted);
        }
        
        [Fact]
        public void IsWeekend_Saturday_ReturnsTrue()
        {
            // Arrange
            var saturday = new DateTime(2024, 1, 6); // A Saturday
            
            // Act
            var isWeekend = TimelineCalculator.IsWeekend(saturday);
            
            // Assert
            Assert.True(isWeekend);
        }
        
        [Fact]
        public void IsWeekend_Monday_ReturnsFalse()
        {
            // Arrange
            var monday = new DateTime(2024, 1, 1); // A Monday
            
            // Act
            var isWeekend = TimelineCalculator.IsWeekend(monday);
            
            // Assert
            Assert.False(isWeekend);
        }
        
        [Fact]
        public void GetUnitDuration_Day_ReturnsOneDay()
        {
            // Act
            var duration = TimelineCalculator.GetUnitDuration(TimeUnit.Day);
            
            // Assert
            Assert.Equal(TimeSpan.FromDays(1), duration);
        }
        
        [Fact]
        public void GetUnitDuration_Hour_ReturnsOneHour()
        {
            // Act
            var duration = TimelineCalculator.GetUnitDuration(TimeUnit.Hour);
            
            // Assert
            Assert.Equal(TimeSpan.FromHours(1), duration);
        }
    }
}