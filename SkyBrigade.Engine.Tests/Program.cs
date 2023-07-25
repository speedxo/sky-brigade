using System.Numerics;

namespace SkyBrigade.Engine.Tests;

internal class Program
{
    private static void Main(string[] args)
    {
        using var instance = GameManager.Instance.Run(GameInstanceParameters.Default with
        {
            InitialGameScreen = typeof(TestMenuGameScreen),
            WindowTitle = "SkyBridge.Engine.Tests",
            InitialWindowSize = new Vector2(1280, 720)
        });
    }
}