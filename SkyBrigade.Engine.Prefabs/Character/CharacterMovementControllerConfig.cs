namespace Horizon.Prefabs.Character; 

public partial class CharacterController
{
    public struct CharacterMovementControllerConfig
    {
        public float BaseMovementSpeed { get; set; }
        public float BaseMouseSensitivity { get; set; }

        public static CharacterMovementControllerConfig Default { get; } = new()
        {
            BaseMovementSpeed = 18.0f,
            BaseMouseSensitivity = 0.1f
        };
    }
}