using Horizon.Core;
using Horizon.Core.Components;

namespace Horizon.Core.Components;

/// <summary>
/// Aggregate of more specific engine events.
/// </summary>
public class EngineEventHandler : IGameComponent
{
    public Action<float>? PreState;
    public Action<float>? PostState;

    public Action<float>? PreRender;
    public Action<float>? PostRender;

    public bool Enabled { get; set; }
    public string Name { get; set; }
    public Entity Parent { get; set; }

    public void Initialize() { }

    public void Render(float dt, object? obj = null) { }

    public void UpdatePhysics(float dt) { }

    public void UpdateState(float dt) { }
}
