using System;
using ImGuiNET;
using Horizon.Content;
using System.Numerics;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;
using Silk.NET.Windowing;
using System.Diagnostics;
using Silk.NET.SDL;
using Monitor = System.Threading.Monitor;
using Horizon.Collections;
using System.Diagnostics.Contracts;

namespace Horizon.Debugging.Debuggers;

public class PerformanceProfilerDebugger : IGameComponent
{
    public string Name { get; set; } = "Performance Profiler";
    public Entity Parent { get; set; }

    private Debugger Debugger { get; set; }

    private Stopwatch _renderStopwatch;
    private Stopwatch _updateStopwatch;

    private LinearBuffer<float> _updateFrameTimes;
    private LinearBuffer<float> _renderFrameTimes;


    private long _prevTimestamp;
    private float _cpuUsage;
    private long _prevCpuTime;
    private float _memoryUsage;

    private readonly object _updateLock = new object();
    private readonly object _renderLock = new object();

    public bool Visible = false;

    public void Initialize()
    {
#pragma warning disable CS8601 // SID, SHIT THE FUCK UP
        Debugger = Parent as Debugger;
#pragma warning restore CS8601 // SID, SHIT THE FUCK UP

        _renderStopwatch = new Stopwatch();
        _updateStopwatch = new Stopwatch();

        _updateFrameTimes = new(500);
        _renderFrameTimes = new(500);

        _prevTimestamp = Stopwatch.GetTimestamp();
        _prevCpuTime = Process.GetCurrentProcess().TotalProcessorTime.Ticks;
    }

    public void UpdateStart()
    {
        if (!Visible) return;

        Monitor.Enter(_updateLock);
        _updateStopwatch.Restart();
    }

    public void UpdateEnd()
    {
        if (!Visible) return;

        _updateStopwatch.Stop();

        _cpuUsage = CalculateCPUUsage();
        _memoryUsage = GetMemoryUsage();

        lock (_updateLock)
        {
            _updateFrameTimes.Append((float)_updateStopwatch.Elapsed.TotalMilliseconds);
        }
    }

    public void RenderStart()
    {
        if (!Visible) return;

        Monitor.Enter(_renderLock);
        _renderStopwatch.Restart();
    }

    public void RenderEnd()
    {
        if (!Visible) return;

        _renderStopwatch.Stop();
        lock (_renderLock)
        {
            _renderFrameTimes.Append((float)_renderStopwatch.Elapsed.TotalMilliseconds);
        }
    }
    public void Draw(float dt, RenderOptions? options = null)
    {
        if (!Visible) return;

        ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Performance Profiler"))
        {
            float maxUpdateFrameTime, maxRenderFrameTime;
            lock (_updateLock) maxUpdateFrameTime = _updateFrameTimes.Buffer.Max();
            lock (_renderLock) maxRenderFrameTime = _renderFrameTimes.Buffer.Max();

            ImGui.Text($"Max Update Frame Time: {maxUpdateFrameTime:0.00} ms");
            ImGui.Text($"Max Render Frame Time: {maxRenderFrameTime:0.00} ms");
            ImGui.Text($"CPU Usage: {_cpuUsage:0.00}%");
            ImGui.Text($"Memory Usage: {_memoryUsage:0.00} MB");

            PlotFrameTimes("Update Frame Times", _updateFrameTimes, maxUpdateFrameTime);
            PlotFrameTimes("Render Frame Times", _renderFrameTimes, maxRenderFrameTime);

            ImGui.End();
        }
    }

    [Pure]
    private static void PlotFrameTimes(string label, LinearBuffer<float> frameTimes, float maxValue)
    {
        float windowWidth = ImGui.GetContentRegionAvail().X;
        float averageFrameTime = frameTimes.Buffer.Average();

        ImGui.PlotLines(
            "",
            ref frameTimes.Buffer[0],
            frameTimes.Length,
            frameTimes.Index,
            $"{label} - avg: {averageFrameTime:0.00} ms",
            0.0f,
            maxValue * 1.2f,
            new Vector2(windowWidth, 80)
        );
    }


    private float CalculateCPUUsage()
    {
        var currentCpuTime = Process.GetCurrentProcess().TotalProcessorTime.Ticks;
        var elapsedCpuTime = currentCpuTime - _prevCpuTime;
        var elapsedTimestamps = Stopwatch.GetTimestamp() - _prevTimestamp;
        _prevCpuTime = currentCpuTime;
        _prevTimestamp = Stopwatch.GetTimestamp();

        // Calculate CPU usage as a percentage of time spent by the process
        // on all available CPU cores.
        float cpuUsage = (float)(100.0 * elapsedCpuTime / elapsedTimestamps);

        return cpuUsage;
    }

    [Pure]
    private static float GetMemoryUsage()
    {
        return (float)(GC.GetTotalMemory(false) / (1024.0 * 1024.0)); // in MB
    }

    public void Update(float dt)
    {

    }
}

