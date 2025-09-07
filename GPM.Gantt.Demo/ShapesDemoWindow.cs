using System.Collections.ObjectModel;
using System.Windows;
using GPM.Gantt;
using GPM.Gantt.Configuration;
using GPM.Gantt.Models;
using GPM.Gantt.Rendering;
using TaskStatus = GPM.Gantt.Models.TaskStatus;

namespace GPM.Gantt.Demo
{
    /// <summary>
    /// Demo application showcasing different task bar shapes.
    /// </summary>
    public partial class ShapesDemoWindow : Window
    {
        private readonly GanttContainer _ganttContainer;
        private readonly ObservableCollection<GanttTask> _tasks;

        public ShapesDemoWindow()
        {
            // InitializeComponent();
            
            _tasks = new ObservableCollection<GanttTask>();
            _ganttContainer = new GanttContainer
            {
                Tasks = _tasks,
                StartTime = DateTime.Today,
                EndTime = DateTime.Today.AddDays(30),
                TimeUnit = TimeUnit.Day,
                Configuration = new GanttConfiguration
                {
                    Rendering = new RenderingConfiguration
                    {
                        UseEnhancedShapeRendering = true,
                        DefaultTaskBarShape = TaskBarShape.Rectangle,
                        EnableVirtualization = false // Disable for demo clarity
                    }
                }
        
            };

            CreateSampleTasks();
            SetupUI();
        }

        private void CreateSampleTasks()
        {
            var today = DateTime.Today;
            
            // Traditional rectangular task
            _tasks.Add(new GanttTask
            {
                Title = "Traditional Rectangle Task",
                Start = today,
                End = today.AddDays(5),
                RowIndex = 1,
                Progress = 60,
                Shape = TaskBarShape.Rectangle,
                Status = TaskStatus.InProgress
            });

            // Diamond-ended task bar
            _tasks.Add(new GanttTask
            {
                Title = "Diamond Ends Task",
                Start = today.AddDays(2),
                End = today.AddDays(8),
                RowIndex = 2,
                Progress = 30,
                Shape = TaskBarShape.DiamondEnds,
                ShapeParameters = new ShapeRenderingParameters
                {
                    DiamondEndHeight = 0.8,
                    DiamondEndWidth = 15,
                    CornerRadius = 3
                },
                Status = TaskStatus.InProgress
            });

            // Rounded rectangle task
            _tasks.Add(new GanttTask
            {
                Title = "Rounded Rectangle Task",
                Start = today.AddDays(1),
                End = today.AddDays(7),
                RowIndex = 3,
                Progress = 80,
                Shape = TaskBarShape.RoundedRectangle,
                ShapeParameters = new ShapeRenderingParameters
                {
                    CornerRadius = 8
                },
                Status = TaskStatus.InProgress
            });

            // Chevron/arrow task
            _tasks.Add(new GanttTask
            {
                Title = "Chevron Arrow Task",
                Start = today.AddDays(3),
                End = today.AddDays(10),
                RowIndex = 4,
                Progress = 45,
                Shape = TaskBarShape.Chevron,
                ShapeParameters = new ShapeRenderingParameters
                {
                    ChevronAngle = 20,
                    CornerRadius = 2
                },
                Status = TaskStatus.InProgress
            });

            // Milestone marker
            _tasks.Add(new GanttTask
            {
                Title = "Project Milestone",
                Start = today.AddDays(12),
                End = today.AddDays(12), // Single day milestone
                RowIndex = 5,
                Progress = 0,
                Shape = TaskBarShape.Milestone,
                Status = TaskStatus.NotStarted
            });

            // Another diamond-ended task with different parameters
            _tasks.Add(new GanttTask
            {
                Title = "Wide Diamond Task",
                Start = today.AddDays(6),
                End = today.AddDays(15),
                RowIndex = 6,
                Progress = 70,
                Shape = TaskBarShape.DiamondEnds,
                ShapeParameters = new ShapeRenderingParameters
                {
                    DiamondEndHeight = 1.0,
                    DiamondEndWidth = 20,
                    CornerRadius = 0
                },
                Status = TaskStatus.InProgress
            });

        }

        private void SetupUI()
        {
            Title = "Gantt Chart - Custom Task Bar Shapes Demo";
            Width = 1200;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _ganttContainer.TaskRowHeight = new GridLength(30);

            var mainGrid = new System.Windows.Controls.Grid();
            
            // Add title
            var titleBlock = new System.Windows.Controls.TextBlock
            {
                Text = "Custom Task Bar Shapes Demo",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 20)
            };
            
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            
            System.Windows.Controls.Grid.SetRow(titleBlock, 0);
            System.Windows.Controls.Grid.SetRow(_ganttContainer, 1);
            
            mainGrid.Children.Add(titleBlock);
            mainGrid.Children.Add(_ganttContainer);
            
            Content = mainGrid;
        }
    }
}