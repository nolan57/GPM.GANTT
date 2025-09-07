using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

namespace GPM.Gantt.Demo
{
    /// <summary>
    /// Interaction logic for AlternatingRowDemo.xaml
    /// </summary>
    public partial class AlternatingRowDemo : Window
    {
        private GanttTheme _defaultTheme;
        private GanttTheme _darkTheme;
        private GanttTheme _lightTheme;
        private GanttTheme _modernTheme;
        private GanttTheme _customTheme;

        public AlternatingRowDemo()
        {
            InitializeComponent();
            InitializeThemes();
            SetupSampleData();
        }

        private void InitializeThemes()
        {
            // Get built-in themes
            _defaultTheme = ThemeManager.GetTheme("Default");
            _darkTheme = ThemeManager.GetTheme("Dark");
            _lightTheme = ThemeManager.GetTheme("Light");
            _modernTheme = ThemeManager.GetTheme("Modern");
            
            // Create custom theme
            _customTheme = ThemeManager.CreateCustomTheme("CustomAlternating", theme =>
            {
                theme.Background.PrimaryColor = Colors.White;
                theme.Background.SecondaryColor = (Color)ColorConverter.ConvertFromString("#E6F7FF"); // Light blue
                theme.Task.DefaultColor = (Color)ColorConverter.ConvertFromString("#2196F3"); // Blue
                theme.TimeScale.TodayMarkerColor = (Color)ColorConverter.ConvertFromString("#FF9800"); // Orange
            });
            
            // Set default theme
            GanttChart.Theme = _defaultTheme;
        }

        private void SetupSampleData()
        {
            // Set sample date range
            var today = DateTime.Today;
            var endDate = today.AddDays(30);
            
            // Create sample tasks
            var tasks = new[]
            {
                new GanttTask { Title = "Project Planning", Start = today, End = today.AddDays(5), RowIndex = 1 },
                new GanttTask { Title = "Requirement Analysis", Start = today.AddDays(5), End = today.AddDays(10), RowIndex = 2 },
                new GanttTask { Title = "Design Phase", Start = today.AddDays(10), End = today.AddDays(15), RowIndex = 3 },
                new GanttTask { Title = "Implementation", Start = today.AddDays(15), End = today.AddDays(25), RowIndex = 4 },
                new GanttTask { Title = "Testing", Start = today.AddDays(25), End = today.AddDays(28), RowIndex = 5 },
                new GanttTask { Title = "Deployment", Start = today.AddDays(28), End = today.AddDays(30), RowIndex = 6 }
            };
            
            GanttChart.Tasks = new ObservableCollection<GanttTask>(tasks);
            GanttChart.StartTime = today;
            GanttChart.EndTime = endDate;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
            {
                string themeName = item.Content.ToString();
                
                switch (themeName)
                {
                    case "Default":
                        GanttChart.Theme = _defaultTheme;
                        UpdateColorSelection(_defaultTheme);
                        break;
                    case "Dark":
                        GanttChart.Theme = _darkTheme;
                        UpdateColorSelection(_darkTheme);
                        break;
                    case "Light":
                        GanttChart.Theme = _lightTheme;
                        UpdateColorSelection(_lightTheme);
                        break;
                    case "Modern":
                        GanttChart.Theme = _modernTheme;
                        UpdateColorSelection(_modernTheme);
                        break;
                    case "Custom":
                        GanttChart.Theme = _customTheme;
                        UpdateColorSelection(_customTheme);
                        break;
                }
            }
        }

        private void UpdateColorSelection(GanttTheme theme)
        {
            // Update row color selection to match current theme
            var rowColor = theme.Background.SecondaryColor;
            foreach (var item in RowColorComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag is string colorTag)
                {
                    if (colorTag == rowColor.ToString())
                    {
                        RowColorComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }
            }
            
            // Update primary color selection to match current theme
            var primaryColor = theme.Background.PrimaryColor;
            foreach (var item in PrimaryColorComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag is string colorTag)
                {
                    if (colorTag == primaryColor.ToString())
                    {
                        PrimaryColorComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }
            }
            
            // Update accent color selection to match current theme
            var accentColor = theme.TimeScale.TodayMarkerColor;
            foreach (var item in AccentColorComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag is string colorTag)
                {
                    if (colorTag == accentColor.ToString())
                    {
                        AccentColorComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }
            }
        }

        private void RowColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCustomTheme();
        }

        private void PrimaryColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCustomTheme();
        }

        private void AccentColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCustomTheme();
        }

        private void UpdateCustomTheme()
        {
            if (RowColorComboBox.SelectedItem is ComboBoxItem rowItem &&
                PrimaryColorComboBox.SelectedItem is ComboBoxItem primaryItem &&
                AccentColorComboBox.SelectedItem is ComboBoxItem accentItem &&
                rowItem.Tag is string rowColorStr &&
                primaryItem.Tag is string primaryColorStr &&
                accentItem.Tag is string accentColorStr)
            {
                // Update custom theme with selected colors
                var primaryColor = (Color)ColorConverter.ConvertFromString(primaryColorStr);
                var rowColor = (Color)ColorConverter.ConvertFromString(rowColorStr);
                var accentColor = (Color)ColorConverter.ConvertFromString(accentColorStr);
                
                _customTheme.Background.PrimaryColor = primaryColor;
                _customTheme.Background.SecondaryColor = rowColor;
                _customTheme.TimeScale.TodayMarkerColor = accentColor;
                _customTheme.Task.DefaultColor = primaryColor;
                
                // Apply updated theme
                GanttChart.Theme = _customTheme;
            }
        }
    }
}