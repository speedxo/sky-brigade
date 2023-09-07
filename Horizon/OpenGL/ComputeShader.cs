using Silk.NET.OpenGL;

namespace Horizon.OpenGL;

/// <summary>
/// this class doesnt work :/ We are stuck with ogl4.1
/// </summary>
public class ComputeShader : IDisposable
{
    private uint _computeShader;
    private uint _computeProgram;

    public ComputeShader(string shaderSource)
    {
        // Create the compute shader
        _computeShader = GameManager.Instance.Gl.CreateShader(ShaderType.ComputeShader);
        GameManager.Instance.Gl.ShaderSource(_computeShader, shaderSource);
        GameManager.Instance.Gl.CompileShader(_computeShader);

        // Create the compute program and attach the shader
        _computeProgram = GameManager.Instance.Gl.CreateProgram();
        GameManager.Instance.Gl.AttachShader(_computeProgram, _computeShader);
        GameManager.Instance.Gl.LinkProgram(_computeProgram);
    }

    public void Dispatch(uint numGroupsX, uint numGroupsY, uint numGroupsZ)
    {
        GameManager.Instance.Gl.UseProgram(_computeProgram);
        GameManager.Instance.Gl.DispatchCompute(numGroupsX, numGroupsY, numGroupsZ);
        GameManager.Instance.Gl.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);
        GameManager.Instance.Gl.UseProgram(0);
    }

    public void SendDataToShader<T>(uint bufferBindingPoint, BufferObject<T> bufferObject)
        where T : unmanaged
    {
        GameManager.Instance.Gl.UseProgram(_computeProgram);
        bufferObject.Bind();
        GameManager.Instance.Gl.BindBufferBase(
            BufferTargetARB.ShaderStorageBuffer,
            bufferBindingPoint,
            bufferObject.Handle
        );
        GameManager.Instance.Gl.UseProgram(0);
    }

    public unsafe T[] ReceiveDataFromShader<T>(
        uint bufferBindingPoint,
        BufferObject<T> bufferObject
    )
        where T : unmanaged
    {
        GameManager.Instance.Gl.UseProgram(_computeProgram);
        GameManager.Instance.Gl.BindBufferBase(
            BufferTargetARB.ShaderStorageBuffer,
            bufferBindingPoint,
            0
        );
        bufferObject.Bind();

        int bufferSize;
        GameManager.Instance.Gl.GetNamedBufferParameter(
            bufferObject.Handle,
            BufferPNameARB.Size,
            &bufferSize
        );

        T[] bufferData = new T[bufferSize];
        fixed (T* dataPtr = bufferData)
        {
            GameManager.Instance.Gl.GetNamedBufferSubData(
                bufferObject.Handle,
                (IntPtr)0,
                (nuint)bufferSize,
                dataPtr
            );
        }
        GameManager.Instance.Gl.UseProgram(0);

        return bufferData;
    }

    public void Dispose()
    {
        GameManager.Instance.Gl.DeleteProgram(_computeProgram);
        GameManager.Instance.Gl.DeleteShader(_computeShader);
    }
}
