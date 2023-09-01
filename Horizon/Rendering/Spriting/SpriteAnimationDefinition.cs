namespace Horizon.Rendering.Spriting
{
    public struct SpriteAnimationDefinition
    {
        public float FrameTime { get; set; }
        public float Timer { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }

        public SpriteDefinition FirstFrame { get; set; }
    }
}