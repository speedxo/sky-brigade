using Horizon.Core.Primitives;

namespace Horizon.Content.Descriptions;

/// <summary>
/// Abstraction for the sake of style conformance, dont lecture me on the double generics, i know.
/// </summary>
/// <typeparam name="AssetType"></typeparam>
/// <typeparam name="DescriptionType"></typeparam>
public interface IAssetFactory<AssetType, DescriptionType>
    where AssetType : IGLObject
    where DescriptionType : IAssetDescription
{
    public static abstract AssetCreationResult<AssetType> Create(in DescriptionType description);
}
