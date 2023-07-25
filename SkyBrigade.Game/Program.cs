using SkyBrigade.Engine;

namespace SkyBrigade.Game;

internal class Program
{
    private static void Main(string[] args)
    {
        var instance = GameManager.Instance.Run(GameInstanceParameters.Default with
        {
            InitialGameScreen = typeof(DemoGameScreen),
            WindowTitle = "vrek",
            InitialWindowSize = new System.Numerics.Vector2(1280, 720)
        });
        instance.Dispose();
    }
}