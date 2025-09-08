using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPM.Gantt.Models;
using GPM.Gantt.Models.Templates;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service interface for managing project templates
    /// </summary>
    public interface ITemplateService
    {
        /// <summary>
        /// Gets all available project templates
        /// </summary>
        /// <returns>List of project templates</returns>
        Task<List<ProjectTemplate>> GetTemplatesAsync();
        
        /// <summary>
        /// Gets templates by category
        /// </summary>
        /// <param name="category">Template category</param>
        /// <returns>List of templates in the specified category</returns>
        Task<List<ProjectTemplate>> GetTemplatesByCategoryAsync(TemplateCategory category);
        
        /// <summary>
        /// Gets a specific template by ID
        /// </summary>
        /// <param name="templateId">Template identifier</param>
        /// <returns>Project template or null if not found</returns>
        Task<ProjectTemplate?> GetTemplateAsync(string templateId);
        
        /// <summary>
        /// Creates a new project template
        /// </summary>
        /// <param name="template">Template to create</param>
        /// <returns>Created template with assigned ID</returns>
        Task<ProjectTemplate> CreateTemplateAsync(ProjectTemplate template);
        
        /// <summary>
        /// Updates an existing template
        /// </summary>
        /// <param name="template">Template to update</param>
        /// <returns>True if update was successful</returns>
        Task<bool> UpdateTemplateAsync(ProjectTemplate template);
        
        /// <summary>
        /// Deletes a template
        /// </summary>
        /// <param name="templateId">ID of template to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteTemplateAsync(string templateId);
        
        /// <summary>
        /// Searches templates by name, description, or tags
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching templates</returns>
        Task<List<ProjectTemplate>> SearchTemplatesAsync(string searchTerm);
        
        /// <summary>
        /// Gets popular/most used templates
        /// </summary>
        /// <param name="count">Number of templates to return</param>
        /// <returns>List of popular templates</returns>
        Task<List<ProjectTemplate>> GetPopularTemplatesAsync(int count = 10);
        
        /// <summary>
        /// Gets recently created templates
        /// </summary>
        /// <param name="count">Number of templates to return</param>
        /// <returns>List of recent templates</returns>
        Task<List<ProjectTemplate>> GetRecentTemplatesAsync(int count = 10);
        
        /// <summary>
        /// Applies a template to create tasks and dependencies
        /// </summary>
        /// <param name="templateId">ID of template to apply</param>
        /// <param name="options">Application options</param>
        /// <returns>List of created tasks</returns>
        Task<List<GanttTask>> ApplyTemplateAsync(string templateId, Models.Templates.TemplateApplicationOptions options);
        
        /// <summary>
        /// Creates a template from existing tasks
        /// </summary>
        /// <param name="tasks">Tasks to include in template</param>
        /// <param name="dependencies">Dependencies to include</param>
        /// <param name="templateName">Name for the new template</param>
        /// <param name="category">Category for the template</param>
        /// <returns>Created template</returns>
        Task<ProjectTemplate> CreateTemplateFromTasksAsync(List<GanttTask> tasks, List<TaskDependency> dependencies, 
            string templateName, TemplateCategory category);
        
        /// <summary>
        /// Validates a template for consistency and completeness
        /// </summary>
        /// <param name="template">Template to validate</param>
        /// <returns>List of validation errors (empty if valid)</returns>
        Task<List<string>> ValidateTemplateAsync(ProjectTemplate template);
        
        /// <summary>
        /// Clones an existing template
        /// </summary>
        /// <param name="templateId">ID of template to clone</param>
        /// <param name="newName">Name for the cloned template</param>
        /// <returns>Cloned template</returns>
        Task<ProjectTemplate?> CloneTemplateAsync(string templateId, string newName);
        
        /// <summary>
        /// Exports a template to a file format
        /// </summary>
        /// <param name="templateId">ID of template to export</param>
        /// <param name="format">Export format</param>
        /// <returns>Exported data as byte array</returns>
        Task<byte[]> ExportTemplateAsync(string templateId, TemplateExportFormat format);
        
        /// <summary>
        /// Imports a template from file data
        /// </summary>
        /// <param name="data">Template data</param>
        /// <param name="format">Import format</param>
        /// <returns>Imported template</returns>
        Task<ProjectTemplate> ImportTemplateAsync(byte[] data, TemplateExportFormat format);
        
        /// <summary>
        /// Gets built-in system templates
        /// </summary>
        /// <returns>List of built-in templates</returns>
        Task<List<ProjectTemplate>> GetBuiltInTemplatesAsync();
        
        /// <summary>
        /// Rates a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="rating">Rating (1-5)</param>
        /// <returns>True if rating was successful</returns>
        Task<bool> RateTemplateAsync(string templateId, int rating);
        
        /// <summary>
        /// Increments usage count for a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>True if successful</returns>
        Task<bool> IncrementUsageCountAsync(string templateId);
        
        /// <summary>
        /// Gets template categories with counts
        /// </summary>
        /// <returns>Dictionary of categories and their template counts</returns>
        Task<Dictionary<TemplateCategory, int>> GetTemplateCategoriesAsync();
    }

    /// <summary>
    /// Export/import formats for templates
    /// </summary>
    public enum TemplateExportFormat
    {
        JSON,
        XML,
        Excel,
        CSV
    }
}