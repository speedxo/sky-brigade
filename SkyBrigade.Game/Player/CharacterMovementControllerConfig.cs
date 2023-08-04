namespace SkyBrigade.Game.Player
{
    public partial class CharacterController
    {
        public struct CharacterMovementControllerConfig
        {
            public float BaseMovementSpeed { get; set; }

            public static CharacterMovementControllerConfig Default { get; } = new()
            {
                BaseMovementSpeed = 18.0f
            };
        }
    }
}