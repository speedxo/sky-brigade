using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Data;
using Horizon.OpenGL.Descriptions;

namespace Horizon.Rendering.Mesh;

public class Mesh3D : Mesh<Vertex3D>
{
    // Standard configuration with a vertex array and an element array.
    protected override VertexArrayObjectDescription ArrayDescription { get; } =
        VertexArrayObjectDescription.VertexBuffer;

    protected override void SetBufferLayout()
    {
        // configure a 3d layout.
        Buffer.Bind();
        Buffer.VertexBuffer.Bind();
        Buffer.SetLayout<Vertex3D>();
        Buffer.VertexBuffer.Unbind();
        Buffer.Unbind();
    }
}
