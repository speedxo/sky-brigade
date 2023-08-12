using System.Numerics;
using System.Runtime.InteropServices;

namespace SkyBrigade.Engine.Rendering
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