namespace Horizon.Primitives;

public interface IUpdateable
{
    /// <summary>
    /// Updates the current object.
    /// </summary>
    /// <param name="dt">The elapsed time since the last render call.</param>
    /// <param name="renderOptions">Optional render options. If not provided, default options will be used.</param>
    public void Update(float dt);
}