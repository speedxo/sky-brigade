using System.Numerics;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;
using Shader = SkyBrigade.Engine.OpenGL.Shader;
using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Engine.Tests;

class Program
{
    static void Main(string[] args)
    {
        GameManager.Instance.Run(GameInstanceParameters.Default with
        {
            InitialGameScreen = typeof(TestMenuGameScreen),
            WindowTitle = "SkyBridge.Engine.Tests",
            InitialWindowSize = new Vector2(1280, 720)
        });

    }
}

