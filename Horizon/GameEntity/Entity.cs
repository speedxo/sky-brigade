using Horizon.GameEntity.Components;
using Horizon.Primitives;
using Horizon.Rendering;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Horizon.GameEntity
{
    /// <summary>
    /// Entity class represents a game entity.
    /// </summary>
    public abstract class Entity : IDrawable, IUpdateable
    {
        private static GameEngine _engine;

        public static void SetGameEngine(in GameEngine engine) => _engine = engine;

        /// <summary>
        /// Static instance of the parent game engine.
        /// </summary>
        public static GameEngine Engine => _engine;

        internal static int _nextId = 0;
        internal ConcurrentStack<Entity> _uninitializedEntities = new();

        /// <summary>
        /// List containing the nested entities within this entity.
        /// </summary>
        public virtual List<Entity> Entities { get; set; } = new();

        /// <summary>
        /// The unique ID for this entity.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets this entities enable flag.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this entity is being rendered by another system.
        /// </summary>
        public bool RenderImplicit { get; set; } = false;

        /// <summary>
        /// Human readable name for this entity.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The total number of components.
        /// </summary>
        public int TotalComponents
        {
            get => Components.Count + Entities.Sum(e => e.TotalComponents);
        }

        /// <summary>
        /// The total sum of entities including their children.
        /// </summary>
        public int TotalEntities
        {
            get => Entities.Count + Entities.Sum(e => e.TotalEntities);
        }

        /// <summary>
        /// The parent entity (if there is one)
        /// </summary>
        public Entity? Parent { get; set; }

        public Dictionary<Type, IGameComponent> Components { get; set; } = new();

        /// <summary>
        /// This method is called after the parent entity is set.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Adds a component of type T to the entity.
        /// </summary>
        /// <typeparam name="T">The type of the component to add.</typeparam>
        /// <param name="component">The instance of the component to add.</param>
        /// <returns>The added component.</returns>
        public T AddComponent<T>(T component)
            where T : IGameComponent
        {
            if (!Components.TryAdd(typeof(T), component))
            {
                var msg = $"Component of the same type ({nameof(T)}) already exists";

                Engine.Logger.Log(Logging.LogLevel.Fatal, msg);
                throw new ArgumentException(msg);
            }

            Type componentType = component.GetType();
            RequiresComponentAttribute[] requiredAttributes = (RequiresComponentAttribute[])
                componentType.GetCustomAttributes(typeof(RequiresComponentAttribute), true);

            // Check if the component has any RequiresComponentAttribute
            if (requiredAttributes.Length > 0)
            {
                foreach (RequiresComponentAttribute requiredAttribute in requiredAttributes)
                {
                    Type requiredComponentType = requiredAttribute.ComponentType;

                    // Check if the entity has the required component
                    if (!HasComponent(requiredComponentType))
                    {
                        Engine.Logger.Log(
                            Logging.LogLevel.Fatal,
                            $"Entity must have component of type '{requiredComponentType.Name}' to add '{componentType.Name}'."
                        );
                        throw new Exception();
                    }
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
        public T AddEntity<T>(T entity)
            where T : Entity
        {
            Entities.Add(entity);
            entity.Parent = this;

            entity.ID = ++_nextId;
            entity.Name ??= entity.GetType().Name;
            entity.Enabled = false;
            _uninitializedEntities.Push(entity);

            return entity;
        }
        protected void PushToInitializationQueue(in Entity entity) => _uninitializedEntities.Push(entity);
        /// <summary>
        /// Adds an entity to the scene and returns the added entity.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        /// <returns>The added entity.</returns>
        public T AddEntity<T>()
            where T : Entity, new() => AddEntity(Activator.CreateInstance<T>());

        /// <summary>
        /// Removes an entity from the scene.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        public void RemoveEntity(Entity entity)
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
        public T AddComponent<T>()
            where T : IGameComponent, new() => AddComponent(Activator.CreateInstance<T>());

        /// <summary>
        /// Removes the component of type T from the entity.
        /// </summary>
        /// <typeparam name="T">The type of the component to remove.</typeparam>
        public void RemoveComponent<T>()
            where T : IGameComponent
        {
            if (!Components.Remove(typeof(T)))
            {
                var msg = $"Component of the same type ({nameof(T)}) doesn't exist";

                Engine.Logger.Log(Logging.LogLevel.Error, msg);
            }
        }

        /// <summary>
        /// Gets the component of type T attached to the entity.
        /// </summary>
        /// <typeparam name="T">The type of the component to get.</typeparam>
        /// <returns>The component of the specified type if found, otherwise null.</returns>
        public T? GetComponent<T>()
            where T : IGameComponent
        {
            if (Components.ContainsKey(typeof(T)))
            {
                return (T)Components[typeof(T)];
            }
            else
            {
                var msg = $"Component of the same type ({nameof(T)}) doesn't exist";

                Engine.Logger.Log(Logging.LogLevel.Error, msg);

                return default;
            }
        }

        /// <summary>
        /// Checks if the entity has a component of type T attached to it.
        /// </summary>
        /// <typeparam name="T">The type of the component to check.</typeparam>
        /// <returns>True if the entity has the specified component, otherwise false.</returns>
        public bool HasComponent<T>()
            where T : IGameComponent
        {
            return Components.ContainsKey(typeof(T));
        }

        public bool HasComponent(Type type)
        {
            return Components.ContainsKey(type);
        }

        /// <summary>
        /// Updates the state logic of the entity, its sub-entities and its components.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        public virtual void UpdateState(float dt)
        {
            if (!Enabled)
                return;

            foreach (var item in Components.Values)
                item.UpdateState(dt);

            for (int i = 0; i < Entities.Count; i++)
                Entities[i].UpdateState(dt);
        }

        /// <summary>
        /// Draws the entity and its components.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        /// <param name="options">Render options (optional).</param>
        public virtual void Render(float dt, ref RenderOptions options)
        {
            while (_uninitializedEntities.Any())
            {
                if (_uninitializedEntities.TryPop(out var ent))
                {
                    ent.Enabled = !ent.RenderImplicit;
                    ent.Initialize();
                }
            }

            if (!Enabled && !RenderImplicit)
                return;

            for (int i = 0; i < Entities.Count; i++)
                Entities[i].Render(dt, ref options);
            for (int i = 0; i < Components.Count; i++)
                Components.Values.ElementAt(i).Render(dt, ref options);
        }

        /// <summary>
        /// Updates the physics logic of the entity, its sub-entities and its components.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        public virtual void UpdatePhysics(float dt)
        {
            if (!Enabled)
                return;

            foreach (var item in Components.Values)
                item.UpdatePhysics(dt);

            for (int i = 0; i < Entities.Count; i++)
                Entities[i].UpdatePhysics(dt);
        }
    }
}
