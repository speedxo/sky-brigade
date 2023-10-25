using Horizon.Collections;
using Horizon.Rendering;
using ImGuiNET;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Security.Cryptography;
using Horizon.GameEntity;
using Monitor = System.Threading.Monitor;
using Horizon.Data;
using System.Diagnostics.CodeAnalysis;
using ImPlotNET;
using System.Xml.Serialization;
using System.Reflection.Emit;

namespace Horizon.Debugging.Debuggers;

public class PerformanceProfilerDebugger : DebuggerComponent
{
    /// <summary>
    /// How many times/s metrics are collected.
    /// </summary>
    public int UpdateRate
    {
        get => (int)(1 / _updateRate);
        set => _updateRate = 1.0f / value;
    }

    private float _updateRate = 1.0f / 50.0f;
    private float _updateTimer = 0.0f;

    private SkylineDebugger Debugger { get; set; }

    public readonly Metrika CpuMetrics = new ();
    public readonly Metrika GpuMetrics = new();


    private LinearBuffer<double> _updateFrameTimes;
    private LinearBuffer<double> _renderFrameTimes;

    private long _prevTimestamp;
    private long _prevCpuTime;

    public override void Initialize()
    {
        Name = "Profiler";
        
        Debugger = (Parent as SkylineDebugger)!;

        _updateFrameTimes = new(100);
        _renderFrameTimes = new(100);


        // Initialize requried dictionaries by inference.
        CpuMetrics.AddCustom("Engine", "CPU", 0.0);
        GpuMetrics.AddCustom("Engine", "GPU", 0.0);

        //// Spritebatches (for 2d)
        CpuMetrics.CreateCategory("EngineComponents");
        GpuMetrics.CreateCategory("EngineComponents");

        Entity.Engine.OnPreDraw += (_) => {
            CpuMetrics.ResetMetrics();
        };
        Entity.Engine.OnPreUpdate += (_) => {
            GpuMetrics.ResetMetrics();
        };

        Entity.Engine.OnPostUpdate += UpdateMetrics;
    }

    private void UpdateMetrics(float dt)
    {
        if (!Visible) return;

        _updateTimer += dt;

        if (_updateTimer > _updateRate)
        {
            _updateTimer = 0.0f;
            _updateFrameTimes.Append((CpuMetrics.Categories["Engine"]["CPU"] * 1000.0));
            _renderFrameTimes.Append((GpuMetrics.Categories["Engine"]["GPU"] * 1000.0));
        }
    }
    public override void Draw(float dt, ref RenderOptions options)
    {
        if (!Visible) return;

        if (ImGui.Begin(Name))
        {
            if (ImGui.CollapsingHeader("Logic Profiler"))
                DrawCpuProfiling();
            if (ImGui.CollapsingHeader("Render Profiler"))
                DrawGpuProfling();

            ImGui.End();
        }
    }

    private void DrawGpuProfling()
    {
        PlotValues("Frametime (GPU)", in _renderFrameTimes);
        DrawProfiler(GpuMetrics);

        if (GpuMetrics.Categories["EngineComponents"].Keys.Any() && ImPlot.BeginPlot(
          "Test",
           new Vector2(ImGui.GetContentRegionAvail().X, 200.0f), ImPlotFlags.Equal))
        {
            string[] names = GpuMetrics.Categories["EngineComponents"].Keys.ToArray();
            double[] values = GpuMetrics.Categories["EngineComponents"].Values.ToArray().Select((r) => { return r * 1000.0; }).ToArray();

            ImPlot.SetupAxes(null, null, ImPlotAxisFlags.AutoFit, ImPlotAxisFlags.AutoFit);
            ImPlot.PlotPieChart(names, ref values[0], names.Length, 0.0, 0.0, 1.0, "", 0.0, ImPlotPieChartFlags.Normalize);

            ImPlot.EndPlot();
        }
    }

    private void DrawCpuProfiling()
    {
        PlotValues("Frametime (CPU)", in _updateFrameTimes);
        DrawProfiler(CpuMetrics);
    }

    private void DrawProfiler(Metrika gpuMetrics)
    {
        foreach (var categoryEntry in gpuMetrics.Categories)
        {
            if (categoryEntry.Key.CompareTo("Engine") == 0) continue;

            ImGui.Text(categoryEntry.Key);

            foreach (var valueEntry in categoryEntry.Value)
            {
                ImGui.Columns(2, "ProfilerTimerValueColumns", true);

                ImGui.Text(valueEntry.Key);
                ImGui.NextColumn();
                ImGui.Text((valueEntry.Value * 1000.0).ToString("0.00"));

                ImGui.Columns(1);
            }
        }

    }

    [Pure]
    private static void PlotValues(
      in string label,
      in LinearBuffer<double> frameTimes,
      in string unit = "ms"
  )
    {
        var windowWidth = ImGui.GetContentRegionAvail().X;
        var averageFrameTime = frameTimes.Buffer.Average();
        var minFrameTime = frameTimes.Buffer.Min();
        var maxFrameTime = frameTimes.Buffer.Max();

        ImPlot.SetNextAxisLimits(ImAxis.X1, 0, frameTimes.Length);
        ImPlot.SetNextAxisLimits(ImAxis.Y1, minFrameTime, maxFrameTime * 1.2);

        if (ImPlot.BeginPlot(
            $"{label} - Avg: {averageFrameTime:0.00}{unit} - Max Diff: {(maxFrameTime - minFrameTime):0.00}{unit} - Excp. FPS: {1.0f / (averageFrameTime / 1000.0f):0}FPS",
            new Vector2(windowWidth, 200.0f)))
        {
            ImPlot.PlotLine("", ref frameTimes.Buffer[0], frameTimes.Length, 1.0f, 0.0, ImPlotLineFlags.Shaded, frameTimes.Index);

            ImPlot.EndPlot();
        }
    }

    public override void Dispose()
    {
        // Metrika subscribes to engine events, so we need to make sure to clean 'em up.
        CpuMetrics.Dispose();
        GpuMetrics.Dispose();
    }

    [Pure]
    private static float GetMemoryUsage()
    {
        return (float)(GC.GetTotalMemory(false) / (1024.0 * 1024.0)); // in MB
    }

    public override void Update(float dt) 
    {  }
}
