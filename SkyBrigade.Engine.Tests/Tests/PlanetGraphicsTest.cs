using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Engine.Tests.Tests
{
    public class PlanetGraphicsTest : IEngineTest
    {
        private struct Planet
        {
            public Mesh Mesh { get; set; }
            public float Size { get; set; }
            public int Index { get; set; }
            public float DriftOffset { get; set; }
        }

        public bool Loaded { get; set; }
        public string Name { get; set; }

        private List<Planet> planets;

        public void LoadContent(GL gl)
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
                Mesh = Rendering.MeshGenerators.CreateSphere(size / 10.0f, mat: AdvancedMaterial.LoadFromZip(Path.Combine("Assets", "planets", $"{materialPath}.material"))),
                Size = size / 10.0f,
                Index = planets.Count,
                DriftOffset = (float)rand.NextDouble() * 2 * MathF.PI
            });

            planets.Last().Mesh.Material.Color = new Vector4(new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()), 1.0f);
        }

        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            var options = renderOptions ?? RenderOptions.Default;

            planets.Sort((o1, o2) =>
            {
                return (int)(Vector3.Distance(options.Camera.Position, o2.Mesh.Position) - Vector3.Distance(options.Camera.Position, o1.Mesh.Position));
            });

            foreach (var planet in planets)
            {
                planet.Mesh.Material.Shader.Use();

                // Statically positioned sun
                planet.Mesh.Material.Shader.SetUniform("lightPositions[0]", Vector3.Zero);

                // The *100.0f is because we have HDR and tonemapping so can go big
                planet.Mesh.Material.Shader.SetUniform("lightColors[0]", new Vector3(100.0f, 100.0f, 0.0f));

                planet.Mesh.Draw(dt, renderOptions);
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
                planets[0].Mesh.Position = Vector3.Zero; // Position the sun at the origin
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
                planet.Mesh.Position = new Vector3(x, 0, z);
            }
        }

        public void Dispose()
        {
        }
    }
}