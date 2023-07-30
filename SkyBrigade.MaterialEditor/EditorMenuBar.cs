namespace SkyBrigade.MaterialEditor;

using ImGuiNET;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;

// The general entity primitive is perfect for stuff like this
// where we need an object to have properties or functions but
// no implmentation, hence why we have both an IEntity interface,
// and a less abstract Entity class with function.
internal class EditorMenuBar : IEntity
{
    public List<IEntity> Entities { get; set; }

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
                if (ImGui.MenuItem("Open File"))
                    OnMenuItemClicked?.Invoke(EditorMenuBarItem.OpenFile);

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