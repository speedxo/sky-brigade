using System.Collections.Concurrent;
using System.Xml.Linq;
using Bogz.Logging;
using Horizon.Content.Descriptions;
using Horizon.Content.Disposers;
using Horizon.Core.Primitives;

namespace Horizon.Content.Managers;

/// <summary>
/// A class build around creating, managing and disposing of game assets in a reliable thread-safe manner.
/// </summary>
public class AssetManager<AssetType, AssetFactoryType, AssetDescriptionType, AssetDisposerType>
    : IDisposable
    where AssetType : IGLObject
    where AssetDescriptionType : IAssetDescription
    where AssetFactoryType : IAssetFactory<AssetType, AssetDescriptionType>
    where AssetDisposerType : IGameAssetFinalizer<AssetType>
{
    protected Action<LogLevel, string> MessageCallback;
    protected readonly string assetName,
        name;

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

        assetName = typeof(AssetType).Name;
        name = $"{assetName}Manager";
    }

    /// <summary>
    /// Sets a delegate which will be called on events.
    /// </summary>
    /// <param name="callback"></param>
    public void SetMessageCallback(in Action<LogLevel, string> callback) =>
        this.MessageCallback = callback;

    public AssetType CreateOrGet(in string name, in AssetDescriptionType description)
    {
        if (NamedAssets.ContainsKey(name))
            return NamedAssets[name];

        return Create(name, description).Asset;
    }

    /// <summary>
    /// Creates a new named managed instance of an asset from a description.
    /// </summary>
    /// <returns>The newly created asset.</returns>
    public AssetCreationResult<AssetType> Create(
        in string name,
        in AssetDescriptionType description
    )
    {
        var result = AssetFactoryType.Create(description);

        if (result.Status != AssetCreationStatus.Success)
        {
            MessageCallback?.Invoke(LogLevel.Error, $"[{name}] {result.Message}");
            return result;
        }

        MessageCallback?.Invoke(
            LogLevel.Info,
            $"[{name}] Successfully created {assetName} '{name}'!"
        );

        if (!NamedAssets.TryAdd(name, result.Asset))
            MessageCallback?.Invoke(LogLevel.Error, $"[{name}] Failed to add {assetName}!");

        if (result.Status > 0 && result.Message?.CompareTo(string.Empty) != 0)
            MessageCallback?.Invoke(LogLevel.Info, $"[{name}] {result.Message}");

        return result;
    }

    /// <summary>
    /// Creates a new unnamed managed instance of an asset from a description.
    /// </summary>
    /// <returns>The newly created asset.</returns>
    public AssetCreationResult<AssetType> Create(AssetDescriptionType description)
    {
        var result = AssetFactoryType.Create(description);

        if (result.Status != AssetCreationStatus.Success)
        {
            MessageCallback?.Invoke(LogLevel.Error, $"[{name}] {result.Message}");
            return result;
        }

        MessageCallback?.Invoke(LogLevel.Info, $"[{name}] Successfully created a new {assetName}!");

        UnnamedAssets.Add(result.Asset);
        return result;
    }

    /// <summary>
    /// Decorator method for managing an instance of an asset manually.
    /// </summary>
    public AssetType Add(AssetType asset)
    {
        UnnamedAssets.Add(asset);
        return asset;
    }

    /// <summary>
    /// Removes an unnamed object.
    /// </summary>
    public bool Remove(AssetType asset)
    {
        AssetDisposerType.Dispose(asset);
        return UnnamedAssets.Remove(asset);
    }

    /// <summary>
    /// Removes a named object from management.
    /// </summary>
    public bool Remove(string name)
    {
        if (NamedAssets.TryRemove(name, out var asset))
        {
            AssetDisposerType.Dispose(asset);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Dispose method for children classes and extensions.
    /// </summary>
    /// <returns>Count of other assets disposed.</returns>
    protected virtual int DisposeOther()
    {
        return 0;
    }

    /// <summary>
    /// Disposes all managed assets.
    /// </summary>
    public void Dispose()
    {
        int count = UnnamedAssets.Count + NamedAssets.Count + DisposeOther();

        AssetDisposerType.DisposeAll(UnnamedAssets);
        AssetDisposerType.DisposeAll(NamedAssets.Values);

        UnnamedAssets.Clear();
        NamedAssets.Clear();

        MessageCallback?.Invoke(
            LogLevel.Info,
            $"[{name}] Successfully finalized {count} {assetName}s!"
        );

        GC.SuppressFinalize(this);
    }
}
