using Horizon.Extentions;
using Horizon.GameEntity;
using Horizon.Logging;
using System.Reflection.Metadata.Ecma335;
using GLEnum = Silk.NET.OpenGL.GLEnum;
using ShaderType = Silk.NET.OpenGL.ShaderType;

namespace Horizon.Content
{
    /// <summary>Static Factory to instantiate shaders intended for the ShaderContentManager to manage.</summary>
    /// <seealso cref="Horizon.Content.AssetFactory" />
    public partial class ShaderFactory : AssetFactory
    {
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
            uint handle = Engine.GL.CreateShader(type);

            // Shader compilation result.
            CompilationResult result =
                new()
                {
                    ErrorMessage = "",
                    Status = CompilationStatus.Pass,
                    Handle = handle
                };

            // Stream shader source into the gl shader.
            Engine.GL.ShaderSource(handle, source);

            // Attempt to compile the shader.
            Engine.GL.CompileShader(handle);

            // Get the compilation result.
            string infoLog = Engine.GL.GetShaderInfoLog(handle);

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
            var handle = Engine.GL.CreateProgram();

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
                Engine.GL.AttachShader(handle, res.Handle);

            // Attempt to link them together.
            Engine.GL.LinkProgram(handle);

            // (cleanup) Detach and delete each shader to the program.
            foreach (var res in aggregatedResults)
            {
                Engine.GL.DetachShader(handle, res.Handle);
                Engine.GL.DeleteShader(res.Handle);
            }

            // Check for linking errors.
            Entity.Engine.GL.GetProgram(handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                return result with
                {
                    ErrorMessage =
                        $"Shader[{handle}] Failed to link with error: {Entity.Engine.GL.GetProgramInfoLog(handle)}",
                    Status = CompilationStatus.Fail
                };
            }

            // Success
            Entity.Engine.Logger.Log(LogLevel.Debug, $"Shader[{handle}] created!");
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

        /// <summary>
        /// Compiles a program from a params array of shader definitions.
        /// If file paths are provided to any shader, pre-processing through the <see cref="ShaderDirectiveProcessor"/> is done here.
        /// </summary>
        public static Shader CompileFromDefinitions(params ShaderDefinition[] shaderDefinitions)
        {
            var result = CompileProgram(shaderDefinitions);

            if (result.Status == CompilationStatus.Fail)
            {
                Engine.Logger.Log(LogLevel.Fatal, result.ErrorMessage);
                throw new Exception(result.ErrorMessage);
            }

            return Engine.Content.Shaders.Add(new Shader(result.Handle));
        }

        /// <summary>
        /// Returns a program compiled from an inferred vertex and fragment shader specified in a directory.
        /// If file paths are provided to any shader, #include directive support is added.
        /// </summary>
        /// <param name="path">The path containing the shaders.</param>
        /// <param name="name">The matching names of the two shaders ending in .vert and .frag for the vertex and fragment shaders respectively.</param>
        public static Shader CompileNamed(string path, string name)
        {
            string vertPath = Path.Combine(path, $"{name}.vert");
            string fragPath = Path.Combine(path, $"{name}.frag");

            EnsureFileExists(vertPath, "vertex");
            EnsureFileExists(fragPath, "fragment");

            return CompileFromDefinitions(
                new ShaderDefinition { File = vertPath, Type = ShaderType.VertexShader },
                new ShaderDefinition { File = fragPath, Type = ShaderType.FragmentShader }
            );
        }

        private static void EnsureFileExists(string filePath, string shaderType)
        {
            if (!File.Exists(filePath))
            {
                string message =
                    $"[ShaderFactory] Cannot compile a program without a {shaderType} shader.";
                Engine.Logger.Log(LogLevel.Fatal, message);
                throw new FileNotFoundException(message);
            }
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                string message =
                    $"[ShaderFactory] Directory '{directoryPath}' doesn't exist! Therefore we cannot load a shader.";
                Engine.Logger.Log(LogLevel.Fatal, message);
                throw new DirectoryNotFoundException(message);
            }
        }
    }
}
