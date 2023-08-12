using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Rendering.Effects.Components;

namespace SkyBrigade.Engine.Rendering
{
    [RequiresComponent(typeof(ShaderComponent))]
    public class UniformBufferManager : IGameComponent
    {
        public Dictionary<string, UniformBufferObject> BufferObjects { get; private set; }

        public string Name { get; set; }
        public Entity Parent { get; set; }

        public Shader Shader { get; private set; }

        public UniformBufferManager(Shader shader)
        {
            this.Shader = shader;
        }

        public void Initialize()
        {
            BufferObjects = new Dictionary<string, UniformBufferObject>();
        }

        public UniformBufferObject GetBuffer(string bindingPoint)
        {
            if (BufferObjects.ContainsKey(bindingPoint))
                return BufferObjects[bindingPoint];

            // TODO: something better
            var buffer = new UniformBufferObject(Shader, bindingPoint);

            Shader.Use();
            buffer.BindToBindingPoint();
            Shader.End();

            BufferObjects.Add(bindingPoint, buffer);

            return buffer;
        }

        public void Use()
        {
            foreach (var (_, item) in BufferObjects)
                item.BindToBindingPoint();
        }

        public void Update(float dt)
        {

        }

        public void Draw(float dt, RenderOptions? options = null)
        {
            
        }
    }
}

