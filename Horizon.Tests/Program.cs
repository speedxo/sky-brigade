using System.Numerics;

namespace Horizon.Tests;

internal class Program
{
    private static void Main(string[] args)
    {
        GameManager.Instance.Initialize(
            GameInstanceParameters.Default with
            {
                InitialGameScreen = typeof(TestMenuGameScreen),
                WindowTitle = "SkyBridge.Engine.Tests",
                InitialWindowSize = new Vector2(1280, 720)
            }
        );

        GameManager.Instance.Run();
    }
}
