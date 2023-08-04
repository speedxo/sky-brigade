using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;

namespace SkyBrigade.Engine;

public interface IGameScreen : IDisposable
{
    List<IEntity> Entities { get; set; }
    void Initialize(GL gl);

    void Update(float dt);

    void Render(GL gl, float dt);
}

public abstract class GameScreen : IGameScreen
{
    public List<IEntity> Entities { get; set; }

    public virtual void Initialize(GL gl)
    {
        Entities = new List<IEntity>();
    }

    public virtual void Render(GL gl, float dt)
    {
        for (int i = 0; i < Entities.Count; i++)
            Entities[i].Draw(dt);
    }

    public virtual void Update(float dt)
    {
        for (int i = 0; i < Entities.Count; i++)
            Entities[i].Update(dt);
    }

    public void AddEntity(IEntity entity) { Entities.Add(entity); }

    public void RemoveEntity(IEntity entity) => Entities.Remove(entity);
    public void RemoveAt(int index) => Entities.RemoveAt(index);

    public abstract void Dispose();

}