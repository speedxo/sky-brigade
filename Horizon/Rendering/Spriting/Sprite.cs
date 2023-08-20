using System;
using System.Numerics;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering.Spriting.Data;

namespace Horizon.Rendering.Spriting
{
    public class Sprite : Entity
    {
        private static int _idCounter = 0;

        public Spritesheet Spritesheet { get; init; }

        public bool ShouldDraw { get; set; } = true;
        public bool Flipped { get; set; } = false;
        internal bool ShouldUpdateVbo { get; private set; }

        public Vector2 Size { get; set; } = Vector2.One;
        public bool IsAnimated { get; set; }
        public string FrameName { get; set; }

        public TransformComponent Transform { get; init; }

        public Sprite(Spritesheet spriteSheet, string name, TransformComponent? inTransform = null)
        {
            this.Spritesheet = AddEntity(spriteSheet);
            this.Transform = AddComponent(inTransform ?? new TransformComponent());

            this.FrameName = name;
            this.ID = _idCounter++;
        }

        public Vertex2D[] GetVertices()
        {
            Vector2[] uv;
            if (IsAnimated) uv = Spritesheet.GetAnimatedTextureCoordinates(FrameName);
            else uv = Spritesheet.GetTextureCoordinates(FrameName);

            int id = Spritesheet.GetNewSpriteId();

            return new Vertex2D[] {
                new Vertex2D(-Size.X / 2.0f, Size.Y / 2.0f, uv[0].X, uv[0].Y, id),
                new Vertex2D(Size.X / 2.0f, Size.Y / 2.0f, uv[1].X, uv[1].Y, id),
                new Vertex2D(Size.X / 2.0f, -Size.Y / 2.0f, uv[2].X, uv[2].Y, id),
                new Vertex2D(-Size.X / 2.0f, -Size.Y / 2.0f, uv[3].X, uv[3].Y, id),
            };
        }

        public Vector2 GetFrameOffset()
        {
            if (IsAnimated)
                return new Vector2(Spritesheet.AnimationManager.GetFrame(FrameName).index, Spritesheet.AnimationManager.GetFrame(FrameName).definition.Position.Y);

            return Vector2.Zero;
        }
    }
}

