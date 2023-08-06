namespace SkyBrigade.Engine.Input
{
    /// <summary>
    /// The VirtualAction enum represents various virtual actions that can be triggered by input devices.
    /// </summary>
    [Flags]
    public enum VirtualAction
    {
        None = 0,

        MoveForwards = 1,   // W/JoystickUp
        MoveBackwards = 2,  // S/JoystickDown
        MoveLeft = 4,       // A/JoystickLeft
        MoveRight = 8,      // D/JoystickRight

        Interact = 16,       // E/X
        Pause = 32,         // Escape/Menu
        Back = 64,

        PrimaryAction = 128, // LeftClick/LeftTrigger
        SecondaryAction = 256 // RightClick/RightTrigger
    }
}
