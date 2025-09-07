# Time Management - GPM.Gantt

This guide explains how to work with time scales, formatting, and timeline calculations in GPM.Gantt.

## Overview

GPM.Gantt provides comprehensive time management capabilities including:
- Multiple time scale units (Hour, Day, Week, Month, Year)
- Culture-aware formatting
- Custom date and time format strings
- Timeline calculations and utilities
- Automatic alignment and scaling

## Time Units

### Available Time Units

| Time Unit | Use Case | Typical Range |
|-----------|----------|---------------|
| `TimeUnit.Hour` | Detailed scheduling, manufacturing | Hours to days |
| `TimeUnit.Day` | Standard project management | Days to months |
| `TimeUnit.Week` | High-level project overview | Weeks to quarters |
| `TimeUnit.Month` | Long-term planning | Months to years |
| `TimeUnit.Year` | Strategic planning | Years to decades |

### Setting Time Units

```csharp
// Programmatically
ganttContainer.TimeUnit = TimeUnit.Week;

// Via ViewModel
ganttChartViewModel.TimeUnit = TimeUnit.Month;

// In XAML
<gantt:GanttContainer TimeUnit="Day" />
```

### Dynamic Time Unit Selection

```xml
<ComboBox SelectedValue="{Binding GanttChart.TimeUnit, Mode=TwoWay}"
          SelectedValuePath="Tag">
    <ComboBoxItem Content="Hourly" Tag="{x:Static models:TimeUnit.Hour}"/>
    <ComboBoxItem Content="Daily" Tag="{x:Static models:TimeUnit.Day}"/>
    <ComboBoxItem Content="Weekly" Tag="{x:Static models:TimeUnit.Week}"/>
    <ComboBoxItem Content="Monthly" Tag="{x:Static models:TimeUnit.Month}"/>
    <ComboBoxItem Content="Yearly" Tag="{x:Static models:TimeUnit.Year}"/>
</ComboBox>
```

## Time Range Configuration

### Setting Time Ranges

```csharp
// Basic time range setup
ganttContainer.StartTime = DateTime.Today;
ganttContainer.EndTime = DateTime.Today.AddMonths(3);

// Project-based range
var projectStart = new DateTime(2024, 1, 1);
var projectEnd = new DateTime(2024, 12, 31);
ganttContainer.StartTime = projectStart;
ganttContainer.EndTime = projectEnd;

// Dynamic range based on tasks
if (tasks.Any())
{
    ganttContainer.StartTime = tasks.Min(t => t.Start).Date;
    ganttContainer.EndTime = tasks.Max(t => t.End).Date.AddDays(1);
}
```

### Automatic Range Calculation

```csharp
public static class TimeRangeHelper
{
    public static (DateTime start, DateTime end) CalculateOptimalRange(
        IEnumerable<GanttTask> tasks, 
        TimeUnit timeUnit, 
        double paddingPercent = 0.1)
    {
        if (!tasks.Any())
        {
            return (DateTime.Today, DateTime.Today.AddDays(30));
        }
        
        var minStart = tasks.Min(t => t.Start);
        var maxEnd = tasks.Max(t => t.End);
        var totalDuration = maxEnd - minStart;
        var padding = TimeSpan.FromTicks((long)(totalDuration.Ticks * paddingPercent));
        
        var start = AlignToTimeUnit(minStart - padding, timeUnit, true);
        var end = AlignToTimeUnit(maxEnd + padding, timeUnit, false);
        
        return (start, end);
    }
    
    private static DateTime AlignToTimeUnit(DateTime dateTime, TimeUnit unit, bool floor)
    {
        return unit switch
        {
            TimeUnit.Hour => floor ? 
                new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0) :
                new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + 1, 0, 0),
            TimeUnit.Day => floor ? dateTime.Date : dateTime.Date.AddDays(1),
            TimeUnit.Week => floor ? GetWeekStart(dateTime) : GetWeekStart(dateTime).AddDays(7),
            TimeUnit.Month => floor ? 
                new DateTime(dateTime.Year, dateTime.Month, 1) :
                new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1),
            TimeUnit.Year => floor ?
                new DateTime(dateTime.Year, 1, 1) :
                new DateTime(dateTime.Year + 1, 1, 1),
            _ => dateTime
        };
    }
}
```

## Date and Time Formatting

### Default Formats by Time Unit

| Time Unit | Default Date Format | Default Time Format | Example Output |
|-----------|-------------------|-------------------|----------------|
| Hour | "MMM dd" | "HH:mm" | "Jan 15 14:30" |
| Day | "MMM dd" | - | "Jan 15" |
| Week | - | - | "Week 3, 2024" |
| Month | "yyyy MMM" | - | "2024 Jan" |
| Year | "yyyy" | - | "2024" |

### Custom Date Formats

