using Horizon.Core.Components;
using Horizon.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Input.Components;

/// <summary>
/// An abstraction providing an interface to aggregate peripheral data to the <see cref="InputManager"/>'s VirtualController.
/// </summary>
public abstract class PeripheralInputManager
{
    public bool Enabled { get; set; } = true;

    protected static InputManager Manager { get; private set; }
    internal static void SetManager(in InputManager manager) => Manager ??= manager;

    /// <summary>
    /// Called after all engine components are initialized.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Aggregates data from a peripheral and implements it into the VirtualController.
    /// </summary>
    /// <param name="dt"></param>
    public abstract void AggregateData(float dt);

    /// <summary>
    /// Used for swapping state buffers.
    /// </summary>
    public abstract void SwapBuffers();

}
