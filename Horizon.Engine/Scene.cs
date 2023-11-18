using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Engine;

public abstract class Scene : GameObject
{
    public abstract Camera ActiveCamera { get; protected set; }
}
