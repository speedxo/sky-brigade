using System.Diagnostics;
using System.Xml.Linq;
using Silk.NET.OpenGL;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Horizon.Primitives;
using Horizon.Rendering;
using Horizon.Rendering.Effects;

namespace Horizon;

/// <summary>
/// Abstract base class for a game screen, based off <see cref="Entity"/>.
/// <seealso cref="IEntity"/>
/// </summary>
public abstract class Scene : Entity, IDisposable
{
    // again, this is trashy. TODO: fix
    private static RenderRectangle? defaultSceneRect=null;

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
        // yes this is trashy.
        defaultSceneRect ??= new RenderRectangle(new EffectStack().Technique, FrameBuffer);
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

        RenderScene(dt, renderOptions);
        PostEffects.PreDraw(dt);
        RenderPost(dt, renderOptions);

        DrawGui(dt);
    }

    public virtual void RenderScene(float dt, RenderOptions? renderOptions = null)
    {
        //if (options.IsPostProcessingEnabled)
        //{
            FrameBuffer.Bind();

            GameManager.Instance.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GameManager.Instance.Gl.Viewport(0, 0, (uint)FrameBuffer.Width, (uint)FrameBuffer.Height);
        //}
        //else
        //{
        //    GameManager.Instance.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        //    GameManager.Instance.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        //    GameManager.Instance.Gl.Viewport(0, 0, (uint)GameManager.Instance.WindowSize.X, (uint)GameManager.Instance.WindowSize.Y);
        //}

        DrawOther(dt, renderOptions);

        for (int i = 0; i < Components.Count; i++)
            Components.Values.ElementAt(i).Draw(dt, renderOptions);

        for (int i = 0; i < Entities.Count; i++)
            Entities[i].Draw(dt, renderOptions);

        //if (options.IsPostProcessingEnabled)
            FrameBuffer.Unbind();
    }

    public abstract void DrawOther(float dt, RenderOptions? renderOptions = null);
    
    public virtual void RenderPost(float dt, RenderOptions? renderOptions = null)
    {
        if (GameManager.Instance.Debugger.GameContainerDebugger.Visible)
            GameManager.Instance.Debugger.GameContainerDebugger.FrameBuffer.Bind();

        GameManager.Instance.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GameManager.Instance.Gl.Viewport(0, 0, (uint)GameManager.Instance.ViewportSize.X, (uint)GameManager.Instance.ViewportSize.Y);

        var options = renderOptions ?? RenderOptions.Default;
        if (options.IsPostProcessingEnabled)
            SceneRect.RenderScene(dt);
        else defaultSceneRect.RenderScene(dt);

        if (GameManager.Instance.Debugger.GameContainerDebugger.Visible)
            GameManager.Instance.Debugger.GameContainerDebugger.FrameBuffer.Unbind();

        GameManager.Instance.Gl.Viewport(0, 0, (uint)GameManager.Instance.WindowSize.X, (uint)GameManager.Instance.WindowSize.Y);
    }

    /// <summary>
    /// (WIP) Here is where all the UI magic will happen.
    /// </summary>
    /// <param name="dt">Deltatime between the last two frames.</param>
    public abstract void DrawGui(float dt);

    /// <summary>
    /// Disposes of the scene and its resources.
    /// </summary>
    public abstract void Dispose();
}
