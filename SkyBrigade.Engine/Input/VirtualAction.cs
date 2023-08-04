namespace SkyBrigade.Engine.Input;

[Flags]
public enum VirtualAction
{
    MoveForwards = 0, // W/JoystickUp
    MoveBackwards = 1, //S/JoystickDown
    MoveLeft = 2, // A/JoystickLeft
    MoveRight = 4, // D/JoystickRight

    Interact = 8, // E/X
    Pause = 16, // Escape/Menu
    Back = 32,

    PrimaryAction = 64, // LeftClick/LeftTrigger
    SecondaryAction = 128 // RightClick/RightTrigger
}

