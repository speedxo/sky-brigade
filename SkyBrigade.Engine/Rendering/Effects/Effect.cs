using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering.Effects.Components;

namespace SkyBrigade.Engine.Rendering.Effects
{
    public class Effect
    {
        protected string Source { get; init; }
        public string UniformBlockName { get; protected init; }

        public Effect(string source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            UniformBlockName = GetUniformBlockName();
        }

        public virtual string GetReadyShaderCode(int index)
        {
            return Source.Replace("ShaderStage", $"ShaderStage_{index}");
        }

        protected virtual string GetUniformBlockName()
        {
            var stringLines = Source.Split(Environment.NewLine);
            var lineSegments = stringLines.First().Split(' ');

            if (lineSegments.Length < 2)
                throw new InvalidOperationException("Invalid shader source format");

            return lineSegments.Last();
        }
    }

    public class EffectStack : Entity
    {
        public LinkedList<Effect> Effects { get; private init; }

        public UniformBufferManager BufferManager { get; private init; }
        public ShaderComponent Shader { get; private init; }

        private static readonly string shaderStageData = @"
struct ShaderData {
	vec4 FragColor;
};

in vec2 texCoords;
uniform sampler2D uAlbedo;
";

        [Pure]
        private static string finalFragmentMethod(int finalIndex)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = finalIndex; i >= 0; i--)
            {
                sb.Append($"ShaderStage_{i}(");
            }
            sb.Append("ShaderData(texture(uAlbedo, texCoords))");
            for (int i = 0; i <= finalIndex; i++)
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

        public EffectStack(params Effect[] effects)
        {
            if (effects == null || effects.Length == 0)
                throw new ArgumentException("At least one effect is required", nameof(effects));

            Effects = new LinkedList<Effect>(effects);

            var fragmentSource = GenerateShader();
            File.WriteAllText("shader.frag", fragmentSource);
            var vertexSource = GetVertexSource();

            Shader = AddComponent(ShaderComponent.FromSource(vertexSource, fragmentSource));
            BufferManager = AddComponent<UniformBufferManager>();
        }

        [Pure]
        protected virtual string GetVertexSource()
        {
            return @"#version 410 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNorm;
layout (location = 2) in vec2 vTexCoords;

uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 uModel;

out vec2 texCoords;

void main()
{
    texCoords = vTexCoords;

    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
}";
        }


        protected virtual string GenerateShader()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("#version 410 core"); // Shader version
            sb.AppendLine(shaderStageData);

            for (int i = 0; i < Effects.Count; i++)
                sb.AppendLine(Effects.ElementAt(i).GetReadyShaderCode(i));

            sb.AppendLine(finalFragmentMethod(Effects.Count - 1));
            return sb.ToString();
        }
    }
}
    