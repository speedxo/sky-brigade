using Horizon.Core.Primitives;

namespace Horizon.Core.Components;

/// <summary>
/// IGameComponent interface represents a game component.
/// </summary>
public interface IGameComponent : IDrawable, IUpdateable
{
    /// <summary>
    /// Gets or sets the enable flag for this component.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the human readable name for this component.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the parent entity of the game component.
    /// </summary>
    public Entity Parent { get; set; }

    /// <summary>
    /// Initializes the game component.
    /// </summary>
    void Initialize();
}
