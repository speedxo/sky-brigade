using Horizon.Logging;
using Horizon.OpenGL;
using Silk.NET.OpenGL;
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

        public override string Name => $"Shader({Handle})";

        private readonly Dictionary<string, int> uniformIndexes;
        private readonly Dictionary<string, uint> uniformBlockIndexes;
        private readonly Dictionary<string, uint> programResourceIndexes;

        /// <summary>
        /// Please initialize using GLShaderFactory.
        /// </summary>
        /// <param name="handle"></param>
        public Shader(in uint handle)
        {
            Handle = handle;
            uniformIndexes = new();
            uniformBlockIndexes = new();
            programResourceIndexes = new();
        }

        /// <summary>
        /// Gets and caches the index of a program resource.
        /// TODO: rewrite!
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="interface">The interface.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        /// Shader program is not created.
        /// or
        /// Shader resource '{name}' not found!
        /// </exception>
        public uint GetResourceIndex(string name, ProgramInterface @interface)
        {
            if (Handle == 0)
                throw new InvalidOperationException("Shader program is not created.");

            if (programResourceIndexes.TryGetValue(name, out uint ind))
                return ind;

            uint index = Engine.GL.GetProgramResourceIndex(Handle, @interface, name);

            if (index < 0)
                throw new InvalidOperationException($"Shader resource '{name}' not found!");

            programResourceIndexes.TryAdd(name, index);
            return index;
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
                //Engine.Logger.Log(
                //    LogLevel.Error,
                //    $"Shader[{Handle}] Uniform('{name}') Error! Check if the uniform is defined!"
                //);
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
                //Engine.Logger.Log(
                //    LogLevel.Error,
                //    $"Shader[{Handle}] Uniform('{name}') Error! Check if the uniform is defined!"
                //);
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
                    unsafe // Don't wanna make the whole method unsafe for a single call.
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

        public void BindBuffer<T>(in string name, in BufferObject<T> bufferObject)
            where T : unmanaged => BindBuffer(programResourceIndexes[name], bufferObject);

        public void BindBuffer<T>(in uint bufferBindingPoint, in BufferObject<T> bufferObject)
            where T : unmanaged
        {
            bufferObject.Bind();
            Engine.GL.BindBufferBase(
                BufferTargetARB.ShaderStorageBuffer,
                bufferBindingPoint,
                bufferObject.Handle
            );
        }

        public void BindBuffer<T>(in string name, in BufferStorageObject<T> bufferObject)
            where T : unmanaged => BindBuffer(programResourceIndexes[name], bufferObject);

        public void BindBuffer<T>(
            in uint bufferBindingPoint,
            in BufferStorageObject<T> bufferObject
        )
            where T : unmanaged
        {
            bufferObject.Bind();
            Engine.GL.BindBufferBase(
                BufferTargetARB.ShaderStorageBuffer,
                bufferBindingPoint,
                bufferObject.Handle
            );
        }

        public unsafe void ReadSSBO<T>(
            uint bufferBindingPoint,
            BufferObject<T> bufferObject,
            ref T[] data,
            int length = 1,
            int offset = 0
        )
            where T : unmanaged
        {
            Engine.GL.UseProgram(Handle);
            Engine.GL.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, bufferBindingPoint, 0);
            bufferObject.Bind();

            //int bufferSize;
            //Engine.GL.GetNamedBufferParameter(
            //    bufferObject.Handle,
            //    BufferPNameARB.Size,
            //    &bufferSize
            //);

            unsafe
            {
                fixed (T* dataPtr = data)
                {
                    Engine.GL.GetNamedBufferSubData(
                        bufferObject.Handle,
                        sizeof(T) * offset,
                        (nuint)(sizeof(T) * length),
                        dataPtr
                    );
                }
            }
            Engine.GL.UseProgram(0);
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
