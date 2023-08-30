using System.Numerics;
using Horizon;
using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.Rendering;

namespace Gravitator2D;

public class BodyRenderer : Entity
{
    private readonly Technique bodyRendererTechnique;
    private readonly Universe universe;
    private Vector2 pos;

    public BodyRenderer(Universe universe)
    {
        this.universe = universe;

        AddEntity(bodyRendererTechnique = new Technique(Shader.CompileShader("shaders/bodies.vert", "shaders/bodies.frag")));
        AddEntity(new RenderTarget(bodyRendererTechnique));
        bodyRendererTechnique.BufferManager.AddUniformBuffer("BodyBuffer");
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        UpdateGpu();
        base.Draw(dt, renderOptions);
    }

    public override void Update(float dt)
    {
        pos += GameManager.Instance.InputManager.GetVirtualController().MovementAxis * dt * 10.0f;

        base.Update(dt);
    }

    private void UpdateGpu()
    {
        bodyRendererTechnique.Use();
        bodyRendererTechnique.SetUniform("uAspectRatio", GameManager.Instance.AspectRatio);
        bodyRendererTechnique.SetUniform("uBodyCount", universe.BodyCount);
        bodyRendererTechnique.SetUniform("uCenter", pos);

        universe.Lock.EnterReadLock();
        try
        {
            bodyRendererTechnique.BufferManager.GetBuffer("BodyBuffer").BufferSingleData((universe.RenderBodies));
        }
        finally
        {
            universe.Lock.ExitReadLock();
        }
    }
}
