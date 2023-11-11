using Horizon.GameEntity;
using Horizon.OpenGL;
using System.Numerics;

namespace Horizon.Rendering
{
    public abstract class Material : Entity
    {
        protected const string UNIFORM_VIEW_MATRIX = "uView";
        protected const string UNIFORM_MODEL_MATRIX = "uModel";
        protected const string UNIFORM_PROJECTION_MATRIX = "uProjection";
        protected const string UNIFORM_USE_WIREFRAME = "uWireframeEnabled";

        public virtual Texture? Texture { get; set; }
        public virtual Vector4 Color { get; set; } = Vector4.One;

        public Technique Technique { get; protected set; }

        public abstract void Use(in RenderOptions options);

        public static void End() => Engine.GL.UseProgram(0);

        public void SetModel(in Matrix4x4 modelMatrix) =>
            Technique.SetUniform(UNIFORM_MODEL_MATRIX, modelMatrix);
    }
}
