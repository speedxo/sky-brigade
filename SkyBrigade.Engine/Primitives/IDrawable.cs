using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Primitives;

public interface IDrawable
{
    public void Draw(float dt, RenderOptions? renderOptions = null);
}