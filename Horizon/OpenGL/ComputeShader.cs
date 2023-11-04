using Horizon.Extentions;
using Horizon.GameEntity;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL;

/// <summary>
/// this class doesnt work :/ We are stuck with ogl4.1
/// </summary>
public class ComputeShader : Entity, IDisposable
{
    private uint _computeProgram;

    public ComputeShader(string shaderSource)
    {
        // Create the compute shader
        var _computeShader = Engine.GL.CreateShader(ShaderType.ComputeShader);
        Engine.GL.ShaderSource(_computeShader, shaderSource);
        Engine.GL.CompileShader(_computeShader);

        // Create the compute program and attach the shader
        _computeProgram = Engine.GL.CreateProgram();
        Engine.GL.AttachShader(_computeProgram, _computeShader);
        Engine.GL.LinkProgram(_computeProgram);
    }

    public void Dispatch(uint numGroupsX, uint numGroupsY, uint numGroupsZ)
    {
        Engine.GL.DispatchCompute(numGroupsX, numGroupsY, numGroupsZ);
    }

    public void SendDataToShader<T>(uint bufferBindingPoint, BufferObject<T> bufferObject)
        where T : unmanaged
    {
        Engine.GL.UseProgram(_computeProgram);
        bufferObject.Bind();
        Engine.GL.BindBufferBase(
            BufferTargetARB.ShaderStorageBuffer,
            bufferBindingPoint,
            bufferObject.Handle
        );
        Engine.GL.UseProgram(0);
    }

    public unsafe T[] ReceiveDataFromShader<T>(
        uint bufferBindingPoint,
        BufferObject<T> bufferObject
    )
        where T : unmanaged
    {
        Engine.GL.UseProgram(_computeProgram);
        Engine.GL.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, bufferBindingPoint, 0);
        bufferObject.Bind();

        int bufferSize;
        Engine.GL.GetNamedBufferParameter(bufferObject.Handle, BufferPNameARB.Size, &bufferSize);

        T[] bufferData = new T[bufferSize];
        fixed (T* dataPtr = bufferData)
        {
            Engine.GL.GetNamedBufferSubData(
                bufferObject.Handle,
                (IntPtr)0,
                (nuint)bufferSize,
                dataPtr
            );
        }
        Engine.GL.UseProgram(0);

        return bufferData;
    }

    public void Dispose()
    {
        Engine.GL.DeleteProgram(_computeProgram);
    }

    public void Bind() => Engine.GL.UseProgram(_computeProgram);

    public void Unbind() => Engine.GL.UseProgram(0);
}
