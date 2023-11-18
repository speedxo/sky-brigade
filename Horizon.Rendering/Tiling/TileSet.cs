using System.Numerics;
using System.Runtime.CompilerServices;
using Bogz.Logging;
using Bogz.Logging.Loggers;
using Horizon.Engine;
using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.OpenGL.Assets;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TileSet : GameObject
    {
        public Material Material { get; private set; }
        private string _texturePath;

        public Dictionary<TTextureID, SpriteDefinition> Tiles { get; init; }
        public Vector2 TileSize { get; init; }
        public int? TileCount { get; internal set; }
        public int ID { get; internal set; }

        public TileSet(string path, Vector2 spriteSize)
        {
            _texturePath = path;
            TileSize = spriteSize;
            Tiles = new();
        }

        public override void Initialize()
        {
            base.Initialize();

            string dir = Path.GetDirectoryName(_texturePath)!;
            int lastIndex = _texturePath.LastIndexOf(MaterialFactory.Delimiter);
            string name = Path.GetFileNameWithoutExtension(_texturePath[..lastIndex]);

            Material = MaterialFactory.Create(dir, name);

            //Texture = Engine
            //    .ContentManager
            //    .Textures
            //    .Create(
            //        new OpenGL.Descriptions.TextureDescription
            //        {
            //            Path = _texturePath,
            //            Definition = OpenGL.Descriptions.TextureDefinition.RgbaUnsignedByte
            //        }
            //    )
            //    .Asset;
        }

        public void RegisterTile(TTextureID key, Vector2 pos, Vector2? size = null)
        {
            if (Tiles.ContainsKey(key))
            {
                ConcurrentLogger
                    .Instance
                    .Log(LogLevel.Error, $"Attempt to add sprite '{key}' which already exists!");
                return;
            }

            Tiles.Add(key, new SpriteDefinition { Position = pos, Size = size ?? TileSize });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetNormalizedTextureCoordinatesFromTiledMapId(int id)
        {
            // Calculate the number of columns in the tileset
            int columns = (int)(Material.Width / (int)TileSize.X);

            // Calculate the X position of the tile in the tileset
            float tileX = id % columns * TileSize.X;

            // Calculate the Y position of the tile in the tileset (using integer division!!!)
            // originally i did 'float tileY = (id / columns) * TileSize.Y;' which didn't work!!!!!
            float tileY = id / columns * TileSize.Y;

            // Normalize the coordinates to a range of [0, 1]
            float normalizedX = tileX / Material.Width;
            float normalizedY = tileY / Material.Height;

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
                ConcurrentLogger
                    .Instance
                    .Log(LogLevel.Error, $"Attempt to get sprite '{key}' which doesn't exist!");
                return Array.Empty<Vector2>();
            }

            // Calculate texture coordinates for the sprite
            Vector2 topLeftTexCoord = sprite.Position / new Vector2(Material.Width, Material.Height); // todo: yk.
            Vector2 bottomRightTexCoord =
                (sprite.Position + sprite.Size) / new Vector2(Material.Width, Material.Height);

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
