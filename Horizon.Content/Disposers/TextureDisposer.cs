using Horizon.Core.Assets;
using Horizon.Core.Content;
using Horizon.Core.Primitives;

namespace Horizon.Content.Disposers;

/// <summary>
/// Unloads injected textures.
/// </summary>
public class TextureDisposer : IGameAssetDisposer<Texture>
{
    public static void Dispose(in Texture asset)
    {
        BaseGameEngine.GL.DeleteTexture(asset.Handle);
    }

    public static unsafe void DisposeAll(in IEnumerable<Texture> assets)
    {
        // aggregate all handles into an array.
        uint[] handles = assets.Select((t) => t.Handle).ToArray();

        // delete all handles.
        fixed (uint* firstHandle = &handles[0])
            BaseGameEngine.GL.DeleteTextures((uint)handles.Length, firstHandle);
    }
}
