using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL
{
    public class UniformBufferObject : IDisposable
    {
        public uint Handle { get; init; }
        private readonly uint _bindingPoint;

        public UniformBufferObject(uint bindingPoint)
        {
            Handle = GameManager.Instance.Gl.GenBuffer();

            _bindingPoint = bindingPoint;
        }

        public void BindToUniformBlockBindingPoint()
        {
            GameManager.Instance.Gl.BindBufferBase(BufferTargetARB.UniformBuffer, _bindingPoint, Handle);
        }

        public unsafe void BufferData<T>(ReadOnlySpan<T> data)
            where T : unmanaged
        {
            Bind();
            fixed (void* d = data)
            {
                GameManager.Instance.Gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)(data.Length * sizeof(T)), d, BufferUsageARB.DynamicDraw);
            }
            Unbind();
        }

        public unsafe void BufferSingleData<T>(T data)
            where T : unmanaged
        {
            Bind();
            GameManager.Instance.Gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)(sizeof(T)), data, BufferUsageARB.DynamicDraw);

            Unbind();
        }
        public unsafe void BufferSingleData<T>(T[] data)
            where T : unmanaged
        {
            Bind();
            GameManager.Instance.Gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)(sizeof(T) * data.Length), in data[0], BufferUsageARB.DynamicDraw);

            Unbind();
        }

        public void Bind()
        {
            GameManager.Instance.Gl.BindBuffer(BufferTargetARB.UniformBuffer, Handle);
        }

        public void Unbind()
        {
            GameManager.Instance.Gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
        }

        public void Dispose()
        {
            try
            {
                GameManager.Instance.Gl.DeleteBuffer(Handle);
            }
            catch
            {
                // Ignoring any deletion errors.
            }
            GC.SuppressFinalize(this);
        }
    }
}
