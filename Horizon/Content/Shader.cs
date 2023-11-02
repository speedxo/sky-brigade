using Horizon.Logging;
using System.Numerics;

namespace Horizon.Content
{
    /// <summary>
    /// A class to encapsulate a shader providing functionality such as setting uniforms and uniform blocks.
    /// This class is designed to be used in conjunction with the ContentManager system.
    /// </summary>
    public class Shader : GameAsset
    {
        /// <summary>
        /// The associated internal GL handle to the underlying shader.
        /// </summary>
        public uint Handle { get; init; }

        private readonly Dictionary<string, int> uniformIndexes;
        private readonly Dictionary<string, uint> uniformBlockIndexes;

        /// <summary>
        /// Please initialize using GLShaderFactory.
        /// </summary>
        /// <param name="handle"></param>
        public Shader(in uint handle)
        {
            Handle = handle;
            uniformIndexes = new();
            uniformBlockIndexes = new();
        }

        /// <summary>
        /// Get the associated uniform block index of the queried block name.
        /// </summary>
        /// <param name="blockName">The name of the uniform block to query.</param>
        /// <returns>The index of the uniform block specified by the blockName.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the shader has not yet been created.</exception>
        public uint GetUniformBlockIndex(string blockName)
        {
            if (Handle == 0)
                throw new InvalidOperationException("Shader program is not created.");

            if (!uniformBlockIndexes.ContainsKey(blockName))
                uniformBlockIndexes.Add(
                    blockName,
                    Engine.GL.GetUniformBlockIndex(Handle, blockName)
                );

            return uniformBlockIndexes[blockName];
        }

        public void SetUniform(in string name, in Matrix4x4 value)
        {
            int location = GetUniformLocation(name);
            if (location == -1) // If GetUniformLocation returns -1, the uniform is not found.
            {
                Engine.Logger.Log(
                    LogLevel.Error,
                    $"Shader[{Handle}] Uniform('{name}') Error! Check if the uniform is defined!"
                );
                return;
            }
            Engine.GL.UniformMatrix4(location, 1, false, value.M11);
        }

        /// <summary>
        /// Sets the specified uniform to a specified value. This is a high performance method and the uniform index is guaranteed to be cached.
        /// </summary>
        public void SetUniform(in string name, in object? value)
        {
            int location = GetUniformLocation(name);
            if (location == -1 || value is null) // If GetUniformLocation returns -1, the uniform is not found.
            {
                Engine.Logger.Log(
                    LogLevel.Error,
                    $"Shader[{Handle}] Uniform('{name}') Error! Check if the uniform is defined!"
                );
                return;
            }

            switch (value)
            {
                case int intValue:
                    Engine.GL.Uniform1(location, intValue);
                    break;

                case float floatValue:
                    Engine.GL.Uniform1(location, floatValue);
                    break;

                case uint uintValue:
                    Engine.GL.Uniform1(location, uintValue);
                    break;

                case Vector2 vector2Value:
                    Engine.GL.Uniform2(location, vector2Value.X, vector2Value.Y);
                    break;

                case Vector3 vector3Value:
                    Engine.GL.Uniform3(location, vector3Value.X, vector3Value.Y, vector3Value.Z);
                    break;

                case Vector4 vector4Value:
                    Engine.GL.Uniform4(
                        location,
                        vector4Value.X,
                        vector4Value.Y,
                        vector4Value.Z,
                        vector4Value.W
                    );
                    break;

                case Matrix4x4 matrixValue:
                    unsafe // Dont wanna make the whole method unsafe for a single call.
                    {
                        Engine.GL.UniformMatrix4(location, 1, false, (float*)&matrixValue);
                    }
                    break;

                case bool boolValue:
                    Engine.GL.Uniform1(location, boolValue ? 1 : 0);
                    break;

                default:
                    Engine.Logger.Log(
                        LogLevel.Error,
                        $"Shader[{Handle}] Attempt to upload uniform of unsupported data type {value!.GetType()}."
                    );
                    return;
            }
        }

        /// <summary>
        /// Helper method that caches the uniform location indexes the shader actually uses.
        /// </summary>
        private int GetUniformLocation(string name)
        {
            if (!uniformIndexes.ContainsKey(name))
                uniformIndexes.Add(name, Engine.GL.GetUniformLocation(Handle, name));

            return uniformIndexes[name];
        }

        public virtual void Use() => Engine.GL.UseProgram(Handle);

        public virtual void End() => Engine.GL.UseProgram(0);

        public override void Dispose()
        {
            GC.SuppressFinalize(this);

            uniformBlockIndexes.Clear();
            uniformIndexes.Clear();

            Engine.GL.DeleteProgram(Handle);
            Engine.Logger.Log(LogLevel.Debug, $"Shader[{Handle}] destroyed!");
        }
    }
}
