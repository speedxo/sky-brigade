using Horizon.Content.Descriptions;
using Horizon.Content.Disposers;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Managers;

namespace Horizon.OpenGL.Finalizers;

/// <summary>
/// Unloads injected buffers.
/// </summary>
public class BufferObjectFinalizer : IGameAssetFinalizer<BufferObject>
{
    public static void Dispose(in BufferObject asset)
    {
        ContentManager.GL.DeleteBuffer(asset.Handle);
    }

    public static unsafe void DisposeAll(in IEnumerable<BufferObject> assets)
    {
        if (!assets.Any())
            return;

        // aggregate all handles into an array.
        uint[] handles = assets.Select((t) => t.Handle).ToArray();

        fixed (uint* first = &handles[0])
            ContentManager.GL.DeleteBuffers((uint)handles.Length, first);
    }
}
