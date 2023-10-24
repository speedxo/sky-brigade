using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;
using ImGuiNET;

namespace Horizon.Dialogs
{
    /// <summary>
    /// Represents an open file dialog for selecting files and directories.
    /// </summary>
    public class OpenFileDialog : Entity
    {
        /// <summary>
        /// Gets or sets the selected file's name with full path.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the file filter for restricting the file types shown in the dialog.
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multiple files can be selected.
        /// </summary>
        public bool Multiselect { get; set; }

        private List<FileItem> files = new();
        private DirectoryInfo? currentDirectory;
        private string[] fileNames = Array.Empty<string>();
        private string[] selectedFileNames = Array.Empty<string>();
        private int index = 0,
            selectedFileIndex = 0;
        private List<FileItem> selectedFiles = new();

        /// <summary>
        /// Delegate for handling the file selection event.
        /// </summary>
        /// <param name="path">The selected file's full path.</param>
        public delegate void OnFileSelected(string path);

        /// <summary>
        /// Event raised when a file is selected.
        /// </summary>
        public event OnFileSelected? FileSelected;

        /// <summary>
        /// Delegate for handling the multiple files selection event.
        /// </summary>
        /// <param name="files">An array of selected files.</param>
        public delegate void OnFilesSelected(FileItem[] files);

        /// <summary>
        /// Event raised when multiple files are selected (if Multiselect is enabled).
        /// </summary>
        public event OnFilesSelected? FilesSelected;

        private static string? prevDir;

        /// <summary>
        /// Initializes a new instance of the OpenFileDialog class.
        /// </summary>
        /// <param name="filter">File filter to restrict the displayed file types (optional).</param>
        public OpenFileDialog(string filter = "")
        {
            Filter = filter;
            LoadFiles(prevDir ?? Directory.GetCurrentDirectory());
        }

        private void LoadFiles(string dir)
        {
            currentDirectory = new DirectoryInfo(dir);
            files.Clear();
            files.Add(
                new FileItem("..", currentDirectory?.Parent?.FullName ?? "", FileItemType.Directory)
            );
            files.AddRange(
                currentDirectory
                    ?.GetDirectories()
                    ?.Select(file => new FileItem(file.Name, file.FullName, FileItemType.Directory))
                    ?? Enumerable.Empty<FileItem>()
            );

            foreach (
                var (file, fileName) in (
                    currentDirectory?.GetFiles() ?? Enumerable.Empty<FileInfo>()
                ).Select(file => (file, file.Name))
            )
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

        /// <summary>
        /// Shows the open file dialog.
        /// </summary>
        /// <returns>True if a file was selected and the dialog was closed; otherwise, false.</returns>
        public bool ShowDialog()
        {
            if (ImGui.Begin("Open File"))
            {
                ImGui.Text("Select a file to open.");

                ImGui.Separator();
                if (currentDirectory != null)
                {
                    string fullName = currentDirectory.FullName;
                    // the fullName with the name concatenated towards the beginning

                    ImGui.Text(fullName);
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
                        if (
                            ImGui.ListBox(
                                "Selected Files",
                                ref selectedFileIndex,
                                selectedFileNames,
                                selectedFileNames.Length
                            )
                        )
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

        /// <summary>
        /// Draws the open file dialog (IEntity implementation).
        /// </summary>
        /// <param name="dt">Time elapsed since the last update.</param>
        /// <param name="options">Optional render options (unused in this implementation).</param>
        public override void Draw(float dt, ref RenderOptions options)
        {
            // Show the open file dialog on the screen.
            ShowDialog();
        }
    }
}
