using System.Xml.Linq;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Primitives;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine;

/// <summary>
/// Abstract base class for a game screen, based off <see cref="Entity"/>.
/// <seealso cref="IEntity"/>
/// </summary>
public abstract class Scene : Entity, IDisposable
{
    /// <summary>
    /// Disposes of the scene and its resources.
    /// </summary>
    public abstract void Dispose();
}
