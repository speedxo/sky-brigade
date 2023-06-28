using System;
using System.Numerics;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;

using Texture = SkyBrigade.Engine.OpenGL.Texture;
using Shader = SkyBrigade.Engine.OpenGL.Shader;
using Color = System.Drawing.Color;

namespace SkyBrigade.Engine.Rendering
{
    /* This class is very convenient for either billboard rendering or simple
     * 2D stuff but it has an iherent flaw in that it requires an OpenGL context
     * to exist before it can ever be instances, else it crashed the whole thing
     * 
     * TODO: the vbo here never gets disposed but yolo we move
     */
    public class RenderRectangle
    {
        private static Vertex[] _vertices;
        private static VertexBufferObject<Vertex> _vbo;
        private static Shader _shader;

        static RenderRectangle()
        {
            _vertices = new Vertex[] {
                new Vertex(-1, -1, 0, 0, 1),
                new Vertex(1, -1, 0, 1, 1),
                new Vertex(1, 1, 0, 1, 0),
                new Vertex(-1, 1, 0, 0, 0)
            };
            _vbo = new VertexBufferObject<Vertex>(GameManager.Instance.Gl);
            _vbo.VertexBuffer.BufferData(_vertices);
            _vbo.ElementBuffer.BufferData(new uint[] {
                 0, 1, 3,
                 1, 2, 3
            });

            //Telling the VAO object how to lay out the attribute pointers
            _vbo.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 0);
            _vbo.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 3 * sizeof(float));
            _vbo.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 6 * sizeof(float));

            _shader = GameManager.Instance.ContentManager.LoadShader("Assets/render_rectangle_shader/basic.vert", "Assets/render_rectangle_shader/basic.frag");
        }


        private Vector3 pos;
        private float rot;
        private Vector2 scale;

        public Vector3 Position { get => pos; set { pos = value; updateModelMatrix(); } }
        public float Rotation { get => rot; set { rot = value; updateModelMatrix(); } }
        public Vector2 Scale { get => scale; set { scale = value; updateModelMatrix(); } }

        public Matrix4x4 ModelMatrix { get; private set; }

        public Texture? Texture { get; set; }
        public Vector4 Color { get; set; } = Vector4.One;

        private void updateModelMatrix()
        {
            ModelMatrix = Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateScale(scale.X, scale.Y, 1.0f) * Matrix4x4.CreateRotationZ(MathHelper.DegreesToRadians(rot));
        }

        public RenderRectangle(Vector3? inPos=null, Vector2? inScale=null, float inRotation=0.0f)
        {
            /* We do this in one operation because swag money BABY
			 */
            pos = inPos ?? Vector3.Zero;
            scale = inScale ?? Vector2.One;
            rot = inRotation;
            updateModelMatrix();
        }

        public void Draw(Camera? camera=null)
        {
            _shader.Use();
            if (Texture != null)
            {
                GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
                GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, Texture.Handle);
                _shader.SetUniform("uTexture", 0);
            }

            _shader.SetUniform("uView", camera == null ? Matrix4x4.Identity : camera.View);
            _shader.SetUniform("uProjection", camera == null ? Matrix4x4.Identity : camera.Projection);
            _shader.SetUniform("uModel", ModelMatrix);
            _shader.SetUniform("uColor", Color);

            _vbo.Bind();

            // once again, i really dont wanna make the whole method unsafe for one call
            unsafe
            {
                GameManager.Instance.Gl.DrawElements(PrimitiveType.Triangles, (uint)6, DrawElementsType.UnsignedInt, null);
            }

            _vbo.Unbind();
            GameManager.Instance.Gl.UseProgram(0);
        }
    }
}

