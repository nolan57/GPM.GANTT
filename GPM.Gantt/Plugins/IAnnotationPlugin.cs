using System.Windows;
using System.Windows.Controls;

namespace GPM.Gantt.Plugins
{
    /// <summary>
    /// Annotation type enumeration
    /// </summary>
    public enum AnnotationType
    {
        Text,
        Shape,
        Line,
        Image,
        Custom
    }

    /// <summary>
    /// Annotation configuration interface
    /// </summary>
    public interface IAnnotationConfig
    {
        string Id { get; set; }
        string Name { get; set; }
        AnnotationType Type { get; set; }
        double X { get; set; }
        double Y { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        string Data { get; set; } // JSON serialized configuration data
    }

    /// <summary>
    /// Annotation plugin interface
    /// </summary>
    public interface IAnnotationPlugin
    {
        /// <summary>
        /// Plugin name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Plugin description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Plugin type
        /// </summary>
        AnnotationType Type { get; }

        /// <summary>
        /// Plugin version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Plugin author
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Get configuration control
        /// </summary>
        /// <returns>Configuration user control</returns>
        UserControl GetConfigurationControl(IAnnotationConfig? config = null);

        /// <summary>
        /// Create annotation element
        /// </summary>
        /// <param name="config">Configuration information</param>
        /// <returns>Annotation UI element</returns>
        UIElement CreateAnnotationElement(IAnnotationConfig config);

        /// <summary>
        /// Configure annotation element
        /// </summary>
        /// <param name="element">Element to configure</param>
        /// <param name="config">Configuration information</param>
        void ConfigureAnnotation(UIElement element, IAnnotationConfig config);

        /// <summary>
        /// Create default configuration
        /// </summary>
        /// <returns>Default configuration object</returns>
        IAnnotationConfig CreateDefaultConfig();

        /// <summary>
        /// Validate configuration
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        /// <returns>Validation result</returns>
        bool ValidateConfig(IAnnotationConfig config);
    }
}