using System.Runtime.InteropServices;
using Horizon.Content;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering.Spriting.Components;
using Horizon.Rendering.Spriting.Data;

namespace Horizon.Rendering.Spriting;

public class SpriteBatch : Entity
{
    public TransformComponent Transform { get; init; }
    public Shader Shader { get; init; }
    public Dictionary<
        Spritesheet,
        (List<Sprite> sprites, SpriteBatchMesh mesh)
    > SpritesheetSprites { get; init; }

    private bool _requiresVboUpdate = false;
    public int Count { get; private set;}

    public SpriteBatch(Shader? shader = null)
    {
        this.Shader = Engine.Content.Shaders.AddNamed("sprite", ShaderFactory.CompileNamed("Assets/sprite_shaders/", "sprites"));
        
        this.SpritesheetSprites = new();

        this.Transform = AddComponent<TransformComponent>();
        Engine.Debugger.GeneralDebugger.AddWatch("Sprite Count", "SpriteBatch", () => Count);
    }

    public void AddSprite(Sprite sprite)
    {
        if (!SpritesheetSprites.ContainsKey(sprite.Spritesheet))
            SpritesheetSprites.Add(sprite.Spritesheet, (new(), new(Shader)));

        if (SpritesheetSprites[sprite.Spritesheet].sprites.Count + 1 >= SpriteBatchMesh.MAX_SPRITES)
            Engine.Logger.Log(
                Logging.LogLevel.Fatal,
                $"You are attempting to add more than {SpriteBatchMesh.MAX_SPRITES} sprites, which is the limit. Please create another sprite batch."
            );

        SpritesheetSprites[sprite.Spritesheet].sprites.Add(sprite);
        _requiresVboUpdate = true;
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        if (_requiresVboUpdate)
            UpdateVBO();

        var options = (renderOptions ?? RenderOptions.Default);

        base.Draw(dt, options);

        foreach (var (spritesheet, (sprites, mesh)) in SpritesheetSprites)
            mesh.Draw(spritesheet, Transform.ModelMatrix, sprites, options);
    }

    public void UpdateVBO()
    {
        Count = 0;
        foreach (var (_, (sprites, mesh)) in SpritesheetSprites)
        {
            GenerateAndUploadSpriteMesh(CollectionsMarshal.AsSpan(sprites), mesh);
            Count += sprites.Count;
        }

        _requiresVboUpdate = false;
    }

    private static void GenerateAndUploadSpriteMesh(
        ReadOnlySpan<Sprite> sprites,
        SpriteBatchMesh mesh
    )
    {
        List<Vertex2D> vertices = new();
        List<uint> elements = new();
        uint vertexCounter = 0;

        uint[] getElements()
        {
            return new uint[]
            {
                vertexCounter,
                vertexCounter + 1,
                vertexCounter + 2,
                vertexCounter,
                vertexCounter + 2,
                vertexCounter + 3
            };
        }
        sprites[0].Spritesheet.ResetSpriteCounter();

        foreach (var sprite in sprites)
        {
            vertices.AddRange(sprite.GetVertices());
            elements.AddRange(getElements());
            vertexCounter += 4;
        }

        mesh.Upload(CollectionsMarshal.AsSpan(vertices), CollectionsMarshal.AsSpan(elements));
        vertices.Clear();
        elements.Clear();
    }
}
