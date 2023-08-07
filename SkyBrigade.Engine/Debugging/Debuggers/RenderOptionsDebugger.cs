using System;
using ImGuiNET;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Debugging.Debuggers
{
    public class RenderOptionsDebugger : IGameComponent
    {
        public string Name { get; set; }

        public Entity Parent { get; set; }
        private Debugger Debugger { get; set; }

        private float gamma, ambientStrength;
        private string[] renderModes;
        private int renderModeIndex;

        public void Initialize()
        {
            // Initialize the properties
            gamma = RenderOptions.Default.Gamma;
            ambientStrength = RenderOptions.Default.AmbientLightingStrength;

            // automatically generate the render modes from the DefferedRenderLayer enum
            renderModes = Enum.GetNames(typeof(DefferedRenderLayer));

            Debugger = Parent as Debugger;
        }

        public RenderOptions RenderOptions { get; private set; }

        public bool Visible = false;

        public void Draw(float dt, RenderOptions? options = null)
        {
            if (!Visible) return;

            // Collapsible header for Render Options window
            if (ImGui.Begin("Render Options"))
            {
                ImGui.DragFloat("Gamma", ref gamma, 0.01f, 0.1f, 10.0f);
                ImGui.DragFloat("Ambient", ref ambientStrength, 0.01f, 0.0f, 10.0f);

                // Listbox to select the render mode.
                ImGui.Combo("Render Mode", ref renderModeIndex, renderModes, renderModes.Length);

                ImGui.End();
            }
        }

        public void Update(float dt)
        {
            RenderOptions = RenderOptions.Default with
            {
                AmbientLightingStrength = ambientStrength,
                Gamma = gamma,
                DebugOptions = DebugRenderOptions.Default with
                {
                    DefferedLayer = (DefferedRenderLayer)renderModeIndex
                }
            };
        }
    }
}

