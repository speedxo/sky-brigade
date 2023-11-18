using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Core.Primitives;
using Silk.NET.Core.Native;

namespace Horizon.Engine;

public abstract class GameObject : Entity
{
    public static GameEngine Engine { get; internal set; }

    //public override void Initialize()
    //{
    //    if (Engine is null && Parent is GameEngine engine)
    //        Engine = engine;

    //    base.Initialize();
    //}
}
