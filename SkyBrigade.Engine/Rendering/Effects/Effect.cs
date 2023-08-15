using System;
using System.IO;
using System.Linq;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Rendering.Effects
{
    public class Effect
    {
        public string Source { get; init; }
        public string UniformBlockName { get; init; }
        
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

        public virtual void UpdateBuffer(float dt, in UniformBufferObject bufferObject)
        {

        }
    }
}
    