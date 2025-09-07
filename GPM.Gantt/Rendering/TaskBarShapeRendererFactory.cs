using System;
using System.Collections.Generic;
using GPM.Gantt.Models;
using GPM.Gantt.Rendering.Shapes;

namespace GPM.Gantt.Rendering
{
    /// <summary>
    /// Factory for creating task bar shape renderers.
    /// </summary>
    public class TaskBarShapeRendererFactory
    {
        private static readonly Dictionary<TaskBarShape, Func<ITaskBarShapeRenderer>> _renderers = new()
        {
            [TaskBarShape.Rectangle] = () => new RectangleTaskBarRenderer(),
            [TaskBarShape.DiamondEnds] = () => new DiamondEndTaskBarRenderer(),
            [TaskBarShape.RoundedRectangle] = () => new RoundedRectangleTaskBarRenderer(),
            [TaskBarShape.Chevron] = () => new ChevronTaskBarRenderer(),
            [TaskBarShape.Milestone] = () => new MilestoneTaskBarRenderer()
        };
        
        private static readonly Dictionary<TaskBarShape, ITaskBarShapeRenderer> _singletonRenderers = new();

        /// <summary>
        /// Gets a renderer for the specified shape type.
        /// </summary>
        /// <param name="shapeType">The shape type to get a renderer for</param>
        /// <returns>A shape renderer instance</returns>
        public static ITaskBarShapeRenderer GetRenderer(TaskBarShape shapeType)
        {
            if (_singletonRenderers.TryGetValue(shapeType, out var cachedRenderer))
            {
                return cachedRenderer;
            }

            if (_renderers.TryGetValue(shapeType, out var factory))
            {
                var renderer = factory();
                _singletonRenderers[shapeType] = renderer;
                return renderer;
            }

            throw new NotSupportedException($"Shape type '{shapeType}' is not supported.");
        }

        /// <summary>
        /// Registers a custom renderer for a shape type.
        /// </summary>
        /// <param name="shapeType">The shape type</param>
        /// <param name="rendererFactory">Factory function to create the renderer</param>
        public static void RegisterRenderer(TaskBarShape shapeType, Func<ITaskBarShapeRenderer> rendererFactory)
        {
            _renderers[shapeType] = rendererFactory;
            
            // Clear cached instance if it exists
            if (_singletonRenderers.ContainsKey(shapeType))
            {
                _singletonRenderers.Remove(shapeType);
            }
        }

        /// <summary>
        /// Gets all supported shape types.
        /// </summary>
        /// <returns>Array of supported shape types</returns>
        public static TaskBarShape[] GetSupportedShapes()
        {
            return [.. _renderers.Keys];
        }
    }
}