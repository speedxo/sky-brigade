using Horizon.Core.Primitives;

namespace Horizon.Core.Content;

/// <summary>
/// Abstraction for the sake of style conformance, dont lecture me on the double generics, i know.
/// </summary>
/// <typeparam name="AssetType"></typeparam>
/// <typeparam name="DescriptionType"></typeparam>
public interface IAssetFactory<out AssetType, DescriptionType> 
    where AssetType: IGLObject
    where DescriptionType: IAssetDescription
{
    public static abstract AssetType Create(in DescriptionType description);
}
