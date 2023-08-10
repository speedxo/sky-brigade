using Silk.NET.OpenGL;
using SkyBrigade.Engine.Logging;
using System.Numerics;
using System.Text;

namespace SkyBrigade.Engine.OpenGL;

public class Shader : IDisposable
{
    //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    public uint Handle { get; private init; }

    private Dictionary<string, int> uniformIndexes;

    public Shader(uint handle)
    {
        Handle = handle;
        uniformIndexes = new Dictionary<string, int>();
    }

    public uint GetUniformBlockIndex(string blockName)
    {
        if (Handle == 0)
            throw new InvalidOperationException("Shader program is not created.");

        return GameManager.Instance.Gl.GetUniformBlockIndex(Handle, blockName);
    }

    public virtual void Use() => GameManager.Instance.Gl.UseProgram(Handle);

    private int GetUniformLocation(string name)
    {
        if (!uniformIndexes.ContainsKey(name))
            uniformIndexes.Add(name, GameManager.Instance.Gl.GetUniformLocation(Handle, name));

        return uniformIndexes[name];
    }

    public unsafe void SetUniform(string name, object? value)
    {
        int location = GetUniformLocation(name);
        if (location == -1) // If GetUniformLocation returns -1, the uniform is not found.
        {
            GameManager.Instance.Logger.Log(LogLevel.Error, $"{name} uniform not found on shader.");
            return;
        }

        switch (value)
        {
            case int intValue:
                GameManager.Instance.Gl.Uniform1(location, intValue);
                break;

            case float floatValue:
                GameManager.Instance.Gl.Uniform1(location, floatValue);
                break;

            case uint uintValue:
                GameManager.Instance.Gl.Uniform1(location, uintValue);
                break;

            case Vector2 vector2Value:
                GameManager.Instance.Gl.Uniform2(location, vector2Value.X, vector2Value.Y);
                break;

            case Vector3 vector3Value:
                GameManager.Instance.Gl.Uniform3(location, vector3Value.X, vector3Value.Y, vector3Value.Z);
                break;

            case Vector4 vector4Value:
                GameManager.Instance.Gl.Uniform4(location, vector4Value.X, vector4Value.Y, vector4Value.Z, vector4Value.W);
                break;

            case Matrix4x4 matrixValue:
                GameManager.Instance.Gl.UniformMatrix4(location, 1, false, (float*)&matrixValue);
                break;

            default:
                GameManager.Instance.Logger.Log(LogLevel.Error, $"Attempt to upload uniform of unsupported data type {value.GetType()}.");
                return;
        }
    }


    public void Dispose()
    {
        // Remember to delete the program when we are done.
        GameManager.Instance.Gl.DeleteProgram(Handle);
        GameManager.Instance.Logger.Log(LogLevel.Debug, $"Shader[{Handle}] destroyed!");
    }

    public virtual void End() => GameManager.Instance.Gl.UseProgram(0);

    private static uint LoadShader(ShaderType type, string path)
    {
        if (!File.Exists(path))
            GameManager.Instance.Logger.Log(LogLevel.Fatal, $"Shader file not found at {path}");

        string src = File.ReadAllText(path);

        uint handle = GameManager.Instance.Gl.CreateShader(type);
        GameManager.Instance.Gl.ShaderSource(handle, src);
        GameManager.Instance.Gl.CompileShader(handle);
        string infoLog = GameManager.Instance.Gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            // LogLevel of fatal throws an exception
            GameManager.Instance.Logger.Log(LogLevel.Fatal, $"Error compiling shader of type {type}, failed with error {infoLog}");
        }
        return handle;
    }

    public static Shader CompileShader(string vertexPath, string fragmentPath)
    {
        //Load the individual shaders.
        uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);

        //Create the shader program.
        var handle = GameManager.Instance.Gl.CreateProgram();

        //Attach the individual shaders.
        GameManager.Instance.Gl.AttachShader(handle, vertex);
        GameManager.Instance.Gl.AttachShader(handle, fragment);
        GameManager.Instance.Gl.LinkProgram(handle);

        //Check for linking errors.
        GameManager.Instance.Gl.GetProgram(handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
            GameManager.Instance.Logger.Log(LogLevel.Fatal, $"Program failed to link with error: {GameManager.Instance.Gl.GetProgramInfoLog(handle)}");
        GameManager.Instance.Logger.Log(LogLevel.Debug, $"Shader[{handle}] created!");

        //Detach and delete the shaders
        GameManager.Instance.Gl.DetachShader(handle, vertex);
        GameManager.Instance.Gl.DetachShader(handle, fragment);
        GameManager.Instance.Gl.DeleteShader(vertex);
        GameManager.Instance.Gl.DeleteShader(fragment);

        return new Shader(handle);
    }

}