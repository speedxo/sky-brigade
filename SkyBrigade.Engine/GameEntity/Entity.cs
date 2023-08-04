using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;
using System;
using System.Collections.Generic;

namespace SkyBrigade.Engine.GameEntity
{
    /// <summary>
    /// Entity class represents a game entity.
    /// </summary>
    public class Entity : IEntity
    {
        /// <summary>
        /// Gets or sets the dictionary of components attached to the entity.
        /// </summary>
        public Dictionary<Type, IGameComponent> Components { get; internal set; } = new Dictionary<Type, IGameComponent>();

        /// <summary>
        /// Gets or sets the list of child entities.
        /// </summary>
        public List<IEntity> Entities { get; set; }

        /// <summary>
        /// Adds a component of type T to the entity.
        /// </summary>
        /// <typeparam name="T">The type of the component to add.</typeparam>
        /// <param name="component">The instance of the component to add.</param>
        /// <returns>The added component.</returns>
        public T AddComponent<T>(T component) where T : IGameComponent
        {
            if (!Components.TryAdd(typeof(T), component))
            {
                // TODO: Handle error: Component of the same type already exists
            }
            component.Parent = this;
            component.Initialize();
            return component;
        }

        /// <summary>
        /// Adds a new component of type T to the entity.
        /// </summary>
        /// <typeparam name="T">The type of the component to add.</typeparam>
        /// <returns>The added component.</returns>
        public T AddComponent<T>() where T : IGameComponent, new() => AddComponent(Activator.CreateInstance<T>());

        /// <summary>
        /// Removes the component of type T from the entity.
        /// </summary>
        /// <typeparam name="T">The type of the component to remove.</typeparam>
        public void RemoveComponent<T>() where T : IGameComponent
        {
            if (!Components.Remove(typeof(T)))
            {
                // TODO: Handle error: Component of the specified type doesn't exist
            }
        }

        /// <summary>
        /// Gets the component of type T attached to the entity.
        /// </summary>
        /// <typeparam name="T">The type of the component to get.</typeparam>
        /// <returns>The component of the specified type if found, otherwise null.</returns>
        public T? GetComponent<T>() where T : IGameComponent
        {
            if (Components.ContainsKey(typeof(T)))
            {
                return (T)Components[typeof(T)];
            }
            else
            {
                // TODO: Handle error: Component of the specified type doesn't exist
                return default;
            }
        }

        /// <summary>
        /// Checks if the entity has a component of type T attached to it.
        /// </summary>
        /// <typeparam name="T">The type of the component to check.</typeparam>
        /// <returns>True if the entity has the specified component, otherwise false.</returns>
        public bool HasComponent<T>() where T : IGameComponent
        {
            return Components.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Updates the entity and its components.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        public virtual void Update(float dt)
        {
            foreach (var item in Components.Values)
                item.Update(dt);
        }

        /// <summary>
        /// Draws the entity and its components.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        /// <param name="renderOptions">Render options (optional).</param>
        public virtual void Draw(float dt, RenderOptions? renderOptions = null)
        {
            foreach (var item in Components.Values)
                item.Draw(dt, renderOptions);
        }
    }
}
