namespace Horizon.Primitives;

public interface IUpdateable
{
    /// <summary>
    /// Updates the current object.
    /// </summary>
    /// <param name="dt">The elapsed time since the last update call.</param>
    public void Update(float dt);
}
