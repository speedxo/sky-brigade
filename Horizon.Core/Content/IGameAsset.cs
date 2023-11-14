using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Core.Content
{
    public interface IGameAsset : IDisposable
    {
        public uint Handle { get; init; }
    }
}
