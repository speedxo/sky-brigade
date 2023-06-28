using System;
using Silk.NET.OpenGL;

namespace SkyBrigade.Engine;

public interface IGameScreen : IDisposable
{
    void Initialize(GL gl);
    void LoadContent();
    void UnloadContent();
    void Update(float dt);
    void Render(GL gl, float dt);
}

