using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

using Bogz.Logging.Loggers;

using Box2D.NetStandard.Dynamics.World;

using Horizon.Core;
using Horizon.Core.Data;
using Horizon.Engine;
using Horizon.Input.Components;
using Horizon.OpenGL;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering;
using Horizon.Rendering.Mesh;
using Horizon.Rendering.Techniques;

using ImGuiNET;

namespace DemoPlane;

class IndirectBufferTestMesh : Entity
{
    public record struct QueuedMeshData(Vertex3D[] Vertices, uint[] Indices, DrawElementsIndirectCommand[] IndirectCommands);

    private const string UNIFORM_ALBEDO = "uTexAlbedo";
    private const string UNIFORM_NORMAL = "uTexNormal";
    private const string UNIFORM_SPECULAR = "uTexSpecular";

    public Material Material { get; set; }
    public Technique Technique { get; set; }
    public bool HasUploadQueued { get; protected set; }
    public QueuedMeshData QueuedData { get; protected set; }

    public VertexBufferObject Buffer { get; protected set; }

    public uint ElementCount { get; protected set; }

    public IndirectBufferTestMesh() { }

    protected VertexArrayObjectDescription ArrayDescription = new VertexArrayObjectDescription
    {
        Buffers = new Dictionary<VertexArrayBufferAttachmentType, BufferObjectDescription> {
            {VertexArrayBufferAttachmentType.ArrayBuffer, BufferObjectDescription.ArrayBuffer },
            {VertexArrayBufferAttachmentType.ElementBuffer, BufferObjectDescription.ElementArrayBuffer },
            {VertexArrayBufferAttachmentType.IndirectBuffer, BufferObjectDescription.IndirectBuffer},
        }
    };

    protected virtual VertexBufferObject AcquireBuffer() => new(
            GameEngine.Instance.ObjectManager.VertexArrays.Create(ArrayDescription)
        );

    public override void Initialize()
    {
        base.Initialize();
        Buffer = AcquireBuffer();
        SetBufferLayout();
    }

    protected void SetBufferLayout()
    {
        // configure a 3d layout.
        Buffer.Bind();
        Buffer.VertexBuffer.Bind();
        Buffer.VertexBuffer.SetLayout<Vertex3D>();
        Buffer.VertexBuffer.Unbind();
        Buffer.Unbind();
    }

    /// <summary>
    /// Called after the technique is bound but before the draw call is issued, by default binds material textures to samplers.
    /// </summary>
    protected virtual void CustomUniforms()
    {
        Material.BindAttachment(MaterialAttachment.Albedo, 0);
        Technique.SetUniform(UNIFORM_ALBEDO, 0);

        Material.BindAttachment(MaterialAttachment.Normal, 1);
        Technique.SetUniform(UNIFORM_NORMAL, 1);

        Material.BindAttachment(MaterialAttachment.Specular, 2);
        Technique.SetUniform(UNIFORM_SPECULAR, 2);
    }

    /// <summary>
    /// Called after the buffer has been bound, by default simply issues a call to glDrawElements.
    /// </summary>
    protected virtual unsafe void DrawBuffer()
    {
        GameEngine
            .Instance
            .GL
            .MultiDrawElementsIndirect(Silk.NET.OpenGL.PrimitiveType.Triangles, Silk.NET.OpenGL.DrawElementsType.UnsignedInt, null, 5, 0);
    }

    public void QueueUpload(in Vertex3D[] vertices, in uint[] indices, in DrawElementsIndirectCommand[] indirectCommands)
    {
        if (HasUploadQueued)
        {
            ConcurrentLogger.Instance.Log(Bogz.Logging.LogLevel.Error, "An attempt was made to queue mesh data for upload when it already had one queued; as a result it was discarded.");
            return;
        }

        HasUploadQueued = true;
        QueuedData = new QueuedMeshData(vertices, indices, indirectCommands);
        ElementCount = (uint)QueuedData.Indices.Length;
    }

