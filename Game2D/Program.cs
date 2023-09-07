using Horizon;

namespace Game2D;

internal class Program
{
    private static void Main(string[] args)
    {
        GameManager.Instance.Initialize(
            GameInstanceParameters.Default with
            {
                InitialGameScreen = typeof(GameScene)
            }
        );
        GameManager.Instance.Run();
    }
}
