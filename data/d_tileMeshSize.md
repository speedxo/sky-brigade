# Vertex Compression 
i want to make my tike map system more memory efficient, since my tile map system is chunked,
i want to send only the local coordinates of each tile vertex and send the chunk position to the vertex shader to compute its final global worldspace position.

## Preface:
The current TileVertex class is inefficient:
```
public readonly struct TileVertex
{
    public readonly Vector2 Position { get; init; }
    public readonly Vector2 TexCoords { get; init; }
    public readonly Vector3 Color { get; init; }

    public TileVertex(in float x, in float y, in float uvX, in float uvY, in Vector3 color)
    {
        Color = new Vector3(color.X, color.Y, color.Z);
        TexCoords = new Vector2(uvX, uvY);
        Position = new Vector2(x, y);
    }

    public static readonly int SizeInBytes = sizeof(float) * 7;
}
```
We do not need to store each vertex coordinate as a full single precision vector3 since the tiles lay within a fixed chunks local coordinates, we can instead store an 8 bit number and send in the chunk coordinates with a shader uniform.
Currently we are using 248MB of mem cumulatively for the tilemaps VBOS, lets see what we can do.