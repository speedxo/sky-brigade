namespace SkyBrigade.MaterialEditor;

using System.Collections.Concurrent;
using System.Diagnostics;
using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;
using static SkyBrigade.Engine.GameManager;

internal class Program : IGameScreen
{
    private static void Main(string[] _)
    { 
        Instance.Initialize(GameInstanceParameters.Default with
        {
            InitialGameScreen = typeof(Program),
            WindowTitle = "Material Editor"
        });

        Instance.Run();
    }

    private Camera camera;
    private List<IEntity> entities;
    private Mesh mesh;

    private Dictionary<string, AdvancedMaterial> loadedMaterials;
    private int selectedMaterialIndex = 0;
    private string[] materialNames;

    public void Initialize(GL gl)
    {
        loadedMaterials = new Dictionary<string, AdvancedMaterial>();

        var menuBar = new EditorMenuBar();
        menuBar.OnMenuItemClicked += MenuBar_MenuItemClicked;

        entities = new List<IEntity> {
            menuBar
        };

        camera = new Camera() { Position = new System.Numerics.Vector3(0, 0, 5), Locked = true };
        mesh = Mesh.CreateSphere(1);
    }

    private void LoadDefaultMaterial()
    {
        //material = AdvancedMaterial.LoadFromZip("default.material");
    }

    private void MenuBar_MenuItemClicked(EditorMenuBarItem item)
    {
        switch (item)
        {
            case EditorMenuBarItem.Close:
                Instance.Window.Close(); break;
            case EditorMenuBarItem.Save:
                SaveMaterial(); break;
            case EditorMenuBarItem.OpenFile:
                var fileDialog = new OpenFileDialog(".material");
                
                fileDialog.FileSelected += (path) => {
                    entities.Remove(fileDialog);
                    LoadMaterial(path);
                }; 
                
                entities.Add(fileDialog); break;
        }
    }

    private void LoadMaterial(string path)
    {
        Instance.Logger.Log(Engine.Logging.LogLevel.Info, $"Loading material({path})...");
        if (loadedMaterials.ContainsKey(path))
        {
            Instance.Logger.Log(Engine.Logging.LogLevel.Info, $"Material({path}) already loaded!");
            return;
        }

        var material = AdvancedMaterial.LoadFromZip(path);
        loadedMaterials.Add(path, material);
        mesh.Material = material;

        materialNames = loadedMaterials.Keys.ToArray().Select(Path.GetFileName).ToArray();
    }

    private void SaveMaterial()
    {
        //if (material != null) material.Save(materialPath);
    }

    public void Render(GL gl, float dt)
    {
        for (int i = 0; i < entities.Count; i++)
            entities[i].Draw();

        if (ImGui.Begin("Materials"))
        {
            if (materialNames != null && ImGui.ListBox("", ref selectedMaterialIndex, materialNames, loadedMaterials.Count))
                mesh.Material = loadedMaterials.Values.ElementAt(selectedMaterialIndex);

            if (ImGui.Button("Remove") & materialNames != null)
            {
                loadedMaterials.Remove(materialNames[selectedMaterialIndex]);
                materialNames = loadedMaterials.Keys.ToArray();
                if (selectedMaterialIndex >= materialNames.Length)
                    selectedMaterialIndex = materialNames.Length - 1;
            }
            ImGui.End();
        }

        mesh.Draw(RenderOptions.Default with
        {
            Camera = camera
        });
    }

    public void Update(float dt)
    {
        for (int i = 0; i < entities.Count; i++)
            entities[i].Update(dt);

        camera.Update(dt);
    }

    //private void CloseWindow()
    //{
    //    Instance.Window.Close();
    //}

    public void Dispose()
    {
    }
}
