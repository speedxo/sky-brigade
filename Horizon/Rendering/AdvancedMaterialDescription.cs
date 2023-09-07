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
                Roughness = GameManager.Instance.ContentManager.GetTexture("gray"),
                AmbientOcclusion = GameManager.Instance.ContentManager.GetTexture("white"),
                Metallicness = GameManager.Instance.ContentManager.GetTexture("gray"),
                Albedo = GameManager.Instance.ContentManager.GetTexture("white"),
                Normals = GameManager.Instance.ContentManager.GetTexture("white")
            };
    }
}
