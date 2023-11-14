using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Components;
using Horizon.Core.Primitives;

namespace Horizon.Core.Content
{
    public interface IContentManager : IGameComponent, IDisposable
    {
        public IAssetManager<IGameAsset> Textures { get; init; }
        public IAssetManager<IGameAsset> Shaders { get; init; }
    }
}
