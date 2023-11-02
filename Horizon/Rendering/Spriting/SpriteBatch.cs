using Horizon.Content;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering.Spriting.Components;
using Horizon.Rendering.Spriting.Data;
using System.Runtime.InteropServices;

namespace Horizon.Rendering.Spriting;

/// <summary>
/// An alternative (high performance) rendering back end for rendering a collection of dynamic sprites.
/// </summary>
/// <seealso cref="Horizon.GameEntity.Entity" />
/// <seealso cref="Horizon.Rendering.Spriting.I2DBatchedRenderer&lt;Horizon.Rendering.Spriting.Sprite&gt;" />
public class SpriteBatch : Entity, I2DBatchedRenderer<Sprite>
{
    /// <summary>
    /// The global transform for all sprite meshes.
    /// </summary>
    public TransformComponent Transform { get; init; }

    /// <summary>
    /// Gets the shader.
    /// </summary>
    public Shader Shader { get; init; }

    /// <summary>
    /// TODO please remind me to make a custom datastruct for this shit
    /// </summary>
    /// <value>
    public Dictionary<
        SpriteSheet,
        (List<Sprite> sprites, SpriteBatchMesh mesh)
    > SpritesheetSprites { get; init; }

    private bool _requiresVboUpdate = false;
    public int Count { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBatch"/> class.
    /// </summary>
    /// <param name="shader">A custom shader used to render sprites. It is recommended to leave default and apply effects using the post processing pipeline.</param>
    public SpriteBatch(Shader? shader = null)
    {
        this.Shader = Engine.Content.Shaders.AddNamed(
            "sprite",
            ShaderFactory.CompileNamed("Assets/sprite_shaders/", "sprites")
        );

        this.SpritesheetSprites = new();

        this.Transform = AddComponent<TransformComponent>();
        Engine.Debugger.GeneralDebugger.AddWatch("Sprite Count", "SpriteBatch", () => Count);
    }

    /// <summary>
    /// Commits an object to be rendered.
    /// </summary>
    /// <param name="sprite"></param>
    public void Add(Sprite sprite)
    {
        if (!SpritesheetSprites.ContainsKey(sprite.Spritesheet))
            SpritesheetSprites.Add(sprite.Spritesheet, (new(), new(sprite.Spritesheet, Shader)));

        if (SpritesheetSprites[sprite.Spritesheet].sprites.Count + 1 >= SpriteBatchMesh.MAX_SPRITES)
            Engine.Logger.Log(
                Logging.LogLevel.Fatal,
                $"You are attempting to add more than {SpriteBatchMesh.MAX_SPRITES} sprites, which is the limit. Please create another sprite batch."
            );

        SpritesheetSprites[sprite.Spritesheet].sprites.Add(sprite);
        _requiresVboUpdate = true;
    }

    /// <summary>
    /// Draws all the sprites commited to this instance.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    /// <param name="options">Render options (optional).</param>
    public override void Draw(float dt, ref RenderOptions options)
    {
        if (_requiresVboUpdate)
            UpdateVBO();

        base.Draw(dt, ref options);

        foreach (var (spritesheet, (sprites, mesh)) in SpritesheetSprites)
            mesh.Draw(spritesheet, Transform.ModelMatrix, sprites, ref options);
    }

    /// <summary>
    /// Forces a recalculation of the sprite meshes.
    /// </summary>
    public void UpdateVBO()
    {
        Count = 0;
        foreach (var (_, (sprites, mesh)) in SpritesheetSprites)
        {
            mesh.Load(GenerateSpriteMeshData(CollectionsMarshal.AsSpan(sprites)));
            Count += sprites.Count;
        }

        _requiresVboUpdate = false;
    }

    private IMeshData<Vertex2D> GenerateSpriteMeshData(Span<Sprite> sprites)
    {
        Vertex2D[] vertices = new Vertex2D[sprites.Length * 4];
        uint[] elements = new uint[sprites.Length * 6];
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

        for (int i = 0; i < sprites.Length; i++)
        {
            var spriteVertices = sprites[i].GetVertices();
            for (int j = 0; j < spriteVertices.Length; j++)
                vertices[i + j] = spriteVertices[j];
            var spritesElements = getElements();
            for (int j = 0; j < spritesElements.Length; j++)
                elements[i + j] = spritesElements[j];

            vertexCounter += 4;
        }

        return new MeshData2D { Vertices = vertices, Elements = elements };
    }

    public void Remove(Sprite input) { }
}
