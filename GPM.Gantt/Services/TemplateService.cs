using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GPM.Gantt.Models;
using GPM.Gantt.Models.Templates;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service for managing project templates
    /// </summary>
    public class TemplateService : ITemplateService
    {
        private readonly List<ProjectTemplate> _templates = new();

        public TemplateService()
        {
            InitializeBuiltInTemplates();
        }

        public async Task<List<ProjectTemplate>> GetTemplatesAsync()
        {
            await Task.Delay(1); // Simulate async operation
            return _templates.OrderBy(t => t.Name).ToList();
        }

        public async Task<List<ProjectTemplate>> GetTemplatesByCategoryAsync(TemplateCategory category)
        {
            await Task.Delay(1);
            return _templates.Where(t => t.Category == category).OrderBy(t => t.Name).ToList();
        }

        public async Task<ProjectTemplate?> GetTemplateAsync(string templateId)
        {
            await Task.Delay(1);
            return _templates.FirstOrDefault(t => t.Id == templateId);
        }

        public async Task<ProjectTemplate> CreateTemplateAsync(ProjectTemplate template)
        {
            await Task.Delay(1);
            if (string.IsNullOrEmpty(template.Id))
                template.Id = Guid.NewGuid().ToString();
            
            template.CreatedDate = DateTime.Now;
            template.ModifiedDate = DateTime.Now;
            
            _templates.Add(template);
            return template;
        }

        public async Task<bool> UpdateTemplateAsync(ProjectTemplate template)
        {
            await Task.Delay(1);
            var existing = _templates.FirstOrDefault(t => t.Id == template.Id);
            if (existing != null)
            {
                template.ModifiedDate = DateTime.Now;
                var index = _templates.IndexOf(existing);
                _templates[index] = template;
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteTemplateAsync(string templateId)
        {
            await Task.Delay(1);
            var template = _templates.FirstOrDefault(t => t.Id == templateId);
            if (template != null && !template.IsBuiltIn)
            {
                _templates.Remove(template);
                return true;
            }
            return false;
        }

        public async Task<List<ProjectTemplate>> SearchTemplatesAsync(string searchTerm)
        {
            await Task.Delay(1);
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _templates.ToList();
            
            var term = searchTerm.ToLowerInvariant();
            return _templates.Where(t => 
                t.Name.ToLowerInvariant().Contains(term) ||
                t.Description.ToLowerInvariant().Contains(term) ||
                t.Tags.Any(tag => tag.ToLowerInvariant().Contains(term))
            ).OrderBy(t => t.Name).ToList();
        }

        public async Task<List<ProjectTemplate>> GetPopularTemplatesAsync(int count = 10)
        {
            await Task.Delay(1);
            return _templates.OrderByDescending(t => t.UsageCount)
                            .ThenByDescending(t => t.Rating)
                            .Take(count)
                            .ToList();
        }

        public async Task<List<ProjectTemplate>> GetRecentTemplatesAsync(int count = 10)
        {
            await Task.Delay(1);
            return _templates.OrderByDescending(t => t.CreatedDate)
                            .Take(count)
                            .ToList();
        }

        public async Task<List<GanttTask>> ApplyTemplateAsync(string templateId, Models.Templates.TemplateApplicationOptions options)
        {
            await Task.Delay(1);
            var template = await GetTemplateAsync(templateId);
            if (template == null)
                return new List<GanttTask>();
            
            await IncrementUsageCountAsync(templateId);
            
            var tasks = new List<GanttTask>();
            var taskIdMapping = new Dictionary<string, Guid>();
            
            // Create tasks from template
            var orderedTasks = template.TaskTemplates.OrderBy(t => t.Order).ToList();
            var currentDate = options.ProjectStartDate;
            var rowIndex = 1;
            
            foreach (var taskTemplate in orderedTasks)
            {
                var taskId = Guid.NewGuid();
                taskIdMapping[taskTemplate.Id] = taskId;
                
                var duration = TimeSpan.FromTicks((long)(taskTemplate.EstimatedDuration.Ticks * options.DurationScale));
                
                var task = new GanttTask
                {
                    Id = taskId,
                    Title = $"{options.TaskNamePrefix}{taskTemplate.Name}",
                    Description = taskTemplate.Description,
                    Start = currentDate,
                    End = currentDate.Add(duration),
                    RowIndex = rowIndex++,
                    Priority = ConvertTemplatePriority(taskTemplate.Priority),
                    Shape = taskTemplate.Shape,
                    Status = Models.TaskStatus.NotStarted
                };
                
                // Apply custom attributes
                foreach (var attr in taskTemplate.CustomAttributes)
                {
                    if (options.CustomPropertyValues.ContainsKey(attr.Key))
                    {
                        // Use provided value instead of template default
                        task.Description += $"\n{attr.Key}: {options.CustomPropertyValues[attr.Key]}";
                    }
                    else
                    {
                        task.Description += $"\n{attr.Key}: {attr.Value}";
                    }
                }
                
                // Handle resource assignments
                if (taskTemplate.RequiredSkills.Any())
                {
                    var assignedResources = new List<string>();
                    foreach (var skill in taskTemplate.RequiredSkills)
                    {
                        if (options.ResourceMappings.ContainsKey(skill))
                        {
                            assignedResources.Add(options.ResourceMappings[skill]);
                        }
                        else
                        {
                            assignedResources.Add(skill); // Use skill name as placeholder
                        }
                    }
                    task.AssignedResources = assignedResources;
                }
                
                tasks.Add(task);
                
                if (!options.AutoSchedule)
                {
                    currentDate = currentDate.AddDays(1); // Simple increment for non-auto-scheduled
                }
            }
            
            // Set up parent-child relationships
            foreach (var taskTemplate in orderedTasks.Where(t => !string.IsNullOrEmpty(t.ParentTaskId)))
            {
                if (taskIdMapping.ContainsKey(taskTemplate.Id) && 
                    taskIdMapping.ContainsKey(taskTemplate.ParentTaskId!))
                {
                    var task = tasks.First(t => t.Id == taskIdMapping[taskTemplate.Id]);
                    task.ParentTaskId = taskIdMapping[taskTemplate.ParentTaskId!];
                }
            }
            
            // Auto-schedule if requested
            if (options.AutoSchedule && options.IncludeDependencies)
            {
                tasks = await ScheduleTasksWithDependencies(tasks, template, taskIdMapping, options);
            }
            
            return tasks;
        }

        public async Task<ProjectTemplate> CreateTemplateFromTasksAsync(List<GanttTask> tasks, 
            List<TaskDependency> dependencies, string templateName, TemplateCategory category)
        {
            await Task.Delay(1);
            
            var template = new ProjectTemplate
            {
                Name = templateName,
                Category = category,
                Description = $"Template created from {tasks.Count} tasks",
                Author = Environment.UserName,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
            
            var taskIdMapping = new Dictionary<Guid, string>();
            
            // Create task templates
            var order = 1;
            foreach (var task in tasks.OrderBy(t => t.RowIndex))
            {
                var templateTaskId = Guid.NewGuid().ToString();
                taskIdMapping[task.Id] = templateTaskId;
                
                var taskTemplate = new TaskTemplate
                {
                    Id = templateTaskId,
                    Name = task.Title,
                    Description = task.Description ?? string.Empty,
                    EstimatedDuration = task.Duration,
                    Priority = ConvertTaskPriority(task.Priority),
                    Shape = task.Shape,
                    Order = order++,
                    RequiredSkills = task.AssignedResources.ToList(),
                    EstimatedEffort = task.Duration.TotalHours,
                    IsMilestone = task.Shape == TaskBarShape.Milestone
                };
                
                if (task.ParentTaskId.HasValue && taskIdMapping.ContainsKey(task.ParentTaskId.Value))
                {
                    taskTemplate.ParentTaskId = taskIdMapping[task.ParentTaskId.Value];
                }
                
                template.TaskTemplates.Add(taskTemplate);
            }
            
            // Create dependency templates
            foreach (var dependency in dependencies)
            {
                var predId = Guid.Parse(dependency.PredecessorTaskId);
                var succId = Guid.Parse(dependency.SuccessorTaskId);
                
                if (taskIdMapping.ContainsKey(predId) && taskIdMapping.ContainsKey(succId))
                {
                    template.DependencyTemplates.Add(new DependencyTemplate
                    {
                        PredecessorTaskId = taskIdMapping[predId],
                        SuccessorTaskId = taskIdMapping[succId],
                        Type = dependency.Type,
                        Lag = dependency.Lag,
                        Description = dependency.Description
                    });
                }
            }
            
            template.EstimatedDuration = tasks.Any() ? 
                tasks.Max(t => t.End) - tasks.Min(t => t.Start) : 
                TimeSpan.Zero;
            
            return await CreateTemplateAsync(template);
        }

        public async Task<List<string>> ValidateTemplateAsync(ProjectTemplate template)
        {
            await Task.Delay(1);
            return template.Validate();
        }

        public async Task<ProjectTemplate?> CloneTemplateAsync(string templateId, string newName)
        {
            await Task.Delay(1);
            var original = await GetTemplateAsync(templateId);
            if (original == null)
                return null;
            
            var clone = JsonSerializer.Deserialize<ProjectTemplate>(JsonSerializer.Serialize(original));
            if (clone != null)
            {
                clone.Id = Guid.NewGuid().ToString();
                clone.Name = newName;
                clone.CreatedDate = DateTime.Now;
                clone.ModifiedDate = DateTime.Now;
                clone.UsageCount = 0;
                clone.Rating = 0;
                clone.RatingCount = 0;
                clone.IsBuiltIn = false;
                
                return await CreateTemplateAsync(clone);
            }
            
            return null;
        }

        public async Task<byte[]> ExportTemplateAsync(string templateId, TemplateExportFormat format)
        {
            await Task.Delay(1);
            var template = await GetTemplateAsync(templateId);
            if (template == null)
                return Array.Empty<byte>();
            
            return format switch
            {
                TemplateExportFormat.JSON => System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true })),
                TemplateExportFormat.XML => ExportToXml(template),
                TemplateExportFormat.Excel => ExportToExcel(template),
                TemplateExportFormat.CSV => ExportToCsv(template),
                _ => Array.Empty<byte>()
            };
        }

        public async Task<ProjectTemplate> ImportTemplateAsync(byte[] data, TemplateExportFormat format)
        {
            await Task.Delay(1);
            
            return format switch
            {
                TemplateExportFormat.JSON => ImportFromJson(data),
                TemplateExportFormat.XML => ImportFromXml(data),
                _ => throw new NotSupportedException($"Import format {format} is not supported")
            };
        }

        public async Task<List<ProjectTemplate>> GetBuiltInTemplatesAsync()
        {
            await Task.Delay(1);
            return _templates.Where(t => t.IsBuiltIn).ToList();
        }

        public async Task<bool> RateTemplateAsync(string templateId, int rating)
        {
            await Task.Delay(1);
            if (rating < 1 || rating > 5)
                return false;
            
            var template = await GetTemplateAsync(templateId);
            if (template != null)
            {
                template.Rating = (template.Rating * template.RatingCount + rating) / (template.RatingCount + 1);
                template.RatingCount++;
                return true;
            }
            return false;
        }

        public async Task<bool> IncrementUsageCountAsync(string templateId)
        {
            await Task.Delay(1);
            var template = await GetTemplateAsync(templateId);
            if (template != null)
            {
                template.UsageCount++;
                return true;
            }
            return false;
        }

        public async Task<Dictionary<TemplateCategory, int>> GetTemplateCategoriesAsync()
        {
            await Task.Delay(1);
            return _templates.GroupBy(t => t.Category)
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        private void InitializeBuiltInTemplates()
        {
            // Software Development Template
            var softwareTemplate = CreateSoftwareDevelopmentTemplate();
            _templates.Add(softwareTemplate);
            
            // Marketing Campaign Template
            var marketingTemplate = CreateMarketingCampaignTemplate();
            _templates.Add(marketingTemplate);
            
            // Event Planning Template
            var eventTemplate = CreateEventPlanningTemplate();
            _templates.Add(eventTemplate);
        }

        private ProjectTemplate CreateSoftwareDevelopmentTemplate()
        {
            var template = new ProjectTemplate
            {
                Name = "Software Development Project",
                Category = TemplateCategory.SoftwareDevelopment,
                Description = "Standard software development lifecycle template",
                Author = "System",
                IsBuiltIn = true,
                Tags = new List<string> { "software", "development", "agile", "scrum" },
                Instructions = "Use this template for standard software development projects with planning, development, testing, and deployment phases."
            };
            
            // Planning Phase
            template.TaskTemplates.Add(new TaskTemplate
            {
                Id = "task1",
                Name = "Requirements Gathering",
                Description = "Gather and document project requirements",
                EstimatedDuration = TimeSpan.FromDays(5),
                Priority = TemplatePriority.High,
                Order = 1,
                RequiredSkills = new List<string> { "Business Analyst", "Product Manager" },
                EstimatedEffort = 40
            });
            
            template.TaskTemplates.Add(new TaskTemplate
            {
                Id = "task2",
                Name = "System Design",
                Description = "Create system architecture and design documents",
                EstimatedDuration = TimeSpan.FromDays(7),
                Priority = TemplatePriority.High,
                Order = 2,
                RequiredSkills = new List<string> { "Software Architect", "Senior Developer" },
                EstimatedEffort = 56
            });
            
            // Development Phase
            template.TaskTemplates.Add(new TaskTemplate
            {
                Id = "task3",
                Name = "Frontend Development",
                Description = "Develop user interface components",
                EstimatedDuration = TimeSpan.FromDays(14),
                Priority = TemplatePriority.Normal,
                Order = 3,
                RequiredSkills = new List<string> { "Frontend Developer", "UI/UX Designer" },
                EstimatedEffort = 112
            });
            
            template.TaskTemplates.Add(new TaskTemplate
            {
                Id = "task4",
                Name = "Backend Development",
                Description = "Develop server-side logic and APIs",
                EstimatedDuration = TimeSpan.FromDays(14),
                Priority = TemplatePriority.Normal,
                Order = 4,
                RequiredSkills = new List<string> { "Backend Developer", "Database Administrator" },
                EstimatedEffort = 112
            });
            
            // Testing Phase
            template.TaskTemplates.Add(new TaskTemplate
            {
                Id = "task5",
                Name = "Unit Testing",
                Description = "Write and execute unit tests",
                EstimatedDuration = TimeSpan.FromDays(5),
                Priority = TemplatePriority.High,
                Order = 5,
                RequiredSkills = new List<string> { "Developer", "QA Engineer" },
                EstimatedEffort = 40
            });
            
            template.TaskTemplates.Add(new TaskTemplate
            {
                Id = "task6",
                Name = "Integration Testing",
                Description = "Test system integration",
                EstimatedDuration = TimeSpan.FromDays(3),
                Priority = TemplatePriority.High,
                Order = 6,
                RequiredSkills = new List<string> { "QA Engineer", "Test Automation Engineer" },
                EstimatedEffort = 24
            });
            
            // Deployment
            template.TaskTemplates.Add(new TaskTemplate
            {
                Id = "task7",
                Name = "Production Deployment",
                Description = "Deploy to production environment",
                EstimatedDuration = TimeSpan.FromDays(1),
                Priority = TemplatePriority.Critical,
                Order = 7,
                RequiredSkills = new List<string> { "DevOps Engineer", "System Administrator" },
                EstimatedEffort = 8,
                IsMilestone = true,
                Shape = TaskBarShape.Milestone
            });
            
            // Dependencies
            template.DependencyTemplates.AddRange(new[]
            {
                new DependencyTemplate { PredecessorTaskId = "task1", SuccessorTaskId = "task2" },
                new DependencyTemplate { PredecessorTaskId = "task2", SuccessorTaskId = "task3" },
                new DependencyTemplate { PredecessorTaskId = "task2", SuccessorTaskId = "task4" },
                new DependencyTemplate { PredecessorTaskId = "task3", SuccessorTaskId = "task5" },
                new DependencyTemplate { PredecessorTaskId = "task4", SuccessorTaskId = "task5" },
                new DependencyTemplate { PredecessorTaskId = "task5", SuccessorTaskId = "task6" },
                new DependencyTemplate { PredecessorTaskId = "task6", SuccessorTaskId = "task7" }
            });
            
            template.EstimatedDuration = TimeSpan.FromDays(49);
            
            return template;
        }

        private ProjectTemplate CreateMarketingCampaignTemplate()
        {
            var template = new ProjectTemplate
            {
                Name = "Marketing Campaign",
                Category = TemplateCategory.Marketing,
                Description = "Standard marketing campaign execution template",
                Author = "System",
                IsBuiltIn = true,
                Tags = new List<string> { "marketing", "campaign", "advertising", "promotion" }
            };
            
            // Add marketing-specific tasks...
            // (Similar structure to software template but with marketing tasks)
            
            return template;
        }

        private ProjectTemplate CreateEventPlanningTemplate()
        {
            var template = new ProjectTemplate
            {
                Name = "Event Planning",
                Category = TemplateCategory.Event,
                Description = "Comprehensive event planning and execution template",
                Author = "System",
                IsBuiltIn = true,
                Tags = new List<string> { "event", "planning", "coordination", "logistics" }
            };
            
            // Add event planning tasks...
            // (Similar structure with event-specific tasks)
            
            return template;
        }

        private async Task<List<GanttTask>> ScheduleTasksWithDependencies(List<GanttTask> tasks, 
            ProjectTemplate template, Dictionary<string, Guid> taskIdMapping, Models.Templates.TemplateApplicationOptions options)
        {
            // This would integrate with the DependencyService for proper scheduling
            // For now, return tasks as-is
            await Task.Delay(1);
            return tasks;
        }

        private TaskPriority ConvertTemplatePriority(TemplatePriority templatePriority)
        {
            return templatePriority switch
            {
                TemplatePriority.Low => TaskPriority.Low,
                TemplatePriority.Normal => TaskPriority.Normal,
                TemplatePriority.High => TaskPriority.High,
                TemplatePriority.Critical => TaskPriority.Critical,
                _ => TaskPriority.Normal
            };
        }

        private TemplatePriority ConvertTaskPriority(TaskPriority taskPriority)
        {
            return taskPriority switch
            {
                TaskPriority.Low => TemplatePriority.Low,
                TaskPriority.Normal => TemplatePriority.Normal,
                TaskPriority.High => TemplatePriority.High,
                TaskPriority.Critical => TemplatePriority.Critical,
                _ => TemplatePriority.Normal
            };
        }

        private byte[] ExportToXml(ProjectTemplate template)
        {
            // Placeholder XML export implementation
            var xml = $"<?xml version=\"1.0\"?><ProjectTemplate><Name>{template.Name}</Name></ProjectTemplate>";
            return System.Text.Encoding.UTF8.GetBytes(xml);
        }

        private byte[] ExportToExcel(ProjectTemplate template)
        {
            // Placeholder Excel export implementation
            // In production, use a library like ClosedXML or EPPlus
            return System.Text.Encoding.UTF8.GetBytes($"Template: {template.Name}");
        }

        private byte[] ExportToCsv(ProjectTemplate template)
        {
            var csv = "Name,Description,Duration,Priority\n";
            foreach (var task in template.TaskTemplates)
            {
                csv += $"{task.Name},{task.Description},{task.EstimatedDuration},{task.Priority}\n";
            }
            return System.Text.Encoding.UTF8.GetBytes(csv);
        }

        private ProjectTemplate ImportFromJson(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<ProjectTemplate>(json) ?? new ProjectTemplate();
        }

        private ProjectTemplate ImportFromXml(byte[] data)
        {
            // Placeholder XML import implementation
            return new ProjectTemplate { Name = "Imported from XML" };
        }
    }
}