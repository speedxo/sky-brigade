using Horizon.GameEntity;
using Horizon.Logging;

namespace Horizon.Content
{
    public abstract class GameContentManager<TKey, TValue> : Entity, IDisposable
        where TValue : GameAsset
        where TKey : notnull
    {
        protected Dictionary<TKey, TValue> NamedAssets { get; init; } = new();
        protected List<TValue> UnnamedAssets { get; init; } = new();

        /// <summary>
        /// Commit an unmanaged content asset to be managed by the class, indexable through its key.
        /// </summary>
        /// <param name="item">An unmanaged asset.</param>
        /// <param name="key">The unique key used to identify this item.</param>
        public virtual TValue AddNamed(in TKey key, in TValue asset)
        {
            if(!NamedAssets.TryAdd(key, asset))
                Engine.Logger.Log(LogLevel.Error, $"[{Name}] Failed to add asset '{key}' as it is already managed.");

            return asset;
        }

        /// <summary>
        /// Commit an unmanaged content asset to be managed by the class.
        /// </summary>
        /// <param name="item">An unmanaged asset</param>
        public virtual TValue Add(in TValue asset)
        {
            if (UnnamedAssets.Contains(asset))
            {
                Engine.Logger.Log(LogLevel.Error, $"[{Name}] Failed to add asset as it is already managed.");
                return asset;
            }

            UnnamedAssets.Add(asset);
            return UnnamedAssets.Last();
        }

        public TValue? this[TKey key]
        {
            get
            {
                if (NamedAssets.TryGetValue(key, out var val))
                    return val;

                return null;
            }
        }

        /// <summary>
        /// Cleanup all managed objects.
        /// </summary>
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);

            foreach (var asset in NamedAssets.Values)
                asset.Dispose();
            foreach (var asset in UnnamedAssets)
                asset.Dispose();

            NamedAssets.Clear();
        }
    }
}
