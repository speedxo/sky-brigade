using Horizon.Rendering;

namespace Horizon.Primitives;

public interface IDrawable
{
    /// <summary>
    /// Draws the current object using the provided render options.
    /// </summary>
    /// <param name="dt">The elapsed time since the last render call.</param>
    /// <param name="renderOptions">Optional render options. If not provided, default options will be used.</param>
    public void Draw(float dt, RenderOptions? renderOptions = null);
}
