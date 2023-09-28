using Horizon;

namespace Game2D;

internal class TestEngine : GameEngine
{
    public TestEngine(GameInstanceParameters parameters) 
    : base(parameters)
    {

    }

    private static void Main(string[] args)
    {
        using var engine = new TestEngine(GameInstanceParameters.Default with
             {
                 InitialGameScreen = typeof(GameScene)
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
