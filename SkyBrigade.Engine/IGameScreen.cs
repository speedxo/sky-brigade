using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Primitives;
using SkyBrigade.Engine.Rendering;
using System;
using System.Collections.Generic;

namespace SkyBrigade.Engine;

/// <summary>
/// Represents an interface for a game screen in the application.
/// </summary>
public interface IGameScreen : IDisposable, IUpdateable, IDrawable
{
    /// <summary>
    /// The list of entities present in the game screen.
    /// </summary>  
    List<IEntity> Entities { get; set; }

    /// <summary>
    /// Initializes the game screen with the OpenGL context (GL).
    /// </summary>
    /// <param name="gl">The OpenGL context used for rendering.</param>
    void Initialize(GL gl);
}
