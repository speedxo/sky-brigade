namespace SkyBrigade.Engine.Rendering
{
    public struct BasicMaterialDescription
    {
        public float Metallicness;
        public float Roughness;
        public float AmbientOcclusion;

        public static BasicMaterialDescription Default { get; } = new BasicMaterialDescription
        {
            Roughness = 0.25f,
            AmbientOcclusion = 1.0f,
            Metallicness = 0.25f
        };
    }
}