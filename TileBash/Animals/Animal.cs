using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Horizon.Rendering.Spriting;
using TileBash.Animals.Behaviors;

namespace TileBash.Animals
{
    internal abstract class Animal : Sprite
    {
        public AnimalBehaviorStateMachineComponent StateMachine { get; init; }

        public Animal(in Vector2 spriteSize)
            : base(spriteSize)
        {
            StateMachine = AddComponent<AnimalBehaviorStateMachineComponent>();
        }
    }
}
