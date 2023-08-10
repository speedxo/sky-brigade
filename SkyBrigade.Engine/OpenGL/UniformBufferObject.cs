using System;
using Silk.NET.OpenGL;

namespace SkyBrigade.Engine.OpenGL
{
    public class UniformBufferObject : BufferObject
    {
        private readonly uint _bindingPoint;

        public UniformBufferObject(Shader shader, string point)
            : base(BufferTargetARB.UniformBuffer)
        {
            _bindingPoint = shader.GetUniformBlockIndex(point);
            Console.WriteLine(_bindingPoint);
        }
        public void BindToBindingPoint()
        {
            GameManager.Instance.Gl.BindBufferBase(BufferTargetARB.UniformBuffer, (uint)_bindingPoint, Handle);
        }
    }
}
