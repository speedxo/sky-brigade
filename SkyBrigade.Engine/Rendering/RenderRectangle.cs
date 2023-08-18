using System;
using Silk.NET.OpenGL;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Horizon.Rendering.Effects.Components;

using Shader = Horizon.OpenGL.Shader;

namespace Horizon.Rendering
{
	public class RenderRectangle : Entity
	{
		public MeshRendererComponent Mesh { get; init; }
		public Technique Technique { get; set; }
		public FrameBufferObject FrameBuffer { get; init; }

		protected TransformComponent Transform { get; init; }

		public RenderRectangle(Technique technique, FrameBufferObject fbo)
		{
			FrameBuffer = fbo;
			Technique = AddEntity(technique);

			Transform = AddComponent<TransformComponent>();

			Mesh = AddComponent<MeshRendererComponent>();
			Mesh.Load(MeshGenerators.CreateRectangle, new CustomMaterial(technique));
        }
        public RenderRectangle(Shader shader, FrameBufferObject fbo)
        {
            FrameBuffer = fbo;
            Technique = AddEntity(new Technique(shader));

            Transform = AddComponent<TransformComponent>();

            Mesh = AddComponent<MeshRendererComponent>();
            Mesh.Load(MeshGenerators.CreateRectangle, new CustomMaterial(shader));
        }

        public void RenderScene(float dt)
		{
			Technique.Use();

			if (FrameBuffer.Attachments.TryGetValue(FramebufferAttachment.ColorAttachment0, out uint albedo))
			{
                GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
                GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, albedo);
                Technique.SetUniform("uAlbedo", 0);
            }

            if (FrameBuffer.Attachments.TryGetValue(FramebufferAttachment.DepthAttachment, out uint depth))
            {
                GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture1);
                GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, depth);
                Technique.SetUniform("uDepth", 1);
            }

            Mesh.Use(RenderOptions.Default);
			Mesh.Draw(dt);

            Technique.End();
		}

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
			
        }

        public override void Update(float dt)
        {

        }
    }
}

