namespace SkyBrigade.Engine.Dialogs;

using ImGuiNET;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;

public class OpenFileDialog : IEntity
{
    public string? FileName { get; set; }
    public string? Filter { get; set; }
    public bool Multiselect { get; set; }

    private List<FileItem> files = new();
    private DirectoryInfo? currentDirectory;
    private string[] fileNames = Array.Empty<string>();
    private string[] selectedFileNames = Array.Empty<string>();
    private int index = 0, selectedFileIndex = 0;
    private List<FileItem> selectedFiles = new();

    public delegate void OnFileSelected(string path);

    public event OnFileSelected? FileSelected;

    public delegate void OnFilesSelected(FileItem[] files);

    public event OnFilesSelected? FilesSelected;

    private static string? prevDir;

    public OpenFileDialog(string filter = "")
    {
        Filter = filter;
        LoadFiles(prevDir ?? Directory.GetCurrentDirectory());
    }

    private void LoadFiles(string dir)
    {
        currentDirectory = new DirectoryInfo(dir);

        files.Clear();

        files.Add(new FileItem("..", currentDirectory?.Parent?.FullName ?? "", FileItemType.Directory));

        files.AddRange(from file in currentDirectory?.GetDirectories()
                       select new FileItem(file.Name, file.FullName, FileItemType.Directory));
        foreach (var (file, fileName) in from file in currentDirectory?.GetFiles()
                                         let fileName = file.Name
                                         select (file, fileName))
        {
            if (!string.IsNullOrEmpty(Filter))
            {
                var ext = Path.GetExtension(fileName);

                if (ext != Filter)
                    continue;
            }

            files.Add(new FileItem(file.Name, file.FullName, FileItemType.File));
        }

        fileNames = files.Select(f => f.Name).ToArray();
        prevDir = dir;
    }

    public bool ShowDialog()
    {
        if (ImGui.Begin("Open File"))
        {
            ImGui.Text("Select a file to open.");

            ImGui.Separator();
            if (currentDirectory != null)
            {
                string fullName = currentDirectory.FullName;

                // the fullName with the name concatanated towards the beginning
                

                ImGui.Text(currentDirectory.FullName);
                ImGui.Separator();
            }

            if (fileNames != null && fileNames.Length > 0)
            {
                if (ImGui.ListBox("Files", ref index, fileNames, fileNames.Length))
                {
                    var targetDir = files[index];
                    if (targetDir.Type == FileItemType.Directory)
                        LoadFiles(files[index].Path);
                    else if (Multiselect && !selectedFiles.Contains(files[index]))
                    {
                        selectedFiles.Add(files[index]);
                        selectedFileNames = selectedFiles.Select(f => f.Name).ToArray();
                    }
                    else if (!Multiselect)
                    {
                        selectedFiles.Clear();
                        selectedFiles.Add(files[index]);
                        selectedFileNames = selectedFiles.Select(f => f.Name).ToArray();
                    }
                }
                ImGui.Separator();
            }

            if (Multiselect)
            {
                if (selectedFileNames != null && selectedFileNames.Length > 0)
                {
                    if (ImGui.ListBox("Selected Files", ref selectedFileIndex, selectedFileNames, selectedFileNames.Length))
                    {
                        selectedFiles.RemoveAt(selectedFileIndex);
                        selectedFileNames = selectedFiles.Select(f => f.Name).ToArray();
                    }
                    ImGui.Separator();
                }
                if (ImGui.Button("Open"))
                {
                    FilesSelected?.Invoke(selectedFiles.ToArray());
                    return true;
                }
            }
            else
            {
                if (ImGui.Button("Open"))
                {
                    if (files != null && files.Count > 0)
                    {
                        FileName = files[index].Path;
                        FileSelected?.Invoke(FileName);
                        return true;
                    }
                }
            }

            ImGui.End();
        }

        return false;
    }

    public void Draw(float dt, RenderOptions? renderOptions = null)
    {
        ShowDialog();
    }

    public void Update(float dt)
    {
    }
}