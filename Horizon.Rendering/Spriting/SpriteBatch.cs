using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Horizon.Content;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering.Spriting.Components;
using Horizon.Rendering.Spriting.Data;

namespace Horizon.Rendering.Spriting;

/// <summary>
/// An alternative (high performance) rendering back end for rendering a collection of dynamic sprites.
/// </summary>
/// <seealso cref="Horizon.GameEntity.Entity" />
/// <seealso cref="Horizon.Rendering.Spriting.I2DBatchedRenderer&lt;Horizon.Rendering.Spriting.Sprite&gt;" />
public class SpriteBatch : GameObject
{
    /// <summary>
    /// Helper struct to aggregate data related to rendering a series of sprites with a common sprite sheet.
    /// </summary>
    private struct SpriteSheetRenderObject
    {
        public SpriteBatchMesh Mesh;

        //public ushort Index;
        public List<Sprite> Sprites;

        public SpriteSheetRenderObject(in SpriteBatchMesh mesh)
        {
            this.Mesh = mesh;
            //this.Index = 0;
            this.Sprites = new();
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
            //if (Index > Sprites.Count)
            //    Array.Resize(ref Sprites, Sprites.Length + 1);

            Sprites.Add(sprite);
        }

        public void AddRange(in Sprite[] sprites)
        {
            Sprites.AddRange(sprites);
            //// Make sure we conform our new array.
            //if (Index + sprites.Length > Sprites.Length)
            //    Array.Resize(ref Sprites, Sprites.Length + sprites.Length);

            //for (ushort i = 0; i < sprites.Length; i++)
            //{
            //    if (Sprites.Contains(sprites[i]))
            //        continue;

            //    Sprites[Index + i] = sprites[i];
            //    Index++;
            //}
        }

        public void AddRange(in List<Sprite> sprites)
        {
            Sprites.AddRange(sprites);
            //// Make sure we conform our new array.
            //if (Index + sprites.Count > Sprites.Length)
            //    Array.Resize(ref Sprites, Sprites.Length + sprites.Count);

            //int counter = 0;
            //for (int i = 0; i < Sprites.Length; i++)
            //{
            //    if (Sprites[i] is null && sprites.Count > 0)
            //    {
            //        Sprites[i] = sprites[counter];
            //        sprites.RemoveAt(counter);
            //        counter++;
            //    }
            //}

            //counter = 0;
            //foreach (var sprite in sprites)
            //{
            //    if (Sprites.Contains(sprite))
            //        continue;
            //}

            //for (ushort i = 0; i < sprites.Count; i++)
            //{
            //    if (Sprites.Contains(sprites[i]))
            //        continue;

            //    Sprites[Index + i] = sprites[i];
            //    Index++;
            //}
        }
    }

    /// <summary>
    /// The global transform for all sprite meshes.
    /// </summary>
    //public TransformComponent Transform { get; init; }

    /// <summary>
    /// Gets the shader.
    /// </summary>
    public Technique Shader { get; init; }

    /// <summary>
    /// TODO please remind me to make a custom datastruct for this shit
    /// </summary>
    /// <value>
    private ConcurrentDictionary<uint, SpriteSheetRenderObject> SpritesheetSprites { get; } = new();
    public int Count { get; private set; }

    private ConcurrentStack<Sprite> _queuedSprites = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBatch"/> class.
    /// </summary>
    /// <param name="shader">A custom shader used to render sprites. It is recommended to leave default and apply effects using the post processing pipeline.</param>
    public SpriteBatch()
    {
        this.Shader = new Technique(Engine
            .Content
            .Shaders
            .Create("sprite", ShaderDescription.FromPath("shaders/spritebatch", "sprites")).Asset);

        // this.Transform = AddComponent<TransformComponent>();
        //Engine.Debugger.GeneralDebugger.AddWatch("Sprite Count", "SpriteBatch", () => Count);
        //Engine
        //    .Debugger
        //    .GeneralDebugger
        //    .AddWatch("Mesh Count", "SpriteBatch", () => SpritesheetSprites.Count);
    }

    /// <summary>
    /// Commits an object to be rendered.
    /// </summary>
    /// <param name="sprite"></param>
    public void Add(Sprite sprite) => _queuedSprites.Push(sprite);

    /// <summary>
    /// Commits an object to be rendered.
    /// </summary>
    /// <param name="sprite"></param>
    public void AddRange(Sprite[] sprites) => _queuedSprites.PushRange(sprites);

    /// <summary>
    /// Draws all the sprites commited to this instance.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    /// <param name="options">Render options (optional).</param>
    public override void Render(float dt)
    {
        base.Render(dt);

        if (!Enabled)
            return;

        if (_queuedSprites.Any())
        {
            int length = _queuedSprites.Count;
            Sprite[] sprites = new Sprite[length];
            if (_queuedSprites.TryPopRange(sprites) == length)
            {
                // Sort sprites into groups via their sprite sheet and setup the SpritesheetSprites key/value pair.
                Dictionary<uint, List<Sprite>> spriteSpriteSheetPairs = new();
                for (int i = 0; i < sprites.Length; i++)
                {
                    if (sprites[i].AnimationManager is null)
                    {
                        // Sprite not yet initialized
                        _queuedSprites.Push(sprites[i]);
                    }
                    else
                    {
                        spriteSpriteSheetPairs.TryAdd(sprites[i].Spritesheet.Handle, new());
                        spriteSpriteSheetPairs[sprites[i].Spritesheet.Handle].Add(sprites[i]);

                        if (!SpritesheetSprites.ContainsKey(sprites[i].Spritesheet.Handle))
                        {
                            SpritesheetSprites.TryAdd(
                                sprites[i].Spritesheet.Handle,
                                new SpriteSheetRenderObject(new(sprites[i].Spritesheet, Shader))
                            );
                        }
                    }
                }

                // Ensure the spritebatch is configured to render all the sprite sheets.
                foreach ((var sheet, var storedSprites) in spriteSpriteSheetPairs)
                {
                    SpritesheetSprites[sheet].AddRange(storedSprites);
                    Count += storedSprites.Count;
                }
            }
        }
        foreach (var (_, renderData) in SpritesheetSprites)
            renderData
                .Mesh
                .Draw( /*Transform.ModelMatrix, */
                    CollectionsMarshal.AsSpan(renderData.Sprites),
                    Camera2D.Instance.ProjView
                );
    }
}