    public override void Render(float dt, object? obj = null)
    {
        if (Buffer is null || Technique is null || Material is null)
            return;

        if (HasUploadQueued)
        {
            HasUploadQueued = false;

            if (ElementCount < QueuedData.Indices.Length) // dont reallocate unless we have to.
                Buffer.VertexBuffer.NamedBufferSubData(QueuedData.Vertices);
            else Buffer.VertexBuffer.NamedBufferData(QueuedData.Vertices);

            Buffer.ElementBuffer.NamedBufferData(QueuedData.Indices);

            if (QueuedData.IndirectCommands is not null)
                Buffer.IndirectBuffer?.NamedBufferData(QueuedData.IndirectCommands);
        }

        if (ElementCount < 1) return;

        // bind shader and set uniforms
        Technique.Bind();
        CustomUniforms();

        // bind and render the vao
        Buffer.Bind();
        DrawBuffer();
        Buffer.Unbind();

        // unbind shader
        Technique.Unbind();
    }
}

class Program : Scene
{
    private const float MOVEMENT_SPEED = 5.0f;
    public override Camera ActiveCamera { get; protected set; }

    private Camera3D camera;

    private IndirectBufferTestMesh mesh;

    public Program()
    {
        ActiveCamera = camera = AddEntity<Camera3D>();
        mesh = AddEntity<IndirectBufferTestMesh>();
    }

    public override void Initialize()
    {
        base.Initialize();

        Engine.GL.Enable(Silk.NET.OpenGL.EnableCap.DepthTest);

        mesh.Material = MaterialFactory.Create("materials", "image");
        mesh.Technique = new BasicMaterialTechnique();

        Engine.GL.ClearColor(System.Drawing.Color.CornflowerBlue);

        MouseInputManager.Mouse.Cursor.CursorMode = Silk.NET.Input.CursorMode.Raw;
        camera.Position = new Vector3(0, 0, -3);

        var indices = new List<uint>();
        var vertices = new List<Vertex3D>();
        var indirects = new List<DrawElementsIndirectCommand>();
        uint vertexCounter, indexCounter;

        vertexCounter = indexCounter = 0;

        for (int i = 0; i < 6; i++)
        {
            var (verts, inds) = MeshGenerator.GenerateSphere(12, new Vector3(0, 0, i * 2));
            vertices.AddRange(verts);

            indirects.Add(new DrawElementsIndirectCommand
            {
                count = (uint)inds.Length,
                instanceCount = 1,
                firstIndex = indexCounter,
                baseVertex = (uint)(vertexCounter),
                baseInstance = (uint)i,
            });

            vertexCounter += (uint)verts.Length;
            indexCounter += (uint)inds.Length;

            indices.AddRange(inds.Select(e => (uint)(e + vertexCounter)));
        }

        mesh.QueueUpload([.. vertices], [.. indices], [.. indirects]);
    }

    public override void Render(float dt, object? obj = null)
    {
        Engine.GL.Viewport(0, 0, (uint)Engine.WindowManager.ViewportSize.X, (uint)Engine.WindowManager.ViewportSize.Y);
        Engine.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit | Silk.NET.OpenGL.ClearBufferMask.DepthBufferBit);

        if (ImGui.Begin("test"))
        {
            ImGui.End();
        }

        base.Render(dt, obj);
    }
    public override void UpdateState(float dt)
    {
        float movementSpeed = MOVEMENT_SPEED * (Engine.InputManager.KeyboardManager.IsKeyDown(Silk.NET.Input.Key.ShiftLeft) ? 2.0f : 1.0f);
        Vector2 axis = Engine.InputManager.GetVirtualController().MovementAxis;
        Vector3 cameraFrontNoPitch = Vector3.Normalize(new Vector3(camera.Front.X, 0, camera.Front.Z));
        Vector3 movement = (Vector3.Normalize(Vector3.Cross(cameraFrontNoPitch, Vector3.UnitY)) * movementSpeed * axis.X * dt +
                            movementSpeed * cameraFrontNoPitch * axis.Y * dt) * new Vector3(1, 0, 1);
        camera.Position += movement;

        if (float.IsNaN(camera.Position.X) || float.IsNaN(camera.Position.Y) || float.IsNaN(camera.Position.Z))
            camera.Position = new Vector3(0, 0, -3);

        base.UpdateState(dt);
    }
    public static void Main(string[] args)
    {
        new GameEngine(GameEngineConfiguration.Default with
        {
            InitialScene = typeof(Program)
        }
        ).Run();
    }
}