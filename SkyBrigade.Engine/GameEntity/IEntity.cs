using SkyBrigade.Engine.Primitives;

namespace SkyBrigade.Engine.GameEntity;

public interface IEntity : IDrawable, IUpdateable
{
    public IEntity? Parent { get; set; }
    public List<IEntity> Entities { get; set; }
}