using Newtonsoft.Json;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.OpenGL;
using System.Numerics;
using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Engine.Rendering
{
    public class BasicMaterial : Material
    {
        public BasicMaterialDescription MaterialDescription;

        // forward some properties
        public override Vector4 Color { get => MaterialDescription.Color; set => MaterialDescription.Color = value; }

        public override Texture? Texture { get => MaterialDescription.Texture; set => MaterialDescription.Texture = value; }

        public BasicMaterial()
        {
            Shader = GameManager.Instance.ContentManager.GetShader("material_basic");
            MaterialDescription = BasicMaterialDescription.Default;
        }

        public override void Load(string path)
        {
            // check if the file at path exists, and load it if it does
            if (!System.IO.File.Exists(path))
                throw new System.IO.FileNotFoundException("File not found", path);

            // return new BasicMaterial() {
            //     MaterialDescription = JsonConvert.DeserializeObject<BasicMaterialDescription>(System.IO.File.ReadAllText(path))
            // };

            this.MaterialDescription = JsonConvert.DeserializeObject<BasicMaterialDescription>(System.IO.File.ReadAllText(path));
        }

        public override void Save(string path)
        {
            // check if a file exists at the destination
            if (System.IO.File.Exists(path))
                File.Delete(path); // TODO: we should log this override.

            // use the path to save the material description using json
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(MaterialDescription));
        }

        public override void Use(RenderOptions? renderOptions = null)
        {
            var options = renderOptions ?? RenderOptions.Default;

            Shader.Use();

            Shader.SetUniform("uMetallicness", MaterialDescription.Metallicness);
            Shader.SetUniform("uRoughness", MaterialDescription.Roughness);
            Shader.SetUniform("uAo", MaterialDescription.AmbientOcclusion);
            Shader.SetUniform("uColor", MaterialDescription.Color * options.Color);

            Shader.SetUniform("uGamma", options.Gamma);
            Shader.SetUniform("uAmbientStrength", options.AmbientLightingStrength);

            if (Texture == null)
                Shader.SetUniform("useTexture", 0);
            else
            {
                GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
                GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, Texture.Handle);
                Shader.SetUniform("uTexture", 0);
                Shader.SetUniform("useTexture", 1);
            }
        }
    }
}