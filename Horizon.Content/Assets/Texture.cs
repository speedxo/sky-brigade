using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Content;
using Horizon.Core.Primitives;

namespace Horizon.Content.Assets
{
    public class Texture : IGameAsset
    {
        public uint Handle { get; init; }

        public void Dispose() { }
    }
}
