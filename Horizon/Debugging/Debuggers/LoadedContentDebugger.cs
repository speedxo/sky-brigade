using Horizon.Content;
using Horizon.OpenGL;
using Horizon.Rendering;
using ImGuiNET;
using System.Numerics;

namespace Horizon.Debugging.Debuggers
{
    public class LoadedContentDebugger : DebuggerComponent
    {
        private SkylineDebugger Debugger { get; set; }
        public ContentManager ContentManager { get; private set; }

        public override void Initialize()
        {
            Debugger = (SkylineDebugger)Parent!;
            ContentManager = GameManager.Instance.ContentManager;

            Name = "Content Manager";
        }

        public override void Draw(float dt, RenderOptions? options = null)
        {
            if (!Visible) return;

            if (ImGui.Begin(Name))
            {
                DrawTextureSection();
                DrawShaderSection();

                ImGui.End();
            }
        }

        private void DrawTextureSection()
        {
            var columnWidth = ImGui.GetContentRegionAvail().X;
            var itemSpacing = ImGui.GetStyle().ItemSpacing.X;

            if (ImGui.TreeNode("Textures"))
            {
                var imageSideLength = 100;
                var imagesPerRow = Math.Max(1, (int)(columnWidth / (imageSideLength + itemSpacing)));

                ImGui.Columns(imagesPerRow, "TextureColumns", false);

                foreach (var texture in ContentManager.GetTextures())
                {
                    texture.Name ??= texture.Handle.ToString();

                    //if (texture.Name is null) continue;

                    ImGui.BeginGroup();

                    ImGui.Image((IntPtr)texture.Handle, new Vector2(imageSideLength, imageSideLength));
                    ImGui.TextWrapped(texture.Name ?? $"Texture({texture.Handle})");

                    ImGui.EndGroup();

                    DrawTextureContextMenu(texture);

                    ImGui.NextColumn();
                }

                ImGui.Columns(1);
                ImGui.TreePop(); // Moved here
            }
        }

        private void DrawTextureContextMenu(Texture texture)
        {
            if (ImGui.BeginPopupContextItem($"TextureContextMenu_{texture.Path}"))
            {
                if (ImGui.MenuItem("Delete"))
                {
                    if (texture.Name is not null)
                        ContentManager.DeleteTexture(texture.Name);
                }
                ImGui.EndPopup();
            }
        }

        private void DrawShaderSection()
        {
            if (ImGui.TreeNode("Shaders"))
            {
                foreach (var shader in ContentManager.GetShaders())
                {
                    ImGui.Text($"Shader: {shader.ToString()}");
                    // Add more shader preview UI elements here
                }
                ImGui.TreePop();
            }
        }

        public override void Update(float dt)
        {
        }
    }
}