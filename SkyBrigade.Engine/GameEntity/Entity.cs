using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Primitives;
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
        /// List containing the nested entities within this entity.
        /// </summary>
        public virtual List<IEntity> Entities { get; set; } = new();

        /// <summary>
        /// The unique ID for this entity.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets this entities enable flag.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Human readable name for this entity.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The total number of components.
        /// </summary>
        public int TotalComponents { get => Components.Count + Entities.Sum(e => e.TotalComponents); }

        /// <summary>
        /// The total sum of entities including their children.
        /// </summary>
        public int TotalEntities { get => Entities.Count + Entities.Sum(e => e.TotalEntities); }

        /// <summary>
        /// The parent entity (if there is one)
        /// </summary>
        public IEntity? Parent { get; set; }
        public Dictionary<Type, IGameComponent> Components { get; set; } = new();

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

            Type componentType = component.GetType();
            RequiresComponentAttribute[] requiredAttributes = (RequiresComponentAttribute[])componentType.GetCustomAttributes(typeof(RequiresComponentAttribute), true);

            // Check if the component has any RequiresComponentAttribute
            if (requiredAttributes.Length > 0)
            {
                foreach (RequiresComponentAttribute requiredAttribute in requiredAttributes)
                {
                    Type requiredComponentType = requiredAttribute.ComponentType;

                    // Check if the entity has the required component
                    if (!HasComponent(requiredComponentType))
                        GameManager.Instance.Logger.Log(Logging.LogLevel.Fatal, $"Entity must have component of type '{requiredComponentType.Name}' to add '{componentType.Name}'.");
                }
            }

            component.Parent = this;
            component.Initialize();

            component.Name ??= component.GetType().Name;

            return component;
        }

        /// <summary>
        /// Adds an entity to the scene and returns the added entity.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        /// <returns>The added entity.</returns>
        public T AddEntity<T>(T entity) where T : IEntity
        {
            Entities.Add(entity);
            entity.Parent = this;

            entity.ID = ++IEntity._nextId;
            entity.Name ??= entity.GetType().Name;

            return entity;
        }

        /// <summary>
        /// Adds an entity to the scene and returns the added entity.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        /// <returns>The added entity.</returns>
        public T AddEntity<T>() where T : IEntity, new() => AddEntity(Activator.CreateInstance<T>());

        /// <summary>
        /// Removes an entity from the scene.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        public void RemoveEntity(IEntity entity)
        {
            Entities.Remove(entity);
        }

        /// <summary>
        /// Removes the entity at the specified index from the scene.
        /// </summary>
        /// <param name="index">The index of the entity to be removed.</param>
        public void RemoveAt(int index)
        {
            Entities.RemoveAt(index);
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

        private bool HasComponent(Type type)
        {
            return Components.ContainsKey(type);
        }


        /// <summary>
        /// Updates the entity and its components.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        public virtual void Update(float dt)
        {
            if (!Enabled) return;

            foreach (var item in Components.Values)
                item.Update(dt);

            for (int i = 0; i < Entities.Count; i++)            
                Entities[i].Update(dt);
        }

        /// <summary>
        /// Draws the entity and its components.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        /// <param name="renderOptions">Render options (optional).</param>
        public virtual void Draw(float dt, RenderOptions? renderOptions = null)
        {
            if (!Enabled) return;

            for (int i = 0; i < Components.Count; i++)
                Components.Values.ElementAt(i).Draw(dt, renderOptions);

            for (int i = 0; i < Entities.Count; i++)
                Entities[i].Draw(dt, renderOptions);
        }
    }
}
