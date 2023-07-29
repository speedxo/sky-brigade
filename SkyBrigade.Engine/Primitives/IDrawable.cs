using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Primitives;

public interface IDrawable
{
    public void Draw(RenderOptions? renderOptions = null);
}