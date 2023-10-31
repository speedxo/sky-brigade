using Horizon;
using Horizon.Content;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Horizon.Prefabs.Character;
using Horizon.Rendering;
using Horizon.Rendering.Spriting.Data;
using ImGuiNET;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Horizon_InstancingDemo;

internal record struct Vert2D(Vector2 Position, Vector3 Color);


internal class Program : Scene
{
    static void Main(string[] args)
    {
        var assemName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        var version = assemName.Version;

        var engine = new BasicEngine(GameInstanceParameters.Default with
        {
            InitialGameScreen = typeof(Program),
            WindowTitle = $"{assemName.Name} ({version})"
        });
        engine.Run();
    }

    private readonly Vert2D[] quadVerts;
    private readonly uint[] indices;
    private readonly Vector2[] offsets;
    private readonly int count = 10000;

    private InstancedVertexBufferObject<Vert2D, Vector2> vbo;
    private Technique shader;
    private CharacterController controller;


    public Program()
    {
        //if (Math.Sqrt(count) % 1 != 0)
        //    throw new Exception("count must be a square number");

        // Construct testing data
        float size = 0.005f;
        quadVerts = new[] {
            new Vert2D(new Vector2(-size, -size), new Vector3(1.0f, 0, 0)),
            new Vert2D(new Vector2(size, -size), new Vector3(0, 1.0f, 0)),
            new Vert2D(new Vector2(size, size), new Vector3(0, 0, 1.0f)),
            new Vert2D(new Vector2(-size, size), new Vector3(1.0f, 0, 1.0f))
        };
        
        indices = new uint[] {
            0, 1, 2, 0, 2, 3
        };

        offsets = new Vector2[count];
        int index = 0;
        float offset = 1f;

        int dimention = (int)Math.Sqrt(count) / 2;

        for (int y = -50; y < 50; y++)
        {
            for (int x = -50; x < 50; x++)
            {
                offsets[index++] = new Vector2(x / 10.0f + (offset * x), y / 10.0f+ (offset * y));
            }
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        Engine.GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        // create shader
        shader = new Technique("shaders", "instancing");

        // create camera
        controller = AddEntity<CharacterController>();
        

        InitializeRenderingPipeline();

        vbo = new();

        vbo.VertexBuffer.BufferData(quadVerts);
        vbo.ElementBuffer.BufferData(indices);
        vbo.InstanceBuffer.BufferData(offsets);

        vbo.VertexArray.Bind();
        vbo.VertexBuffer.Bind();

        uint vert2DSize = sizeof(float) * 5;
        vbo.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, vert2DSize, 0);
        vbo.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, vert2DSize, sizeof(float) * 2);

        vbo.InstanceBuffer.Bind();
        vbo.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, sizeof(float) * 2, 0);
        vbo.VertexAttributeDivisor(2, 1);


        vbo.Unbind();
    }

    public override void Update(float dt)
    {
        base.Update(dt);
    }
    int counter = 100;
    public override void DrawOther(float dt, ref RenderOptions options)
    {
        shader.Use();
        
        shader.SetUniform("uView", controller.Camera.View);
        shader.SetUniform("uProjection", controller.Camera.Projection);

        //shader.SetUniform("uModel", modelMatrix);
        vbo.VertexArray.Bind();
        unsafe
        {
            Engine.GL.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null, (uint)count);
        }
        vbo.VertexArray.Unbind();
        shader.End();
    }

    public override void DrawGui(float dt)
    {
        if(ImGui.Begin("debug"))
        {
            ImGui.SliderInt("count", ref counter, 100, 200);
            ImGui.End();
        }
    }

    public override void Dispose()
    {
        shader.Dispose();
        vbo.Dispose();
    }
}