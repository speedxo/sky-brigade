using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Horizon.OpenGL.Assets;

namespace Horizon.OpenGL;

[StructLayout(LayoutKind.Sequential)]
public struct DrawElementsIndirectCommand
{
    /// <summary>
    /// Number of elements.
    /// </summary>
    public uint count;
    /// <summary>
    /// Number of instances to draw.
    /// </summary>
    public uint instanceCount;
    /// <summary>
    /// Specifies a byte offset (cast to a pointer type) into the buffer bound to GL_ELEMENT_ARRAY_BUFFER to start reading indices from.
    /// </summary>
    public uint firstIndex;
    /// <summary>
    /// Specifies a constant that should be added to each element of indices​ when chosing elements from the enabled vertex arrays.
    /// </summary>
    public uint baseVertex;
    /// <summary>
    /// Specifies the base instance for use in fetching instanced vertex attributes.
    /// </summary>
    public uint baseInstance;
}