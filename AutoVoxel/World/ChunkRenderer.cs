﻿using AutoVoxel.Rendering;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.OpenGL;
using Horizon.Rendering;

namespace AutoVoxel.World;

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
    }

    public void Render(float dt, object? obj = null)
    {
        Technique.Bind();
        BindMaterialAttachments();

        foreach (var chunk in manager.Chunks)
        {
            Technique.SetUniform("uChunkPosition", chunk.Position);
            chunk.Render(dt);
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
