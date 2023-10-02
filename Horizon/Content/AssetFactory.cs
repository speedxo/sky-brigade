namespace Horizon.Content
{
    /// <summary>
    /// An abstract parent class that function soley to provide a GameEngine instance to children factories.
    /// </summary>
    public abstract class AssetFactory
    {
        protected static GameEngine Engine { get; private set; }

        public static void SetGameEngine(in GameEngine engine) => Engine = engine;
    }
}
