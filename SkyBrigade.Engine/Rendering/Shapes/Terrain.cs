using System;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;

namespace SkyBrigade.Engine.Rendering.Shapes
{
	public class Terrain : Entity
	{
		public TransformComponent Transform { get; init; }
		public MeshRendererComponent MeshComponent { get; init; }

		public Material Material => MeshComponent.Material;

		public Terrain()
		{
			Transform = AddComponent<TransformComponent>();
			MeshComponent = AddComponent<MeshRendererComponent>();
		}

		public void GenerateTerrain(int seed)
		{

		}
	}
}

