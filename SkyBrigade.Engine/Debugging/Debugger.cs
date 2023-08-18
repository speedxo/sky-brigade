using System;
using ImGuiNET;
using Horizon.Debugging.Debuggers;
using Horizon.GameEntity;
using Horizon.Rendering;

namespace Horizon.Debugging
{
    public class Debugger : Entity
    {
        /// <summary>
        /// When true, the debugger will be displayed.
        /// </summary>
        public bool Enabled { get; set; }

        public RenderOptionsDebugger RenderOptionsDebugger { get; init; }
        public SceneEntityDebugger SceneEntityDebugger { get; init; }
        public LoadedContentDebugger LoadedContentDebugger { get; init; }
        public DockedGameContainerDebugger GameContainerDebugger { get; init; }
        public PerformanceProfilerDebugger PerformanceDebugger { get; init; }

        public bool RenderToConatiner { get; private set; }

        public Debugger()
        {
            RenderOptionsDebugger = AddComponent<RenderOptionsDebugger>();
            SceneEntityDebugger = AddComponent<SceneEntityDebugger>();
            LoadedContentDebugger = AddComponent<LoadedContentDebugger>();
            GameContainerDebugger = AddComponent<DockedGameContainerDebugger>();
            PerformanceDebugger = AddComponent<PerformanceProfilerDebugger>();
        }

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
            RenderToConatiner = Enabled && GameContainerDebugger.Visible;

            if (!Enabled) return;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Engine"))
                {
                    if (ImGui.MenuItem("Close"))
                        GameManager.Instance.Window.Close();

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Rendering"))
                {
                    ImGui.MenuItem("Rendering", "", ref RenderOptionsDebugger.Visible);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Performance"))
                {
                    ImGui.MenuItem("Profiler", "", ref PerformanceDebugger.Visible);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Scene"))
                {
                    ImGui.MenuItem("Scene Tree", "", ref SceneEntityDebugger.Visible);
                    ImGui.MenuItem("Debug Entire Instance", "", ref SceneEntityDebugger.DebugInstance);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Content"))
                {
                    ImGui.MenuItem("Asset Manager", "", ref LoadedContentDebugger.Visible);
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }

            base.Draw(dt, renderOptions);
        }
    }
}

