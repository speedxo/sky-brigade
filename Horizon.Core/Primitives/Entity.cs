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
    public Entity Parent { get; init; }
    public List<Entity> Children { get; init; }
    public Dictionary<Type, IGameComponent> Components { get; init; }
    public bool Enabled { get; set; } = true;

    private ConcurrentStack<Entity> _uninitializedEntities = new();
    private ConcurrentStack<IGameComponent> _uninitializedComponents = new();

    public Entity()
    {
        Children = new();
        Components = new();
    }

    public void Initialize() { }

    public virtual void Render(float dt)
    {
        while (!_uninitializedComponents.IsEmpty)
        {
            if (_uninitializedComponents.TryPop(out IGameComponent? result))
            {
                if (result is null) continue;
                
                result.Initialize();
                result.Enabled = true;
                
                Components.Add(result.GetType(), result);
            }
        }
        while (!_uninitializedEntities.IsEmpty)
        {
            if (_uninitializedEntities.TryPop(out var result))
            {
                if (result is null) continue;
                
                result.Initialize();
                result.Enabled = true;

                Children.Add(result);
            }
        }

        foreach (var (_, comp) in Components)
            comp.Render(dt);
        for (int i = 0; i < Children.Count; i++)
            Children[i].Render(dt);
    }

    public virtual void UpdatePhysics(float dt) { }

    public virtual void UpdateState(float dt) { }

    /// <summary>
    /// Attempts to return a reference to a specified type of Component.
    /// </summary>
    public T GetComponent<T>()
        where T : IGameComponent => (T)Components[typeof(T)];

    /// <summary>
    /// Attempts to find all reference to a specified type of Entity.
    /// </summary>
    public List<Entity> GetEntities<T>()
        where T : Entity => Children.FindAll(e => e.GetType() == typeof(T));

    /// <summary>
    /// Attempts to return a reference to a specified type of Entity. (if multiple are found, the first one is selected.)
    /// </summary>
    public T? GetEntity<T>()
        where T : Entity => (T)Children.FindAll(e => e.GetType() == typeof(T)).FirstOrDefault();

    /// <summary>
    /// Attempts to attach a component to this Entity.
    /// </summary>
    /// <returns>A reference to the component.</returns>
    public T AddComponent<T>(T component) where T: IGameComponent
    {
        if (!Components.ContainsKey(component.GetType()) && !_uninitializedComponents.Contains(component))
            _uninitializedComponents.Push(component);

        return (T)Components[component.GetType()];
    }

    /// <summary>
    /// Attempts to attach a component to this Entity.
    /// </summary>
    /// <returns>A reference to the component.</returns>
    public T AddComponent<T>()
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
    public T AddEntity<T>(T entity) where T: Entity
    {
        if (!Children.Contains(entity) && !_uninitializedEntities.Contains(entity))
            _uninitializedEntities.Push(entity);
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
    public T AddEntity<T>()
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
