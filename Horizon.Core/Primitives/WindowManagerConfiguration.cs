using System.Numerics;

namespace Horizon.Core.Primitives;

/// <summary>
/// Configuration for <see cref="WindowManager"/>.
/// </summary>
public readonly struct WindowManagerConfiguration
{
    public readonly Vector2 WindowSize { get; init; }
    public readonly string WindowTitle { get; init; }

    public static WindowManagerConfiguration Default1600x900 { get; } = new WindowManagerConfiguration
    {
        WindowSize = new Vector2(1600, 900),
        WindowTitle = "Horizon Engine"
    };
}
