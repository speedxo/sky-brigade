using System;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Tests.Tests
{
    public class MeshLoadingEngineTest : IEngineTest
    {
        public bool Loaded { get; set; } = false;
        public string Name { get; set; } = "Mesh Loading Test";

        Mesh? mesh;

        public void LoadContent(GL gl)
        {
            if (Loaded) return;

            //mesh = Mesh.CreateRectangle();
            mesh = Mesh.FromObj("Assets/teapot.obj");

            Loaded = true;
        }

        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            mesh?.Draw(renderOptions);
        }

        public void Update(float dt)
        {

        }

        public void Dispose()
        {
            Loaded = false;
            mesh?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}

