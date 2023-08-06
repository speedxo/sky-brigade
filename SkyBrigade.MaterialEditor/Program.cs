namespace SkyBrigade.MaterialEditor;

using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Dialogs;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Prefabs;
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
    private GameObject sphere;

    private Dictionary<string, AdvancedMaterial> loadedMaterials;
    private int selectedMaterialIndex = 0;
    private string[] materialNames;


    public List<IEntity> Entities { get; set; }

    public void Initialize(GL gl)
    {
        loadedMaterials = new Dictionary<string, AdvancedMaterial>();

        var menuBar = new EditorMenuBar();
        menuBar.OnMenuItemClicked += MenuBar_MenuItemClicked;

        camera = new Camera() { Position = new System.Numerics.Vector3(0, 0, 5), Locked = true };
        sphere = new GameObject();
        sphere.MeshRenderer.Load(MeshGenerators.CreateSphere(1));

        Entities = new List<IEntity> {
            menuBar,
            sphere
        };

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
                OpenFile(); break;
        }
    }

    private void OpenFile()
    {
        var fileDialog = new OpenFileDialog(".material")
        {
            Multiselect = true
        };

        fileDialog.FilesSelected += (files) =>
        {
            Entities.Remove(fileDialog);
            LoadMaterials(files);
        };

        Entities.Add(fileDialog);
    }

#pragma warning disable CS8601, CS8619 // Possible null reference assignment.
    private void LoadMaterials(FileItem[] files)
    {
        // safety check ensuring we dont try load directories!
        foreach (var path in (from file in files where file.Type == FileItemType.File select file.Path).Distinct())
            LoadMaterial(path);

        if (loadedMaterials == null || loadedMaterials.Count < 1) return;

        sphere.Material = loadedMaterials.Values.FirstOrDefault();
        materialNames = loadedMaterials.Keys.ToArray().Select(Path.GetFileName).ToArray();
    }
#pragma warning restore CS8601, CS8619 // Possible null reference assignment.

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
    }

    private void SaveMaterial()
    {
        //if (material != null) material.Save(materialPath);
    }

#pragma warning disable CS8619, CS8602 // Nullability of reference types in value doesn't match target type.
    public void Render(GL gl, float dt, RenderOptions? renderOptions = null)
    {
        if (ImGui.Begin("Materials"))
        {
            if (materialNames != null && ImGui.ListBox("", ref selectedMaterialIndex, materialNames, loadedMaterials.Count))
                sphere.Material = loadedMaterials.Values.ElementAt(selectedMaterialIndex);

            if (loadedMaterials.Count > 0 && ImGui.Button("Remove"))
            {
                loadedMaterials.ElementAt(selectedMaterialIndex).Value.Destroy();

                loadedMaterials.Remove(materialNames[selectedMaterialIndex]);
                materialNames = loadedMaterials.Keys.ToArray().Select(Path.GetFileName).ToArray();
                if (selectedMaterialIndex >= materialNames.Length)
                    selectedMaterialIndex = materialNames.Length - 1;
            }
            ImGui.End();
        }

        var options = renderOptions ?? RenderOptions.Default with { 
            Camera = camera
        };

        for (int i = 0; i < Entities.Count; i++)
            Entities[i].Draw(dt, options);
    }
#pragma warning restore CS8619, CS8602 // Nullability of reference types in value doesn't match target type.

    public void Update(float dt)
    {
        for (int i = 0; i < Entities.Count; i++)
            Entities[i].Update(dt);

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