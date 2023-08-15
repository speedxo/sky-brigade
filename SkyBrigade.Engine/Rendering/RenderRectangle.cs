using System;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Rendering.Effects.Components;

using Shader = SkyBrigade.Engine.OpenGL.Shader;

namespace SkyBrigade.Engine.Rendering
{
	public class RenderRectangle : Entity
	{
		public MeshRendererComponent Mesh { get; init; }
		public ShaderComponent ScreenShader { get; init; }
		public FrameBufferObject FrameBuffer { get; init; }

		protected TransformComponent Transform { get; init; }
		protected Camera Camera;

		public RenderRectangle(Shader screenShader, FrameBufferObject fbo)
		{
			FrameBuffer = fbo;
			ScreenShader = AddComponent(new ShaderComponent(screenShader));

			Transform = AddComponent<TransformComponent>();

			Mesh = AddComponent<MeshRendererComponent>();
			Mesh.Load(MeshGenerators.CreateRectangle, new CustomMaterial(ScreenShader));
        }

		public void RenderScene(float dt)
		{
			ScreenShader.Use();

			if (FrameBuffer.Attachments.TryGetValue(FramebufferAttachment.ColorAttachment0, out uint albedo))
			{
                GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
                GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, albedo);
                ScreenShader.SetUniform("uAlbedo", 0);
            }

            if (FrameBuffer.Attachments.TryGetValue(FramebufferAttachment.DepthAttachment, out uint depth))
            {
                GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture1);
                GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, depth);
                ScreenShader.SetUniform("uDepth", 1);
            }

            Mesh.Use(RenderOptions.Default);
			Mesh.Draw(dt);

			ScreenShader.End();
		}

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
			
        }

        public override void Update(float dt)
        {

        }
    }
}

