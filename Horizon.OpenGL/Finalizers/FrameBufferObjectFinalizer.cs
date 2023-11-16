using Horizon.Content.Disposers;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Managers;

namespace Horizon.OpenGL.Finalizers;

/// <summary>
/// Unloads injected buffers.
/// </summary>
public class FrameBufferObjectFinalizer : IGameAssetFinalizer<FrameBufferObject>
{
    public static void Dispose(in FrameBufferObject asset)
    {
        ContentManager.GL.DeleteFramebuffer(asset.Handle);
    }

    public static unsafe void DisposeAll(in IEnumerable<FrameBufferObject> assets)
    {
        if (!assets.Any())
            return;

        // aggregate all handles into an array.
        uint[] handles = assets.Select((t) => t.Handle).ToArray();

        fixed (uint* first = &handles[0])
            ContentManager.GL.DeleteFramebuffers((uint)handles.Length, first);
    }
}
