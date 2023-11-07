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
        public override string Name { get; set; } = "Cat";

        public Cat()
            : base("content/cat.png", new Vector2(32))
        {
            Spritesheet.AddAnimationRange(
                new (string name, Vector2 position, int length, float frameTime, Vector2? inSize)[]
                {
                    ("idle", new Vector2(0, 0), 4, 0.25f, null),
                    ("run", new Vector2(0, 4), 8, 0.125f, null)
                }
            );

            IsAnimated = true;
            Size = new Vector2(2.0f);

            StateMachine.AddState(AnimalBehavior.Idle, new GenericIdleState(this, StateMachine));
            StateMachine.AddState(
                AnimalBehavior.Wander,
                new GenericWanderState(this, StateMachine)
            );
        }
    }
}
