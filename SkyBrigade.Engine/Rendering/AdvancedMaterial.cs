using Silk.NET.OpenGL;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Rendering
{
    public class AdvancedMaterial : Material
    {
        // this is to store the material description in a serializable format, more specifically the paths to the textures
        private struct AdvancedMaterialSerializable
        {
            public string MetallicnessTexturePath {get;set;}
            public string RoughnessTexturePath {get;set;}
            public string AmbientOcclusionTexturePath {get;set;}
            public string AlbedoTexturePath {get;set;}
            public string NormalsTexturePath {get;set;}
        }

        public AdvancedMaterialDescription MaterialDescription { get; set; }

        public AdvancedMaterial()
        {
            Shader = GameManager.Instance.ContentManager.GetShader("material_advanced");
            MaterialDescription = AdvancedMaterialDescription.Default;
        }

        public override void Use()
        {
            Shader.Use();

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, MaterialDescription.Albedo.Handle);
            Shader.SetUniform("uAlbedo", 0);

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture1);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, MaterialDescription.Metallicness.Handle);
            Shader.SetUniform("uMetallicness", 1);

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture2);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, MaterialDescription.Roughness.Handle);
            Shader.SetUniform("uRoughness", 2);

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture3);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, MaterialDescription.AmbientOcclusion.Handle);
            Shader.SetUniform("uAo", 3);

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture4);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, MaterialDescription.AmbientOcclusion.Handle);
            Shader.SetUniform("uNormals", 4);
        }

        public override void Save(string path)
        {
            // check if a file exists at the destination
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path); // TODO: we should log this override.

            AdvancedMaterialSerializable serializable = new AdvancedMaterialSerializable()
            {
                MetallicnessTexturePath = MaterialDescription.Metallicness.path,
                RoughnessTexturePath = MaterialDescription.Roughness.path,
                AmbientOcclusionTexturePath = MaterialDescription.AmbientOcclusion.path,
                AlbedoTexturePath = MaterialDescription.Albedo.path,
                NormalsTexturePath = MaterialDescription.Normals.path
            };

            // use the path to save the material description using json
            System.IO.File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(serializable));
        }

        public override void Load(string path)
        {
            // check if the file at path exists, and load it if it does
            if (!System.IO.File.Exists(path))
                throw new System.IO.FileNotFoundException("File not found", path);

            AdvancedMaterialSerializable serializable = Newtonsoft.Json.JsonConvert.DeserializeObject<AdvancedMaterialSerializable>(System.IO.File.ReadAllText(path));

            MaterialDescription = new AdvancedMaterialDescription()
            {
                Metallicness = GameManager.Instance.ContentManager.GetTexture(serializable.MetallicnessTexturePath),
                Roughness = GameManager.Instance.ContentManager.GetTexture(serializable.RoughnessTexturePath),
                AmbientOcclusion = GameManager.Instance.ContentManager.GetTexture(serializable.AmbientOcclusionTexturePath),
                Albedo = GameManager.Instance.ContentManager.GetTexture(serializable.AlbedoTexturePath),
                Normals = GameManager.Instance.ContentManager.GetTexture(serializable.NormalsTexturePath)
            };
        }

        // loads all the textures in the directory and returns a material description
        public static AdvancedMaterial LoadFromDirectory(string path)
        {
            return new AdvancedMaterial()
            {
                MaterialDescription = new AdvancedMaterialDescription()
                {
                    Metallicness = GameManager.Instance.ContentManager.LoadTexture(path + "/metallicness.png"),
                    Roughness = GameManager.Instance.ContentManager.LoadTexture(path + "/roughness.png"),
                    AmbientOcclusion = GameManager.Instance.ContentManager.LoadTexture(path + "/ao.png"),
                    Albedo = GameManager.Instance.ContentManager.LoadTexture(path + "/albedo.png"),
                    Normals = GameManager.Instance.ContentManager.LoadTexture(path + "/normals.png")
                }
            };
        }
    }
}

