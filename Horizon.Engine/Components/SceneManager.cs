using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core;
using Horizon.Core.Components;

namespace Horizon.Engine.Components;

public class SceneManager : InstanceManager<Scene>, IGameComponent
{
    public bool Enabled { get; set; }
    public string Name { get; set; }
    public Entity Parent { get; set; }

    private bool _halt = false;

    public void Initialize() { }

    public override void ChangeInstance(Type type)
    {
        base.ChangeInstance(type);

        _halt = true;
        CurrentInstance!.Enabled = false;
        CurrentInstance!.Parent = Parent; // pass through the engine.
    }

    public void Render(float dt, object? obj = null)
    {
        if (_halt && CurrentInstance is not null)
        {
            CurrentInstance.Initialize();
            CurrentInstance.Enabled = true;
            _halt = false;
        }
        CurrentInstance?.Render(dt);
    }

    public void UpdatePhysics(float dt)
    {
        if (_halt || !Enabled)
            return;
        CurrentInstance?.UpdatePhysics(dt);
    }

    public void UpdateState(float dt)
    {
        if (_halt || !Enabled)
            return;
        CurrentInstance?.UpdateState(dt);
    }
}
