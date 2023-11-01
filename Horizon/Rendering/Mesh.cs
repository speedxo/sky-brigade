using Horizon.Primitives;
using Microsoft.Extensions.Options;

namespace Horizon.Rendering;

/// <summary>
/// Interface to interface geometry with the Horizon rendering backend.
/// </summary>
public abstract class Mesh<T> : IDisposable, IDrawable
    where T: unmanaged
{
    /// <summary>
    /// The material used for rendering the mesh.
    /// </summary>
    public Material Material { get; set; }

    /// <summary>
    /// Delegate used to obtain data for the mesh.
    /// </summary>
    /// <returns>The mesh data containing vertices and elements of the mesh.</returns>
    public delegate IMeshData<T> MeshDataDelegate();

    /// <summary>
    /// Sets a uniform in the material's shader with a value.
    /// </summary>
    public void SetUniform(string name, object? value) =>
        Material.Technique.SetUniform(name, value);

    /// <summary>
    /// Transfers the ownership of the specified vertex and index arrays to the mesh.
    /// </summary>
    public abstract void Load(in IMeshData<T> data, in Material? mat = null);

    /// <summary>
    /// Transfers the ownership of the specified data loader delegate to the mesh.
    /// </summary>
    public void Load(in MeshDataDelegate dataDelegate, in Material? mat=null) 
        => Load(dataDelegate(), mat);

    public abstract void Dispose();
    public abstract void Draw(float dt, ref RenderOptions options);
}