namespace Horizon.Core.Primitives;

/// <summary>
/// Configuration for <see cref="BaseGameEngine"/> and derived classes.
/// </summary>
public readonly struct GameEngineConfiguration
{
    public readonly WindowManagerConfiguration WindowConfiguration { get; init; }

    public static GameEngineConfiguration Default { get; } = new GameEngineConfiguration {
        WindowConfiguration = WindowManagerConfiguration.Default1600x900
    };
}
