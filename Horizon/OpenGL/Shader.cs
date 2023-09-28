using Horizon.GameEntity;
using Horizon.Logging;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Horizon.OpenGL;

// FIXME cross static ref to Entity.Engine
public class Shader : IDisposable
{
    //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    public uint Handle { get; private init; }

    public static Shader Default
    {
        get => Entity.Engine.Content.GetShader("default");
    }

    private Dictionary<string, int> uniformIndexes;
    private Dictionary<string, uint> uniformBlockIndexes;

    public Shader(uint handle)
    {
        Handle = handle;
        uniformIndexes = new();
        uniformBlockIndexes = new();
    }

    public uint GetUniformBlockIndex(string blockName)
    {
        if (Handle == 0)
            throw new InvalidOperationException("Shader program is not created.");

        if (!uniformBlockIndexes.ContainsKey(blockName))
            uniformBlockIndexes.Add(
                blockName,
                Entity.Engine.GL.GetUniformBlockIndex(Handle, blockName)
            );

        return uniformBlockIndexes[blockName];
    }

    public virtual void Use() => Entity.Engine.GL.UseProgram(Handle);

    private int GetUniformLocation(string name)
    {
        if (!uniformIndexes.ContainsKey(name))
            uniformIndexes.Add(name, Entity.Engine.GL.GetUniformLocation(Handle, name));

        return uniformIndexes[name];
    }

    public unsafe void SetUniform(string name, in object? value)
    {
        int location = GetUniformLocation(name);
        if (location == -1 || value is null) // If GetUniformLocation returns -1, the uniform is not found.
        {
            //Entity.Engine.Logger.Log(LogLevel.Error, $"{name} uniform not found on shader.");
            return;
        }

        switch (value)
        {
            case int intValue:
                Entity.Engine.GL.Uniform1(location, intValue);
                break;

            case float floatValue:
                Entity.Engine.GL.Uniform1(location, floatValue);
                break;

            case uint uintValue:
                Entity.Engine.GL.Uniform1(location, uintValue);
                break;

            case Vector2 vector2Value:
                Entity.Engine.GL.Uniform2(location, vector2Value.X, vector2Value.Y);
                break;

            case Vector3 vector3Value:
                Entity.Engine.GL.Uniform3(
                    location,
                    vector3Value.X,
                    vector3Value.Y,
                    vector3Value.Z
                );
                break;

            case Vector4 vector4Value:
                Entity.Engine.GL.Uniform4(
                    location,
                    vector4Value.X,
                    vector4Value.Y,
                    vector4Value.Z,
                    vector4Value.W
                );
                break;

            case Matrix4x4 matrixValue:
                Entity.Engine.GL.UniformMatrix4(location, 1, false, (float*)&matrixValue);
                break;

            case bool boolValue:
                Entity.Engine.GL.Uniform1(location, boolValue ? 1 : 0);
                break;

            default:
                Entity.Engine.Logger.Log(
                    LogLevel.Error,
                    $"Attempt to upload uniform of unsupported data type {value!.GetType()}."
                );
                return;
        }
    }

    public void Dispose()
    {
        // Remember to delete the program when we are done.
        Entity.Engine.GL.DeleteProgram(Handle);
        Entity.Engine.Logger.Log(LogLevel.Debug, $"Shader[{Handle}] destroyed!");
    }

    public virtual void End() => Entity.Engine.GL.UseProgram(0);

    private static uint LoadShader(ShaderType type, string path)
    {
        if (!File.Exists(path))
            Entity.Engine.Logger.Log(LogLevel.Fatal, $"Shader file not found at {path}");

        string src = File.ReadAllText(path);

        uint handle = Entity.Engine.GL.CreateShader(type);
        Entity.Engine.GL.ShaderSource(handle, src);
        Entity.Engine.GL.CompileShader(handle);
        string infoLog = Entity.Engine.GL.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            // LogLevel of fatal throws an exception
            Entity.Engine.Logger.Log(
                LogLevel.Fatal,
                $"Error compiling shader of type {type}, failed with error {infoLog}"
            );
        }
        return handle;
    }

    private static uint LoadShaderFromSource(ShaderType type, string source)
    {
        uint handle = Entity.Engine.GL.CreateShader(type);
        Entity.Engine.GL.ShaderSource(handle, source);
        Entity.Engine.GL.CompileShader(handle);
        string infoLog = Entity.Engine.GL.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            // LogLevel of fatal throws an exception
            Entity.Engine.Logger.Log(
                LogLevel.Fatal,
                $"Error compiling shader of type {type}, failed with error {infoLog}"
            );
        }
        return handle;
    }

    public static Shader CompileShaderFromSource(string vertexSource, string fragmentSource)
    {
        //Load the individual shaders.
        uint vertex = LoadShaderFromSource(ShaderType.VertexShader, vertexSource);
        uint fragment = LoadShaderFromSource(ShaderType.FragmentShader, fragmentSource);

        //Create the shader program.
        var handle = Entity.Engine.GL.CreateProgram();

        //Attach the individual shaders.
        Entity.Engine.GL.AttachShader(handle, vertex);
        Entity.Engine.GL.AttachShader(handle, fragment);
        Entity.Engine.GL.LinkProgram(handle);

        //Check for linking errors.
        Entity.Engine.GL.GetProgram(handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
            Entity.Engine.Logger.Log(
                LogLevel.Fatal,
                $"Program failed to link with error: {Entity.Engine.GL.GetProgramInfoLog(handle)}"
            );
        Entity.Engine.Logger.Log(LogLevel.Debug, $"Shader[{handle}] created!");

        //Detach and delete the shaders
        Entity.Engine.GL.DetachShader(handle, vertex);
        Entity.Engine.GL.DetachShader(handle, fragment);
        Entity.Engine.GL.DeleteShader(vertex);
        Entity.Engine.GL.DeleteShader(fragment);

        return new Shader(handle);
    }

    public static Shader CompileShader(
        string vertexPath,
        string fragmentPath,
        string geometryPath = ""
    )
    {
        bool generateGeometry = !string.IsNullOrEmpty(geometryPath);

        //Load the individual shaders.
        uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
        uint? geometry = generateGeometry
            ? LoadShader(ShaderType.GeometryShader, geometryPath)
            : null;

        //Create the shader program.
        var handle = Entity.Engine.GL.CreateProgram();

        //Attach the individual shaders.
        Entity.Engine.GL.AttachShader(handle, vertex);
        Entity.Engine.GL.AttachShader(handle, fragment);
        if (generateGeometry)
            Entity.Engine.GL.AttachShader(handle, geometry!.Value);
        Entity.Engine.GL.LinkProgram(handle);

        //Check for linking errors.
        Entity.Engine.GL.GetProgram(handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
            Entity.Engine.Logger.Log(
                LogLevel.Fatal,
                $"Program failed to link with error: {Entity.Engine.GL.GetProgramInfoLog(handle)}"
            );
        Entity.Engine.Logger.Log(LogLevel.Debug, $"Shader[{handle}] created!");

        //Detach and delete the shaders
        Entity.Engine.GL.DetachShader(handle, vertex);
        Entity.Engine.GL.DetachShader(handle, fragment);
        if (generateGeometry)
            Entity.Engine.GL.DetachShader(handle, geometry!.Value);

        Entity.Engine.GL.DeleteShader(vertex);
        Entity.Engine.GL.DeleteShader(fragment);
        if (generateGeometry)
            Entity.Engine.GL.DeleteShader(geometry!.Value);

        return new Shader(handle);
    }
}
