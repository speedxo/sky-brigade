using System.IO.Compression;
using Newtonsoft.Json;

namespace SkyBrigade.Engine.Rendering
{
    public class BasicMaterial : Material
    {
        public BasicMaterialDescription MaterialDescription;

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

        public override void Use()
        {
            Shader.Use();
            
            Shader.SetUniform("uMetallicness", MaterialDescription.Metallicness);
            Shader.SetUniform("uRoughness", MaterialDescription.Roughness);
            Shader.SetUniform("uAo", MaterialDescription.AmbientOcclusion);
        }
    }
}

