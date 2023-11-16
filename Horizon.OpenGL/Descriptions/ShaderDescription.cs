using Horizon.Content.Descriptions;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Descriptions;

/// <summary>
/// Contains the data needed to create a new <see cref="Shader"/>
/// </summary>
public readonly struct ShaderDescription : IAssetDescription
{
    public readonly ShaderDefinition[] Definitions { get; init; }

    /// <summary>
    /// Creates a shader description from files contained in a directory.
    /// </summary>
    /// <param name="path">The path to be searched.</param>
    /// <param name="name">The common file name of all shaders.</param>
    public static ShaderDescription FromPath(in string path, in string name)
    {
        if (!Path.Exists(path))
            return default;

        List<ShaderDefinition> definitions = new();

        foreach (var file in Directory.GetFiles(path, $"{name}.*"))
        {
            // yummy expensive string manipulations
            string ext = Path.GetExtension(file).ToLower().Trim('.');

            definitions.Add(
                new ShaderDefinition
                {
                    Type = ext switch
                    {
                        "vert" or "vs" or "vsh" => ShaderType.VertexShader,
                        "frag" or "fs" or "fsh" => ShaderType.FragmentShader,
                        "comp" or "cs" or "csh" => ShaderType.ComputeShader,
                        _ => throw new Exception($"Unrecognized shader file extension '{ext}'!")
                    },
                    File = file
                }
            );
        }

        return new ShaderDescription { Definitions = definitions.ToArray() };
    }
}
