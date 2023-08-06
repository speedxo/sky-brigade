using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Primitives;
using SkyBrigade.Engine.Rendering;
using System;
using System.Collections.Generic;

namespace SkyBrigade.Engine;

/// <summary>
/// Represents a generalised interface for a game screen.
/// </summary>
public interface IGameScreen : IDisposable, IUpdateable, IDrawable
{
    /// <summary>
    /// The list of entities present in the game screen.
    /// </summary>  
    List<IEntity> Entities { get; set; }
}
