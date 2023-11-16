using Horizon.Core.Primitives;
using Horizon.OpenGL.Managers;

namespace Horizon.OpenGL;

internal class TechniqueUniformManager : IndexManager
{
    public TechniqueUniformManager(in IGLObject obj)
        : base(obj) { }

    protected override uint GetIndex(in string name) =>
        (uint)ContentManager.GL.GetUniformLocation(glObject.Handle, name);
}
