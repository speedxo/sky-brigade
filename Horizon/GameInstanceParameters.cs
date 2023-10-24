using System.Numerics;

namespace Horizon
{
    public struct GameInstanceParameters
    {
        public string WindowTitle { get; set; }
        public Vector2 InitialWindowSize { get; set; }
        public Type InitialGameScreen { get; set; }

        public static GameInstanceParameters Default { get; } =
            new GameInstanceParameters
            {
                WindowTitle = "Horizon Game",
                InitialWindowSize = new Vector2(1280, 720)
            };
    }
}
