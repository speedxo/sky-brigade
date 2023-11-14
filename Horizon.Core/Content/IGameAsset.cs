using Horizon.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Core.Content;

/// <summary>
/// A primitive type for building game assets around.
/// </summary>
public interface IGameAsset
{
    /// <summary>
    /// The underlying platform independent handle.
    /// </summary>
    public uint Handle { get; init; }
}
