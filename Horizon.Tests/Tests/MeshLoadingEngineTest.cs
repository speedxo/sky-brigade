using Horizon.GameEntity;
using Horizon.Rendering;
using ImGuiNET;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Horizon.Tests.Tests
{
    public class MeshLoadingEngineTest : IEngineTest
    {
        private Vector3[] lightColors;
        private GameObject lightMesh;

        // lights
        private readonly Vector3[] lightPositions = new[] {
            new Vector3(-10.0f,  10.0f, 10.0f),
            new Vector3( 10.0f,  10.0f, 10.0f),
            new Vector3(-10.0f, -10.0f, 10.0f),
            new Vector3( 10.0f, -10.0f, 10.0f)
        };

        private List<GameObject> objects;
        private readonly Random rand = new();
        private Vector3 rot = Vector3.Zero;
        private Vector3 scale = Vector3.One;
        private float totalTime;
        public bool Loaded { get; set; } = false;
        public string Name { get; set; } = "Mesh Loading Test";

        public void Dispose()
        {
            Loaded = false;

            GC.SuppressFinalize(this);
        }

        public MeshLoadingEngineTest()
        {
            if (Loaded) return;

            // make some random vector3
            int intensity = 1000;
            lightColors = new Vector3[4];
            for (int i = 0; i < lightColors.Length; i++)
            {
                lightColors[i] = new Vector3(intensity);
            }

            var materials = new AdvancedMaterial[] {
                AdvancedMaterial.LoadFromZip("Assets/pbr_textures/metal_ball.material"),
                AdvancedMaterial.LoadFromZip("Assets/pbr_textures/bricks_mortar.material"),
                AdvancedMaterial.LoadFromZip("Assets/pbr_textures/black_tiles.material"),
            };

            // dynamically create a mesh for each material, assinging the material to the mesh
            objects = new List<GameObject>(materials.Length);

            for (int i = 0; i < materials.Length; i++)
                objects.Add(new GameObject(material: materials[i], meshData: MeshGenerators.CreateSphere(1, 50)));

            lightMesh = new GameObject(meshData: MeshGenerators.CreateSphere());

            int counter = 0;
            //meshes[2] = Mesh.FromObj("Assets/teapot.obj");
            foreach (var mesh in objects)
            {
                mesh.Position = new Vector3(counter * 2.5f, 0, 0);
                counter++;
            }

            GameManager.Instance.Gl.Enable(EnableCap.CullFace);
            GameManager.Instance.Gl.Enable(EnableCap.DepthTest);
            Loaded = true;
        }

        public void Render(float dt, RenderOptions? renderOptions = null)
        {
            // Accumulate the time
            totalTime += dt;
            // Get the render options
            var options = renderOptions ?? RenderOptions.Default;

            objects.Sort((o1, o2) =>
            {
                // TODO: expensive square root operation.
                // EDIT: nevermind there is a DistanceSquared method for that
                if (Vector3.DistanceSquared(options.Camera.Position, o1.Position) < Vector3.DistanceSquared(options.Camera.Position, o2.Position)) return 1;
                return 0;
            });

            foreach (var item in objects)
            {
                item.Material.Use();

                for (int v = 0; v < lightColors.Length; v++)
                {
                    var newPos = lightPositions[v] + new Vector3(MathF.Sin(totalTime * 2.0f) * 100, 0.0f, MathF.Cos(totalTime * 2.0f) * 100);

                    item.MeshRenderer.SetUniform("lightPositions[" + v + "]", newPos);
                    item.MeshRenderer.SetUniform("lightColors[" + v + "]", lightColors[v]);

                    lightMesh.Position = newPos;
                    lightMesh.Draw(dt, RenderOptions.Default with { Camera = options.Camera });
                }

                item.Draw(dt, options);
            }
        }

        public void RenderGui()
        {
            ImGui.DragFloat3("Scale", ref scale, 0.01f);
            ImGui.DragFloat3("Rotation", ref rot, 0.1f);

            if (ImGui.Button("Reset Lights"))
            {
                float multiplier = 1000;
                lightColors = new[] {
                    new Vector3(rand.NextSingle() * multiplier, rand.NextSingle() * multiplier, rand.NextSingle() * multiplier),
                    new Vector3(rand.NextSingle() * multiplier, rand.NextSingle() * multiplier, rand.NextSingle() * multiplier),
                    new Vector3(rand.NextSingle() * multiplier, rand.NextSingle() * multiplier, rand.NextSingle() * multiplier),
                    new Vector3(rand.NextSingle() * multiplier, rand.NextSingle() * multiplier, rand.NextSingle() * multiplier)
                };
            }
        }

        // This method is called every frame.
        public void Update(float dt)
        {
            // Loop through all the meshes in the meshes array.
            foreach (var mesh in objects)
            {
                // Check if the mesh's scale matches the scale instance variable.
                if (mesh.Transform.Scale != scale)
                    // If it doesn't, set the mesh's scale to the scale instance variable.
                    mesh.Transform.Scale = scale;
                if (mesh.Rotation != rot)
                    mesh.Rotation = rot;
            }
        }
    }
}