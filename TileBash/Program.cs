using Horizon;
using Horizon.Engine;

namespace TileBash;

internal class Program
{
    private static void Main(string[] args)
    {
        var assemName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        var version = assemName.Version;

        var engine = new GameEngine(
            GameEngineConfiguration.Default with
            {
                InitialScene = typeof(GameScene)
            }
        );

        engine.Run();
    }
}
