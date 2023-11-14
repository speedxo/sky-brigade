namespace Horizon.Core.Content;

/// <summary>
/// Generic interface implementing a way to aggregate and efficiently unload a array of assets.
/// </summary>
public interface IGameAssetDisposer<AssetType> where AssetType: IGameAsset 
{
    /// <summary>
    /// Unloads all of a designated asset type.
    /// </summary>
    public static abstract void DisposeAll(in IEnumerable<AssetType> assets);
}
