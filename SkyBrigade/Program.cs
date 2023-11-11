using Horizon;

namespace SkyBrigade;

internal class Program
{
    private static void Main(string[] args)
    {
        using var engine = new BasicEngine(
            GameInstanceParameters.Default with
            {
                InitialGameScreen = typeof(DemoGameScreen)
            }
        );

        engine.Run();
    }
}
