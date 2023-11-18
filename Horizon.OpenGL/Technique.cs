using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Horizon.Content;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Managers;
using Silk.NET.OpenGL;
using Shader = Horizon.OpenGL.Assets.Shader;

namespace Horizon.OpenGL;

public class Technique
{
    private Shader shader;
    private TechniqueUniformManager uniformManager;
    private TechniqueResourceIndexManager resourceManager;

    public Technique() { }

    public Technique(in Shader shader)
    {
        this.shader = shader;
        uniformManager = new(shader);
        resourceManager = new(shader);
    }

    /// <summary>
    /// Sets the internal IGLObject Shader (if null), useful for derived classes.
    /// </summary>
    protected void SetShader(in Shader inShader)
    {
        shader ??= inShader;
        uniformManager ??= new(shader);
        resourceManager ??= new(shader);
    }

    public Technique(AssetCreationResult<Shader> asset)
        : this(asset.Asset) { }

    public void BindBuffer(in string name, in BufferObject bufferObject)
    {
        bufferObject.Bind();
        ContentManager
            .GL
            .BindBufferBase(
                BufferTargetARB.ShaderStorageBuffer,
                resourceManager.GetLocation(name),
                bufferObject.Handle
            );
    }

    /// <summary>
    /// Sets the specified uniform to a specified value, the uniform index is guaranteed to be cached.
    /// </summary>
    public void SetUniform(in string name, in object? value)
    {
        int location = (int)uniformManager.GetLocation(name);
        //if (location == -1 || value is null) // If GetUniformLocation returns -1, the uniform is not found.
        //{
        //    //ConcurrentLogger.Instance.Log(
        //    //    LogLevel.Error,
        //    //    $"Shader[{Handle}] Uniform('{name}') Error! Check if the uniform is defined!"
        //    //);
        //    return;
        //}

        switch (value)
        {
            case int intValue:
                ContentManager.GL.Uniform1(location, intValue);
                break;

            case float floatValue:
                ContentManager.GL.Uniform1(location, floatValue);
                break;

            case uint uintValue:
                ContentManager.GL.Uniform1(location, uintValue);
                break;

            case Vector2 vector2Value:
                ContentManager.GL.Uniform2(location, vector2Value.X, vector2Value.Y);
                break;

            case Vector3 vector3Value:
                ContentManager
                    .GL
                    .Uniform3(location, vector3Value.X, vector3Value.Y, vector3Value.Z);
                break;

            case Vector4 vector4Value:
                ContentManager
                    .GL
                    .Uniform4(
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
                    ContentManager.GL.UniformMatrix4(location, 1, false, (float*)&matrixValue);
                }
                break;

            case bool boolValue:
                ContentManager.GL.Uniform1(location, boolValue ? 1 : 0);
                break;

            default:
                //ConcurrentLogger.Instance.Log(
                //    LogLevel.Error,
                //    $"Shader[{Handle}] Attempt to upload uniform of unsupported data type {value!.GetType()}."
                //);
                return;
        }
    }

    /// <summary>
    /// Called after the shader is bound.
    /// </summary>
    protected virtual void SetUniforms() { }

    public void Bind()
    {
        ContentManager.GL.UseProgram(shader.Handle);
        SetUniforms();
    }

    public void Unbind() => ContentManager.GL.UseProgram(Shader.Invalid.Handle);
}
