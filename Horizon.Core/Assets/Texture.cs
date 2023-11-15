using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Content;
using Horizon.Core.Primitives;

namespace Horizon.Core.Assets;

public readonly struct Texture : IGLObject
{
    public readonly uint Width { get; init; }
    public readonly uint Height { get; init; }

    public readonly uint Handle { get; init; }

    public static Texture Invalid { get; } = new Texture { 
        Handle = 0,
        Width = 0,
        Height = 0
    };
}
