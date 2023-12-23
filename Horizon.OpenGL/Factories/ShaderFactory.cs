using System.Reflection.Metadata;
using Horizon.Content;
using Horizon.Content.Descriptions;
using Horizon.Core;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Managers;
using Horizon.OpenGL.Processors;
using Silk.NET.OpenGL;
using Shader = Horizon.OpenGL.Assets.Shader;

namespace Horizon.OpenGL.Factories;

/// <summary>
/// Asset factory for creating instances of <see cref="Shader"/>.
/// </summary>
public class ShaderFactory : IAssetFactory<Shader, ShaderDescription>
{
    public static AssetCreationResult<Shader> Create(in ShaderDescription description)
    {
        // Program compilation result.
        var result = new Shader { Handle = ObjectManager.GL.CreateProgram() };

        // Enumerate and compile each program in the source.
        var aggregatedResults = CompileShaderSources(description.Definitions).ToArray();
        var failed = aggregatedResults.Where(res => res.Status == CompilationStatus.Fail).ToArray();

        // If any of our shaders failed to compile throw an error.
        if (failed.Any())
        {
            string errorMessage =
                "[ShaderFactory] Cannot compile Program({handle}) with failed shaders:\n\n";
            foreach (var item in failed)
                errorMessage += $"[{item.Handle}:{item.Status}] {item.ErrorMessage}\n";

            return new()
            {
                Asset = result,
                Message = errorMessage,
                Status = AssetCreationStatus.Failed
            };
        }

        // Attach each shader to the program.
        foreach (var res in aggregatedResults)
            ObjectManager.GL.AttachShader(result.Handle, res.Handle);

        // Attempt to link them together.
        ObjectManager.GL.LinkProgram(result.Handle);

        // (cleanup) Detach and delete each shader to the program.
        foreach (var res in aggregatedResults)
        {
            ObjectManager.GL.DetachShader(result.Handle, res.Handle);
            ObjectManager.GL.DeleteShader(res.Handle);
        }

        // Check for linking errors.
        ObjectManager.GL.GetProgram(result.Handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            return new()
            {
                Asset = result,
                Message =
                    $"Shader Failed to link with error: {ObjectManager.GL.GetProgramInfoLog(result.Handle)}",
                Status = AssetCreationStatus.Failed
            };
        }

        // Success
        //ObjectManager.Logger.Log(LogLevel.Debug, $"Shader[{handle}] created!");
        return new() { Asset = result, Status = AssetCreationStatus.Success };
    }

    /* Internal data structures to help transfer state information between stages. */
    private enum CompilationStatus
    {
        Pass = 0,
        Fail = 1
    }

    private readonly record struct CompilationResult(
        uint Handle,
        CompilationStatus Status,
        string ErrorMessage
    );

    /// <summary>
    /// Compiles the shader from source.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="source">The source.</param>
    /// <returns>A <see cref="CompilationResult"/> fully encapsulating the result of attempting to compile the shader, including the shader handle if successful.</returns>
    private static CompilationResult CompileShaderFromSource(in ShaderType type, in string source)
    {
        // Create a shader handle.
        uint handle = ObjectManager.GL.CreateShader(type);

        // Shader compilation result.
        CompilationResult result =
            new()
            {
                ErrorMessage = "",
                Status = CompilationStatus.Pass,
                Handle = handle
            };

        // Stream shader source into the gl shader.
        ObjectManager.GL.ShaderSource(handle, source);

        // Attempt to compile the shader.
        ObjectManager.GL.CompileShader(handle);

        // Get the compilation result.
        string infoLog = ObjectManager.GL.GetShaderInfoLog(handle);

        // If the result is empty then the compilation was a success.
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            return result with
            {
                Status = CompilationStatus.Fail,
                ErrorMessage = $"[ShaderFactory] Error compiling shader of type {type}: {infoLog}"
            };
        }

        // Return the result.
        return result;
    }

    /// <summary>
    /// Helper method to attempt to compile all the shaders in a program with directive support, returning an intermediate <see cref="CompilationResult" />.
    /// If file paths are provided to any shader, #include directive support is added.
    /// </summary>
    /// <param name="shaderDefinitions">The shader definitions.</param>
    private static IEnumerable<CompilationResult> CompileShaderSources(
        params ShaderDefinition[] shaderDefinitions
    )
    {
        using var preprocessor = new ShaderDirectiveProcessor();
        foreach (var (type, file, source) in shaderDefinitions)
        {
            // If a file is provided we can manually parse #include preprocessor statements for convenience.
            yield return file is null
                ? CompileShaderFromSource(
                    type,
                    preprocessor.ProcessSource(string.Empty, source.SplitToLines())
                )
                : CompileShaderFromSource(type, preprocessor.ProcessFile(file));
        }
    }
}
