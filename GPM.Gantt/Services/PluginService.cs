using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GPM.Gantt.Plugins;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Plugin management service implementation
    /// </summary>
    public class PluginService : IPluginService
    {
        private readonly Dictionary<AnnotationType, IAnnotationPlugin> _plugins;
        private readonly List<PluginInfo> _pluginInfos;

        public PluginService()
        {
            _plugins = new Dictionary<AnnotationType, IAnnotationPlugin>();
            _pluginInfos = new List<PluginInfo>();
            
            // Register built-in plugins
            RegisterBuiltInPlugins();
        }

        public IEnumerable<IAnnotationPlugin> GetAnnotationPlugins()
        {
            return _plugins.Values;
        }

        public IAnnotationPlugin GetPluginByType(AnnotationType type)
        {
            return _plugins.TryGetValue(type, out var plugin) ? plugin : null;
        }

        public void RegisterPlugin(IAnnotationPlugin plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));

            _plugins[plugin.Type] = plugin;
            
            var pluginInfo = new PluginInfo
            {
                Name = plugin.Name,
                Description = plugin.Description,
                Type = plugin.Type,
                Version = plugin.Version,
                Author = plugin.Author,
                IsEnabled = true,
                LoadedTime = DateTime.Now
            };

            var existingInfo = _pluginInfos.FirstOrDefault(p => p.Type == plugin.Type);
            if (existingInfo != null)
            {
                _pluginInfos.Remove(existingInfo);
            }
            
            _pluginInfos.Add(pluginInfo);
        }

        public void UnregisterPlugin(AnnotationType pluginType)
        {
            if (_plugins.ContainsKey(pluginType))
            {
                _plugins.Remove(pluginType);
                
                var pluginInfo = _pluginInfos.FirstOrDefault(p => p.Type == pluginType);
                if (pluginInfo != null)
                {
                    _pluginInfos.Remove(pluginInfo);
                }
            }
        }

        public async Task<bool> LoadPluginFromAssemblyAsync(string assemblyPath)
        {
            try
            {
                if (!File.Exists(assemblyPath))
                    return false;

                var assembly = await Task.Run(() => Assembly.LoadFrom(assemblyPath));
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IAnnotationPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var pluginType in pluginTypes)
                {
                    var plugin = (IAnnotationPlugin)Activator.CreateInstance(pluginType);
                    RegisterPlugin(plugin);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Failed to load plugin from {assemblyPath}: {ex.Message}");
                return false;
            }
        }

        public IEnumerable<PluginInfo> GetPluginInfos()
        {
            return _pluginInfos.AsReadOnly();
        }

        private void RegisterBuiltInPlugins()
        {
            RegisterPlugin(new TextAnnotationPlugin());
            RegisterPlugin(new ShapeAnnotationPlugin());
            RegisterPlugin(new LineAnnotationPlugin());
        }
    }
}