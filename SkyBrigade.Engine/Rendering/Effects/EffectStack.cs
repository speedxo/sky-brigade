using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using Silk.NET.SDL;
using Horizon.GameEntity;
using Horizon.Rendering.Effects.Components;

namespace Horizon.Rendering.Effects
{
    public class EffectStack : Entity
    {
        public List<Effect> Effects { get; private init; }

        public UniformBufferManager BufferManager => Technique.BufferManager;

        public Technique Technique { get; init; }


        private static readonly string ShaderStageData = @"
struct ShaderData {
    vec4 FragColor;
    vec4 DepthComponent;
};

in vec2 texCoords;

uniform sampler2D uAlbedo;
uniform sampler2D uDepth;

";

        [Pure]
        private static string FinalFragmentMethod(int finalIndex)
        {
            var sb = new StringBuilder();

            if (finalIndex < 0)
            {
                GameManager.Instance.Logger.Log(Logging.LogLevel.Warning, "An empty EffectStack was created!");
                return $@"
out vec4 FinalFragColor;

void main()
{{
    FinalFragColor = texture(uAlbedo, texCoords);
}}
";
            }
            else
            {
                for (var i = finalIndex; i >= 0; i--)
                {
                    sb.Append($"ShaderStage_{i}(");
                }
                sb.Append("ShaderData(texture(uAlbedo, texCoords), texture(uDepth, texCoords))");
                for (var i = 0; i <= finalIndex; i++)
                {
                    sb.Append(')');
                }

                return $@"
out vec4 FinalFragColor;

void main()
{{
    FinalFragColor = {sb}.FragColor;
}}
";
            }
        }

        public EffectStack(string vertexPath = "", params Effect[] effects)
        {
            // Default state is empty
            Enabled = effects != null && effects.Length > 0;
            Effects = new List<Effect>(effects ?? Array.Empty<Effect>());

            var fragmentSource = GenerateShader();
            File.WriteAllText("shader.frag", fragmentSource);
            var vertexSource = string.IsNullOrEmpty(vertexPath) ? GetVertexSource() : File.ReadAllText(vertexPath);

            Technique = AddEntity(new Technique(ShaderComponent.FromSource(vertexSource, fragmentSource)));
        }

        // TODO: Optimize the universe while we're at it!
        public override void Update(float dt)
        {
            base.Update(dt);
            foreach (var effect in Effects)
            {
                var buffer = BufferManager.GetBuffer(effect.UniformBlockName);
                effect.UpdateBuffer(dt, buffer);
            }
        }

        [Pure]
        protected virtual string GetVertexSource()
        {
            return @"#version 410 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNorm;
layout (location = 2) in vec2 vTexCoords;

//uniform mat4 uView;
//uniform mat4 uProjection;
uniform mat4 uModel;

out vec2 texCoords;

void main()
{
    texCoords = vTexCoords;

    // Trying to understand the universe through vertex manipulation!
    gl_Position = /*uProjection * uView */ uModel * vec4(vPos, 1.0);
}";
        }

        protected virtual string GenerateShader()
        {
            var sb = new StringBuilder();

            sb.AppendLine("#version 410 core"); // Shader version
            sb.AppendLine(ShaderStageData);

            for (var i = 0; i < Effects.Count; i++)
            {
                sb.AppendLine(Effects[i].GetReadyShaderCode(i));
            }

            sb.AppendLine(FinalFragmentMethod(Effects.Count - 1));
            return sb.ToString();
        }
    }
}
