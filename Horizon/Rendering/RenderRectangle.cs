using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Silk.NET.OpenGL;
using Shader = Horizon.OpenGL.Shader;

namespace Horizon.Rendering;

public class RenderTarget : Entity
{
    public MeshRendererComponent Mesh { get; init; }
    public Technique Technique { get; set; }

    protected TransformComponent Transform { get; init; }

    public RenderTarget(Technique technique)
    {
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

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        Engine.GL.Viewport(
            0,
            0,
            (uint)Engine.Window.ViewportSize.X,
            (uint)Engine.Window.ViewportSize.Y
        );
        Technique.Use();

        Mesh.Draw(dt);

        Technique.End();
    }

    public override void Update(float dt) { }
}

public class RenderRectangle : Entity
{
    public MeshRendererComponent Mesh { get; init; }
    public Technique Technique { get; set; }
    public FrameBufferObject FrameBuffer { get; init; }

    protected TransformComponent Transform { get; init; }

    public RenderRectangle(Technique technique, int width = 0, int height = 0)
    {
        FrameBuffer = new FrameBufferObject(
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
        FrameBuffer = new FrameBufferObject(
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

    public void RenderScene(float dt)
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
            Technique.SetUniform("uAlbedo", 0);
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
            Technique.SetUniform("uDepth", 1);
        }

        Mesh.Use(RenderOptions.Default);
        Mesh.Draw(dt);

        Technique.End();
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null) { }

    public override void Update(float dt) { }
}
