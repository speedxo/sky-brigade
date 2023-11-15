using Horizon.Core.Components;
using Horizon.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Content.Managers;

public class ObjectManager : IGameComponent
{
    public bool Enabled { get; set; }
    public string Name { get; set; }
    public Entity Parent { get; set; }



    public ObjectManager()
    {
        
    }

    public void Initialize()
    {

    }

    public void Render(float dt) { }
    public void UpdatePhysics(float dt) { }
    public void UpdateState(float dt) { }
}
