using System.Numerics;

using Horizon.Engine;
using Horizon.OpenGL.Buffers;

using Silk.NET.OpenGL;

namespace Horizon.Rendering;

public class DeferredRenderer2DTechnique : Renderer2DTechnique
{
    protected virtual string UNIFORM_NORMAL { get; } = "uTexNormal";
    protected virtual string UNIFORM_FRAGPOS { get; } = "uTexFragPos";
    protected override string ShaderFileName => "deferred";

    public DeferredRenderer2DTechnique(FrameBufferObject frameBuffer)
        : base(frameBuffer) { }

    protected override void SetUniforms()
    {
        frameBuffer.BindAttachment(FramebufferAttachment.ColorAttachment0, 0);
        SetUniform(UNIFORM_ALBEDO, 0);
        frameBuffer.BindAttachment(FramebufferAttachment.ColorAttachment1, 1);
        SetUniform(UNIFORM_NORMAL, 1);
        frameBuffer.BindAttachment(FramebufferAttachment.ColorAttachment2, 2);
        SetUniform(UNIFORM_FRAGPOS, 2);

        //LightingStuff();
    }

    private void LightingStuff()
    {
        var mouseData = GameEngine.Instance.InputManager.MouseManager.GetData();
        Vector2 normalizedScreenPosition = new Vector2(
            (mouseData.Position.X / GameEngine.Instance.WindowManager.WindowSize.X),
            1.0f - (mouseData.Position.Y / GameEngine.Instance.WindowManager.WindowSize.Y)
        );
        SetUniform("lights[0].Position", new Vector2(128));
        SetUniform("lights[0].Radius", 500.0f);
        SetUniform("lights[0].Color", new Vector3(1.0f, 1.0f, 1.0f));
        SetUniform(
            "viewPos",
            new Vector2(
                GameEngine.Instance.ActiveCamera.Position.X,
                GameEngine.Instance.ActiveCamera.Position.Y
            )
        );
    }
}
