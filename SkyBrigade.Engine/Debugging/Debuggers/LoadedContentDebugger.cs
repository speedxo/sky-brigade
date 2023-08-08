using System;
using System.Numerics;
using ImGuiNET;
using SkyBrigade.Engine.Content;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Debugging.Debuggers
{
	public class LoadedContentDebugger : IGameComponent
	{
        public string Name { get; set; }

        public Entity Parent { get; set; }
        private Debugger Debugger { get; set; }
        public ContentManager ContentManager { get; private set; }

        public void Initialize()
        {
            Debugger = Parent as Debugger;
            ContentManager = GameManager.Instance.ContentManager;
        }

        public bool Visible = false;

        public void Draw(float dt, RenderOptions? options = null)
        {
            if (!Visible) return;

            if (ImGui.Begin("Content Manager"))
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
                    if (texture.Name is null) continue;

                    ImGui.BeginGroup();

                    ImGui.Image((IntPtr)texture.Handle, new Vector2(imageSideLength, imageSideLength));
                    ImGui.TextWrapped(texture.Path.ToString());

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



        public void Update(float dt)
        {
            
        }
    }
}

