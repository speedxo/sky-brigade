using Horizon.Core.Content;
using Horizon.Core.Primitives;
using Silk.NET.OpenGL;
using Horizon.Content.Processors;
using Horizon.Core;

using Shader = Horizon.Core.Assets.Shader;

namespace Horizon.Content.Factories;

/// <summary>
/// Asset factory for creating instances of <see cref="Shader"/>.
/// </summary>
public class ShaderFactory : IAssetFactory<Shader, ShaderDescription>
{
    public static Shader Create(in ShaderDescription description)
    {
        var result = CompileProgram(description.Definitions);

        if (result.Status == CompilationStatus.Fail)
        {
            throw new Exception(result.ErrorMessage);
        }

        return new Shader { Handle = result.Handle };
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
    private static CompilationResult CompileShaderFromSource(
        in ShaderType type,
        in string source
    )
    {
        // Create a shader handle.
        uint handle = BaseGameEngine.GL.CreateShader(type);

        // Shader compilation result.
        CompilationResult result =
            new()
            {
                ErrorMessage = "",
                Status = CompilationStatus.Pass,
                Handle = handle
            };

        // Stream shader source into the gl shader.
        BaseGameEngine.GL.ShaderSource(handle, source);

        // Attempt to compile the shader.
        BaseGameEngine.GL.CompileShader(handle);

        // Get the compilation result.
        string infoLog = BaseGameEngine.GL.GetShaderInfoLog(handle);

        // If the result is empty then the compilation was a success.
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            return result with
            {
                Status = CompilationStatus.Fail,
                ErrorMessage =
                    $"[ShaderFactory] Error compiling shader of type {type}: {infoLog}"
            };
        }

        // Return the result.
        return result;
    }

    /// <summary>
    /// Compiles and returns the handle to shader.
    /// </summary>
    private static CompilationResult CompileProgram(params ShaderDefinition[] shaderDefinitions)
    {
        // Ensure that both a fragment and vertex shader have been specified.
        if (
            !shaderDefinitions.Any(s => s.Type == ShaderType.FragmentShader)
            && shaderDefinitions.Any(s => s.Type == ShaderType.VertexShader)
        )
        {
            throw new InvalidOperationException(
                "[ShaderFactory] You are required to specify a vertex and fragment shader in order to compile a program."
            );
        }

        // Create the shader program.
        var handle = BaseGameEngine.GL.CreateProgram();

        // Program compilation result.
        var result = new CompilationResult(handle, CompilationStatus.Pass, "");

        // Enumerate and compile each program in the source.
        var aggregatedResults = CompileShaderSources(shaderDefinitions).ToArray();
        var failed = aggregatedResults
            .Where(res => res.Status == CompilationStatus.Fail)
            .ToArray();

        // If any of our shaders failed to compile throw an error.
        if (failed.Any())
        {
            string errorMessage =
                "[ShaderFactory] Cannot compile Program({handle}) with failed shaders:\n\n";
            foreach (var item in failed)
                errorMessage += $"[{item.Handle}:{item.Status}] {item.ErrorMessage}\n";

            return result with
            {
                ErrorMessage = errorMessage,
                Status = CompilationStatus.Fail
            };
        }

        // Attach each shader to the program.
        foreach (var res in aggregatedResults)
            BaseGameEngine.GL.AttachShader(handle, res.Handle);

        // Attempt to link them together.
        BaseGameEngine.GL.LinkProgram(handle);

        // (cleanup) Detach and delete each shader to the program.
        foreach (var res in aggregatedResults)
        {
            BaseGameEngine.GL.DetachShader(handle, res.Handle);
            BaseGameEngine.GL.DeleteShader(res.Handle);
        }

        // Check for linking errors.
        BaseGameEngine.GL.GetProgram(handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            return result with
            {
                ErrorMessage =
                    $"Shader[{handle}] Failed to link with error: {BaseGameEngine.GL.GetProgramInfoLog(handle)}",
                Status = CompilationStatus.Fail
            };
        }

        // Success
        //BaseGameEngine.Logger.Log(LogLevel.Debug, $"Shader[{handle}] created!");
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
