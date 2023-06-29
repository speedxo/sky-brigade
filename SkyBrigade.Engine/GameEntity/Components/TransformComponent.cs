using System;
using System.Numerics;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.GameEntity.Components
{
	public class TransformComponent : IGameComponent
	{
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;
        public Entity Parent { get; set; }

        public void Initialize()
        {

        }

        public void Update(float dt)
        {

        }

        public void Draw(RenderOptions? options = null)
        {

        }
    }
}

