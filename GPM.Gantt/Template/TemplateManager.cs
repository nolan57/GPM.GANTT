using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

namespace GPM.Gantt.Template
{
    /// <summary>
    /// Manages template operations for the Gantt chart.
    /// </summary>
    public class TemplateManager
    {
        /// <summary>
        /// Applies a project template using the configured template service
        /// </summary>
        public static async Task<List<GanttTask>> ApplyTemplateAsync(ITemplateService templateService, string templateId, Models.Templates.TemplateApplicationOptions options)
        {
            if (templateService == null)
            {
                System.Diagnostics.Debug.WriteLine("Template service not configured");
                return new List<GanttTask>();
            }
            
            return await templateService.ApplyTemplateAsync(templateId, options);
        }
    }
}