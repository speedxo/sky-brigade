using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Horizon.Rendering.Effects.Components;
using System.Collections.Generic;

namespace Horizon.Rendering
{
    [RequiresComponent(typeof(ShaderComponent))]
    public class UniformBufferManager : IGameComponent, IDisposable
    {
        private readonly Dictionary<uint, UniformBufferObject> _bufferObjects = new ();

        public string Name { get; set; }
        public Entity Parent { get; set; }

        public Shader Shader { get; set; }

        public UniformBufferManager(Shader shader)
        {
            Shader = shader;
        }

        public void Initialize()
        {
            Shader ??= Parent.GetComponent<ShaderComponent>();
        }

        public UniformBufferObject AddUniformBuffer(uint bindingPoint)
        {
            if (_bufferObjects.ContainsKey(bindingPoint)) return _bufferObjects[bindingPoint];

            var buffer = new UniformBufferObject(bindingPoint);
            buffer.Bind();
            buffer.BindToUniformBlockBindingPoint();
            buffer.Unbind();
            _bufferObjects[bindingPoint] = buffer;
            return buffer;
        }

        public UniformBufferObject GetBuffer(uint bindingPoint)
        {
            if (_bufferObjects.TryGetValue(bindingPoint, out var buffer))
                return buffer;
            throw new DirectoryNotFoundException($"Cannot find a UBO with the bindingPoint{bindingPoint}!");
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

        public void Draw(float dt, RenderOptions? options = null)
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
