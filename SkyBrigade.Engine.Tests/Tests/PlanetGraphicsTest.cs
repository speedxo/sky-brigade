using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;
using System.Numerics;

using Vector4 = System.Numerics.Vector4;
using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;
using SkyBrigade.Engine.GameEntity;

namespace SkyBrigade.Engine.Tests.Tests
{
    public class PlanetGraphicsTest : IEngineTest
    {
        private struct Planet
        {
            public GameObject Object { get; set; }
            public float Size { get; set; }
            public int Index { get; set; }
            public float DriftOffset { get; set; }
        }

        public bool Loaded { get; set; }
        public string Name { get; set; }

        private List<Planet> planets;

        public PlanetGraphicsTest()
        {
            Name = "Planets Test";
            Loaded = true;

            planets = new List<Planet>();

            AddPlanet("star2", 10);
            for (int i = 0; i < 9; i++)
            {
                AddPlanet(rand.NextDouble() > 0.5 ? "metal_planet" : "rock_planet", rand.NextInt64(2, 10));
            }

            UpdatePlanets(1 / 60.0f); // 🥶
        }

        private Random rand = new Random(420);

        private void AddPlanet(string materialPath, float size)
        {
            planets.Add(new Planet
            {
                Object = new GameObject(material: AdvancedMaterial.LoadFromZip(Path.Combine("Assets", "planets", $"{materialPath}.material")), MeshGenerators.CreateSphere(size / 10.0f)),
                Size = size / 10.0f,
                Index = planets.Count,
                DriftOffset = (float)rand.NextDouble() * 2 * MathF.PI
            });

            planets.Last().Object.Material.Color = new Vector4(new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()), 1.0f);
        }

        public void Render(float dt, RenderOptions? renderOptions = null)
        {
            var options = renderOptions ?? RenderOptions.Default;

            planets.Sort((o1, o2) =>
            {
                return (int)(Vector3.Distance(options.Camera.Position, o2.Object.Position) - Vector3.Distance(options.Camera.Position, o1.Object.Position));
            });

            foreach (var planet in planets)
            {
                planet.Object.Material.Use(options);

                // Statically positioned sun
                planet.Object.Material.Shader.SetUniform("lightPositions[0]", Vector3.Zero);

                // The *100.0f is because we have HDR and tonemapping so can go big
                planet.Object.Material.Shader.SetUniform("lightColors[0]", new Vector3(100.0f, 100.0f, 0.0f));

                planet.Object.Draw(dt, renderOptions);
            }
        }

        public void RenderGui()
        {
            ImGui.DragFloat("Orbital Speed", ref orbitSpeed, 0.01f, 0.0f, 10.0f);
        }

        public void Update(float dt)
        {
            UpdatePlanets(dt);
        }

        private float totalGameTime = 0.0f;
        private float orbitSpeed = 0.2f; // Adjust this value to change the speed of the orbits

        private void UpdatePlanets(float dt)
        {
            totalGameTime += dt;

            // Assuming the first planet is the sun and should be stationary at the origin
            if (planets.Count > 0)
            {
                planets[0].Object.Position = Vector3.Zero; // Position the sun at the origin
            }
            foreach (var planet in planets)
            {
                if (planet.Index < 1) continue;

                // Calculate the orbit radius based on the planet's index and size
                float orbitRadius = planets[0].Size + planet.Size * MathF.Pow(planet.Index, 1.5f) + 2.0f;

                // Calculate the new position in the orbit using the circular motion equations
                float x = MathF.Cos(totalGameTime * orbitSpeed * planet.DriftOffset) * orbitRadius;
                float z = MathF.Sin(totalGameTime * orbitSpeed * planet.DriftOffset) * orbitRadius;

                // Update the planet's position (scaling down by 10 fold)
                planet.Object.Position = new Vector3(x, 0, z);
            }
        }

        public void Dispose()
        {
        }
    }
}