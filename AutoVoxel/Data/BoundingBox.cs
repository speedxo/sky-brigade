using System.Numerics;

namespace AutoVoxel.Data;

public struct BoundingBox
{
    public Vector3 Min, Max;

    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public bool Contains(Vector3 point)
    {
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y &&
               point.Z >= Min.Z && point.Z <= Max.Z;
    }

    public Vector3 Center
    {
        get { return (Min + Max) / 2f; }
    }
}
