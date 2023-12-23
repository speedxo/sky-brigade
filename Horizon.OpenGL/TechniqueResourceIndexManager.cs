using Horizon.Core.Primitives;
using Horizon.OpenGL.Managers;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL;

internal class TechniqueResourceIndexManager : IndexManager
{
    public TechniqueResourceIndexManager(in IGLObject obj)
        : base(obj) { }

    protected override uint GetIndex(in string name) =>
        ObjectManager.GL.GetProgramResourceIndex(glObject.Handle, ProgramInterface.ShaderStorageBlock, name);
}
