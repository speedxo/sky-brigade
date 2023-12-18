using AutoVoxel.Data;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.Rendering;

namespace AutoVoxel.Rendering;

public class ChunkRenderer : IGameComponent
{
    private const string UNIFORM_ALBEDO = "uTexAlbedo";
    private const string UNIFORM_NORMAL = "uTexNormal";
    private const string UNIFORM_SPECULAR = "uTexSpecular";
    private readonly ChunkManager manager;

    public Material Material { get; set; }
    public Technique Technique { get; set; }

    public bool Enabled { get; set; }
    public string Name { get; set; }
    public Entity Parent { get; set; }

    public ChunkRenderer(in ChunkManager manager)
    {
        this.manager = manager;
    }

    public void Initialize()
    {
        Material = MaterialFactory.Create("content/atlas", "atlas");
        Technique = new ChunkTechnique();

        // enable depth testing and culling
        GameEngine.Instance.GL.Enable(Silk.NET.OpenGL.EnableCap.DepthTest);
        GameEngine.Instance.GL.Enable(Silk.NET.OpenGL.EnableCap.CullFace);
        GameEngine.Instance.GL.Enable(Silk.NET.OpenGL.EnableCap.Blend);
    }

    public void Render(float dt, object? obj = null)
    {
        // make sure to clear the color and depth buffers
        GameEngine.Instance.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.DepthBufferBit | Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit);

        Technique.Bind();
        BindMaterialAttachments();

        // todo: indirect drawing??
        GameEngine.Instance.GL.Enable(Silk.NET.OpenGL.EnableCap.CullFace);
        GameEngine.Instance.GL.BlendFunc(Silk.NET.OpenGL.BlendingFactor.SrcAlpha, Silk.NET.OpenGL.BlendingFactor.OneMinusSrcAlpha);
        foreach (var chunk in manager.Chunks)
        {
            Technique.SetUniform("uChunkPosition", chunk.Position);
            chunk.Render(dt);
        }
        GameEngine.Instance.GL.Disable(Silk.NET.OpenGL.EnableCap.CullFace);
        //GameEngine.Instance.GL.BlendFunc(Silk.NET.OpenGL.BlendingFactor.SrcAlpha, Silk.NET.OpenGL.BlendingFactor.One);
        foreach (var chunk in manager.Chunks)
        {
            Technique.SetUniform("uChunkPosition", chunk.Position);
            chunk.RenderFolliage(dt);
        }

        Technique.Unbind();
    }

    public void UpdateState(float dt) { }

    public void UpdatePhysics(float dt) { }

    protected void BindMaterialAttachments()
    {
        Material.BindAttachment(MaterialAttachment.Albedo, 0);
        Technique.SetUniform(UNIFORM_ALBEDO, 0);

        Material.BindAttachment(MaterialAttachment.Normal, 1);
        Technique.SetUniform(UNIFORM_NORMAL, 1);

        Material.BindAttachment(MaterialAttachment.Specular, 2);
        Technique.SetUniform(UNIFORM_SPECULAR, 2);
    }
}
