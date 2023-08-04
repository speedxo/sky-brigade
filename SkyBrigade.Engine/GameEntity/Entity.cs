﻿using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.GameEntity;

public class Entity : IEntity
{
    public Dictionary<Type, IGameComponent> Components { get; internal set; } = new Dictionary<Type, IGameComponent>();
    public List<IEntity> Entities { get; set; }

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

    public T AddComponent<T>() where T : IGameComponent, new() => AddComponent(Activator.CreateInstance<T>());

    public void RemoveComponent<T>() where T : IGameComponent
    {
        if (!Components.Remove(typeof(T)))
        {
            // TODO: Handle error: Component of the specified type doesn't exist
        }
    }

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

    public bool HasComponent<T>() where T : IGameComponent
    {
        return Components.ContainsKey(typeof(T));
    }

    public virtual void Update(float dt)
    {
        foreach (var item in Components.Values)
            item.Update(dt);
    }

    public virtual void Draw(float dt, RenderOptions? renderOptions = null)
    {
        foreach (var item in Components.Values)
            item.Draw(dt, renderOptions);
    }
}