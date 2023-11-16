using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Primitives;

namespace Horizon.Content;

public enum AssetCreationStatus
{
    Failed = 0,
    Success = 1
}

public readonly struct AssetCreationResult<AssetType>
    where AssetType : IGLObject
{
    public readonly AssetType Asset { get; init; }
    public readonly AssetCreationStatus Status { get; init; }
    public readonly string Message { get; init; }
}
