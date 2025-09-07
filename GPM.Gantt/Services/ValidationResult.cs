using System;
using System.Collections.Generic;
using System.Linq;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid => !Errors.Any();
        
        /// <summary>
        /// Gets the collection of validation error messages.
        /// </summary>
        public List<string> Errors { get; } = new();
        
        /// <summary>
        /// Gets the collection of validation warning messages.
        /// </summary>
        public List<string> Warnings { get; } = new();
        
        /// <summary>
        /// Adds an error message to the validation result.
        /// </summary>
        /// <param name="error">The error message to add.</param>
        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                Errors.Add(error);
        }
        
        /// <summary>
        /// Adds a warning message to the validation result.
        /// </summary>
        /// <param name="warning">The warning message to add.</param>
        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
                Warnings.Add(warning);
        }
        
        /// <summary>
        /// Merges another validation result into this one.
        /// </summary>
        /// <param name="other">The validation result to merge.</param>
        public void Merge(ValidationResult other)
        {
            if (other != null)
            {
                Errors.AddRange(other.Errors);
                Warnings.AddRange(other.Warnings);
            }
        }
        
        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <returns>A validation result with no errors or warnings.</returns>
        public static ValidationResult Success() => new();
        
        /// <summary>
        /// Creates a failed validation result with the specified error.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>A validation result with the specified error.</returns>
        public static ValidationResult Failure(string error)
        {
            var result = new ValidationResult();
            result.AddError(error);
            return result;
        }
    }
}