```csharp
// American format
ganttContainer.DateFormat = "MM/dd/yyyy";  // 01/15/2024

// European format
ganttContainer.DateFormat = "dd/MM/yyyy";  // 15/01/2024

// ISO format
ganttContainer.DateFormat = "yyyy-MM-dd";  // 2024-01-15

// Long format
ganttContainer.DateFormat = "MMMM dd, yyyy";  // January 15, 2024

// Short format
ganttContainer.DateFormat = "MMM dd";  // Jan 15
```

### Custom Time Formats

```csharp
// 24-hour format
ganttContainer.TimeFormat = "HH:mm";       // 14:30

// 12-hour format with AM/PM
ganttContainer.TimeFormat = "hh:mm tt";    // 02:30 PM

// With seconds
ganttContainer.TimeFormat = "HH:mm:ss";    // 14:30:45

// Compact 12-hour
ganttContainer.TimeFormat = "h:mm tt";     // 2:30 PM
```

### Format Examples for Different Locales

```csharp
// German locale
ganttContainer.Culture = new CultureInfo("de-DE");
ganttContainer.DateFormat = "dd.MM.yyyy";    // 15.01.2024
ganttContainer.TimeFormat = "HH:mm";         // 14:30

// French locale
ganttContainer.Culture = new CultureInfo("fr-FR");
ganttContainer.DateFormat = "dd/MM/yyyy";    // 15/01/2024
ganttContainer.TimeFormat = "HH:mm";         // 14:30

// Japanese locale
ganttContainer.Culture = new CultureInfo("ja-JP");
ganttContainer.DateFormat = "yyyy/MM/dd";    // 2024/01/15
ganttContainer.TimeFormat = "HH:mm";         // 14:30
```

## Culture and Localization

### Setting Culture

```csharp
// System culture
ganttContainer.Culture = CultureInfo.CurrentCulture;

// Specific culture
ganttContainer.Culture = new CultureInfo("en-US");

// Custom culture with modifications
var culture = new CultureInfo("en-US");
culture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
ganttContainer.Culture = culture;
```

### Culture-Specific Features

```csharp
// Weekend detection based on culture
var culture = new CultureInfo("ar-SA"); // Saudi Arabia
// Friday and Saturday are weekends

var isWeekend = TimelineCalculator.IsWeekend(someDate);

// Week numbering
var weekNumber = culture.Calendar.GetWeekOfYear(
    DateTime.Now, 
    CalendarWeekRule.FirstFourDayWeek, 
    culture.DateTimeFormat.FirstDayOfWeek);
```

## Timeline Calculations

### Using TimelineCalculator

```csharp
// Generate timeline ticks
var ticks = TimelineCalculator.GenerateTicks(
    startDate: DateTime.Today,
    endDate: DateTime.Today.AddMonths(2),
    unit: TimeUnit.Week,
    culture: CultureInfo.CurrentCulture);

// Align dates to boundaries
var alignedStart = TimelineCalculator.AlignToUnitFloor(
    DateTime.Now, TimeUnit.Month);

var alignedEnd = TimelineCalculator.AlignToUnitCeiling(
    DateTime.Now, TimeUnit.Month);

// Format for display
var formatted = TimelineCalculator.FormatTick(
    DateTime.Now, 
    TimeUnit.Day, 
    CultureInfo.GetCultureInfo("fr-FR"));
```

### Custom Timeline Generation

```csharp
public static class CustomTimelineGenerator
{
    public static List<DateTime> GenerateBusinessDays(DateTime start, DateTime end)
    {
        var result = new List<DateTime>();
        var current = start.Date;
        
        while (current <= end.Date)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && 
                current.DayOfWeek != DayOfWeek.Sunday)
            {
                result.Add(current);
            }
            current = current.AddDays(1);
        }
        
        return result;
    }
    
    public static List<DateTime> GenerateShiftSchedule(DateTime start, DateTime end, 
        TimeSpan shiftStart, TimeSpan shiftEnd)
    {
        var result = new List<DateTime>();
        var current = start.Date;
        
        while (current <= end.Date)
        {
            var shiftBegin = current.Add(shiftStart);
            var shiftFinish = current.Add(shiftEnd);
            
            // Add hourly intervals during shift
            var hour = shiftBegin;
            while (hour <= shiftFinish)
            {
                result.Add(hour);
                hour = hour.AddHours(1);
            }
            
            current = current.AddDays(1);
        }
        
        return result;
    }
}
```

## Advanced Time Scenarios

### Multi-Timezone Support

```csharp
public class MultiTimezoneGanttViewModel : ViewModelBase
{
    private TimeZoneInfo _displayTimeZone = TimeZoneInfo.Local;
    
    public TimeZoneInfo DisplayTimeZone
    {
        get => _displayTimeZone;
        set
        {
            if (SetProperty(ref _displayTimeZone, value))
            {
                ConvertTaskTimezones();
            }
        }
    }
    
    private void ConvertTaskTimezones()
    {
        foreach (var task in GanttChart.Tasks)
        {
            // Convert from UTC to display timezone
            task.Start = TimeZoneInfo.ConvertTimeFromUtc(
                task.Model.Start.ToUniversalTime(), _displayTimeZone);
            task.End = TimeZoneInfo.ConvertTimeFromUtc(
                task.Model.End.ToUniversalTime(), _displayTimeZone);
        }
    }
}
```

