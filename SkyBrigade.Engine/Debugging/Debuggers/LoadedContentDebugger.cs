using System;
using System.Numerics;
using ImGuiNET;
using SkyBrigade.Engine.Content;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
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
                var columnWidth = ImGui.GetContentRegionAvail().X;
                var itemSpacing = ImGui.GetStyle().ItemSpacing.X;

                if (ImGui.TreeNodeEx("Textures", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var imageSideLength = 100;
                    var imagesPerRow = Math.Max(1, (int)(columnWidth / (imageSideLength + itemSpacing)));

                    ImGui.Columns(imagesPerRow, "TextureColumns", false);

                    foreach (var texture in ContentManager.GetTextures())
                    {
                        ImGui.BeginGroup();

                        ImGui.Image((IntPtr)texture.Handle, new Vector2(imageSideLength, imageSideLength));
                        ImGui.TextWrapped(texture.Path.ToString());

                        ImGui.EndGroup();
                        ImGui.NextColumn();
                    }

                    ImGui.Columns(1);
                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Shaders", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    foreach (var shader in ContentManager.GetShaders())
                    {
                        ImGui.Text($"Shader: {shader.ToString()}");
                        // Add more shader preview UI elements here
                    }
                    ImGui.TreePop();
                }

                ImGui.End();
            }
        }

        public void Update(float dt)
        {
            
        }
    }
}

