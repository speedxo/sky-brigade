using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Managers;

namespace Horizon.Rendering;

/// <summary>
/// Material system, please see <see cref="MaterialFactory"/>.
/// </summary>
public class Material
{
    public static Material Invalid { get; } =
        new Material
        {
            Attachments = new()
            {
                { MaterialAttachment.Albedo, Texture.Invalid },
                { MaterialAttachment.Normal, Texture.Invalid }
            }
        };

    public Dictionary<MaterialAttachment, Texture> Attachments { get; init; }

    public uint Width { get; init; }
    public uint Height { get; init; }

    public Texture GetAttachment(in MaterialAttachment type) => Attachments[type];

    /// <summary>
    /// Binds a specified attachment to a texture unit.
    /// </summary>
    public void BindAttachment(in MaterialAttachment type, in uint index) =>
        ContentManager.GL.BindTextureUnit(index, Attachments[type].Handle);
}
