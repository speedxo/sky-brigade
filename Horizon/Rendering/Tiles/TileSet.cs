using System;
using Horizon.GameEntity;
using Horizon.Rendering.Spriting;
using System.Numerics;
using Horizon.OpenGL;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TileSet : Entity
    {
        public Texture Texture { get; init; }
        public Dictionary<TTextureID, SpriteDefinition> Tiles { get; init; }

        public Vector2 TileSize { get; init; }
        public TileSet(Texture texture, Vector2 spriteSize)
            : base()
        {
            this.Texture = texture;
            this.TileSize = spriteSize;
            this.Tiles = new();
        }

        public void RegisterTile(TTextureID key, Vector2 pos, Vector2? size = null)
        {
            if (Tiles.ContainsKey(key))
            {
                GameManager.Instance.Logger.Log(Logging.LogLevel.Error, $"Attempt to add sprite '{key}' which already exists!");
                return;
            }

            this.Tiles.Add(key, new SpriteDefinition { Position = pos, Size = size ?? TileSize });
        }

        public Vector2[] GetTextureCoordinates(TTextureID key)
        {
            if (!Tiles.TryGetValue(key, out var sprite))
            {
                GameManager.Instance.Logger.Log(Logging.LogLevel.Error, $"Attempt to get sprite '{key}' which doesn't exist!");
                return Array.Empty<Vector2>();
            }

            // Calculate texture coordinates for the sprite
            Vector2 topLeftTexCoord = sprite.Position / Texture.Size;
            Vector2 bottomRightTexCoord = (sprite.Position + sprite.Size) / Texture.Size;

            return new Vector2[]
            {
                topLeftTexCoord,
                new Vector2(bottomRightTexCoord.X, topLeftTexCoord.Y),
                bottomRightTexCoord,
                new Vector2(topLeftTexCoord.X, bottomRightTexCoord.Y)
            };
        }

        public bool ContainsTextureID(TTextureID textureID)
            => Tiles.ContainsKey(textureID);
    }

}