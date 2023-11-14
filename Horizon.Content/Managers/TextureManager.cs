using Horizon.Content.Assets;
using Horizon.Content.Disposers;
using Horizon.Core.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Content.Managers
{
    internal class TextureManager : IAssetManager<Texture>
    {
        public IGameAssetDisposer<Texture> Disposer { get; } = new TextureDisposer();

        public void Dispose()
        {
            Disposer.DisposeAll();
        }
    }
}
