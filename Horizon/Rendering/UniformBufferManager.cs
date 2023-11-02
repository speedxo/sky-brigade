using Horizon.Content;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;

namespace Horizon.Rendering
{
    public class UniformBufferManager : IGameComponent, IDisposable
    {
        private readonly Dictionary<uint, UniformBufferObject> _bufferObjects = new();

        public string Name { get; set; }
        public Entity Parent { get; set; }

        public Shader Shader { get; set; }

        public UniformBufferManager(Shader shader)
        {
            Shader = shader;
        }

        public void Initialize() { }

        public UniformBufferObject AddUniformBuffer(uint bindingPoint)
        {
            if (_bufferObjects.ContainsKey(bindingPoint))
                return _bufferObjects[bindingPoint];

            var buffer = new UniformBufferObject(bindingPoint);
            buffer.Bind();
            buffer.BindToUniformBlockBindingPoint();
            buffer.Unbind();
            _bufferObjects[bindingPoint] = buffer;
            return buffer;
        }

        public UniformBufferObject AddUniformBuffer(string name)
        {
            return AddUniformBuffer(Shader.GetUniformBlockIndex(name));
        }

        public UniformBufferObject GetBuffer(uint bindingPoint)
        {
            if (_bufferObjects.TryGetValue(bindingPoint, out var buffer))
                return buffer;
            throw new DirectoryNotFoundException(
                $"Cannot find a UBO with the bindingPoint{bindingPoint}!"
            );
        }

        public UniformBufferObject GetBuffer(string name)
        {
            return GetBuffer(Shader.GetUniformBlockIndex(name));
        }

        //public void Use()
        //{
        //    foreach (var item in _bufferObjects.Values)
        //    {
        //        item.BindToBindingPoint();
        //    }
        //}

        public void Update(float dt)
        {
            // Update logic here if needed
        }

        public void Draw(float dt, ref RenderOptions options)
        {
            // Drawing logic here if needed
        }

        public void Dispose()
        {
            foreach ((_, var buffer) in _bufferObjects)
            {
                buffer.Dispose();
            }
        }
    }
}
