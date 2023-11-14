using Horizon.Content.Assets;
using Horizon.Core.Content;
using Horizon.Core.Primitives;

namespace Horizon.Content.Disposers
{
    /// <summary>
    /// Unloads injected textures.
    /// </summary>
    internal class TextureDisposer : IGameAssetDisposer<Texture>
    {
        public static unsafe void DisposeAll(in IEnumerable<Texture> assets)
        {
            // aggregate all handles into an array.
            uint[] data = assets.Select((t) => t.Handle).ToArray();

            // delete all handles.
            fixed (uint* firstHandle = &data[0])
                Entity.Engine.GL.DeleteTextures((uint)data.Length, firstHandle);
        }
    }
}
