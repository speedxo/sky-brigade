using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Rendering.Effects.Components;
using System.Collections.Generic;

namespace SkyBrigade.Engine.Rendering
{
    [RequiresComponent(typeof(ShaderComponent))]
    public class UniformBufferManager : IGameComponent
    {
        private readonly Dictionary<string, UniformBufferObject> _bufferObjects = new Dictionary<string, UniformBufferObject>();

        public string Name { get; set; }
        public Entity Parent { get; set; }

        public Shader Shader { get; set; }

        public UniformBufferManager()
        {
        }

        public UniformBufferManager(Shader shader)
        {
            Shader = shader;
        }

        public void Initialize()
        {
            Shader ??= Parent.GetComponent<ShaderComponent>();
        }

        public UniformBufferObject GetBuffer(string bindingPoint)
        {
            if (_bufferObjects.TryGetValue(bindingPoint, out var buffer))
            {
                return buffer;
            }

            buffer = new UniformBufferObject(Shader, bindingPoint);
            Shader.Use();
            buffer.BindToBindingPoint();
            Shader.End();

            _bufferObjects.Add(bindingPoint, buffer);

            return buffer;
        }

        public void Use()
        {
            foreach (var item in _bufferObjects.Values)
            {
                item.BindToBindingPoint();
            }
        }

        public void Update(float dt)
        {
            // Update logic here if needed
        }

        public void Draw(float dt, RenderOptions? options = null)
        {
            // Drawing logic here if needed
        }
    }
}
