using Horizon.GameEntity;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL
{
    public class UniformBufferObject : IDisposable
    {
        public uint Handle { get; init; }
        private readonly uint _bindingPoint;

        public UniformBufferObject(uint bindingPoint)
        {
            Handle = Entity.Engine.GL.GenBuffer();

            _bindingPoint = bindingPoint;
        }

        public void BindToUniformBlockBindingPoint()
        {
            Entity.Engine.GL.BindBufferBase(
                BufferTargetARB.UniformBuffer,
                _bindingPoint,
                Handle
            );
        }

        public unsafe void BufferData<T>(ReadOnlySpan<T> data)
            where T : unmanaged
        {
            Bind();
            fixed (void* d = data)
            {
                Entity.Engine.GL.BufferData(
                    BufferTargetARB.UniformBuffer,
                    (nuint)(data.Length * sizeof(T)),
                    d,
                    BufferUsageARB.DynamicDraw
                );
            }
            Unbind();
        }

        public unsafe void BufferSingleData<T>(T data)
            where T : unmanaged
        {
            Bind();
            Entity.Engine.GL.BufferData(
                BufferTargetARB.UniformBuffer,
                (nuint)(sizeof(T)),
                data,
                BufferUsageARB.DynamicDraw
            );

            Unbind();
        }

        public unsafe void BufferSingleData<T>(T[] data)
            where T : unmanaged
        {
            Bind();
            Entity.Engine.GL.BufferData(
                BufferTargetARB.UniformBuffer,
                (nuint)(sizeof(T) * data.Length),
                in data[0],
                BufferUsageARB.DynamicDraw
            );

            Unbind();
        }

        public void Bind()
        {
            Entity.Engine.GL.BindBuffer(BufferTargetARB.UniformBuffer, Handle);
        }

        public void Unbind()
        {
            Entity.Engine.GL.BindBuffer(BufferTargetARB.UniformBuffer, 0);
        }

        public void Dispose()
        {
            try
            {
                Entity.Engine.GL.DeleteBuffer(Handle);
            }
            catch
            {
                // Ignoring any deletion errors.
            }
            GC.SuppressFinalize(this);
        }
    }
}
