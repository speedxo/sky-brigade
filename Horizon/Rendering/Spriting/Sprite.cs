using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering.Spriting.Data;
using System.Numerics;

namespace Horizon.Rendering.Spriting;

public class Sprite : Entity
{
    private static int _idCounter = 0;
    private bool _hasBeenSetup = false;

    public Spritesheet Spritesheet { get; private set; }

    public bool ShouldDraw { get; set; } = true;
    public bool Flipped { get; set; } = false;
    internal bool ShouldUpdateVbo { get; private set; }

    public Vector2 Size { get; set; } = Vector2.One;
    public bool IsAnimated { get; set; }
    public string FrameName { get; set; }

    public TransformComponent2D Transform { get; private set; }

    public void Setup(
        Spritesheet spriteSheet,
        string name,
        TransformComponent2D? inTransform = null
    )
    {
        this.Spritesheet = AddEntity(spriteSheet);
        this.Transform = AddComponent(inTransform ?? new TransformComponent2D());

        this.FrameName = name;
        this.ID = _idCounter++;

        _hasBeenSetup = true;
    }

    public Vertex2D[] GetVertices()
    {
        if (!_hasBeenSetup)
            GameManager.Instance.Logger.Log(
                Logging.LogLevel.Error,
                "[Sprite] Setup() has not been called!"
            );

        Vector2[] uv = IsAnimated
            ? Spritesheet.GetAnimatedTextureCoordinates(FrameName)
            : Spritesheet.GetTextureCoordinates(FrameName);

        int id = Spritesheet.GetNewSpriteId();

        return new Vertex2D[]
        {
            new Vertex2D(-Size.X / 2.0f, Size.Y / 2.0f, uv[0].X, uv[0].Y, id),
            new Vertex2D(Size.X / 2.0f, Size.Y / 2.0f, uv[1].X, uv[1].Y, id),
            new Vertex2D(Size.X / 2.0f, -Size.Y / 2.0f, uv[2].X, uv[2].Y, id),
            new Vertex2D(-Size.X / 2.0f, -Size.Y / 2.0f, uv[3].X, uv[3].Y, id),
        };
    }

    public Vector2 GetFrameOffset()
    {
        if (IsAnimated)
        {
            var (definition, index) = Spritesheet.AnimationManager[FrameName];

            return definition.Position + new Vector2(index, 0);
        }
        return Vector2.Zero;
    }
}
