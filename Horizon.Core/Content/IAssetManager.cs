using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Engine.Components;

namespace Horizon.Core.Content
{
    public interface IAssetManager<out T> : IDisposable
        where T : IGameAsset
    {
        public IContentManager ContentManager { get; init; }
    }
}
