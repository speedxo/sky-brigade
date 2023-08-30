using System.Numerics;
using System.Runtime.InteropServices;

namespace Gravitator2D;

[StructLayout(LayoutKind.Sequential)]
public struct Body
{
    public Vector2 Position;
    public float Radius;
    public Vector3 Color;
    private int _spacer1;
    private int _spacer2;
}
