﻿using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.Rendering;
using SkyBrigade.Engine.Rendering.Shapes;
using SkyBrigade.Engine.Prefabs.Character;
using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Rendering.Effects;
using SkyBrigade.Engine.Rendering.Effects.Components;

namespace SkyBrigade.Game;

internal class DemoGameScreen : Scene
{
    private Plane plane;
    private CharacterController character;

    public DemoGameScreen()
        :base()
    {
        InitializeRenderingPipeline();

        GameManager.Instance.Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        GameManager.Instance.Gl.Enable(EnableCap.DepthTest);

        character = new CharacterController();
        AddEntity(character);

        plane = new Plane(new BasicMaterial());
        
        plane.Transform.Scale = new System.Numerics.Vector3(10);
        plane.Transform.Rotation = new System.Numerics.Vector3(90, 0, 0);

        plane.Material.Texture = GameManager.Instance.ContentManager.GetTexture("debug");

        AddEntity(plane);

        GameManager.Instance.Debugger.Enabled = true;

    }

    protected override Effect[] GeneratePostProccessingEffects()
    {
        return new[] { new FlashingEffect() };
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        var options = (renderOptions ?? RenderOptions.Default) with
        {
            Camera = character.Camera
        };

        base.Draw(dt, options);
    }

    public override void DrawGui(float dt)
    {
        if (ImGui.Begin("Debug"))
        {
            ImGui.Text($"Memory Consumption: {float.Round(GC.GetTotalMemory(false) / 1024.0f / 1024, 2)}MB");
            ImGui.Text($"Memory Delta: {memoryTracker.GetAverage()}KB");
            ImGui.PlotLines("dKb/dt", ref memoryTracker.Buffer[0], memoryTracker.Buffer.Length);
            ImGui.Text($"FPS: {MathF.Round(1.0f / dt)}");
            ImGui.End();
        }

        if (ImGui.Begin("Character Controller"))
        {
            ImGui.Text($"Position: {character.Position}");
            ImGui.Text($"Rotation: {character.Rotation}");
            ImGui.End();
        }
    }

    private readonly DeltaTracker<float> memoryTracker = new((prev, current) => current - prev);

    public override void Update(float dt)
    {
        base.Update(dt);

        memoryTracker.Update(GC.GetTotalMemory(false) / 1024.0f / 1024.0f);

        if (GameManager.Instance.InputManager.WasPressed(Engine.Input.VirtualAction.Interact))
            GameManager.Instance.Debugger.Enabled = !GameManager.Instance.Debugger.Enabled;
    }


    public override void Dispose()
    {

    }
}