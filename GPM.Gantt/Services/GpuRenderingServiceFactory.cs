using System;
using System.Collections.Generic;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// GPU rendering service factory
    /// </summary>
    public static class GpuRenderingServiceFactory
    {
        private static readonly Dictionary<GpuRenderingTechnology, Func<IGpuRenderingService>> _serviceFactories = 
            new Dictionary<GpuRenderingTechnology, Func<IGpuRenderingService>>
            {
                { GpuRenderingTechnology.Default, () => new DefaultRenderingService() }
                // Other technology implementations will be added later
            };
        
        private static readonly Dictionary<GpuRenderingTechnology, IGpuRenderingService> _cachedServices = 
            new Dictionary<GpuRenderingTechnology, IGpuRenderingService>();
        
        /// <summary>
        /// Create a rendering service for the specified technology
        /// </summary>
        /// <param name="technology">Rendering technology</param>
        /// <returns>Rendering service instance</returns>
        public static IGpuRenderingService CreateService(GpuRenderingTechnology technology)
        {
            // First try to return a cached instance
            if (_cachedServices.TryGetValue(technology, out var cachedService))
            {
                return cachedService;
            }
            
            // Create a new instance
            if (_serviceFactories.TryGetValue(technology, out var factory))
            {
                var service = factory();
                if (service.Initialize())
                {
                    _cachedServices[technology] = service;
                    return service;
                }
            }
            
            // If the specified technology is not available, fall back to default rendering
            if (technology != GpuRenderingTechnology.Default)
            {
                return CreateService(GpuRenderingTechnology.Default);
            }
            
            throw new NotSupportedException($"Unable to create {technology} rendering service");
        }
        
        /// <summary>
        /// Register a new rendering service implementation
        /// </summary>
        /// <param name="technology">Rendering technology</param>
        /// <param name="factory">Service factory function</param>
        public static void RegisterService(GpuRenderingTechnology technology, Func<IGpuRenderingService> factory)
        {
            _serviceFactories[technology] = factory;
            
            // Clear cached instance (if exists)
            if (_cachedServices.ContainsKey(technology))
            {
                _cachedServices[technology]?.Dispose();
                _cachedServices.Remove(technology);
            }
        }
        
        /// <summary>
        /// Get all supported rendering technologies
        /// </summary>
        /// <returns>Array of supported rendering technologies</returns>
        public static GpuRenderingTechnology[] GetSupportedTechnologies()
        {
            var technologies = new List<GpuRenderingTechnology>(_serviceFactories.Keys);
            return technologies.ToArray();
        }
        
        /// <summary>
        /// Clear all cached service instances
        /// </summary>
        public static void ClearCache()
        {
            foreach (var service in _cachedServices.Values)
            {
                service.Dispose();
            }
            _cachedServices.Clear();
        }
    }
}