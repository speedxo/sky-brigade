using Horizon.Core.Primitives;
using System.Collections.Concurrent;

namespace Horizon.Core.Content;

/// <summary>
/// A class build around creating, managing and disposing of game assets in a reliable thread-safe manner.
/// </summary>
public class AssetManager<AssetType, AssetFactoryType, AssetDescriptionType, AssetDisposerType> : IDisposable
    where AssetType : IGLObject
    where AssetDescriptionType : IAssetDescription
    where AssetFactoryType : IAssetFactory<AssetType, AssetDescriptionType>
    where AssetDisposerType : IGameAssetDisposer<AssetType>
{
    /// <summary>
    /// All keyed assets.
    /// </summary>
    public ConcurrentDictionary<string, AssetType> NamedAssets { get; init; }

    /// <summary>
    /// All unnamed but managed assets.
    /// </summary>
    public List<AssetType> UnnamedAssets { get; init; }

    public AssetManager()
    {
        NamedAssets = new();
        UnnamedAssets = new();
    }

    /// <summary>
    /// Creates a new named managed instance of an asset from a description.
    /// </summary>
    /// <returns>The newly created asset.</returns>
    public AssetType Create(in string name, in AssetDescriptionType description)
    {
        AssetType asset = AssetFactoryType.Create(description);
        if (!NamedAssets.TryAdd(name, asset))
        {
            // TODO: failed to add asset.
        }
        return asset;
    }

    /// <summary>
    /// Creates a new unnamed managed instance of an asset from a description.
    /// </summary>
    /// <returns>The newly created asset.</returns>
    public AssetType Create(in AssetDescriptionType description)
    {
        AssetType asset = AssetFactoryType.Create(description);
        UnnamedAssets.Add(asset);
        return asset;
    }

    /// <summary>
    /// Decorator method for managing an instance of an asset manually.
    /// </summary>
    public AssetType Add(in AssetType asset) { UnnamedAssets.Add(asset); return asset; }

    /// <summary>
    /// Removes an unnamed object.
    /// </summary>
    public bool Remove(in AssetType asset)
    {
        AssetDisposerType.Dispose(asset);
        return UnnamedAssets.Remove(asset);
    }

    /// <summary>
    /// Removes a named object from management.
    /// </summary>
    public bool Remove(in string name)
    {
        if (NamedAssets.TryRemove(name, out var asset))
        {
            AssetDisposerType.Dispose(asset);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Disposes all managed assets.
    /// </summary>
    public void Dispose()
    {
        AssetDisposerType.DisposeAll(UnnamedAssets);
        AssetDisposerType.DisposeAll(NamedAssets.Values);
    }
}
