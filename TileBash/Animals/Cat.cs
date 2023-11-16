using Horizon.OpenGL.Descriptions;
using Horizon.Rendering.Spriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TileBash.Animals.Behaviors;

namespace TileBash.Animals
{
    internal class Cat : Animal
    {
        public override void Initialize()
        {
            ConfigureSpriteSheet(
               SpriteSheet.FromTexture(Engine.Content.Textures.CreateOrGet("cat_spritesheet", new TextureDescription
               {
                   Path = "content/cat.png"
               }), new Vector2(32)),
               "idle"
           );

            AddAnimationRange(
                new (string name, Vector2 position, uint length, float frameTime, Vector2? inSize)[]
                {
                    ("idle", new Vector2(0, 0), 4, 0.25f, null),
                    ("run", new Vector2(0, 4), 8, 0.125f, null)
                }
            );

            IsAnimated = true;
            SetAnimation("idle");

            StateMachine.AddState(AnimalBehavior.Idle, new GenericIdleState(this, StateMachine));
            StateMachine.AddState(
                AnimalBehavior.Wander,
                new GenericWanderState(this, StateMachine)
            );
            Transform.Scale = Vector2.One * 2.0f;
            base.Initialize();
        }
    }
}
