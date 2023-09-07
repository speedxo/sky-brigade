namespace Gravitator2D;

public class Program
{
    public static void Main(string[] _)
    {
        Horizon.GameManager.Instance.Initialize(
            Horizon.GameInstanceParameters.Default with
            {
                InitialGameScreen = typeof(SimulatorScene),
                WindowTitle = "bodies"
            }
        );
        Horizon.GameManager.Instance.Run();
    }
}
