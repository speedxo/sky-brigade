using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Core.Content;

public interface IAssetFactory<AssetType, DescriptionType> 
    where AssetType: IGameAsset 
    where DescriptionType: IAssetDescription
{
    public static abstract AssetType Create(in DescriptionType description);
}
