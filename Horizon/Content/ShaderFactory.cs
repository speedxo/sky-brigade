using Horizon.GameEntity;
using Horizon.Logging;

using GLEnum = Silk.NET.OpenGL.GLEnum;
using ShaderType = Silk.NET.OpenGL.ShaderType;

namespace Horizon.Content
{
    /// <summary>
    /// Static Factory to create shaders intended for the ShaderContentManager to manage.
    /// </summary>
    public partial class ShaderFactory : AssetFactory
    {
        private enum CompilationStatus
        {
            Pass = 0,
            Fail = 1
        }

        private record struct CompilationResult(uint Handle, CompilationStatus Status, string ErrorMessage);

        private static CompilationResult CompileShaderFromSource(in ShaderType type, in string source)
        {
            // Create a shader handle.
            uint handle = Engine.GL.CreateShader(type);

            // Shader compilation result.
            CompilationResult result = new () { ErrorMessage = "", Status = CompilationStatus.Pass, Handle = handle };

            // Stream shader source into the gl shader.
            Engine.GL.ShaderSource(handle, source);

            // Attempt to compile the shader.
            Engine.GL.CompileShader(handle);

            // Get the compilation result.
            string infoLog = Engine.GL.GetShaderInfoLog(handle);

            // If the result is empty then the compilation was a success.
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                return result with {
                    Status = CompilationStatus.Fail,
                    ErrorMessage = $"[ShaderFactory] Error compiling shader of type {type}: {infoLog}"
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
            if (!shaderDefinitions.Any(s => s.Type == ShaderType.FragmentShader) &&
                shaderDefinitions.Any(s => s.Type == ShaderType.VertexShader))
            {
                throw new InvalidOperationException("[ShaderFactory] You are required to specify a vertex and fragment shader in order to compile a program.");
            }

            // Create the shader program.
            var handle = Engine.GL.CreateProgram();

            // Program compilation result.
            var result = new CompilationResult(handle, CompilationStatus.Pass, "");

            // Enumerate and compile each program in the source.
            var aggregatedResults = CompileShaderSources(shaderDefinitions).ToArray();

            // If any of our shaders failed to compile throw an error.
            if (aggregatedResults.Any(res => res.Status == CompilationStatus.Fail))
            {
                return result with {
                    ErrorMessage = $"[ShaderFactory] Cannot compile Program({handle}) with failed shaders!",
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
                return result with {
                    ErrorMessage = $"Shader[{handle}] Failed to link with error: {Entity.Engine.GL.GetProgramInfoLog(handle)}",
                    Status = CompilationStatus.Fail
                };
            }

            // Success
            Entity.Engine.Logger.Log(LogLevel.Debug, $"Shader[{handle}] created!");
            return result;
        }

        /// <summary>
        /// Helper method to attempt to compile all the shaders in a program, returning an intermediate <see cref="CompilationResult"/>.
        /// </summary>
        private static IEnumerable<CompilationResult> CompileShaderSources(params ShaderDefinition[] shaderDefinitions)
        {
            foreach (var (type, source) in shaderDefinitions)
            {
                yield return CompileShaderFromSource(type, source);
            }
        }

        /// <summary>
        /// Compiles a program from an array of shader definitions.
        /// </summary>
        public static Shader CompileFromDefinitions(params ShaderDefinition[] shaderDefinitions)
        {
            var result = CompileProgram(shaderDefinitions);

            if (result.Status == CompilationStatus.Fail)
            {
                Engine.Logger.Log(LogLevel.Fatal, result.ErrorMessage);
                throw new Exception(result.ErrorMessage);
            }

            return new Shader(result.Handle);
        }

        /// <summary>
        /// Returns a program compiled from an inferred vertex and fragment shader specified in a directory.
        /// </summary>
        /// <param name="path">The path containing the shaders.</param>
        /// <param name="name">The matching names of the two shaders ending in .vert and .frag for the vertex and fragment shaders respecively.</param>
        public static Shader CompileNamed(in string path, in string name)
        {
            // FIXME: alot of repeated code here.
            if (!Directory.Exists(path))
            {
                string message =
                    $"[ShaderFactory] Directory '{path}' doesn't exist! Therefore we cannot load a shader. unless....";
                Engine.Logger.Log(LogLevel.Fatal, message);
                throw new DirectoryNotFoundException(message);
            }

            var vertPath = Path.Combine(path, name + ".vert");
            var fragPath = Path.Combine(path, name + ".frag");

            if (!File.Exists(vertPath))
            {
                string message =
                    $"[ShaderFactory] Cannot compile a program without a vertex shader.";
                Engine.Logger.Log(LogLevel.Fatal, message);
                throw new FileNotFoundException(message);
            }

            if (!File.Exists(fragPath))
            {
                string message =
                    $"[ShaderFactory] Cannot compile a program without a fragment shader.";
                Engine.Logger.Log(LogLevel.Fatal, message);
                throw new FileNotFoundException(message);
            }

            var vert = File.ReadAllText(vertPath);
            var frag = File.ReadAllText(fragPath);

            // We can reuse this method.
            return CompileFromDefinitions(new ShaderDefinition
            {
                Source = vert,
                Type = ShaderType.VertexShader
            }, new ShaderDefinition
            {
                Source = frag,
                Type = ShaderType.FragmentShader
            });
        }
    }
}
