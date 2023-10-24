using Horizon;

namespace TileBash;

internal class TestEngine : GameEngine
{
    public TestEngine(GameInstanceParameters parameters) 
    : base(parameters)
    {

    }

    private static void Main(string[] args)
    {
        var assemName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        var version = assemName.Version;

        var engine = new TestEngine(GameInstanceParameters.Default with
             {
                 InitialGameScreen = typeof(GameScene),
                 WindowTitle = $"{assemName.Name} ({version})"
             });
        engine.Run();
        
        // GameManager.Instance.Initialize(
        //     GameInstanceParameters.Default with
        //     {
        //         InitialGameScreen = typeof(GameScene)
        //     }
        // );
        // GameManager.Instance.Run();
    }
}
