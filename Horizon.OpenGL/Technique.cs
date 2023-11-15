using Horizon.Core.Assets;
using Horizon.Core.Content;
using Horizon.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.OpenGL;

internal class TechniqueUniformManager : IndexManager
{
    public TechniqueUniformManager(in IGLObject obj)
        : base(obj) { }

    protected override int GetIndex(in string name)
        => BaseGameEngine.GL.GetUniformLocation(glObject.Handle, name);
}

public class Technique
{
    private Shader shader;
    private TechniqueUniformManager uniformManager;

    public Technique(in Shader shader)
    {
        this.shader = shader;
        uniformManager = new(shader);
    }

    /// <summary>
    /// Sets the specified uniform to a specified value, the uniform index is guaranteed to be cached.
    /// </summary>
    public void SetUniform(in string name, in object? value)
    {
        int location = uniformManager.GetLocation(name);
        if (location == -1 || value is null) // If GetUniformLocation returns -1, the uniform is not found.
        {
            //Engine.Logger.Log(
            //    LogLevel.Error,
            //    $"Shader[{Handle}] Uniform('{name}') Error! Check if the uniform is defined!"
            //);
            return;
        }

        switch (value)
        {
            case int intValue:
                BaseGameEngine.GL.Uniform1(location, intValue);
                break;

            case float floatValue:
                BaseGameEngine.GL.Uniform1(location, floatValue);
                break;

            case uint uintValue:
                BaseGameEngine.GL.Uniform1(location, uintValue);
                break;

            case Vector2 vector2Value:
                BaseGameEngine.GL.Uniform2(location, vector2Value.X, vector2Value.Y);
                break;

            case Vector3 vector3Value:
                BaseGameEngine.GL.Uniform3(location, vector3Value.X, vector3Value.Y, vector3Value.Z);
                break;

            case Vector4 vector4Value:
                BaseGameEngine.GL.Uniform4(
                    location,
                    vector4Value.X,
                    vector4Value.Y,
                    vector4Value.Z,
                    vector4Value.W
                );
                break;

            case Matrix4x4 matrixValue:
                unsafe // Don't wanna make the whole method unsafe for a single call.
                {
                    BaseGameEngine.GL.UniformMatrix4(location, 1, false, (float*)&matrixValue);
                }
                break;

            case bool boolValue:
                BaseGameEngine.GL.Uniform1(location, boolValue ? 1 : 0);
                break;

            default:
                //Engine.Logger.Log(
                //    LogLevel.Error,
                //    $"Shader[{Handle}] Attempt to upload uniform of unsupported data type {value!.GetType()}."
                //);
                return;
        }
    }
    public void Bind() => BaseGameEngine.GL.UseProgram(shader.Handle);
    public void Unbind() => BaseGameEngine.GL.UseProgram(Shader.Invalid.Handle);
}
