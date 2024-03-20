using Horizon;
using Horizon.Engine;

namespace VoxelExplorer
{
    class Program
    {
        public static void Main()
        {
            var assemName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            var version = assemName.Version;

            var engine = new GameEngine(
                GameEngineConfiguration.Default with
                {
                    InitialScene = typeof(GameScene),
                    WindowConfiguration = Horizon.Core.WindowManagerConfiguration.Default1600x900 with
                    {
                        WindowTitle = $"{assemName.Name} ({version})"
                    }
                }
            );
            engine.Run();
        }
    }
}