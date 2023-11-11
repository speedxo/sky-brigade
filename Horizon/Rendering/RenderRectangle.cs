using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Silk.NET.OpenGL;
using System.Numerics;
using Shader = Horizon.Content.Shader;
using Texture = Horizon.OpenGL.Texture;

namespace Horizon.Rendering;

public class RenderTarget : Entity
{
    public MeshRendererComponent Mesh { get; init; }
    public Technique Technique { get; set; }
    public Texture Texture { get; init; }

    protected TransformComponent Transform { get; init; }

    public RenderTarget(Technique technique, Texture texture)
    {
        this.Texture = texture;
        Technique = AddEntity(technique);

        Transform = AddComponent<TransformComponent>();

        Mesh = AddComponent<MeshRendererComponent>();
        Mesh.Load(MeshGenerators.CreateRectangle, new CustomMaterial(technique));
    }

    public RenderTarget(Shader shader)
    {
        Technique = AddEntity(new Technique(shader));

        Transform = AddComponent<TransformComponent>();

        Mesh = AddComponent<MeshRendererComponent>();
        Mesh.Load(MeshGenerators.CreateRectangle, new CustomMaterial(Technique.Shader));
    }

    public void RenderScene(float dt) { }

    public override void Render(float dt, ref RenderOptions options)
    {
        Engine.GL.Viewport(
            0,
            0,
            (uint)Engine.Window.ViewportSize.X,
            (uint)Engine.Window.ViewportSize.Y
        );
        Technique.Use();
        Technique.SetUniform("useTexture", true);
        Texture.Bind(TextureUnit.Texture0);
        Technique.SetUniform("uTexture", 0);
        Technique.SetUniform("uModel", Matrix4x4.Identity);
        Technique.SetUniform("uView", Matrix4x4.Identity);
        Technique.SetUniform("uProjection", Matrix4x4.Identity);

        Mesh.Render(dt, ref options);

        Technique.End();
    }

    public override void UpdateState(float dt) { }
}

public class RenderRectangle : Entity
{
    public MeshRendererComponent Mesh { get; init; }
    public Technique Technique { get; set; }
    public FrameBufferObject FrameBuffer { get; init; }

    protected TransformComponent Transform { get; init; }

    public RenderRectangle(Technique technique, int width = 0, int height = 0)
    {
        FrameBuffer = FrameBufferManager.CreateFrameBuffer(
            width == 0 ? (int)Engine.Window.ViewportSize.X : width,
            height == 0 ? (int)Engine.Window.ViewportSize.Y : height
        );
        Technique = AddEntity(technique);

        Transform = AddComponent<TransformComponent>();

        Mesh = AddComponent<MeshRendererComponent>();
        Mesh.Load(MeshGenerators.CreateRectangle, new CustomMaterial(technique));
    }

    public RenderRectangle(Shader shader, int width = 0, int height = 0)
    {
        FrameBuffer = FrameBufferManager.CreateFrameBuffer(
            width == 0 ? (int)Engine.Window.ViewportSize.X : width,
            height == 0 ? (int)Engine.Window.ViewportSize.Y : height
        );
        Technique = AddEntity(new Technique(shader));

        Transform = AddComponent<TransformComponent>();

        Mesh = AddComponent<MeshRendererComponent>();
        Mesh.Load(MeshGenerators.CreateRectangle, new CustomMaterial(Technique.Shader));
    }

    public RenderRectangle(Shader shader, FrameBufferObject fbo)
    {
        FrameBuffer = fbo;
        Technique = AddEntity(new Technique(shader));

        Transform = AddComponent<TransformComponent>();

        Mesh = AddComponent<MeshRendererComponent>();
        Mesh.Load(MeshGenerators.CreateRectangle, new CustomMaterial(Technique.Shader));
    }

    public RenderRectangle(Technique technique, FrameBufferObject fbo)
    {
        FrameBuffer = fbo;
        Technique = AddEntity(technique);

        Transform = AddComponent<TransformComponent>();

        Mesh = AddComponent<MeshRendererComponent>();
        Mesh.Load(MeshGenerators.CreateRectangle, new CustomMaterial(Technique.Shader));
    }

    protected const string UNIFORM_ALBEDO = "uAlbedo";
    protected const string UNIFORM_DEPTH = "uDepth";

    public void RenderScene(float dt, ref RenderOptions options)
    {
        Technique.Use();

        if (
            FrameBuffer.Attachments.TryGetValue(
                FramebufferAttachment.ColorAttachment0,
                out uint albedo
            )
        )
        {
            Engine.GL.ActiveTexture(TextureUnit.Texture0);
            Engine.GL.BindTexture(TextureTarget.Texture2D, albedo);
            Technique.SetUniform(UNIFORM_ALBEDO, 0);
        }

        if (
            FrameBuffer.Attachments.TryGetValue(
                FramebufferAttachment.DepthAttachment,
                out uint depth
            )
        )
        {
            Engine.GL.ActiveTexture(TextureUnit.Texture1);
            Engine.GL.BindTexture(TextureTarget.Texture2D, depth);
            Technique.SetUniform(UNIFORM_DEPTH, 1);
        }

        Mesh.Render(dt, ref options);

        Technique.End();
    }

    public override void Render(float dt, ref RenderOptions options) { }

    public override void UpdateState(float dt) { }
}
