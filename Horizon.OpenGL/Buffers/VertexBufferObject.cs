using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Horizon.Content;
using Horizon.Core.Data;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Managers;

using Silk.NET.Core;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Buffers;

//The vertex array object abstraction.
public class VertexBufferObject
{
    public uint Handle
    {
        get => vao.Handle;
    }

    public BufferObject VertexBuffer { get; init; }
    public BufferObject ElementBuffer { get; init; }
    public BufferObject? InstanceBuffer { get; init; }

    private VertexArrayObject vao;

    public VertexBufferObject(in VertexArrayObject vao)
    {
        this.vao = vao;

        VertexBuffer = vao.Buffers[VertexArrayBufferAttachmentType.ArrayBuffer];
        ElementBuffer = vao.Buffers[VertexArrayBufferAttachmentType.ElementBuffer];

        if (vao.Buffers.ContainsKey(VertexArrayBufferAttachmentType.InstanceBuffer))
            InstanceBuffer = vao.Buffers[VertexArrayBufferAttachmentType.InstanceBuffer];

        Bind();
        VertexBuffer.Bind();
        Unbind();
    }

    public VertexBufferObject(AssetCreationResult<VertexArrayObject> result)
        : this(result.Asset) { }

    public unsafe void VertexAttributePointer(
        uint index,
        int count,
        VertexAttribPointerType type,
        uint vertexSize,
        int offSet
    )
    {
        ContentManager
            .GL
            .VertexAttribPointer(index, count, type, false, vertexSize, (void*)(offSet));
        ContentManager.GL.EnableVertexAttribArray(index);
    }
    public unsafe void VertexAttributeIPointer(
        uint index,
        int count,
        VertexAttribIType type,
        uint vertexSize,
        int offSet
    )
    {
        ContentManager
            .GL
            .VertexAttribIPointer(index, count, type, vertexSize, (void*)(offSet));
        ContentManager.GL.EnableVertexAttribArray(index);
    }

    private readonly struct VertexLayoutDescription
    {
        public readonly uint Index { get; init; }
        public readonly int Size { get; init; }
        public readonly int Count { get; init; }
        public readonly int Offset { get; init; }
        public readonly VertexAttribPointerType Type { get; init; }
    }

    public unsafe void SetLayout<T>()
        where T : unmanaged
    {
        // get all fields
        var fields = typeof(T)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance) // read all fields and sort by index, we trust every propert has the attribute.
            .OrderBy(p => p.GetCustomAttributes().OfType<VertexLayout>().First().Index)
            .ToArray(); // remember enumerate the array.

        // store queue of layout.
        var queue = new Queue<VertexLayoutDescription>();

        // iterate
        int totalSizeInBytes = 0;
        for (uint i = 0; i < fields.Length; i++)
        {
            var attribute =
                (fields[i].GetCustomAttribute(typeof(VertexLayout)) as VertexLayout)
                ?? throw new Exception("Undescribed property!");

            int count = fields[i].FieldType.IsPrimitive ? 1 : Math.Max(fields[i].FieldType.GetFields().Length, 1);
            int size = count * GetSizeFromVertexAttribPointerType(attribute.Type);

            queue.Enqueue(
                new VertexLayoutDescription
                {
                    Index = i,
                    Size = size,
                    Count = count,
                    Offset = totalSizeInBytes,
                    Type = attribute.Type
                }
            );
            totalSizeInBytes += size;
        }

        if (totalSizeInBytes != sizeof(T))
            throw new Exception($"Size of {nameof(T)} doesn't match VertexLayout declarations!");

        if (totalSizeInBytes % 4 != 0)
            throw new Exception($"Size of {nameof(T)} doesn't align to 4 byte boundary!");


        while (queue.Count > 0)
        {
            var ptr = queue.Dequeue();
            switch (ptr.Type)
            {
                case VertexAttribPointerType.Int:
                case VertexAttribPointerType.Byte:
                case VertexAttribPointerType.UnsignedByte:
                case VertexAttribPointerType.Short:
                case VertexAttribPointerType.UnsignedShort:
                case VertexAttribPointerType.UnsignedInt:
                    VertexAttributeIPointer(
                        ptr.Index,
                        ptr.Count,
                        (VertexAttribIType)ptr.Type,
                        (uint)totalSizeInBytes,
                        ptr.Offset
                    );
                    break;

                default:
                    VertexAttributePointer(
                        ptr.Index,
                        ptr.Count,
                        ptr.Type,
                        (uint)totalSizeInBytes,
                        ptr.Offset
                    );
                    break;
            }
        }
    }

    public static int GetSizeFromVertexAttribPointerType(in VertexAttribPointerType type)
    {
        return type switch
        {
            VertexAttribPointerType.Double => sizeof(double),

            VertexAttribPointerType.Float => sizeof(float),
            VertexAttribPointerType.Int => sizeof(int),
            VertexAttribPointerType.UnsignedInt => sizeof(uint),

            VertexAttribPointerType.UnsignedShort => sizeof(uint),
            VertexAttribPointerType.HalfFloat => sizeof(float) / 2,
            _ => 0
        };
    }

    public void VertexAttributeDivisor(uint index, uint divisor)
    {
        ContentManager.GL.VertexAttribDivisor(index, divisor);
    }

    public virtual void Bind()
    {
        // Binding the vertex array.
        ContentManager.GL.BindVertexArray(Handle);
        VertexBuffer.Bind();
        ElementBuffer.Bind();
    }

    public virtual void Unbind()
    {
        // Unbinding the vertex array.
        ContentManager.GL.BindVertexArray(0);
        VertexBuffer.Unbind();
        ElementBuffer.Unbind();
    }
}
