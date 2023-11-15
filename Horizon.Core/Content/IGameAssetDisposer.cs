using Horizon.Core.Primitives;

namespace Horizon.Core.Content;

/// <summary>
/// Generic interface implementing a way to aggregate and efficiently unload a array of assets.
/// </summary>
public interface IGameAssetDisposer<AssetType> where AssetType: IGLObject
{
    /// <summary>
    /// Unloads all of a designated asset type.
    /// </summary>
    public static abstract void DisposeAll(in IEnumerable<AssetType> assets);

    /// <summary>
    /// Unloads one of a designated asset type.
    /// </summary>
    public static abstract void Dispose(in AssetType asset);
}
