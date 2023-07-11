using System.Numerics;

namespace SkyBrigade.Engine
{
    public struct GameInstanceParameters
    {
        public string WindowTitle { get; set; }
        public Vector2 InitialWindowSize { get; set; }
        public Type InitialGameScreen { get; set; }

        public static GameInstanceParameters Default { get; } = new GameInstanceParameters
        {
            WindowTitle = "SkyBrigade Engine Test",
            InitialWindowSize = new Vector2(1280, 720)
        };
    }
}