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

    public IEntity? Parent { get; set; }
    public List<IEntity> Entities { get; set; }
    public Dictionary<Type, IGameComponent> Components { get; protected set; }
}