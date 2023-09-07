using Horizon.Collections;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;

namespace Horizon;

/// <summary>
/// A class that manages instances of any class implementing <see cref="IGameScreen"/>,
/// each object is unique to its type.
/// This class implements <see cref="InstanceManager{IGameScreen}"/>.
/// </summary>
public class GameScreenManager : InstanceManager<Scene>, IEntity, IDisposable
{
    /// <summary>
    /// The total number of components.
    /// </summary>
    public int TotalComponents
    {
        get => Components.Count + Instances.Values.Sum(e => e.TotalComponents);
    }

    /// <summary>
    /// Gets or sets this entities enable flag.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The total sum of entities including their children.
    /// </summary>
    public int TotalEntities
    {
        get => Entities.Count + Instances.Values.Sum(e => e.TotalEntities);
    }

    public IEntity? Parent { get; set; }
    public List<IEntity> Entities { get; set; } = new List<IEntity>();
    public Dictionary<Type, IGameComponent> Components { get; set; } = new();

    public int ID { get; set; }
    public string Name { get; set; } = "Scene Manager";

    public T AddComponent<T>(T component)
        where T : IGameComponent
    {
        throw new NotImplementedException();
    }

    public T AddComponent<T>()
        where T : IGameComponent, new()
    {
        throw new NotImplementedException();
    }

    public T AddEntity<T>()
        where T : IEntity, new()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        foreach (var (_, instance) in Instances)
            instance.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Draws the currently selected game screen
    /// </summary>
    /// <param name="dt">Delta time</param>
    /// <param name="renderOptions">Render Options</param>
    public void Draw(float dt, RenderOptions? renderOptions = null)
    {
        if (!Enabled)
            return;
        GetCurrentInstance().Draw(dt, renderOptions);
    }

    public T? GetComponent<T>()
        where T : IGameComponent
    {
        return default;
    }

    public bool HasComponent<T>()
        where T : IGameComponent
    {
        return default;
    }

    public bool HasComponent(Type type)
    {
        return default;
    }

    public void Initialize() { }

    public void RemoveComponent<T>()
        where T : IGameComponent { }

    public void Update(float dt)
    {
        if (!Enabled)
            return;
        GetCurrentInstance().Update(dt);
    }
}
