using Bogz.Logging;
using Bogz.Logging.Loggers;

namespace Horizon.Core.Components;

public class LoggerComponent : ConcurrentLogger, IGameComponent
{
    public LoggerComponent(string logFile)
        : base(logFile) { }

    public LoggerComponent()
        : base(string.Empty) { }

    public bool Enabled { get; set; }
    public string Name { get; set; } = "Logger";
    public Entity Parent { get; set; }

    public override void Log(LogLevel level, string message)
    {
        if (!Enabled)
            return;

        base.Log(level, message);
    }

    public void Initialize() { }

    public void Render(float dt) { }

    public void UpdatePhysics(float dt) { }

    public void UpdateState(float dt) { }
}
