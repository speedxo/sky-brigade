using Horizon.Core.Assets;
using Horizon.Core.Content;
using Horizon.Core.Primitives;

namespace Horizon.Content.Disposers;

/// <summary>
/// Unloads injected shaders.
/// </summary>
public class ShaderDisposer : IGameAssetDisposer<Shader>
{
    public static void Dispose(in Shader asset)
    {
        BaseGameEngine.GL.DeleteProgram(asset.Handle);
    }

    public static unsafe void DisposeAll(in IEnumerable<Shader> assets)
    {
        // aggregate all handles into an array.
        uint[] handles = assets.Select((t) => t.Handle).ToArray();

        for (int i = 0; i < handles.Length; i++)
            BaseGameEngine.GL.DeleteProgram(handles[i]);
    }
}
