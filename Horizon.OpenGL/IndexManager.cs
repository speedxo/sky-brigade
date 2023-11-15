using Horizon.Core.Primitives;

namespace Horizon.OpenGL;

/// <summary>
/// Internal class for managing GL indexes associated with strings
/// </summary>
internal abstract class IndexManager
{
    protected readonly Dictionary<string, int> namedIndices;
    protected readonly IGLObject glObject;

    public IndexManager(in IGLObject obj)
    {
        glObject = obj;
        namedIndices = new();
    }

    public int GetLocation(in string name)
    {
        if (!namedIndices.ContainsKey(name))
            namedIndices.Add(name, GetIndex(name));

        return namedIndices[name];
    }

    protected abstract int GetIndex(in string name);
}
