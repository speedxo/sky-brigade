using Box2D.NetStandard.Dynamics.World.Callbacks;
using Horizon.Data;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Horizon.Primitives;
using Horizon.Rendering;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;
using Color = Box2D.NetStandard.Dynamics.World.Color;

namespace Horizon.Extentions;

public class Box2DDebugDrawCallback : DebugDraw, IGameComponent, IDisposable
{
    public string Name { get; set; }
    public Entity Parent { get; set; }
    public bool Enabled { get; set; } = true;

    private class DebugDrawIntermediaryMeshData : Entity
    {
        public VertexBufferObject<Vertex> VBO { get; init; }

        public List<Vertex> Vertices { get; init; }
        public List<uint> Indices { get; init; }

        private readonly PrimitiveType primitive;

        public DebugDrawIntermediaryMeshData(PrimitiveType primitive)
        {
            VBO = new();

            // Telling the VAO object how to lay out the attribute pointers
            VBO.VertexAttributePointer(
                0,
                3,
                VertexAttribPointerType.Float,
                (uint)Vertex.SizeInBytes,
                0
            );
            VBO.VertexAttributePointer(
                1,
                3,
                VertexAttribPointerType.Float,
                (uint)Vertex.SizeInBytes,
                3 * sizeof(float)
            );
            VBO.VertexAttributePointer(
                2,
                2,
                VertexAttribPointerType.Float,
                (uint)Vertex.SizeInBytes,
                6 * sizeof(float)
            );

            Vertices = new();
            Indices = new();
            this.primitive = primitive;
        }

        public void Draw(float dt, ref RenderOptions options)
        {
            if (Indices.Count < 1)
                return;

            VBO.VertexBuffer.BufferData(CollectionsMarshal.AsSpan(Vertices));
            VBO.ElementBuffer.BufferData(CollectionsMarshal.AsSpan(Indices));

            VBO.Bind();

            // Once again, I really don't want to make the whole method unsafe for one call.
            unsafe
            {
                Entity.Engine.GL.DrawElements(
                    primitive,
                    (uint)Indices.Count,
                    DrawElementsType.UnsignedInt,
                    null
                );
            }

            VBO.Unbind();

            Clear();
        }

        public void Clear()
        {
            Vertices.Clear();
            Indices.Clear();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Clear();
            VBO.Dispose();
        }
    }

    private Technique Technique { get; init; }

    private DebugDrawIntermediaryMeshData polygonMesh { get; init; }
    private DebugDrawIntermediaryMeshData segmentMesh { get; init; }
    private DebugDrawIntermediaryMeshData circleMesh { get; init; }

    private uint polygonMeshIndexCount = 0;

    public Box2DDebugDrawCallback()
    {
        Name = "Box2D Debug Draw Callback";

        segmentMesh = new(PrimitiveType.LineStrip);
        circleMesh = new(PrimitiveType.TriangleFan);
        polygonMesh = new(PrimitiveType.Triangles);
        Technique = new Technique(Entity.Engine.Content.Shaders["basic"]!);
    }

    public void Initialize() { }

    public void Update(float dt) { }

    public void Draw(float dt, ref RenderOptions options)
    {
        if (!Enabled)
        {
            circleMesh.Clear();
            polygonMesh.Clear();
            segmentMesh.Clear();
            return;
        }

        

        Technique.Use();

        Technique.SetUniform("uProjection", options.Camera.Projection);
        Technique.SetUniform("uView", options.Camera.View);
        Technique.SetUniform("uModel", Matrix4x4.Identity);
        Technique.SetUniform("useNormalAsColor", true);

        polygonMesh.Draw(dt, ref options);
        circleMesh.Draw(dt, ref options);
        segmentMesh.Draw(dt, ref options);

        Technique.End();

        polygonMeshIndexCount = 0;
    }

    [Obsolete]
    public override void DrawCircle(
        in Box2D.NetStandard.Common.Vec2 center,
        float radius,
        in Color color
    )
    {
        AddCircle(center, radius, color);
    }

    public override void DrawPoint(in Vector2 position, float size, in Color color)
    {
        AddCircle(position, size, color);
    }

