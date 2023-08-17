using System;
using ImGuiNET;
using SkyBrigade.Engine.Content;
using System.Numerics;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;
using Silk.NET.Windowing;
using System.Diagnostics;
using Silk.NET.SDL;
using Monitor = System.Threading.Monitor;
using SkyBrigade.Engine.Collections;

namespace SkyBrigade.Engine.Debugging.Debuggers;

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

    private object _updateLock = new object();
    private object _renderLock = new object();

    public bool Visible = false;

    public void Initialize()
    {
        Debugger = Parent as Debugger;

        _renderStopwatch = new Stopwatch();
        _updateStopwatch = new Stopwatch();

        _updateFrameTimes = new(100);
        _renderFrameTimes = new(100);

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

        if (ImGui.Begin("Performance Profiler"))
        {
            float maxUpdateFrameTime, maxRenderFrameTime;
            lock (_updateLock)
            {
                maxUpdateFrameTime = _updateFrameTimes.Buffer.Max();
            }
            lock (_renderLock)
            {
                maxRenderFrameTime = _renderFrameTimes.Buffer.Max();
            }

            ImGui.Text($"Max Update Frame Time: {maxUpdateFrameTime:0.00} ms");
            ImGui.Text($"Max Render Frame Time: {maxRenderFrameTime:0.00} ms");
            ImGui.Text($"CPU Usage: {_cpuUsage:0.00}%");
            ImGui.Text($"Memory Usage: {_memoryUsage:0.00} MB");

            // Plot the update frame times history
            lock (_updateLock)
            {
                ImGui.PlotLines("", ref _updateFrameTimes.Buffer[0], _updateFrameTimes.Length, _updateFrameTimes.Index, $"CPU(frametime) Avg: {_updateFrameTimes.Buffer.Average():0.00} ms", 0.0f, maxUpdateFrameTime * 1.2f, new System.Numerics.Vector2(0, 80));
            }

            // Plot the render frame times history
            lock (_renderLock)
            {
                ImGui.PlotLines("", ref _renderFrameTimes.Buffer[0], _renderFrameTimes.Length, _renderFrameTimes.Index, $"GPU(frametime) Avg: {_renderFrameTimes.Buffer.Average():0.00} ms", 0.0f, maxRenderFrameTime * 1.2f, new System.Numerics.Vector2(0, 80));
            }

            ImGui.End();
        }
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


    private float GetMemoryUsage()
    {
        return (float)(GC.GetTotalMemory(false) / (1024.0 * 1024.0)); // in MB
    }

    public void Update(float dt)
    {

    }
}

