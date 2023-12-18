namespace Horizon.Input
{
    /// <summary>
    /// The VirtualAction enum represents various virtual actions that can be triggered by input devices.
    /// </summary>
    [Flags]
    public enum VirtualAction
    {
        None = 0,

        MoveForwards = 1, // W/JoystickUp
        MoveBackwards = 2, // S/JoystickDown
        MoveLeft = 4, // A/JoystickLeft
        MoveRight = 8, // D/JoystickRight
        MoveJump = 16, // Space/JoystickX

        Interact = 32, // E/X
        Pause = 64, // Escape/Menu
        Back = 128,

        PrimaryAction = 256, // LeftClick/LeftTrigger
        SecondaryAction = 512 // RightClick/RightTrigger
    }
}
