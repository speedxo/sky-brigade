using Silk.NET.OpenGL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;
using System.Numerics;

namespace SkyBrigade.Engine.Rendering
{
    /* This class is very convenient for either billboard rendering or simple
     * 2D stuff but it has an iherent flaw in that it requires an OpenGL context
     * to exist before it can ever be instances, else it crashed the whole thing
     *
     * TODO: the vbo here never gets disposed but yolo we move even if backwards
     */

    public class Plane
    {
        private Vertex[] _vertices;
        private VertexBufferObject<Vertex> _vbo;
        /*  There definetly is better way to do this
         *  TODO: improve somehow
         */
        private Vector3 pos;
        private float rot;
        private Vector2 scale, size;

        public Material Material { get; set; }

        public void Use() => Material.Use();

        public void SetUniform(string name, float value) => Material.Shader.SetUniform(name, value);

        public void SetUniform(string name, int value) => Material.Shader.SetUniform(name, value);

        public void SetUniform(string name, Vector3 value) => Material.Shader.SetUniform(name, value);

        public void SetUniform(string name, Matrix4x4 value) => Material.Shader.SetUniform(name, value);

        public void SetUniform(string name, Vector4 value) => Material.Shader.SetUniform(name, value);

        public Vector3 Position
        { get => pos; set { pos = value; updateModelMatrix(); } }

        public float Rotation
        { get => rot; set { rot = value; updateModelMatrix(); } }

        public Vector2 Scale
        { get => scale; set { scale = value; updateModelMatrix(); } }

        public Vector2 Size
        { get => size; set { size = value; updateVertices(); } }

        private void updateVertices()
        {
            _vertices = new Vertex[] {
                        new Vertex(-Size.X, -Size.Y, 0, 0, 1),
                        new Vertex(Size.X, -Size.Y, 0, 1, 1),
                        new Vertex(Size.X, Size.Y, 0, 1, 0),
                        new Vertex(-Size.X, Size.Y, 0, 0, 0)
            };

            _vbo.VertexBuffer.BufferData(_vertices);
        }

        public Matrix4x4 ModelMatrix { get; private set; }

        public RectangleF Bounds { get => new RectangleF(pos.X, pos.Y, size.X, size.Y); }

        public bool CheckIntersection(Plane rect2)
        {
            // Get the corner points of each rectangle
            Vector2 rect1TopLeft = new Vector2(Position.X - Size.X, Position.Y + Size.Y);
            Vector2 rect1BottomRight = new Vector2(Position.X + Size.X, Position.Y - Size.Y);

            Vector2 rect2TopLeft = new Vector2(rect2.Position.X - rect2.Size.X, rect2.Position.Y + rect2.Size.Y);
            Vector2 rect2BottomRight = new Vector2(rect2.Position.X + rect2.Size.X, rect2.Position.Y - rect2.Size.Y);

            // Check for intersection
            if (rect1TopLeft.X <= rect2BottomRight.X && rect1BottomRight.X >= rect2TopLeft.X &&
                rect1TopLeft.Y >= rect2BottomRight.Y && rect1BottomRight.Y <= rect2TopLeft.Y)
            {
                // Rectangles intersect
                return true;
            }

            // Rectangles do not intersect
            return false;
        }

        private void updateModelMatrix()
        {
            ModelMatrix = Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateRotationZ(MathHelper.DegreesToRadians(rot)) * Matrix4x4.CreateScale(scale.X, scale.Y, 1.0f);
        }

        public Plane(Vector3? inPos = null, Vector2? inSize = null, Vector2? inScale = null, float inRotation = 0.0f, Material? mat = null)
        {
            /* We do this in one call of updateModelMatrix because swag money BABY
			 */
            pos = inPos ?? Vector3.Zero;
            scale = inScale ?? Vector2.One;
            rot = inRotation;
            size = inSize ?? Vector2.One;

            _vbo = new VertexBufferObject<Vertex>(GameManager.Instance.Gl);

            _vbo.ElementBuffer.BufferData(new uint[] {
                     0, 1, 3,
                     1, 2, 3
            });

            //Telling the VAO object how to lay out the attribute pointers
            _vbo.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 0);
            _vbo.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 3 * sizeof(float));
            _vbo.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 6 * sizeof(float));

            updateModelMatrix();
            updateVertices();

            Material = mat ?? new EmptyMaterial();
        }

        public void Draw(RenderOptions? renderOptions = null)
        {
            var options = renderOptions ?? RenderOptions.Default;
            Use();

            SetUniform("uView", options.Camera.View);
            SetUniform("uProjection", options.Camera.Projection);
            SetUniform("uModel", ModelMatrix);
            SetUniform("uColor", Material.Color * options.Color);

            _vbo.Bind();

            // once again, i really dont wanna make the whole method unsafe for one call
            unsafe
            {
                GameManager.Instance.Gl.DrawElements(PrimitiveType.Triangles, (uint)6, DrawElementsType.UnsignedInt, null);
            }

            _vbo.Unbind();
            Material.End();
        }
    }
}