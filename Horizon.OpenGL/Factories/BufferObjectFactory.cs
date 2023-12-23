using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection.Metadata;
using Horizon.Content;
using Horizon.Content.Descriptions;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Managers;

namespace Horizon.OpenGL.Factories;

public class BufferObjectFactory : IAssetFactory<BufferObject, BufferObjectDescription>
{
    public static unsafe AssetCreationResult<BufferObject> Create(
        in BufferObjectDescription description
    )
    {
        var buffer = new BufferObject
        {
            Handle = ObjectManager.GL.CreateBuffer(),
            Type = description.Type
        };

        if (buffer.Handle == 0)
            return new() { Asset = buffer, Status = AssetCreationStatus.Failed };

        if (description.IsStorageBuffer)
            ObjectManager
                .GL
                .NamedBufferStorage(
                    buffer.Handle,
                    description.Size,
                    null,
                    description.StorageMasks
                );

        return new()
        {
            Asset = buffer,
            Status = AssetCreationStatus.Success,
            Message = description.IsStorageBuffer
                ? $"Storage buffer with size {description.Size} created!"
                : string.Empty
        };
    }
}
