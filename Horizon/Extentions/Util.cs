using Silk.NET.OpenGL;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Horizon.Extentions
{
    public static class Util
    {
        [Pure]
        public static float Clamp(float value, float min, float max)
        {
            return value < min
                ? min
                : value > max
                    ? max
                    : value;
        }

        [Conditional("DEBUG")]
        public static void CheckGlError(this GL gl, string title)
        {
            var error = gl.GetError();
            if (error != GLEnum.NoError)
            {
                Debug.Print($"{title}: {error}");
            }
        }
    }
}
