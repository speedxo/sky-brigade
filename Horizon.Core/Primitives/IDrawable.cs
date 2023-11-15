namespace Horizon.Core.Primitives;

public interface IDrawable
{
    /// <summary>
    /// Draws the current object in its exact state.
    /// </summary>
    /// <param name="dt">The elapsed time since the last render call.</param>
    public void Render(float dt);
}
