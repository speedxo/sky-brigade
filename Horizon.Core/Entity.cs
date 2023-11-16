﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Components;
using Horizon.Core.Primitives;
using Silk.NET.Core.Native;

namespace Horizon.Core;

public abstract class Entity : IRenderable, IUpdateable, IDisposable
{
    public bool Enabled { get; set; }
    public bool RenderImplicit { get; set; } = false;
    public string Name { get; protected set; }

    public Entity Parent { get; set; }
    public ConcurrentDictionary<Type, IGameComponent> Components { get; init; }
    public List<Entity> Children { get; init; }

    private readonly ConcurrentStack<Entity> _uninitializedEntities = new();
    private readonly ConcurrentStack<IGameComponent> _uninitializedComponents = new();

    public Entity()
    {
        Children = new();
        Components = new();
    }

    /// <summary>
    /// Called after the constructor, guaranteeing that there will be a valid GL context.
    /// </summary>
    public virtual void Initialize() { }

    public virtual void Render(float dt)
    {
        while (!_uninitializedComponents.IsEmpty)
        {
            if (_uninitializedComponents.TryPop(out IGameComponent? result))
            {
                if (result is null)
                    continue;

                result.Initialize();
                result.Enabled = true;
            }
        }
        while (!_uninitializedEntities.IsEmpty)
        {
            if (_uninitializedEntities.TryPop(out var result))
            {
                if (result is null)
                    continue;

                result.Initialize();
                result.Enabled = true;
            }
        }

        foreach (var (_, comp) in Components)
            comp.Render(dt);
        for (int i = 0; i < Children.Count; i++)
            Children[i].Render(dt);
    }

    public virtual void UpdatePhysics(float dt)
    {
        foreach (var (_, comp) in Components)
            comp.UpdatePhysics(dt);
        for (int i = 0; i < Children.Count; i++)
            Children[i].UpdatePhysics(dt);
    }

    public virtual void UpdateState(float dt)
    {
        foreach (var (_, comp) in Components)
            comp.UpdateState(dt);
        for (int i = 0; i < Children.Count; i++)
            Children[i].UpdateState(dt);
    }

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
    public T AddComponent<T>(T component)
        where T : IGameComponent
    {
        if (
            !Components.ContainsKey(component.GetType())
            && !_uninitializedComponents.Contains(component)
        )
            _uninitializedComponents.Push(component);

        component.Parent = this;
        component.Enabled = false;
        component.Name ??= component.GetType().Name;

        Components.TryAdd(component.GetType(), component);
        return component;
    }

    /// <summary>
    /// Attempts to attach a component to this Entity.
    /// </summary>
    /// <returns>A reference to the component.</returns>
    public T AddComponent<T>()
        where T : IGameComponent, new()
    {
        var component = Activator.CreateInstance(typeof(T));
        if (component is null)
        {
            // failed to create component.
        }

        return AddComponent((T)component!);
    }

    public void PushToInitializationQueue(in Entity entity) => _uninitializedEntities.Push(entity);

    /// <summary>
    /// Attempts to attach a child entity to this Entity.
    /// </summary>
    /// <returns>A reference to the child entity.</returns>
    public T AddEntity<T>(in T entity)
        where T : Entity
    {
        if (!Children.Contains(entity) && !_uninitializedEntities.Contains(entity))
            _uninitializedEntities.Push(entity);
        else
        {
            // entity already exists, dupe.
        }

        entity.Parent = this;
        entity.Enabled = false;
        entity.Name ??= entity.GetType().Name;
        Children.Add(entity);

        return entity;
    }

    /// <summary>
    /// Attempts to attach a child entity  to this Entity.
    /// </summary>
    /// <returns>A reference to the child entity.</returns>
    public T AddEntity<T>()
        where T : Entity, new()
    {
        var entity = Activator.CreateInstance(typeof(T));
        if (entity is null)
        {
            // failed to create entity.
        }

        return AddEntity((T)entity!);
    }

    public void Dispose()
    {
        foreach (var item in Components.Values)
            if (item is IDisposable managedItem)
                managedItem.Dispose();
        Components.Clear();

        foreach (var item in Children)
            if (item is IDisposable managedItem)
                managedItem.Dispose();
        Children.Clear();

        GC.SuppressFinalize(this);
    }
}