namespace SkyBrigade.MaterialEditor;

using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;
using static SkyBrigade.Engine.GameManager;

internal enum EditorMenuBarItem
{
    Save,
    Close
}

// The general entity primitive is perfect for stuff like this
// where we need an object to have properties or functions but
// no implmentation, hence why we have both an IEntity interface,
// and a less abstract Entity class with function.
internal class EditorMenuBar : IEntity
{
    public delegate void OnMenuItemClickDelegate(EditorMenuBarItem item);

    public event OnMenuItemClickDelegate? OnMenuItemClicked;

    // we will not be using renderOptions here as this entire
    // class is just a big wrapper around ImGui
    public void Draw(RenderOptions? renderOptions = null)
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Save"))
                    OnMenuItemClicked?.Invoke(EditorMenuBarItem.Save);

                if (ImGui.MenuItem("Exit"))
                    OnMenuItemClicked?.Invoke(EditorMenuBarItem.Close);
            }
        }
    }

    public void Update(float dt)
    {
    }
}

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

    private List<IEntity> entities;

    public void Initialize(GL gl)
    {
        //if (!TryLoadMaterial(ProgramArgs.FirstOrDefault()))
        //    LoadDefaultMaterial();

        var menuBar = new EditorMenuBar();
        menuBar.OnMenuItemClicked += MenuBar_MenuItemClicked;

        entities = new List<IEntity> {
            menuBar
        };
    }

    //private bool TryLoadMaterial(string? path)
    //{
    //    if (path?.CompareTo(string.Empty) == 0) return false;
    //    if (!File.Exists(path)) return false;

    //    return true;
    //}

    //private void LoadDefaultMaterial()
    //{
    //    material = AdvancedMaterial.LoadFromZip("default.material");
    //}

    private void MenuBar_MenuItemClicked(EditorMenuBarItem item)
    {
        //switch (item)
        //{
        //    case EditorMenuBarItem.Close:
        //        CloseWindow(); break;
        //    case EditorMenuBarItem.Save:
        //        SaveMaterial(); break;
        //}
    }

    //private void SaveMaterial()
    //{
    //}

    public void Render(GL gl, float dt)
    {
        entities.ForEach(e => e.Draw());
    }

    public void Update(float dt)
    {
        entities.ForEach(e => e.Update(dt));
    }

    //private void CloseWindow()
    //{
    //    Instance.Window.Close();
    //}

    public void Dispose()
    {
    }
}