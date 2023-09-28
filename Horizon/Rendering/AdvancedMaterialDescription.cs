using Horizon.GameEntity;
using Texture = Horizon.OpenGL.Texture;

namespace Horizon.Rendering
{
    public struct AdvancedMaterialDescription
    {
        public Texture Albedo { get; set; }
        public Texture Metallicness { get; set; }
        public Texture Roughness { get; set; }
        public Texture AmbientOcclusion { get; set; }
        public Texture Normals { get; set; }

        public static AdvancedMaterialDescription Default { get; } =
            new AdvancedMaterialDescription
            {
                Roughness = Entity.Engine.Content.GetTexture("gray"),
                AmbientOcclusion = Entity.Engine.Content.GetTexture("white"),
                Metallicness = Entity.Engine.Content.GetTexture("gray"),
                Albedo = Entity.Engine.Content.GetTexture("white"),
                Normals = Entity.Engine.Content.GetTexture("white")
            };
    }
}
