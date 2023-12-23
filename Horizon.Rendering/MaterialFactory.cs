using Horizon.Engine;
using Horizon.OpenGL.Assets;

namespace Horizon.Rendering;

/// <summary>
/// Creates a material from a sequence of images.
/// </summary>
public static class MaterialFactory
{
    public static char Delimiter { get; } = '_';

    private static readonly Dictionary<string, MaterialAttachment> _fileSuffixes =
        new()
        {
            { $"{Delimiter}albedo", MaterialAttachment.Albedo },
            { $"{Delimiter}normal", MaterialAttachment.Normal },
            { $"{Delimiter}specular", MaterialAttachment.Specular }
        };

    /// <summary>
    /// Creates a material from a sequence of images located inside a directory with a common name, each image attachment is identified by a suffix seperated from its name by <see cref="MaterialFactory.Delimiter"/>.
    /// </summary>
    /// <param name="directory">The folder the images are in.</param>
    /// <param name="name">The common name of the images.</param>
    public static Material Create(in string directory, in string name)
    {
        if (!Directory.Exists(directory))
            return Material.Invalid;

        var attachments = new Dictionary<MaterialAttachment, Texture>();

        // loop through all files
        foreach (var fullFile in Directory.GetFiles(directory))
        {
            // check if the file follows the nomenclature
            string rawFileName = Path.GetFileNameWithoutExtension(fullFile);
            int lastIndex = rawFileName.LastIndexOf(Delimiter);

            if (lastIndex < 0)
                continue; // file name doesnt have delimiter

            // get the name of the file excluding any suffixes
            string fileName = rawFileName[..lastIndex];

            // check if it matches
            if (fileName.CompareTo(name) == 0)
            {
                // check what attachment we are
                string identifier = rawFileName[lastIndex..];

                // load the image
                var texture = GameEngine
                    .Instance
                    .ObjectManager
                    .Textures
                    .CreateOrGet(
                        $"{name}{identifier}",
                        new OpenGL.Descriptions.TextureDescription { Path = fullFile }
                    );

                // add it to attachment list
                attachments.Add(_fileSuffixes[identifier], texture);
            }
        }

        // check which attachments we dont have and attach invalid textures
        foreach (var (_, type) in _fileSuffixes)
        {
            if (!attachments.ContainsKey(type))
                attachments.Add(type, Texture.Invalid);
        }

        // return the material
        return new Material()
        {
            Attachments = attachments,
            Width = attachments[MaterialAttachment.Albedo].Width,
            Height = attachments[MaterialAttachment.Albedo].Height
        };
    }
}