    public void DrawRectangleF(in RectangleF rect, in Color color)
    {
        if (!Enabled)
            return;

        polygonMesh.Vertices.AddRange(
            new Vertex[]
            {
                new Vertex(rect.X, rect.Y, 0, 0, 0, color.R, color.G, color.B),
                new Vertex(rect.X + rect.Width, rect.Y, 0, 0, 0, color.R, color.G, color.B),
                new Vertex(
                    rect.X + rect.Width,
                    rect.Y + rect.Height,
                    0,
                    0,
                    0,
                    color.R,
                    color.G,
                    color.B
                ),
                new Vertex(rect.X, rect.Y + rect.Height, 0, 0, 0, color.R, color.G, color.B)
            }
        );

        polygonMesh.Indices.AddRange(
            new uint[]
            {
                polygonMeshIndexCount + 0,
                polygonMeshIndexCount + 1,
                polygonMeshIndexCount + 2,
                polygonMeshIndexCount + 0,
                polygonMeshIndexCount + 2,
                polygonMeshIndexCount + 3
            }
        );
        polygonMeshIndexCount += 4;
    }

    private void AddCircle(in Vector2 position, float radius, in Color color)
    {
        if (!Enabled)
            return;

        int vCount = 12;
        float angle = 360.0f / (vCount - 1);

        circleMesh.Vertices.Add(
            new Vertex(position.X, position.Y, 0, 0, 0, color.R, color.G, color.B)
        );

        // positions
        for (int i = 0; i <= vCount; i++)
        {
            float currentAngle = angle * i;
            float x = radius * MathF.Cos(MathHelper.DegreesToRadians(currentAngle)) + position.X;
            float y = radius * MathF.Sin(MathHelper.DegreesToRadians(currentAngle)) + position.Y;
            float z = 0.0f;

            circleMesh.Vertices.Add(new Vertex(x, y, z, 0, 0, color.R, color.G, color.B));
            circleMesh.Indices.Add((uint)(circleMesh.Indices.Count));
        }
    }

    [Obsolete]
    public override void DrawPolygon(
        in Box2D.NetStandard.Common.Vec2[] vertices,
        int vertexCount,
        in Color color
    )
    {
        AddPolygon(vertices, vertexCount, color);
    }

    [Obsolete]
    public override void DrawSegment(
        in Box2D.NetStandard.Common.Vec2 p1,
        in Box2D.NetStandard.Common.Vec2 p2,
        in Color color
    )
    {
        DrawSegment(p1, p2, color);
    }

    public void DrawSegment(in Vector2 p1, in Vector2 p2, in Color color)
    {
        if (!Enabled)
            return;

        segmentMesh.Vertices.AddRange(
            new[]
            {
                new Vertex(p1.X, p1.Y, 0, 0, 0, color.R, color.G, color.B),
                new Vertex(p2.X, p2.Y, 0, 0, 0, color.R, color.G, color.B)
            }
        );

        segmentMesh.Indices.Add((uint)segmentMesh.Indices.Count);
        segmentMesh.Indices.Add((uint)segmentMesh.Indices.Count);
    }

    [Obsolete]
    public override void DrawSolidCircle(
        in Box2D.NetStandard.Common.Vec2 center,
        float radius,
        in Box2D.NetStandard.Common.Vec2 axis,
        in Color color
    )
    {
        AddCircle(center, radius, color);
    }

    [Obsolete]
    public override void DrawSolidPolygon(
        in Box2D.NetStandard.Common.Vec2[] vertices,
        int vertexCount,
        in Color color
    )
    {
        AddPolygon(vertices, vertexCount, color);
    }

    [Obsolete]
    private void AddPolygon(
        in Box2D.NetStandard.Common.Vec2[] vertices,
        int vertexCount,
        in Color color
    )
    {
        if (!Enabled)
            return;

        if (vertexCount != 4)
            return;

        for (int i = 0; i < vertexCount; i++)
            polygonMesh.Vertices.Add(
                new Vertex(vertices[i].X, vertices[i].Y, 0, 0, 0, color.R, color.G, color.B)
            );

        polygonMesh.Indices.AddRange(
            new uint[]
            {
                polygonMeshIndexCount + 0,
                polygonMeshIndexCount + 1,
                polygonMeshIndexCount + 2,
                polygonMeshIndexCount + 0,
                polygonMeshIndexCount + 2,
                polygonMeshIndexCount + 3
            }
        );
        polygonMeshIndexCount += 4;
    }

    public override void DrawTransform(in Box2D.NetStandard.Common.Transform xf) { }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        circleMesh.Dispose();
        polygonMesh.Dispose();
        segmentMesh.Dispose();
    }
}
