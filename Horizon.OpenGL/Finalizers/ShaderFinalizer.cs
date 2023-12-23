using Horizon.Content.Descriptions;
using Horizon.Content.Disposers;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Managers;

namespace Horizon.OpenGL.Finalizers;

/// <summary>
/// Unloads injected shaders.
/// </summary>
public class ShaderFinalizer : IGameAssetFinalizer<Shader>
{
    public static void Dispose(in Shader asset)
    {
        ObjectManager.GL.DeleteProgram(asset.Handle);
    }

    public static unsafe void DisposeAll(in IEnumerable<Shader> assets)
    {
        if (!assets.Any())
            return;

        // aggregate all handles into an array.
        uint[] handles = assets.Select((t) => t.Handle).ToArray();

        for (int i = 0; i < handles.Length; i++)
            ObjectManager.GL.DeleteProgram(handles[i]);
    }
}
