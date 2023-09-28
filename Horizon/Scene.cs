using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.Rendering;
using Horizon.Rendering.Effects;
using Silk.NET.OpenGL;

namespace Horizon;

/// <summary>
/// Abstract base class for a game screen, based off <see cref="Entity"/>.
/// <seealso cref="IEntity"/>
/// </summary>
public abstract class Scene : Entity, IDisposable
{
    // again, this is trashy. TODO: fix
    private static RenderRectangle? defaultSceneRect = null;

    public EffectStack PostEffects { get; protected set; }
    public RenderRectangle SceneRect { get; protected set; }
    public FrameBufferObject FrameBuffer { get; protected set; }

    private bool hasRenderPipelineBeenInitialized = false;

    protected void InitializeRenderingPipeline()
    {
        if (hasRenderPipelineBeenInitialized)
        {
            Engine.Logger.Log(
                Logging.LogLevel.Fatal,
                "The scenes render pipeline has already been initialized!"
            );
            throw new Exception("The scenes render pipeline has already been initialized!");
        }

        hasRenderPipelineBeenInitialized = (
            InitializeFrameBuffer() && InitializeEffectStack() && InitializeRenderFrame()
        );
        if (!hasRenderPipelineBeenInitialized)
            throw new Exception();
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
        FrameBuffer = FrameBufferManager.CreateFrameBuffer(
            (int)Engine.Window.ViewportSize.X,
            (int)Engine.Window.ViewportSize.Y
        );

        FrameBuffer.AddAttachment(FramebufferAttachment.ColorAttachment0);
        FrameBuffer.AddAttachment(FramebufferAttachment.DepthAttachment);

        return FrameBuffer.ContructFrameBuffer();
    }

    protected virtual bool InitializeEffectStack()
    {
        PostEffects = AddEntity(
            new EffectStack("Assets/effects/basic.vert", GeneratePostProccessingEffects())
        );
        return true;
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        if (!Enabled)
            return;

        if (!hasRenderPipelineBeenInitialized)
            Engine.Logger.Log(
                Logging.LogLevel.Fatal,
                "The scenes render pipeline has not been initialized!"
            );

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

        Engine.GL.Clear(
            ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit
        );
        Engine.GL.Viewport(0, 0, (uint)FrameBuffer.Width, (uint)FrameBuffer.Height);
        //}
        //else
        //{
        //    Engine.GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        //    Engine.GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        //    Engine.GL.Viewport(0, 0, (uint)Engine.WindowSize.X, (uint)Engine.WindowSize.Y);
        //}

        for (int i = 0; i < Components.Count; i++)
            Components.Values.ElementAt(i).Draw(dt, renderOptions);

        for (int i = 0; i < Entities.Count; i++)
            Entities[i].Draw(dt, renderOptions);

        DrawOther(dt, renderOptions);

        //if (options.IsPostProcessingEnabled)
        FrameBuffer.Unbind();
    }

    public abstract void DrawOther(float dt, RenderOptions? renderOptions = null);

    public virtual void RenderPost(float dt, RenderOptions? renderOptions = null)
    {
        if (Engine.Debugger.GameContainerDebugger.Visible)
            Engine.Debugger.GameContainerDebugger.FrameBuffer.Bind();

        Engine.GL.Clear(
            ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit
        );
        Engine.GL.Viewport(
            0,
            0,
            (uint)(Engine.Window.ViewportSize.X),
            (uint)Engine.Window.ViewportSize.Y
        );

        var options = renderOptions ?? RenderOptions.Default;
        if (options.IsPostProcessingEnabled)
            SceneRect.RenderScene(dt);
        else
            defaultSceneRect!.RenderScene(dt); // we do a test on hasRenderPipelineBeenInitialized in Draw

        if (Engine.Debugger.GameContainerDebugger.Visible)
            Engine.Debugger.GameContainerDebugger.FrameBuffer.Unbind();
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
