using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2D.NetStandard.Dynamics.World;
using Horizon.Engine;
using Horizon.Rendering.Techniques;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace AutoVoxel.World;

public class GameWorld : GameObject
{
    public ChunkManager ChunkManager { get; }
    public ChunkRenderer ChunkRenderer { get; }

    public GameWorld()
    {
        ChunkManager = AddComponent<ChunkManager>();
        ChunkRenderer = AddComponent<ChunkRenderer>(new(ChunkManager));
    }

    public override void Initialize()
    {
        base.Initialize();
    }
}
