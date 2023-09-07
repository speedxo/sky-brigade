using Horizon;

namespace SkyBrigade;

internal class Program
{
    private static void Main(string[] args)
    {
        _ = GameManager.Instance.Initialize(
            GameInstanceParameters.Default with
            {
                InitialGameScreen = typeof(DemoGameScreen),
                WindowTitle = "vrek",
                InitialWindowSize = new System.Numerics.Vector2(1280, 720)
            }
        );
        GameManager.Instance.Run();
    }
}
