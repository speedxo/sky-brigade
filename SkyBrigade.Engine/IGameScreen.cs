using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;
using System;
using System.Collections.Generic;

namespace SkyBrigade.Engine
{
    /// <summary>
    /// Interface for a game screen.
    /// </summary>
    public interface IGameScreen : IDisposable
    {
        List<IEntity> Entities { get; set; }
        void Initialize(GL gl);
        void Update(float dt);
        void Render(GL gl, float dt);
    }

    /// <summary>
    /// Abstract base class for a game screen. Provides higher level function than the IGameScreen interface.
    /// </summary>
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

        public IEntity AddEntity(IEntity entity)
        {
            Entities.Add(entity);
            return entity;
        }

        public void RemoveEntity(IEntity entity)
        {
            Entities.Remove(entity);
        }

        public void RemoveAt(int index)
        {
            Entities.RemoveAt(index);
        }

        public abstract void Dispose();
    }
}
