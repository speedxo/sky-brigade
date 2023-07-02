using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Engine.Rendering
{
    public struct AdvancedMaterialDescription
    {
        public Texture Metallicness { get; set; }
        public Texture Roughness { get; set; }
        public Texture AmbientOcclusion { get; set; }
        public static AdvancedMaterialDescription Default { get; } = new AdvancedMaterialDescription
        {
            Roughness = GameManager.Instance.ContentManager.GetTexture("gray"),
            AmbientOcclusion = GameManager.Instance.ContentManager.GetTexture("white"),
            Metallicness = GameManager.Instance.ContentManager.GetTexture("gray")
        };
    }   
}

