using System;
using ImGuiNET;
using Horizon.Debugging.Debuggers;
using Horizon.GameEntity;
using Horizon.Rendering;
using Horizon.Logging;

namespace Horizon.Debugging
{
    public class SkylineDebugger : Entity
    {
        private readonly struct DebuggerCatagoryNames
        {
            public static string Home { get; } = "Horizon";
            public static string Graphics { get; } = "Graphics";
            public static string Metrics { get; } = "Metrics";
            public static string Scene { get; } = "Scene";
            public static string Content { get; } = "Content";
        }

        public RenderOptionsDebugger RenderOptionsDebugger { get; init; }
        public SceneEntityDebugger SceneEntityDebugger { get; init; }
        public LoadedContentDebugger LoadedContentDebugger { get; init; }
        public DockedGameContainerDebugger GameContainerDebugger { get; init; }
        public PerformanceProfilerDebugger PerformanceDebugger { get; init; }
        public GeneralDebugger GeneralDebugger { get; init; }

        public bool RenderToConatiner { get; private set; }

        public SkylineDebugger()
        {
            RenderOptionsDebugger = AddComponent<RenderOptionsDebugger>();
            SceneEntityDebugger = AddComponent<SceneEntityDebugger>();
            LoadedContentDebugger = AddComponent<LoadedContentDebugger>();
            GameContainerDebugger = AddComponent<DockedGameContainerDebugger>();
            PerformanceDebugger = AddComponent<PerformanceProfilerDebugger>();
            GeneralDebugger = AddComponent<GeneralDebugger>();
        }

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
            RenderToConatiner = Enabled && GameContainerDebugger.Visible;

            if (!Enabled) return;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu(DebuggerCatagoryNames.Home))
                {
                    if (ImGui.MenuItem("Close"))
                        GameManager.Instance.Window.Close();

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu(DebuggerCatagoryNames.Graphics))
                {
                    ImGui.MenuItem(RenderOptionsDebugger.Name, "", ref RenderOptionsDebugger.Visible);
                    ImGui.MenuItem(GameContainerDebugger.Name, "", ref GameContainerDebugger.Visible);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu(DebuggerCatagoryNames.Metrics))
                {
                    ImGui.MenuItem(PerformanceDebugger.Name, "", ref PerformanceDebugger.Visible);
                    ImGui.MenuItem(GeneralDebugger.Name, "", ref GeneralDebugger.Visible);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu(DebuggerCatagoryNames.Scene))
                {
                    ImGui.MenuItem(SceneEntityDebugger.Name, "", ref SceneEntityDebugger.Visible);
                    ImGui.MenuItem("Debug Entire Instance", "", ref SceneEntityDebugger.DebugInstance);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu(DebuggerCatagoryNames.Content))
                {
                    ImGui.MenuItem(LoadedContentDebugger.Name, "", ref LoadedContentDebugger.Visible);
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }

            base.Draw(dt, renderOptions);
        }
    }
}

