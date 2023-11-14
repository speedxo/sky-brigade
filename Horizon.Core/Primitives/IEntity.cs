using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Components;

namespace Horizon.Core.Primitives;

public abstract class Entity
{
    public IGameEngine Engine { get; private set; }

    public Entity Parent { get; init; }
    public List<Entity> Children { get; init; }
    public Dictionary<Type, IGameComponent> Components { get; init; }
    public bool Enabled { get; set; } = true;

    public Entity()
    {
        Children = new();
        Components = new();
    }

    public void Initialize() { }

    public virtual void Render(float dt) { }

    public virtual void UpdatePhysics(float dt) { }

    public virtual void UpdateState(float dt) { }

    /// <summary>
    /// Attempts to return a reference to a specified type of Component.
    /// </summary>
    public IGameComponent GetComponent<T>()
        where T : IGameComponent => Components[typeof(T)];

    /// <summary>
    /// Attempts to find all reference to a specified type of Entity.
    /// </summary>
    public List<Entity> GetEntities<T>()
        where T : Entity => Children.FindAll(e => e.GetType() == typeof(T));

    /// <summary>
    /// Attempts to return a reference to a specified type of Entity. (if multiple are found, the first one is selected.)
    /// </summary>
    public Entity? GetEntity<T>()
        where T : Entity => Children.FindAll(e => e.GetType() == typeof(T)).FirstOrDefault();

    /// <summary>
    /// Attempts to attach a component to this Entity.
    /// </summary>
    /// <returns>A reference to the component.</returns>
    public IGameComponent AddComponent(IGameComponent component)
    {
        if (!Components.ContainsKey(component.GetType()))
            Components.TryAdd(component.GetType(), component);

        return Components[component.GetType()];
    }

    /// <summary>
    /// Attempts to attach a component to this Entity.
    /// </summary>
    /// <returns>A reference to the component.</returns>
    public IGameComponent AddComponent<T>()
        where T : IGameComponent
    {
        var component = Activator.CreateInstance(typeof(T));
        if (component is null)
        {
            // failed to create component.
        }

        return AddComponent((T)component!);
    }

    /// <summary>
    /// Attempts to attach a child entity to this Entity.
    /// </summary>
    /// <returns>A reference to the child entity.</returns>
    public Entity AddEntity(Entity entity)
    {
        if (!Children.Contains(entity))
            Children.Add(entity);
        else
        {
            // entity already exists, dupe.
        }

        return entity;
    }

    /// <summary>
    /// Attempts to attach a child entity  to this Entity.
    /// </summary>
    /// <returns>A reference to the child entity.</returns>
    public Entity AddEntity<T>()
        where T : Entity
    {
        var entity = Activator.CreateInstance(typeof(T));
        if (entity is null)
        {
            // failed to create entity.
        }

        return AddEntity((T)entity!);
    }
}
