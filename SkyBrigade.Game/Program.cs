using System.Numerics;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;
using Shader = SkyBrigade.Engine.OpenGL.Shader;
using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Game;

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

