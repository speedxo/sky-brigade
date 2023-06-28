using Silk.NET.OpenGL;
using SkyBrigade.Engine;

namespace SkyBrigade.Game;

class DemoGameScreen : IGameScreen
{
    public void Dispose()
    {

    }

    public void Initialize(GL gl)
    {
        gl.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);
    }

    public void LoadContent()
    {

    }

    public void Render(GL gl, float dt)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void UnloadContent()
    {

    }

    public void Update(float dt)
    {

    }
}

class Program
{
    static void Main(string[] args)
    {
        GameManager.Instance.Run(GameInstanceParameters.Default with {
            InitialGameScreen = typeof(DemoGameScreen),
            WindowTitle = "vrek"
        });
    }
}

