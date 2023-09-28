using Horizon.GameEntity;
using Horizon.OpenGL;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Horizon.Rendering.Spriting
{
    public class Spritesheet : Entity
    {
        private static int _idCounter = 0;
        private int SpriteCounter = 0;

        public int GetNewSpriteId()
        {
            return SpriteCounter++;
        }

        public Texture Texture { get; init; }
        public Dictionary<string, SpriteDefinition> Sprites { get; init; }

        public Vector2 SpriteSize { get; init; }

        public Vector2 SingleSpriteSize { get; init; }

        public SpritesheetAnimationManager AnimationManager { get; init; }

        public Spritesheet(Texture texture, Vector2 spriteSize)
            : base()
        {
            this.ID = _idCounter++;

            this.Texture = texture;
            this.SpriteSize = spriteSize;
            this.Sprites = new();

            SingleSpriteSize = SpriteSize / Texture.Size;

            AnimationManager = this.AddComponent<SpritesheetAnimationManager>();
        }

        public void AddAnimation(
            string name,
            Vector2 position,
            int length,
            float frameTime = 0.1f,
            Vector2? inSize = null
        ) => AnimationManager.AddAnimation(name, position, length, frameTime, inSize);

        public void AddAnimationRange((
            string name,
            Vector2 position,
            int length,
            float frameTime,
            Vector2? inSize)[] animations)
        {
            foreach (var (name, position, length, frameTime, inSize) in animations)
                AddAnimation(name, position, length, frameTime, inSize);
        }

        public void AddSprite(string name, Vector2 pos, Vector2? size = null)
        {
            if (Sprites.ContainsKey(name))
            {
                Engine.Logger.Log(
                    Logging.LogLevel.Error,
                    $"Attempt to add sprite '{name}' which already exists!"
                );
                return;
            }

            this.Sprites.Add(
                name,
                new SpriteDefinition { Position = pos, Size = size ?? SpriteSize }
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2[] GetAnimatedTextureCoordinates(string name)
        {
            if (!AnimationManager.Animations.TryGetValue(name, out var sprite))
            {
                Engine.Logger.Log(
                    Logging.LogLevel.Error,
                    $"Attempt to get sprite '{name}' which doesn't exist!"
                );
                return Array.Empty<Vector2>();
            }

            // Calculate texture coordinates for the sprite
            Vector2 topLeftTexCoord =
                sprite.FirstFrame.Position / Texture.Size
                - new Vector2(SingleSpriteSize.X / 4.0f, 0);
            Vector2 bottomRightTexCoord = topLeftTexCoord + (sprite.FirstFrame.Size / Texture.Size);

            return new Vector2[]
            {
                topLeftTexCoord,
                new Vector2(bottomRightTexCoord.X, topLeftTexCoord.Y),
                bottomRightTexCoord,
                new Vector2(topLeftTexCoord.X, bottomRightTexCoord.Y)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2[] GetTextureCoordinates(string name)
        {
            if (!Sprites.TryGetValue(name, out var sprite))
            {
                Engine.Logger.Log(
                    Logging.LogLevel.Error,
                    $"Attempt to get sprite '{name}' which doesn't exist!"
                );
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

        public void ResetSpriteCounter()
        {
            SpriteCounter = 0;
        }
    }
}
