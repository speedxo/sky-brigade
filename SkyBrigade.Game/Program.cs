using System.Numerics;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Rendering;
using Shader = SkyBrigade.Engine.OpenGL.Shader;
using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Game;

class DemoGameScreen : IGameScreen
{
    private RenderRectangle rect;
    private Camera testCamera;


    public void Initialize(GL gl)
    {
        gl.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);

        rect = new() {
            Texture = GameManager.Instance.ContentManager.GenerateNamedTexture("amongus", "Assets/among.png")
        };
        
        testCamera = new Camera() { Position = new System.Numerics.Vector3(0, 0, 5) };
    }

    public void Render(GL gl, float dt)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.Viewport(0, 0, (uint)GameManager.Instance.Window.FramebufferSize.X, (uint)GameManager.Instance.Window.FramebufferSize.Y);

        rect.Draw(testCamera);
    }



    public void Update(float dt)
    {
        testCamera.Update(dt);

        rect.Rotation += dt * 100.0f;
    }

    public void Dispose()
    {

    }

}

class Program
{
    static void Main(string[] args)
    {
        GameManager.Instance.Run(GameInstanceParameters.Default with
        {
            InitialGameScreen = typeof(DemoGameScreen),
            WindowTitle = "vrek",
            InitialWindowSize = new System.Numerics.Vector2(1280, 720)
        });
    }
}

