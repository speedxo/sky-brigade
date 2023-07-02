namespace SkyBrigade.Engine.Rendering
{
    public class BasicMaterial : Material
    {
        public BasicMaterialDescription MaterialDescription { get; set; }

        public BasicMaterial()
        {
            Shader = GameManager.Instance.ContentManager.GetShader("material_basic");
            MaterialDescription = BasicMaterialDescription.Default;
        }

        public override void Use()
        {
            Shader.Use();
            
            Shader.SetUniform("uMetallicness", MaterialDescription.Metallicness);
            Shader.SetUniform("uRoughness", MaterialDescription.Roughness);
            Shader.SetUniform("uAo", MaterialDescription.AmbientOcclusion);
        }
    }
}

