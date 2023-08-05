using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;
using System;
using System.Collections.Generic;

namespace SkyBrigade.Engine;

/// <summary>
/// Represents an interface for a game screen in the application.
/// </summary>
public interface IGameScreen : IDisposable
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

    /// <summary>
    /// Updates the game screen with the elapsed time (dt).
    /// </summary>
    /// <param name="dt">The elapsed time since the last update call.</param>
    void Update(float dt);

    /// <summary>
    /// Renders the game screen using the provided OpenGL context (GL) and render options.
    /// </summary>
    /// <param name="gl">The OpenGL context used for rendering.</param>
    /// <param name="dt">The elapsed time since the last render call.</param>
    /// <param name="renderOptions">Optional render options. If not provided, default options will be used.</param>
    void Render(GL gl, float dt, RenderOptions? renderOptions = null);
}
