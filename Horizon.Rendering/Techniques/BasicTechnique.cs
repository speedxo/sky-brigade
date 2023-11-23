using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Descriptions;

namespace Horizon.Rendering.Techniques;

public class BasicTechnique : Technique
{
    private const string UNIFORM_VIEW = "uCameraView";
    private const string UNIFORM_PROJECTION = "uCameraProjection";

    public BasicTechnique()
    {
        SetShader(
            GameEngine
                .Instance
                .ContentManager
                .Shaders
                .CreateOrGet(
                    "basic_technique",
                    ShaderDescription.FromPath("shaders/basic", "basic_technique")
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
