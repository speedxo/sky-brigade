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

    }

    public void LoadContent()
    {

    }

    public void Render(GL gl, float dt)
    {

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
        GameInstance.Instance.Run(GameInstanceParameters.Default with {
            InitialGameScreen = typeof(DemoGameScreen),
            WindowTitle = "vrek"
        });
    }
}

