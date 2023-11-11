using Horizon.Primitives;

namespace Horizon.Rendering.Spriting;

/// <summary>
/// Abstract interface providing a basic layout for different batched rendering systems.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface I2DBatchedRenderer<T> : IDrawable
{
    /// <summary>
    /// Commits an object to be rendered.
    /// </summary>
    public void Add(T input);

    /// <summary>
    /// Remove an object from management.
    /// </summary>
    public void Remove(T input);
}
