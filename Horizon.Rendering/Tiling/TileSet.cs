using Bogz.Logging;
using Horizon.Engine;
using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.OpenGL.Assets;
using Horizon.Rendering.Spriting;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TileSet : GameObject
    {
        public Texture Texture { get; init; }
        public Dictionary<TTextureID, SpriteDefinition> Tiles { get; init; }
        public Vector2 TileSize { get; init; }
        public int? TileCount { get; internal set; }
        public int ID { get; internal set; }

        public TileSet(Texture texture, Vector2 spriteSize)
        {
            Texture = texture;
            TileSize = spriteSize;
            Tiles = new();
        }

        public void RegisterTile(TTextureID key, Vector2 pos, Vector2? size = null)
        {
            if (Tiles.ContainsKey(key))
            {
                Engine.Logger.Log(
                    LogLevel.Error,
                    $"Attempt to add sprite '{key}' which already exists!"
                );
                return;
            }

            Tiles.Add(key, new SpriteDefinition { Position = pos, Size = size ?? TileSize });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetNormalizedTextureCoordinatesFromTiledMapId(int id)
        {
            // Calculate the number of columns in the tileset
            int columns = (int)(Texture.Width / (int)TileSize.X);

            // Calculate the X position of the tile in the tileset
            float tileX = id % columns * TileSize.X;

            // Calculate the Y position of the tile in the tileset (using integer division!!!)
            // originally i did 'float tileY = (id / columns) * TileSize.Y;' which didn't work!!!!!
            float tileY = id / columns * TileSize.Y;

            // Normalize the coordinates to a range of [0, 1]
            float normalizedX = tileX / Texture.Width;
            float normalizedY = tileY / Texture.Height;

            // Calculate the texture coordinates
            //float left = normalizedX;
            //float right = normalizedX + TileSize.X / Texture.Width;
            //float top = normalizedY;
            //float bottom = normalizedY + TileSize.Y / Texture.Height;

            return new Vector2(normalizedX, normalizedY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2[] GetTextureCoordinates(TTextureID key)
        {
            if (!Tiles.TryGetValue(key, out var sprite))
            {
                Engine.Logger.Log(
                    LogLevel.Error,
                    $"Attempt to get sprite '{key}' which doesn't exist!"
                );
                return Array.Empty<Vector2>();
            }

            // Calculate texture coordinates for the sprite
            Vector2 topLeftTexCoord = sprite.Position / new Vector2(Texture.Width, Texture.Height); // todo: yk.
            Vector2 bottomRightTexCoord = (sprite.Position + sprite.Size) / new Vector2(Texture.Width, Texture.Height);

            return new Vector2[]
            {
                topLeftTexCoord,
                new Vector2(bottomRightTexCoord.X, topLeftTexCoord.Y),
                bottomRightTexCoord,
                new Vector2(topLeftTexCoord.X, bottomRightTexCoord.Y)
            };
        }

        public bool ContainsTextureID(TTextureID textureID)
        {
            return Tiles.ContainsKey(textureID);
        }
    }
}
