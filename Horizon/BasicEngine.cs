using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon;

/// <summary>
/// A basic implementation of the <see cref="GameEngine"/> implementing no addition functionality.
/// </summary>
public class BasicEngine : GameEngine
{
    public BasicEngine(GameInstanceParameters parameters) : base(parameters)
    {
    }
}
