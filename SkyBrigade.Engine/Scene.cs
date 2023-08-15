using System.Xml.Linq;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Primitives;
using SkyBrigade.Engine.Rendering;
using SkyBrigade.Engine.Rendering.Effects;

namespace SkyBrigade.Engine;

/// <summary>
/// Abstract base class for a game screen, based off <see cref="Entity"/>.
/// <seealso cref="IEntity"/>
/// </summary>
public abstract class Scene : Entity, IDisposable
{
    public EffectStack PostEffects { get; protected set; }
    public RenderRectangle SceneRect { get; protected set; }
    public FrameBufferObject FrameBuffer { get; protected set; }

    private bool hasRenderPipelineBeenInitialized = false;

    protected void InitializeRenderingPipeline()
    {
        if (hasRenderPipelineBeenInitialized)
            GameManager.Instance.Logger.Log(Logging.LogLevel.Fatal, "The scenes render pipeline has already been initialized!");

        hasRenderPipelineBeenInitialized = (
            InitializeFrameBuffer() &&
            InitializeEffectStack() &&
            InitializeRenderFrame()
        );
    }

    protected virtual Effect[] GeneratePostProccessingEffects()
    {
        return Array.Empty<Effect>();
    }

    protected virtual bool InitializeRenderFrame()
    {
        SceneRect = new RenderRectangle(PostEffects.Technique.Shader, FrameBuffer);
        return true;
    }

    protected virtual bool InitializeFrameBuffer()
    {
        FrameBuffer = FrameBufferManager.CreateFrameBuffer((int)GameManager.Instance.ViewportSize.X, (int)GameManager.Instance.ViewportSize.Y);

        FrameBuffer.AddAttachment(FramebufferAttachment.ColorAttachment0);
        FrameBuffer.AddAttachment(FramebufferAttachment.DepthAttachment);

        return FrameBuffer.ContructFrameBuffer();
    }

    protected virtual bool InitializeEffectStack()
    {
        PostEffects = AddEntity(new EffectStack("Assets/effects/basic.vert", GeneratePostProccessingEffects()));
        return true;
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        if (!Enabled) return;

        if (!hasRenderPipelineBeenInitialized)
            GameManager.Instance.Logger.Log(Logging.LogLevel.Fatal, "The scenes render pipeline has not been initialized!");

        FrameBuffer.Bind();

        GameManager.Instance.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GameManager.Instance.Gl.Viewport(0, 0, (uint)FrameBuffer.Width, (uint)FrameBuffer.Height);

        RenderScene(dt, renderOptions);

        FrameBuffer.Unbind();

        RenderPost(dt, renderOptions);


        DrawGui(dt);
    }

    public virtual void RenderScene(float dt, RenderOptions? renderOptions = null)
    {
        for (int i = 0; i < Components.Count; i++)
            Components.Values.ElementAt(i).Draw(dt, renderOptions);

        for (int i = 0; i < Entities.Count; i++)
            Entities[i].Draw(dt, renderOptions);
    }

    public virtual void RenderPost(float dt, RenderOptions? renderOptions = null)
    {
        if (GameManager.Instance.Debugger.Enabled)
            GameManager.Instance.Debugger.GameContainerDebugger.FrameBuffer.Bind();

        GameManager.Instance.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        if (GameManager.Instance.Debugger.Enabled)
            GameManager.Instance.Gl.Viewport(0, 0, (uint)GameManager.Instance.Debugger.GameContainerDebugger.FrameBuffer.Width, (uint)GameManager.Instance.Debugger.GameContainerDebugger.FrameBuffer.Height);
        else
            GameManager.Instance.Gl.Viewport(0, 0, (uint)GameManager.Instance.ViewportSize.X, (uint)GameManager.Instance.ViewportSize.Y);

        SceneRect.RenderScene(dt);

        if (GameManager.Instance.Debugger.Enabled)
            GameManager.Instance.Debugger.GameContainerDebugger.FrameBuffer.Unbind();

        GameManager.Instance.Gl.Viewport(0, 0, (uint)GameManager.Instance.Window.Size.X * 2, (uint)GameManager.Instance.Window.Size.Y * 2);
    }
    public virtual void DrawGui(float dt)
    {

    }

    /// <summary>
    /// Disposes of the scene and its resources.
    /// </summary>
    public abstract void Dispose();
}
