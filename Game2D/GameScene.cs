using System.Numerics;
using Horizon;
using Horizon.Rendering;
using Horizon.Rendering.Spriting;
using Horizon.Rendering.Tilemap;
using Silk.NET.OpenGL;

namespace Game2D;

public class GameScene : Scene
{
    Sprite flame;
    SpriteBatch spriteBatch;
    Camera cam;

    Tilemap tilemap;

    public GameScene()
    {
        InitializeGl();

        var sprSheet1 = AddEntity(new Spritesheet(GameManager.Instance.ContentManager.LoadTexture("content/burning_loop_2.png"), new Vector2(24, 32)));
        sprSheet1.AddAnimation("flame", Vector2.Zero, 8);

        spriteBatch = AddEntity(new SpriteBatch());
        spriteBatch.AddSprite(flame = new Sprite(sprSheet1, "flame") { IsAnimated = true, Size = new Vector2(0.1f) });
        spriteBatch.UpdateVBO();

        cam = new Camera()
        {
            Position = new Vector3(0, 0, 1)
        };

        AddEntity(tilemap = new Tilemap());

        var sheet = tilemap.AddSpritesheet("tileset", new Spritesheet(GameManager.Instance.ContentManager.LoadTexture("content/tileset.png"), new Vector2(16)));
        sheet.AddSprite("grass", new Vector2(16, 0));
        sheet.AddSprite("dirt", new Vector2(16, 16));

        tilemap.GenerateTiles(GenerateTile);
        tilemap.GenerateMeshes();

        InitializeRenderingPipeline();
    }

    public Tile GenerateTile(Vector2 local, Vector2 global)
    {
        return new DirtTile(tilemap, local, global);
    }

    private static void InitializeGl()
    {
        GameManager.Instance.Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        GameManager.Instance.Gl.Enable(EnableCap.Texture2D);
        GameManager.Instance.Gl.Enable(EnableCap.Blend);
        GameManager.Instance.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Silk.NET.Input.Key.Q))
        {
            cam.Position += new Vector3(0, 0, dt);
        }
        if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Silk.NET.Input.Key.E))
        {
            cam.Position += new Vector3(0, 0, -dt);
        }
        

        cam.Position += new Vector3(GameManager.Instance.InputManager.GetVirtualController().MovementAxis * dt * 2.0f * cam.Position.Z, 0.0f);
        flame.Transform.Position = cam.Position * new Vector3(1, 1, 0);
        cam.Update(dt);
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        var options = (renderOptions ?? RenderOptions.Default) with
        {
            Camera = cam
        };

        base.Draw(dt, options);
    }

    public override void DrawOther(float dt, RenderOptions? renderOptions = null)
    {

    }
    public override void Dispose()
    {

    }

    public override void DrawGui(float dt)
    {
        
    }

}

