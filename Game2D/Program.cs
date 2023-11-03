using Horizon;

namespace TileBash;

internal class Program
{
    private static void Main(string[] args)
    {
        var assemName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        var version = assemName.Version;

        var engine = new BasicEngine(
            GameInstanceParameters.Default with
            {
                InitialGameScreen = typeof(GameScene),
                WindowTitle = $"{assemName.Name} ({version})"
            }
        );
        engine.Run();
    }
}
