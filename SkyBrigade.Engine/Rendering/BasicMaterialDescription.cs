namespace SkyBrigade.Engine.Rendering
{
    public struct BasicMaterialDescription
    {
		public float Metallicness { get; set; }
		public float Roughness { get; set; }
		public float AmbientOcclusion { get; set; }
		public static BasicMaterialDescription Default { get; } = new BasicMaterialDescription {
			Roughness = 0.25f,
			AmbientOcclusion = 1.0f,
			Metallicness  = 0.25f
		};
	}
}

