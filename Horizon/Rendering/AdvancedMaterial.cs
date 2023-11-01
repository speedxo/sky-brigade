using Silk.NET.OpenGL;

namespace Horizon.Rendering
{
    public class AdvancedMaterial : Material
    {
        private readonly string realPath;
        public AdvancedMaterialDescription MaterialDescription { get; set; }

        public AdvancedMaterial(string path)
        {
            this.realPath = path;
            Technique = AddEntity(new Technique("Assets/material_shader", "advanced"));
            Technique.BufferManager.AddUniformBuffer(0);
            MaterialDescription = AdvancedMaterialDescription.Default;
        }

        public override void Use(in RenderOptions options)
        {
            Technique.Use();

            var matOptions = new MaterialRenderOptions
            {
                DefferedRenderLayer = (int)options.DebugOptions.DefferedLayer,
                Gamma = options.Gamma,
                AmbientStrength = options.AmbientLightingStrength,
                Color = options.Color * Color
            };

            var ubo = Technique.BufferManager.GetBuffer(0);
            ubo.BufferSingleData(matOptions);

            // Bind the albedo texture to texture unit 0
            Engine.GL.ActiveTexture(TextureUnit.Texture0);
            Engine.GL.BindTexture(
                TextureTarget.Texture2D,
                MaterialDescription.Albedo.Handle
            );
            Technique.SetUniform("uAlbedo", 0);

            // Bind the metallicness texture to texture unit 1
            Engine.GL.ActiveTexture(TextureUnit.Texture1);
            Engine.GL.BindTexture(
                TextureTarget.Texture2D,
                MaterialDescription.Metallicness.Handle
            );
            Technique.SetUniform("uMetallicness", 1);

            // Bind the roughness texture to texture unit 2
            Engine.GL.ActiveTexture(TextureUnit.Texture2);
            Engine.GL.BindTexture(
                TextureTarget.Texture2D,
                MaterialDescription.Roughness.Handle
            );
            Technique.SetUniform("uRoughness", 2);

            // Bind the ambient occlusion texture to texture unit 3
            Engine.GL.ActiveTexture(TextureUnit.Texture3);
            Engine.GL.BindTexture(
                TextureTarget.Texture2D,
                MaterialDescription.AmbientOcclusion.Handle
            );
            Technique.SetUniform("uAo", 3);

            // Bind the normals texture to texture unit 4
            Engine.GL.ActiveTexture(TextureUnit.Texture4);
            Engine.GL.BindTexture(
                TextureTarget.Texture2D,
                MaterialDescription.Normals.Handle
            );
            Technique.SetUniform("uNormals", 4);
        }

        //public override void Save(string path)
        //{
        //    // check if a file exists at the destination
        //    if (System.IO.File.Exists(path))
        //        System.IO.File.Delete(path);

        //    AdvancedMaterialSerializable serializable = new()
        //    {
        //        MetallicnessTexturePath = MaterialDescription.Metallicness.Path,
        //        RoughnessTexturePath = MaterialDescription.Roughness.Path,
        //        AmbientOcclusionTexturePath = MaterialDescription.AmbientOcclusion.Path,
        //        AlbedoTexturePath = MaterialDescription.Albedo.Path,
        //        NormalsTexturePath = MaterialDescription.Normals.Path
        //    };

        //    // use the path to save the material description using json
        //    System.IO.File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(serializable));
        //}

        //public override void Load(string path)
        //{
        //    // check if the file at path exists, and load it if it does
        //    if (!System.IO.File.Exists(path))
        //        throw new System.IO.FileNotFoundException("File not found", path);

        //    AdvancedMaterialSerializable serializable = Newtonsoft.Json.JsonConvert.DeserializeObject<AdvancedMaterialSerializable>(System.IO.File.ReadAllText(path));

        //    MaterialDescription = new AdvancedMaterialDescription()
        //    {
        //        Metallicness = Engine.ContentManager.GetTexture(serializable.MetallicnessTexturePath),
        //        Roughness = Engine.ContentManager.GetTexture(serializable.RoughnessTexturePath),
        //        AmbientOcclusion = Engine.ContentManager.GetTexture(serializable.AmbientOcclusionTexturePath),
        //        Albedo = Engine.ContentManager.GetTexture(serializable.AlbedoTexturePath),
        //        Normals = Engine.ContentManager.GetTexture(serializable.NormalsTexturePath)
        //    };
        //}

        private static readonly string[] fileNames = new[]
        {
            "metallicness.png",
            "roughness.png",
            "ao.png",
            "albedo.png",
            "normals.png"
        };

        public static AdvancedMaterial LoadFromZip(string path)
        {
            // check if the file at path exists, and load it if it does
            if (!System.IO.File.Exists(path))
                Engine.Logger.Log(Logging.LogLevel.Fatal, $"File({path}) not found");

            // extract the zip file to a temporary directory
            string tempDirectory =
                System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName();
            System.IO.Compression.ZipFile.ExtractToDirectory(path, tempDirectory);

            var files = Directory.GetFiles(tempDirectory).Select(Path.GetFileName).ToArray();
            List<string> missingFiles = fileNames
                .Where(fileName => !files.Contains(fileName))
                .ToList();

            if (missingFiles.Count > 0)
            {
                Engine.Logger.Log(
                    Logging.LogLevel.Error,
                    $"Material({path}) is invalid!\n\nThe following textures could not be located:\n{string.Join(", ", missingFiles)}.\n"
                );
            }

            // load the material from the directory
            AdvancedMaterial material = LoadFromDirectory(path, tempDirectory);

            // delete the temporary directory
            System.IO.Directory.Delete(tempDirectory, true);

            return material;
        }

        // loads all the textures in the directory and returns a material description
        public static AdvancedMaterial LoadFromDirectory(string realPath, string path)
        {
            return new AdvancedMaterial(realPath)
            {
                MaterialDescription = new AdvancedMaterialDescription()
                {
                    Metallicness = Engine.Content.GenerateNamedTexture(
                        Path.Combine(realPath, "metallicness.png"),
                        Path.Combine(path, "metallicness.png")
                    ),
                    Roughness = Engine.Content.GenerateNamedTexture(
                        Path.Combine(realPath, "roughness.png"),
                        Path.Combine(path, "roughness.png")
                    ),
                    AmbientOcclusion = Engine.Content.GenerateNamedTexture(
                        Path.Combine(realPath, "ao.png"),
                        Path.Combine(path, "ao.png")
                    ),
                    Albedo = Engine.Content.GenerateNamedTexture(
                        Path.Combine(realPath, "albedo.png"),
                        Path.Combine(path, "albedo.png")
                    ),
                    Normals = Engine.Content.GenerateNamedTexture(
                        Path.Combine(realPath, "normals.png"),
                        Path.Combine(path, "normals.png")
                    )
                }
            };
        }

        public void Destroy()
        {
            var textureNames = new[]
            {
                Path.Combine(realPath, "metallicness.png"),
                Path.Combine(realPath, "roughness.png"),
                Path.Combine(realPath, "ao.png"),
                Path.Combine(realPath, "albedo.png"),
                Path.Combine(realPath, "normals.png")
            };
            foreach (var item in textureNames)
                Engine.Content.DeleteTexture(item);
        }
    }
}
