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
public class GameScreenManagerComponent : InstanceManager<Scene>, IGameComponent, IDisposable
{
    public Dictionary<Type, IGameComponent> Components { get; set; } = new();

    public int ID { get; set; }
    public string Name { get; set; } = "Scene Manager";
    public Entity Parent { get; set; }

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
    /// <param name="options">Render Options</param>
    public void Draw(float dt, ref RenderOptions options)
    {
        GetCurrentInstance().Draw(dt, ref options);
    }

    public override void AddInstance<T>(Scene instance)
    {
        base.AddInstance<T>(instance);
        instance.Initialize();
    }

    public void Initialize() { }

    public void RemoveComponent<T>()
        where T : IGameComponent { }

    public void Update(float dt)
    {
        GetCurrentInstance().Update(dt);
    }
}