### Custom Work Calendars

```csharp
public class WorkCalendar
{
    private readonly HashSet<DayOfWeek> _workingDays;
    private readonly HashSet<DateTime> _holidays;
    private readonly TimeSpan _workDayStart;
    private readonly TimeSpan _workDayEnd;
    
    public WorkCalendar()
    {
        _workingDays = new HashSet<DayOfWeek>
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday
        };
        _holidays = new HashSet<DateTime>();
        _workDayStart = new TimeSpan(9, 0, 0);  // 9 AM
        _workDayEnd = new TimeSpan(17, 0, 0);   // 5 PM
    }
    
    public bool IsWorkingDay(DateTime date)
    {
        return _workingDays.Contains(date.DayOfWeek) && 
               !_holidays.Contains(date.Date);
    }
    
    public bool IsWorkingHour(DateTime dateTime)
    {
        if (!IsWorkingDay(dateTime)) return false;
        
        var time = dateTime.TimeOfDay;
        return time >= _workDayStart && time <= _workDayEnd;
    }
    
    public DateTime GetNextWorkingDay(DateTime date)
    {
        var next = date.Date.AddDays(1);
        while (!IsWorkingDay(next))
        {
            next = next.AddDays(1);
        }
        return next;
    }
    
    public TimeSpan CalculateWorkingHours(DateTime start, DateTime end)
    {
        var totalHours = TimeSpan.Zero;
        var current = start.Date;
        
        while (current <= end.Date)
        {
            if (IsWorkingDay(current))
            {
                var dayStart = current.Add(_workDayStart);
                var dayEnd = current.Add(_workDayEnd);
                
                var effectiveStart = start > dayStart ? start : dayStart;
                var effectiveEnd = end < dayEnd ? end : dayEnd;
                
                if (effectiveStart < effectiveEnd)
                {
                    totalHours = totalHours.Add(effectiveEnd - effectiveStart);
                }
            }
            current = current.AddDays(1);
        }
        
        return totalHours;
    }
}
```

### Fiscal Year Support

```csharp
public class FiscalYearCalendar
{
    private readonly int _fiscalYearStartMonth;
    
    public FiscalYearCalendar(int fiscalYearStartMonth = 7) // July start
    {
        _fiscalYearStartMonth = fiscalYearStartMonth;
    }
    
    public int GetFiscalYear(DateTime date)
    {
        return date.Month >= _fiscalYearStartMonth ? 
            date.Year + 1 : date.Year;
    }
    
    public DateTime GetFiscalYearStart(int fiscalYear)
    {
        return new DateTime(fiscalYear - 1, _fiscalYearStartMonth, 1);
    }
    
    public DateTime GetFiscalYearEnd(int fiscalYear)
    {
        return GetFiscalYearStart(fiscalYear + 1).AddDays(-1);
    }
    
    public List<DateTime> GenerateFiscalQuarters(int fiscalYear)
    {
        var fyStart = GetFiscalYearStart(fiscalYear);
        return new List<DateTime>
        {
            fyStart,                    // Q1
            fyStart.AddMonths(3),       // Q2
            fyStart.AddMonths(6),       // Q3
            fyStart.AddMonths(9)        // Q4
        };
    }
}
```

## Performance Considerations

### Large Time Ranges

```csharp
// For very large time ranges, use appropriate time units
public static TimeUnit GetOptimalTimeUnit(DateTime start, DateTime end)
{
    var duration = end - start;
    
    return duration.TotalDays switch
    {
        <= 7 => TimeUnit.Day,
        <= 90 => TimeUnit.Week,
        <= 730 => TimeUnit.Month,  // ~2 years
        _ => TimeUnit.Year
    };
}
```

### Caching Timeline Calculations

```csharp
public class CachedTimelineCalculator
{
    private static readonly ConcurrentDictionary<string, List<DateTime>> _cache = new();
    
    public static List<DateTime> GetCachedTicks(DateTime start, DateTime end, 
        TimeUnit unit, CultureInfo culture)
    {
        var key = $"{start:yyyyMMdd}_{end:yyyyMMdd}_{unit}_{culture.Name}";
        
        return _cache.GetOrAdd(key, _ => 
            TimelineCalculator.GenerateTicks(start, end, unit, culture));
    }
    
    public static void ClearCache()
    {
        _cache.Clear();
    }
}
```

## Best Practices

1. **Choose appropriate time units** for your data granularity
2. **Use culture-aware formatting** for international applications
3. **Cache timeline calculations** for better performance
4. **Align time ranges** to natural boundaries
5. **Consider working calendars** for business applications
6. **Test with different cultures** and timezones
7. **Provide user controls** for time unit selection
8. **Validate time ranges** before applying to the chart