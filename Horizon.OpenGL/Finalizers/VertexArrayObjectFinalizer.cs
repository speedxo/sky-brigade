using Horizon.Content.Descriptions;
using Horizon.Content.Disposers;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Managers;

namespace Horizon.OpenGL.Finalizers;

/// <summary>
/// Unloads injected buffers.
/// </summary>
public class VertexArrayObjectFinalizer : IGameAssetFinalizer<VertexArrayObject>
{
    public static void Dispose(in VertexArrayObject asset)
    {
        ObjectManager.GL.DeleteVertexArray(asset.Handle);
    }

    public static unsafe void DisposeAll(in IEnumerable<VertexArrayObject> assets)
    {
        if (!assets.Any())
            return;

        // aggregate all handles into an array.
        uint[] handles = assets.Select((t) => t.Handle).ToArray();

        fixed (uint* first = &handles[0])
            ObjectManager.GL.DeleteVertexArrays((uint)handles.Length, first);
    }
}
