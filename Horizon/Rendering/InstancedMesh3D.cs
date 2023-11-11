using Horizon.Data;
using Silk.NET.OpenGL;

namespace Horizon.Rendering;

public class InstancedMesh3D<InstanceDataType> : InstancedMesh<Vertex, InstanceDataType>
    where InstanceDataType : unmanaged
{
    public override void Load(in InstancedMeshData<Vertex, InstanceDataType> data, in Material? mat = null)
    {
        Load(data as IMeshData<Vertex>, in mat);

        Buffer.InstanceBuffer.Bind();
        Buffer.InstanceBuffer.BufferData(data.InstanceData.Span);
        Buffer.InstanceBuffer.Unbind();
    }

    protected override void SetVboLayout()
    {
        Buffer.VertexArray.Bind();
        Buffer.VertexBuffer.Bind();

        Buffer.VertexAttributePointer(
            0,
            3,
            VertexAttribPointerType.Float,
            (uint)Vertex.SizeInBytes,
            0
        );
        Buffer.VertexAttributePointer(
            1,
            3,
            VertexAttribPointerType.Float,
            (uint)Vertex.SizeInBytes,
            3 * sizeof(float)
        );
        Buffer.VertexAttributePointer(
            2,
            2,
            VertexAttribPointerType.Float,
            (uint)Vertex.SizeInBytes,
            6 * sizeof(float)
        );

        Buffer.VertexBuffer.Unbind();
        Buffer.VertexArray.Unbind();
    }
}