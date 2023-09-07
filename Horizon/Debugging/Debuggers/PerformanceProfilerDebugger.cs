using Horizon.Collections;
using Horizon.Rendering;
using ImGuiNET;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using Monitor = System.Threading.Monitor;

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

    private SkylineDebugger Debugger { get; set; }

    private Stopwatch _renderStopwatch;
    private Stopwatch _updateStopwatch;

    private LinearBuffer<float> _updateFrameTimes;
    private LinearBuffer<float> _renderFrameTimes;
    private LinearBuffer<float> _memoryUsage;
    private LinearBuffer<float> _frameTimers;

    private long _prevTimestamp;
    private float _cpuUsage;
    private long _prevCpuTime;
    private float _memoryFootprint;

    private readonly object _updateLock = new object();
    private readonly object _renderLock = new object();
    private readonly object _memoryLock = new object();

    private float _updateTimer;
    private float _renderTimer;

    private bool _pauseRenderMetrics;
    private bool _pauseUpdateMetrics;

    public override void Initialize()
    {
#pragma warning disable CS8601 // SID, SHIT THE FUCK UP
        Debugger = Parent as SkylineDebugger;
#pragma warning restore CS8601 // SID, SHIT THE FUCK UP

        _renderStopwatch = new Stopwatch();
        _updateStopwatch = new Stopwatch();

        _updateFrameTimes = new(100);
        _renderFrameTimes = new(100);
        _frameTimers = new(100);
        _memoryUsage = new(100);

        _prevTimestamp = Stopwatch.GetTimestamp();
        _prevCpuTime = Process.GetCurrentProcess().TotalProcessorTime.Ticks;

        Name = "Performance Metrics";
    }

    public void UpdateStart(float dt)
    {
        if (!Visible)
            return;
        _updateTimer += dt;

        if (_pauseUpdateMetrics = (_updateTimer < _updateRate))
            return;

        _updateTimer = 0.0f;
        Monitor.Enter(_updateLock);
        _updateStopwatch.Restart();
    }

    public void UpdateEnd()
    {
        if (!Visible || _pauseUpdateMetrics)
            return;

        _updateStopwatch.Stop();

        _cpuUsage = CalculateCPUUsage();
        _memoryFootprint = GetMemoryUsage();

        lock (_updateLock)
        {
            _updateFrameTimes.Append((float)_updateStopwatch.Elapsed.TotalMilliseconds);
        }
        lock (_memoryLock)
        {
            _memoryUsage.Append(_memoryFootprint);
        }
    }

    public void RenderStart(float dt)
    {
        if (!Visible)
            return;
        _renderTimer += dt;

        if (_pauseRenderMetrics = _renderTimer < _updateRate)
            return;
        _renderTimer = 0.0f;

        Monitor.Enter(_renderLock);
        _renderStopwatch.Restart();
    }

    public void RenderEnd()
    {
        if (!Visible || _pauseRenderMetrics)
            return;

        _renderStopwatch.Stop();
        lock (_renderLock)
        {
            _renderFrameTimes.Append((float)_renderStopwatch.Elapsed.TotalMilliseconds);
        }
    }

    public override void Draw(float dt, RenderOptions? options = null)
    {
        if (!Visible)
            return;

        _frameTimers.Append(dt);

        ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.FirstUseEver);

        if (ImGui.Begin(Name))
        {
            float maxUpdateFrameTime,
                maxRenderFrameTime,
                maxMemoryUsage;
            lock (_updateLock)
                maxUpdateFrameTime = _updateFrameTimes.Buffer.Max();
            lock (_renderLock)
                maxRenderFrameTime = _renderFrameTimes.Buffer.Max();
            lock (_memoryLock)
                maxMemoryUsage = _memoryUsage.Buffer.Max();

            ImGui.Text($"Max Update Frame Time: {maxUpdateFrameTime:0.00} ms");
            ImGui.Text($"Max Render Frame Time: {maxRenderFrameTime:0.00} ms");
            ImGui.Text($"CPU Usage: {_cpuUsage:0.00}%");
            ImGui.Text($"Memory Usage: {_memoryFootprint:0.00} MB");

            ImGui.Text($"{1.0f / _frameTimers.Buffer.Average():0}FPS");

            PlotFrameTimes("Update Frame Times", _updateFrameTimes, maxUpdateFrameTime);
            PlotFrameTimes("Render Frame Times", _renderFrameTimes, maxRenderFrameTime);
            PlotFrameTimes("Memory Usage", _memoryUsage, maxMemoryUsage, "MB");

            ImGui.End();
        }
    }

    [Pure]
    private static void PlotFrameTimes(
        string label,
        LinearBuffer<float> frameTimes,
        float maxValue,
        string unit = "ms"
    )
    {
        float windowWidth = ImGui.GetContentRegionAvail().X;
        float averageFrameTime = frameTimes.Buffer.Average();

        ImGui.PlotLines(
            "",
            ref frameTimes.Buffer[0],
            frameTimes.Length,
            frameTimes.Index,
            $"{label} - avg: {averageFrameTime:0.00} {unit}",
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

    public override void Update(float dt) { }
}
