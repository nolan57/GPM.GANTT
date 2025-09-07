using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPM.Gantt.Plugins;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Plugin management service interface
    /// </summary>
    public interface IPluginService
    {
        /// <summary>
        /// Get all available annotation plugins
        /// </summary>
        /// <returns>Plugin list</returns>
        IEnumerable<IAnnotationPlugin> GetAnnotationPlugins();

        /// <summary>
        /// Get plugin by type
        /// </summary>
        /// <param name="type">Plugin type</param>
        /// <returns>Plugin of the specified type</returns>
        IAnnotationPlugin GetPluginByType(AnnotationType type);

        /// <summary>
        /// Register plugin
        /// </summary>
        /// <param name="plugin">Plugin to register</param>
        void RegisterPlugin(IAnnotationPlugin plugin);

        /// <summary>
        /// Unregister plugin
        /// </summary>
        /// <param name="pluginType">Plugin type to unregister</param>
        void UnregisterPlugin(AnnotationType pluginType);

        /// <summary>
        /// Dynamically load plugin from assembly
        /// </summary>
        /// <param name="assemblyPath">Plugin assembly path</param>
        /// <returns>Load result</returns>
        Task<bool> LoadPluginFromAssemblyAsync(string assemblyPath);

        /// <summary>
        /// Get plugin information
        /// </summary>
        /// <returns>Plugin information list</returns>
        IEnumerable<PluginInfo> GetPluginInfos();
    }

    /// <summary>
    /// Plugin information
    /// </summary>
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public AnnotationType Type { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime LoadedTime { get; set; }
    }
}