using Horizon.GameEntity.Components;
using Horizon.Primitives;

namespace Horizon.GameEntity;

public interface IEntity : IDrawable, IUpdateable
{
    internal static int _nextId = 0;

    public int ID { get; set; }
    public string Name { get; set; }
    public int TotalEntities { get; }
    public int TotalComponents { get; }
    public bool Enabled { get; set; }

    public void Initialize();

    public IEntity? Parent { get; set; }
    public List<IEntity> Entities { get; set; }
    public Dictionary<Type, IGameComponent> Components { get; protected set; }
    public T AddComponent<T>(T component)
        where T : IGameComponent;
    public T AddComponent<T>()
        where T : IGameComponent, new();
    public T AddEntity<T>()
        where T : IEntity, new();
    public T? GetComponent<T>()
        where T : IGameComponent;
    public void RemoveComponent<T>()
        where T : IGameComponent;
    public bool HasComponent<T>()
        where T : IGameComponent;
    public bool HasComponent(Type type);
}
