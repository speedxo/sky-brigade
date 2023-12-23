using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Content;
using Horizon.Content.Descriptions;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Managers;

namespace Horizon.OpenGL.Factories;

public class VertexArrayObjectFactory
    : IAssetFactory<VertexArrayObject, VertexArrayObjectDescription>
{
    public static AssetCreationResult<VertexArrayObject> Create(
        in VertexArrayObjectDescription description
    )
    {
        uint vaoHandle = ObjectManager.GL.CreateVertexArray();

        Dictionary<VertexArrayBufferAttachmentType, BufferObject> buffers = new();

        ObjectManager.GL.BindVertexArray(vaoHandle);

        foreach (var (type, desc) in description.Buffers)
        {
            var buffer = ObjectManager.Instance.Buffers.Create(desc);
            if (buffer.Status != AssetCreationStatus.Success)
            {
                // make sure to free the VertexArrayObject
                ObjectManager.GL.BindVertexArray(0);
                ObjectManager.GL.DeleteVertexArray(vaoHandle);

                // incase the first buffer wasnt the one to fail.
                foreach (var (_, item) in buffers)
                    ObjectManager.Instance.Buffers.Remove(item);
                buffers.Clear();

                return new AssetCreationResult<VertexArrayObject>
                {
                    Asset = new(),
                    Status = AssetCreationStatus.Failed,
                    Message = $"Failed to create and attach BufferObject[{type}] to VAO!"
                };
            }
            else
                buffers.Add(type, buffer.Asset);

            // simply binding the buffer is enough to attach it to the VertexArrayObject.
            ObjectManager.GL.BindVertexArray(vaoHandle);
            buffer.Asset.Bind();
            buffer.Asset.Unbind();
        }
        ObjectManager.GL.BindVertexArray(0);

        return new AssetCreationResult<VertexArrayObject>
        {
            Asset = new() { Handle = vaoHandle, Buffers = buffers },
            Status = AssetCreationStatus.Success
        };
    }
}
