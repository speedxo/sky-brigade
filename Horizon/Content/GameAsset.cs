using Horizon.GameEntity;

namespace Horizon.Content
{
    /// <summary>
    /// A parent class to wrap a managed asset around.
    /// </summary>
    public abstract class GameAsset : Entity, IDisposable
    {
        public abstract void Dispose();
    }
}
