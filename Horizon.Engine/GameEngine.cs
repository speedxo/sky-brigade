using Horizon.Content.Managers;
using Horizon.Core.Primitives;
using Horizon.Input;

namespace Horizon.Engine;

public class GameEngine : BaseGameEngine
{
    public ContentManager Content { get; init; }
    public InputManager Input { get; init; }

    public GameEngine(in GameEngineConfiguration engineConfiguration)
        : base(engineConfiguration)
    {
        Content = AddComponent<ContentManager>();
        Input = AddComponent<InputManager>();
    }
}
