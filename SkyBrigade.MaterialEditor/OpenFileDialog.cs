namespace SkyBrigade.MaterialEditor;
using ImGuiNET;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;

public class OpenFileDialog : IEntity
{
    public string? FileName { get; set; }
    public string? Filter { get; set; }

    private enum FileItemType
    {
        File,
        Directory
    }

    private record struct FileItem(string Name, string Path, FileItemType Type);

    
    private List<FileItem> files = new();
    private DirectoryInfo? currentDirectory;
    private string[] fileNames = Array.Empty<string>();
    private int index = 0;

    public delegate void OnFileSelected(string path);
    public event OnFileSelected? FileSelected;

    private static string? prevDir;


    public OpenFileDialog(string filter="")
    {
        Filter = filter;
        LoadFiles(prevDir ?? Directory.GetCurrentDirectory());
    }

    private void LoadFiles(string dir)
    {
        currentDirectory = new DirectoryInfo(dir);

        files.Clear();
        files.Add(new FileItem("..", currentDirectory?.Parent?.FullName ?? "", FileItemType.Directory));

        foreach (var file in currentDirectory.GetDirectories())
            files.Add(new FileItem(file.Name, file.FullName, FileItemType.Directory));

        foreach (var file in currentDirectory.GetFiles())
        {
            string fileName = file.Name;

            if (!string.IsNullOrEmpty(Filter))
            {
                var ext = Path.GetExtension(fileName);
                
                if (ext != Filter)
                    continue;
            }

            files.Add(new FileItem(file.Name, file.FullName, FileItemType.File));
        }


        fileNames = files.Select(f => f.Name).ToArray();
    }

    public bool ShowDialog()
    {
        if (ImGui.Begin("Open File"))
        {
            ImGui.Text("Select a file to open.");
            ImGui.Separator();
            if (ImGui.ListBox("Files", ref index, fileNames, fileNames.Length))
            {
                var targetDir = files[index];
                if (targetDir.Type == FileItemType.Directory)
                    LoadFiles(files[index].Path);
            }

            if (ImGui.Button("Open"))
            {
                FileName = files[index].Path;
                FileSelected?.Invoke(FileName);
                return true;
            }
            ImGui.End();
        }

        return false;
    }

    public void Draw(RenderOptions? renderOptions = null)
    {
        ShowDialog();
    }

    public void Update(float dt)
    {

    }
}