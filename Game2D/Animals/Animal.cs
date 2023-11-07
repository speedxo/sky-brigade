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
    internal abstract class Animal : Sprite
    {
        public override string Name { get; set; } = "Animal";

        public AnimalBehaviorStateMachineComponent StateMachine { get; init; }

        public Animal(string spriteSheetPath, Vector2 spriteSize)
        {
            ConfigureSpriteSheet(
                Engine.Content.AddTexture(new SpriteSheet(spriteSheetPath, spriteSize)),
                "",
                new Horizon.GameEntity.Components.TransformComponent2D()
                {
                    Position = new Vector2(32)
                }
            );

            StateMachine = AddComponent<AnimalBehaviorStateMachineComponent>();
        }
    }
}
