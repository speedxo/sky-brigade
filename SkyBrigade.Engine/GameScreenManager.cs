using Silk.NET.OpenGL;
using SkyBrigade.Engine.Collections;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Primitives;
using SkyBrigade.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SkyBrigade.Engine
{
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
        public int TotalComponents { get => Components.Count + Instances.Values.Sum(e => e.TotalComponents); }

        /// <summary>
        /// Gets or sets this entities enable flag.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The total sum of entities including their children.
        /// </summary>
        public int TotalEntities { get => Entities.Count + Instances.Values.Sum(e => e.TotalEntities); }

        public IEntity? Parent { get; set; }
        public List<IEntity> Entities { get; set; } = new List<IEntity>();
        public Dictionary<Type, IGameComponent> Components { get; set; } = new();

        public int ID { get; set; }
        public string Name { get; set; } = "Scene Manager";

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
            GetCurrentInstance().Draw(dt, renderOptions);
        }

        public void Update(float dt)
        {
            GetCurrentInstance().Update(dt);
        }
    }
}
