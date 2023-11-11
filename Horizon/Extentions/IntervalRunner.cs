using Horizon.GameEntity;

namespace Horizon.Extentions;

public class IntervalRunner : Entity
{
    public float TimeInterval { get; init; }
    private float _timer = 0.0f;
    private Action _action;

    public IntervalRunner(float timeInterval, Action action)
    {
        TimeInterval = timeInterval;
        _action = action;
    }

    public void SetAction(Action action)
    {
        _action = action;
    }

    public override void UpdateState(float dt)
    {
        if (!Enabled)
        {
            _timer = 0.0f;
            return;
        }

        _timer += dt;
        if (_timer >= TimeInterval)
        {
            _timer = 0.0f;
            _action.Invoke();
        }
    }
}
