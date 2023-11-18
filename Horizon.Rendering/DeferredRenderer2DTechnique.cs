using System.Numerics;

using Horizon.Engine;
using Horizon.OpenGL.Buffers;

using Silk.NET.OpenGL;

namespace Horizon.Rendering;

public class DeferredRenderer2DTechnique : Renderer2DTechnique
{
    protected virtual string UNIFORM_NORMALFRAGPOS { get; } = "uTexNormalFragPos";
    protected override string ShaderFileName => "deferred";

    public DeferredRenderer2DTechnique(FrameBufferObject frameBuffer)
        : base(frameBuffer) { }

    protected override void SetUniforms()
    {
        frameBuffer.BindAttachment(FramebufferAttachment.ColorAttachment0, 0);
        SetUniform(UNIFORM_ALBEDO, 0);
        frameBuffer.BindAttachment(FramebufferAttachment.ColorAttachment1, 1);
        SetUniform(UNIFORM_NORMALFRAGPOS, 1);

        LightingStuff();
    }

    Vector2 lightPos;
    Vector3 color = Vector3.One;
    int indexer = 0, max = 32;

    private void LightingStuff()
    {
        var data = GameEngine.Instance.InputManager.MouseManager.GetData();
        lightPos = GameEngine.Instance.ActiveCamera.ScreenToWorld(data.Position);

        SetUniform($"uTime", GameEngine.Instance.TotalTime);
        SetUniform($"uAspectRatio", GameEngine.Instance.WindowManager.AspectRatio);
        SetUniform($"lights[{indexer}].Position", (lightPos));
        SetUniform($"lights[{indexer}].Radius", 200.0f);
        SetUniform($"lights[{indexer}].Color", color);


        if (GameEngine.Instance.InputManager.GetPreviousVirtualController().IsPressed(Input.VirtualAction.SecondaryAction))
        {
            if (!GameEngine.Instance.InputManager.IsPressed(Input.VirtualAction.SecondaryAction))
            {
                indexer = ++indexer % max;
                color = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
            }
        }



        SetUniform(
            "viewPos",
            new Vector2(
                GameEngine.Instance.ActiveCamera.Position.X,
                GameEngine.Instance.ActiveCamera.Position.Y
            )
        );
    }
}
