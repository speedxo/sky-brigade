using System.Numerics;
using Horizon;
using Horizon.Rendering;
using Horizon.Rendering.Spriting;
using ImGuiNET;
using Silk.NET.OpenGL;
using static Horizon.Rendering.Tiling<Game2D.GameScene.TileID, Game2D.GameScene.TileTextureID>;

namespace Game2D;

public class NoiseGenerator
{
    RenderRectangle renderRect;
    Technique technique;

    public uint TextureHandle { get => renderRect.FrameBuffer.Attachments[FramebufferAttachment.ColorAttachment0]; }

    public NoiseGenerator(int width, int height, Vector2 scale)
    {
        renderRect = new RenderRectangle(technique = new Technique(GameManager.Instance.ContentManager.LoadShader("content/noise/noise.vert", "content/noise/noise.frag")), width, height);
        renderRect.FrameBuffer.AddAttachment(FramebufferAttachment.ColorAttachment0);
        System.Diagnostics.Debug.Assert(renderRect.FrameBuffer.ContructFrameBuffer());

        renderRect.FrameBuffer.Bind();
        GameManager.Instance.Gl.Clear(ClearBufferMask.ColorBufferBit);
        renderRect.FrameBuffer.Unbind();

        technique.Use();
        technique.SetUniform("uNoiseScale", scale);

        renderRect.RenderScene(1.0f);

        GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, renderRect.FrameBuffer.Attachments[FramebufferAttachment.ColorAttachment0]);

        // Allocate array for pixel data
        byte[] pixelData = new byte[width * height * 4]; // RGBA format, assuming 8 bits per channel

        unsafe
        {
            fixed (void* data = pixelData)
            {
                // Get the pixel data from the texture
                GameManager.Instance.Gl.GetTexImage(GLEnum.Texture2D, 0, GLEnum.Rgba, GLEnum.UnsignedByte, data);
            }
        }
        GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, 0);
        //renderRect.FrameBuffer.Dispose();

        ProcessData(pixelData, width, height);

        //technique.Dispose();
    }

    public float[,] Data { get; private set; } 

    private void ProcessData(byte[] pixelData, int width, int height)
    {
        Data = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixelIndex = (y * width + x) * 4; // RGBA format
                byte r = pixelData[pixelIndex];

                Data[x, y] = r / 255.0f;
            }
        }
    }
}

public class GameScene : Scene
{
    public enum TileID
    {
        Dirt = 1
    }

    public enum TileTextureID
    {
        Dirt = 1,
        Grass = 2
    }

    Sprite player;
    SpriteBatch spriteBatch;
    Camera cam;
    TileMap tilemap;

    public GameScene()
    {
        InitializeGl();
        AddEntity(tilemap = new());

        var sheet = tilemap.AddTileSet("tileset", new(GameManager.Instance.ContentManager.LoadTexture("content/tileset.png"), new Vector2(16)));
        sheet.RegisterTile(TileTextureID.Grass, new Vector2(16, 0));
        sheet.RegisterTile(TileTextureID.Dirt, new Vector2(16, 16));

        //tilemap.GenerateTiles(GenerateTile);
        tilemap.PopulateTiles(PopulateTiles);
        tilemap.GenerateMeshes();

        var sprSheet1 = AddEntity(new Spritesheet(GameManager.Instance.ContentManager.LoadTexture("content/spritesheet.png"), new Vector2(69, 44)));
        sprSheet1.AddAnimation("idle", new Vector2(0, 0), 6);
        sprSheet1.AddAnimation("run", new Vector2(0, 1), 6);

        spriteBatch = AddEntity(new SpriteBatch());
        spriteBatch.AddSprite(player = new Sprite(sprSheet1, "idle") { IsAnimated = true, Size = new Vector2(0.2f * (69.0f / 44.0f), 0.2f) });
        spriteBatch.UpdateVBO();

        cam = new Camera()
        {
            Position = new Vector3(0, 0, 1)
        };


        InitializeRenderingPipeline();
    }

    private void PopulateTiles(Tile?[,] tiles, TileMapChunk chunk)
    {
        var noise = new NoiseGenerator(TileMap.WIDTH * TileMapChunk.WIDTH, 1, new Vector2(2.0f));

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            int height = (int)((noise.Data[x + chunk.Slice * TileMapChunk.WIDTH, 0]) * ((tiles.GetLength(1) - 1) / 2.0f) + (tiles.GetLength(1) - 1) / 4.0f);

            for (int y = height; y < tiles.GetLength(1); y++)
            {
                tiles[x, y] = new DirtTile(chunk, new Vector2(x, y));
            }
        }
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

        var movementDir = GameManager.Instance.InputManager.GetVirtualController().MovementAxis;

        player.FrameName = Math.Abs(movementDir.X) > 0 ? "run" : "idle";
        player.Flipped = movementDir.X < 0;

        cam.Position += new Vector3(movementDir * dt * cam.Position.Z, 0.0f);
        player.Transform.Position = cam.Position * new Vector3(1, 1, 0);
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

