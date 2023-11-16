using Horizon.Content.Descriptions;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Descriptions;

public readonly struct BufferObjectDescription : IAssetDescription
{
    public readonly BufferTargetARB Type { get; init; }
    public readonly bool IsStorageBuffer { get; init; }
    public readonly uint Size { get; init; }
    public readonly BufferStorageMask StorageMasks { get; init; }

    /// <summary>
    /// Array buffer preset.
    /// </summary>
    public static BufferObjectDescription ArrayBuffer { get; } =
        new BufferObjectDescription
        {
            Type = BufferTargetARB.ArrayBuffer,
            IsStorageBuffer = false,
            Size = 0
        };

    /// <summary>
    /// Element array buffer preset.
    /// </summary>
    public static BufferObjectDescription ElementArrayBuffer { get; } =
        new BufferObjectDescription
        {
            Type = BufferTargetARB.ElementArrayBuffer,
            IsStorageBuffer = false,
            Size = 0
        };
}
