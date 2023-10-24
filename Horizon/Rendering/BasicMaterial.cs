using Silk.NET.OpenGL;
using System.Numerics;
using Texture = Horizon.OpenGL.Texture;

namespace Horizon.Rendering
{
    public class BasicMaterial : Material
    {
        public BasicMaterialDescription MaterialDescription;

        // forward some properties
        public override Vector4 Color
        {
            get => MaterialDescription.Color;
            set => MaterialDescription.Color = value;
        }

        public override Texture? Texture
        {
            get => MaterialDescription.Texture;
            set => MaterialDescription.Texture = value;
        }

        public BasicMaterial()
        {
            Technique = AddEntity(new Technique("Assets/material_shader", "basic"));
            MaterialDescription = BasicMaterialDescription.Default;
        }

        //public override void Load(string path)
        //{
        //    // check if the file at path exists, and load it if it does
        //    if (!System.IO.File.Exists(path))
        //        throw new System.IO.FileNotFoundException("File not found", path);

        //    // return new BasicMaterial() {
        //    //     MaterialDescription = JsonConvert.DeserializeObject<BasicMaterialDescription>(System.IO.File.ReadAllText(path))
        //    // };

        //    this.MaterialDescription = JsonConvert.DeserializeObject<BasicMaterialDescription>(System.IO.File.ReadAllText(path));
        //}

        //public override void Save(string path)
        //{
        //    // check if a file exists at the destination
        //    if (System.IO.File.Exists(path))
        //        File.Delete(path);

        //    // use the path to save the material description using json
        //    System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(MaterialDescription));
        //}

        public override void Use(ref RenderOptions options)
        {
            

            Technique.Use();

            Technique.SetUniform("uMetallicness", MaterialDescription.Metallicness);
            Technique.SetUniform("uRoughness", MaterialDescription.Roughness);
            Technique.SetUniform("uAo", MaterialDescription.AmbientOcclusion);
            Technique.SetUniform("uColor", MaterialDescription.Color * options.Color);

            Technique.SetUniform("uGamma", options.Gamma);
            Technique.SetUniform("uAmbientStrength", options.AmbientLightingStrength);

            if (Texture == null)
                Technique.SetUniform("useTexture", 0);
            else
            {
                Engine.GL.ActiveTexture(TextureUnit.Texture0);
                Engine.GL.BindTexture(TextureTarget.Texture2D, Texture.Handle);
                Technique.SetUniform("uTexture", 0);
                Technique.SetUniform("useTexture", 1);
            }
        }
    }
}
