using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;

namespace Horizon.Core.Data;

[StructLayout(LayoutKind.Sequential)] // explicitly set sequential layout
public struct Vertex3D
{
    [VertexLayout(0, VertexAttribPointerType.Float)]
    private Vector3 position; // 12 bytes

    [VertexLayout(1, VertexAttribPointerType.Float)]
    private Vector3 normal; // 12 bytes

    [VertexLayout(2, VertexAttribPointerType.Float)]
    private Vector2 texCoord; // 8 bytes

    public Vertex3D(Vector3 position, Vector3 normal, Vector2 texCoord)
    {
        this.position = position;
        this.normal = normal;
        this.texCoord = texCoord;
    }

    public Vertex3D(
        float x,
        float y,
        float z,
        float nX = 0,
        float nY = 0,
        float nZ = 0,
        float tX = 0,
        float tY = 0
    )
    {
        this.position = new Vector3(x, y, z);
        this.normal = new Vector3(nX, nY, nZ);
        this.texCoord = new Vector2(tX, tY);
    }

    public Vector3 Position
    {
        readonly get => position;
        set => position = value;
    }

    public Vector3 Normal
    {
        readonly get => normal;
        set => normal = value;
    }

    public Vector2 TexCoord
    {
        readonly get => texCoord;
        set => texCoord = value;
    }
}
