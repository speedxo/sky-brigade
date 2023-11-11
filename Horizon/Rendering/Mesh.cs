using Horizon.Primitives;

namespace Horizon.Rendering;

/// <summary>
/// Interface to interface geometry with the Horizon rendering backend.
/// </summary>
public abstract class Mesh<T> : IDisposable, IDrawable
    where T : unmanaged
{
    protected const string UNIFORM_VIEW_MATRIX = "uView";
    protected const string UNIFORM_MODEL_MATRIX = "uModel";
    protected const string UNIFORM_PROJECTION_MATRIX = "uProjection";
    protected const string UNIFORM_USE_WIREFRAME = "uWireframeEnabled";

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

    /// <summary>Loads the specified data.</summary>
    /// <param name="data">The data.</param>
    /// <param name="mat">The mat.</param>
    public abstract void Load(in IMeshData<T> data, in Material? mat = null);

    /// <summary>Loads the specified data delegate.</summary>
    /// <param name="dataDelegate">The data delegate.</param>
    /// <param name="mat">The mat.</param>
    public void Load(in MeshDataDelegate dataDelegate, in Material? mat = null) =>
        Load(dataDelegate(), mat);

    public abstract void Dispose();

    /// <summary>Draws the current object using the provided render options.</summary>
    /// <param name="dt">The elapsed time since the last render call.</param>
    /// <param name="options">Optional render options. If not provided, default options will be used.</param>
    public abstract void Render(float dt, ref RenderOptions options);
}
