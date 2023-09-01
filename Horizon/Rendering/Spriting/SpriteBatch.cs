using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering.Effects.Components;
using Horizon.Rendering.Spriting.Components;
using Horizon.Rendering.Spriting.Data;

namespace Horizon.Rendering.Spriting;

public class SpriteBatch : Entity
{
    private static int count = 0;
    public TransformComponent Transform { get; init; }

    public List<Sprite> Sprites { get; init; }
    public ShaderComponent Shader { get; init; }
    public Dictionary<Spritesheet, SpriteBatchMesh> Spritesheets { get; init; }

    public SpriteBatch(ShaderComponent? shader = null)
    {
        this.Sprites = new List<Sprite>();
        this.Shader = shader ?? new ShaderComponent("Assets/sprite_shaders/sprites.vert", "Assets/sprite_shaders/sprites.frag");
        this.Spritesheets = new();

        this.Transform = AddComponent<TransformComponent>();

        GameManager.Instance.Debugger.GeneralDebugger.AddWatch($"Spritebatch({count++}) Metrics", "Total Sprites", () => Sprites.Count + $"/{SpriteBatchMesh.MAX_SPRITES}");
    }

    public void AddSprite(Sprite sprite)
    {
        if (Sprites.Count + 1 >= SpriteBatchMesh.MAX_SPRITES)
        {
            GameManager.Instance.Logger.Log(Logging.LogLevel.Fatal, $"You are attempting to add more than {SpriteBatchMesh.MAX_SPRITES} sprites, which is the limit. Please create another sprite batch.");
        }

        if (!Spritesheets.TryGetValue(sprite.Spritesheet, out var spriteBatchMesh))
        {
            spriteBatchMesh = new SpriteBatchMesh(Shader);
            Spritesheets.Add(sprite.Spritesheet, spriteBatchMesh);
        }

        Sprites.Add(sprite);
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        var options = (renderOptions ?? RenderOptions.Default);

        base.Draw(dt, options);
        int counter = 0;
        foreach (var (sheet, mesh) in Spritesheets)
        {
            IEnumerable<Sprite> sprites()
            {
                foreach (Sprite sprite in Sprites)
                {
                    if (sprite.Spritesheet == sheet)
                    {
                        counter++;
                        yield return sprite;
                    }
                }
            }

            mesh.Draw(sheet, Transform.ModelMatrix, sprites(), renderOptions);
        }
        System.Diagnostics.Debug.Assert(counter == Sprites.Count);
    }

    public void UpdateVBO()
    {
        foreach (var (spriteSheet, mesh) in Spritesheets)
        {
            var sprites = (from Sprite sprite in Sprites
                           where sprite.Spritesheet == spriteSheet
                           select sprite);

            GenerateAndUploadSpriteMesh(sprites, mesh);
        }
    }

    private static void GenerateAndUploadSpriteMesh(IEnumerable<Sprite> sprites, SpriteBatchMesh mesh)
    {
        List<Vertex2D> vertices = new();
        List<uint> elements = new();
        uint vertexCounter = 0;

        uint[] getElements()
        {
            return new uint[] {
                vertexCounter, vertexCounter + 1, vertexCounter + 2,
                vertexCounter, vertexCounter + 2, vertexCounter + 3
            };
        }
        sprites.First().Spritesheet.ResetSpriteCounter();

        foreach (var sprite in sprites)
        {
            vertices.AddRange(sprite.GetVertices());
            elements.AddRange(getElements());
            vertexCounter += 4;
        }

        mesh.Upload(vertices.ToArray(), elements.ToArray());
    }
}