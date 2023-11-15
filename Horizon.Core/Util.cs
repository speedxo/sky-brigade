using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Core;

public static class Util
{
    /// <summary>
    /// Splits a string into an array of lines, platform agnostic.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns></returns>
    public static IEnumerable<string> SplitToLines(this string input)
    {
        if (input == null)
        {
            yield break;
        }

        using System.IO.StringReader reader = new System.IO.StringReader(input);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            yield return line;
        }
    }

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