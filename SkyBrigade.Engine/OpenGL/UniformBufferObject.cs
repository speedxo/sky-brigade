using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace SkyBrigade.Engine.OpenGL
{
    public class UniformBufferObject
    {
        public uint Handle { get; init; }

        private readonly uint _bindingPoint;

        public UniformBufferObject(Shader shader, string point)
        {
            Handle = GameManager.Instance.Gl.GenBuffer();
            _bindingPoint = shader.GetUniformBlockIndex(point);
        }

        public void BindToBindingPoint()
        {
            GameManager.Instance.Gl.BindBufferBase(BufferTargetARB.UniformBuffer, _bindingPoint, Handle);
        }
        public virtual unsafe void BufferData<T>(ReadOnlySpan<T> data)
            where T: unmanaged
        {
            Bind();
            fixed (void* d = data)
            {
                GameManager.Instance.Gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)(data.Length * sizeof(T)), d, BufferUsageARB.StaticDraw);
            }
            GameManager.Instance.Gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
        }

        public virtual void Bind()
        {
            /* Binding the buffer object, with the correct buffer type.
             */
            GameManager.Instance.Gl.BindBuffer(BufferTargetARB.UniformBuffer, Handle);
        }

        public virtual void Dispose()
        {
            try
            {
                GameManager.Instance.Gl.DeleteBuffer(Handle);
            }
            catch
            {
                /* i dont fucking care */
            }
            GC.SuppressFinalize(this);
        }
    }
}
