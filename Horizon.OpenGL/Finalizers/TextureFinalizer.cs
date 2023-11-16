using Horizon.Content.Descriptions;
using Horizon.Content.Disposers;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Managers;

namespace Horizon.OpenGL.Finalizers;

/// <summary>
/// Unloads injected textures.
/// </summary>
public class TextureFinalizer : IGameAssetFinalizer<Texture>
{
    public static void Dispose(in Texture asset)
    {
        ContentManager.GL.DeleteTexture(asset.Handle);
    }

    public static unsafe void DisposeAll(in IEnumerable<Texture> assets)
    {
        if (!assets.Any())
            return;

        // aggregate all handles into an array.
        uint[] handles = assets.Select((t) => t.Handle).ToArray();

        // delete all handles.
        fixed (uint* firstHandle = &handles[0])
            ContentManager.GL.DeleteTextures((uint)handles.Length, firstHandle);
    }
}
