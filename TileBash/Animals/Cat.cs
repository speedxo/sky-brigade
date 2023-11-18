using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Bogz.Logging.Loggers;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering.Spriting;
using TileBash.Animals.Behaviors;

namespace TileBash.Animals
{
    internal class Cat : Animal
    {
        private float age = 0.0f,
            maxAge = 10.0f;

        public Cat()
            : base(new Vector2(32)) { }

        public override void Initialize()
        {
            maxAge = Random.Shared.NextSingle() * 5.0f + 5.0f;

            ConfigureSpriteSheet(
                SpriteSheet.FromTexture(
                    Engine
                        .ContentManager
                        .Textures
                        .CreateOrGet(
                            "cat_spritesheet",
                            new TextureDescription { Path = "content/cat.png" }
                        ),
                    new Vector2(32)
                ),
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
            base.Initialize();
        }

        public override void UpdateState(float dt)
        {
            age += dt;

            if (age > maxAge)
            {
                Batch.Remove(this);
                Parent.RemoveEntity(this);
            }

            base.UpdateState(dt);
        }
    }
}
