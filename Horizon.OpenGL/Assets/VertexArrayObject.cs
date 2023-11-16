using Horizon.Core.Primitives;
using Horizon.OpenGL.Descriptions;

namespace Horizon.OpenGL.Assets;

public class VertexArrayObject : IGLObject
{
    public uint Handle { get; init; }

    public Dictionary<VertexArrayBufferAttachmentType, BufferObject> Buffers { get; init; }

    public static VertexArrayObject Invalid { get; } = new VertexArrayObject { Handle = 0 };
}
