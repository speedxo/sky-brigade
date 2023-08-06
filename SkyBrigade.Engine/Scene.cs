using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine;

/// <summary>
/// Abstract base class for a game screen. Provides higher-level functions than the IGameScreen interface.
/// </summary>
public abstract class Scene : IGameScreen
{
    /// <summary>
    /// The list of entities present in the scene.
    /// </summary>
    public List<IEntity> Entities { get; set; } = new List<IEntity>();

    /// <summary>
    /// Renders the scene using the provided OpenGL context (GL) and render options.
    /// </summary>
    /// <param name="gl">The OpenGL context used for rendering.</param>
    /// <param name="dt">The elapsed time since the last render call.</param>
    /// <param name="renderOptions">Optional render options. If not provided, default options will be used.</param>
    public virtual void Draw(float dt, RenderOptions? renderOptions = null)
    {
        for (int i = 0; i < Entities.Count; i++)
            Entities[i].Draw(dt, renderOptions);
    }

    /// <summary>
    /// Updates the scene with the elapsed time (dt).
    /// </summary>
    /// <param name="dt">The elapsed time since the last update call.</param>
    public virtual void Update(float dt)
    {
        for (int i = 0; i < Entities.Count; i++)
            Entities[i].Update(dt);
    }

    /// <summary>
    /// Adds an entity to the scene and returns the added entity.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <returns>The added entity.</returns>
    public T AddEntity<T>(T entity) where T: IEntity
    {
        Entities.Add(entity);
        entity.Parent = null;
        return entity;
    }

    /// <summary>
    /// Adds an entity to the scene and returns the added entity.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <returns>The added entity.</returns>
    public T AddEntity<T>() where T : IEntity => AddEntity(Activator.CreateInstance<T>());

    /// <summary>
    /// Removes an entity from the scene.
    /// </summary>
    /// <param name="entity">The entity to be removed.</param>
    public void RemoveEntity(IEntity entity)
    {
        Entities.Remove(entity);
    }

    /// <summary>
    /// Removes the entity at the specified index from the scene.
    /// </summary>
    /// <param name="index">The index of the entity to be removed.</param>
    public void RemoveAt(int index)
    {
        Entities.RemoveAt(index);
    }

    /// <summary>
    /// Disposes of the scene and its resources.
    /// </summary>
    public abstract void Dispose();
}
