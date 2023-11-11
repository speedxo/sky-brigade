namespace Horizon.Rendering.Spriting
{
    public struct SpriteAnimationDefinition
    {
        public float FrameTime { get; set; }
        public float Timer { get; set; }
        public uint Index { get; set; }
        public uint Length { get; set; }

        public SpriteDefinition FirstFrame { get; set; }
    }
}
