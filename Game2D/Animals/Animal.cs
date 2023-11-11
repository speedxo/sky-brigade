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

        public Animal()
        {
            StateMachine = AddComponent<AnimalBehaviorStateMachineComponent>();
        }
    }
}
