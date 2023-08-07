using System;
using ImGuiNET;
using SkyBrigade.Engine.Debugging.Debuggers;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Debugging
{
    public class Debugger : Entity
    {
        /// <summary>
        /// When true, the debugger will be displayed.
        /// </summary>
        public bool IsVisible { get; set; }

        public RenderOptionsDebugger RenderOptionsDebugger { get; init; }
        public SceneEntityDebugger SceneEntityDebugger { get; init; }

        public Debugger()
        {
            RenderOptionsDebugger = AddComponent<RenderOptionsDebugger>();
            SceneEntityDebugger = AddComponent<SceneEntityDebugger>();
        }

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
            if (!IsVisible) return;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Rendering"))
                {
                    ImGui.MenuItem("Rendering", "", ref RenderOptionsDebugger.Visible);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Scene"))
                {
                    ImGui.MenuItem("Scene Tree", "", ref SceneEntityDebugger.Visible);
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }

            base.Draw(dt, renderOptions);
        }
    }
}

