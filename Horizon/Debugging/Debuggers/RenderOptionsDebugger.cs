using System;
using ImGuiNET;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;

namespace Horizon.Debugging.Debuggers
{
    public class RenderOptionsDebugger : DebuggerComponent
    {
        private SkylineDebugger Debugger { get; set; }

        private float gamma, ambientStrength;
        private string[] renderModes;
        private int renderModeIndex;
        private bool isPostProcessingEnabled;

        public override void Initialize()
        {
            // Initialize the properties
            gamma = RenderOptions.Default.Gamma;
            ambientStrength = RenderOptions.Default.AmbientLightingStrength;

            // automatically generate the render modes from the DefferedRenderLayer enum
            renderModes = Enum.GetNames(typeof(DefferedRenderLayer));

            isPostProcessingEnabled = RenderOptions.Default.IsPostProcessingEnabled;

            Debugger = Parent as SkylineDebugger;

            Name = "Render Options";
        }

        public RenderOptions RenderOptions { get; private set; }

        public override void Draw(float dt, RenderOptions? options = null)
        {
            if (!Visible) return;

            // Collapsible header for Render Options window
            if (ImGui.Begin(Name))
            {
                ImGui.DragFloat("Gamma", ref gamma, 0.01f, 0.1f, 10.0f);
                ImGui.DragFloat("Ambient", ref ambientStrength, 0.01f, 0.0f, 10.0f);

                // Listbox to select the render mode.
                ImGui.Combo("Render Mode", ref renderModeIndex, renderModes, renderModes.Length);

                ImGui.Checkbox("Post Processing", ref isPostProcessingEnabled);

                ImGui.End();
            }
        }

        public override void Update(float dt)
        {
            RenderOptions = RenderOptions.Default with
            {
                AmbientLightingStrength = ambientStrength,
                Gamma = gamma,
                DebugOptions = DebugRenderOptions.Default with
                {
                    DefferedLayer = (DefferedRenderLayer)renderModeIndex
                },
                IsPostProcessingEnabled = isPostProcessingEnabled
            };
        }
    }
}

