namespace SkyBrigade.Engine.Input;

/* The idea here is to allow for a serialisable and customisable way to 
	 * hotswap multiple binding profiles.
	 */
public struct JoystickBindings
{
    public Dictionary<JoystickButton, VirtualAction> ButtonActionPairs { get; set; }

    public static JoystickBindings Default { get; } = new()
    {
        ButtonActionPairs = new() {
            { JoystickButton.Y, VirtualAction.Back },
            { JoystickButton.X, VirtualAction.Interact },
            { JoystickButton.Start, VirtualAction.Pause }
        }
    };
}

