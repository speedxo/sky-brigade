using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Descriptions;


namespace VoxelExplorer.Data
{
    public class ChunkTechnique : Technique
    {
        private const string UNIFORM_VIEW = "uCameraView";
        private const string UNIFORM_PROJECTION = "uCameraProjection";

        public ChunkTechnique()
        {
            SetShader(
                GameEngine
                    .Instance
                    .ObjectManager
                    .Shaders
                    .CreateOrGet(
                        "chunk_technique",
                        ShaderDescription.FromPath("content/shaders/", "world")
                    )
            );
        }

        protected override void SetUniforms()
        {
            SetUniform(
                UNIFORM_VIEW,
                GameEngine.Instance.SceneManager.CurrentInstance.ActiveCamera.View
            );
            SetUniform(
                UNIFORM_PROJECTION,
                GameEngine.Instance.SceneManager.CurrentInstance.ActiveCamera.Projection
            );
        }
    }
}
