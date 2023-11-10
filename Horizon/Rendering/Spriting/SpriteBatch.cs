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
    /// Helper struct to aggregate data related to rendering a series of sprites with a common sprite sheet.
    /// </summary>
    private struct SpriteSheetRenderObject
    {
        public SpriteBatchMesh Mesh;
        public ushort Index;
        public Sprite[] Sprites;

        public SpriteSheetRenderObject(in SpriteBatchMesh mesh)
        {
            this.Mesh = mesh;
            this.Index = 0;
            this.Sprites = new Sprite[1];
        }

        /// <summary>
        /// Adds the specified sprite, performing an additional check to ensure we don't already contain the specified sprite.
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        public void Add(in Sprite sprite)
        {
            // Ensure we don't already contain this sprite.
            if (Sprites.Contains(sprite))
                return;

            // Make sure we conform our new array.
            if (Index > Sprites.Length)
                Array.Resize(ref Sprites, Sprites.Length + 1);

            Sprites[Index++] = sprite;
        }

        public void AddRange(in Sprite[] sprites)
        {
            // Make sure we conform our new array.
            if (Index + sprites.Length > Sprites.Length)
                Array.Resize(ref Sprites, Sprites.Length + sprites.Length);

            for (ushort i = 0; i < sprites.Length; i++)
            {
                if (Sprites.Contains(sprites[i]))
                    continue;

                Sprites[Index + i] = sprites[i];
                Index++;
            }
        }

        public void AddRange(in List<Sprite> sprites)
        {
            // Make sure we conform our new array.
            if (Index + sprites.Count > Sprites.Length)
                Array.Resize(ref Sprites, Sprites.Length + sprites.Count);

            for (ushort i = 0; i < sprites.Count; i++)
            {
                if (Sprites.Contains(sprites[i]))
                    continue;

                Sprites[Index + i] = sprites[i];
                Index++;
            }
        }
    }

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
    private Dictionary<SpriteSheet, SpriteSheetRenderObject> SpritesheetSprites { get; init; }
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
        Engine.Debugger.GeneralDebugger.AddWatch("Sprite Maximum", "SpriteBatch", () => Count);
    }

    /// <summary>
    /// Commits an object to be rendered.
    /// </summary>
    /// <param name="sprite"></param>
    public void Add(Sprite sprite)
    {
        // Ensure this sprite's sprite sheet has a render object associated.
        if (!SpritesheetSprites.ContainsKey(sprite.Spritesheet))
            SpritesheetSprites.Add(
                sprite.Spritesheet,
                new SpriteSheetRenderObject(new(sprite.Spritesheet, Shader))
            );

        // Add the sprite
        SpritesheetSprites[sprite.Spritesheet].Add(sprite);
        Count++;

        // Flag the Vertex array to be rebuilt.
        _requiresVboUpdate = true;
    }

    /// <summary>
    /// Commits an object to be rendered.
    /// </summary>
    /// <param name="sprite"></param>
    public void AddRange(Sprite[] sprites)
    {
        // Sort sprites into groups via their sprite sheet and setup the SpritesheetSprites key/value pair.
        Dictionary<SpriteSheet, List<Sprite>> spriteSpriteSheetPairs = new();
        for (int i = 0; i < sprites.Length; i++)
        {
            spriteSpriteSheetPairs.TryAdd(sprites[i].Spritesheet, new());
            spriteSpriteSheetPairs[sprites[i].Spritesheet].Add(sprites[i]);

            if (!SpritesheetSprites.ContainsKey(sprites[i].Spritesheet))
                SpritesheetSprites.Add(
                    sprites[i].Spritesheet,
                    new SpriteSheetRenderObject(new(sprites[i].Spritesheet, Shader))
                );
        }

        // Ensure the spritebatch is configured to render all the sprite sheets.
        foreach ((var sheet, var storedSprites) in spriteSpriteSheetPairs)
        {
            SpritesheetSprites[sheet].AddRange(storedSprites);
            Count += storedSprites.Count;
        }

        // Flag the Vertex array to be rebuilt.
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

        foreach (var (spritesheet, renderData) in SpritesheetSprites)
            renderData.Mesh.Draw(
                spritesheet,
                Transform.ModelMatrix,
                renderData.Sprites,
                ref options
            );
    }

    /// <summary>
    /// Forces a recalculation of the sprite meshes.
    /// </summary>
    public void UpdateVBO()
    {
        foreach (var (_, renderData) in SpritesheetSprites)
        {
            renderData.Mesh.Load(GenerateSpriteMeshData(renderData.Sprites));
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
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] is null)
                continue;

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
