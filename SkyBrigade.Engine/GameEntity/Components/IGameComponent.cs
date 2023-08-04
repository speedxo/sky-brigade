using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.GameEntity.Components
{
    public interface IGameComponent
    {
        public Entity Parent { get; set; }

        void Initialize();

        void Update(float dt);

        void Draw(float dt, RenderOptions? options = null);
    }
}