using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.GameEntity.Components
{
    public interface IGameComponent
    {
        public Entity Parent { get; internal set; }

        void Initialize();

        void Update(float dt);

        void Draw(RenderOptions? options = null);
    }
}