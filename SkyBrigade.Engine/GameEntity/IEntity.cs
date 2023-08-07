using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Primitives;

namespace SkyBrigade.Engine.GameEntity;

public interface IEntity : IDrawable, IUpdateable
{
    public int ID { get; set; }
    public string Name { get; set; }

    public IEntity? Parent { get; set; }
    public List<IEntity> Entities { get; set; }
    public Dictionary<Type, IGameComponent> Components { get; protected set; }
}