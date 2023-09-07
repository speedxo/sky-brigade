using System.Numerics;
using System.Runtime.InteropServices;

namespace Horizon.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialRenderOptions
    {
        public int DefferedRenderLayer;
        public float Gamma;
        public float AmbientStrength;
        public Vector4 Color;
    }
}
