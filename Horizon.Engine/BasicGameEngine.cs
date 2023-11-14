using Horizon.Content;
using Horizon.Core.Components;
using Horizon.Core.Content;
using Horizon.Core.Primitives;

namespace Horizon.Engine
{
    public abstract class BasicGameEngine : GameEngine
    {
        public BasicGameEngine()
            : base()
        {
            Content = new ContentManager(this);
        }
    }
}
