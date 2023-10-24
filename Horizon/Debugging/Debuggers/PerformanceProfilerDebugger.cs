using Horizon.Collections;
using Horizon.Rendering;
using ImGuiNET;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Security.Cryptography;
using Horizon.GameEntity;
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

        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        Entity.Engine.OnPreDraw += RenderStart;
        Entity.Engine.OnPostDraw += RenderEnd;
        Entity.Engine.OnPreUpdate += UpdateStart;
        Entity.Engine.OnPostUpdate += UpdateEnd;
    }

    protected void UpdateStart(float dt)
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

    protected  void UpdateEnd(float dt)
    {
        if (!Visible || _pauseUpdateMetrics)
            return;

        _updateStopwatch.Stop();

        _cpuUsage = CalculateCpuUsage();
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

    protected  void RenderStart(float dt, ref RenderOptions options)
    {
        if (!Visible)
            return;
        _renderTimer += dt;
        _pauseRenderMetrics = _renderTimer < _updateRate;
        if (_pauseRenderMetrics)
            return;
        _renderTimer = 0.0f;

        Monitor.Enter(_renderLock);
        _renderStopwatch.Restart();
    }

    protected  void RenderEnd(float dt, ref RenderOptions options)
    {
        if (!Visible || _pauseRenderMetrics)
            return;

        _renderStopwatch.Stop();
        lock (_renderLock)
        {
            _renderFrameTimes.Append((float)_renderStopwatch.Elapsed.TotalMilliseconds);
        }
    }

    public override void Draw(float dt, ref RenderOptions options)
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

            PlotValues("Update Frame Times", _updateFrameTimes, maxUpdateFrameTime);
            PlotValues("Render Frame Times", _renderFrameTimes, maxRenderFrameTime);
            PlotValues("Memory Usage", _memoryUsage, maxMemoryUsage, "MB");

            ImGui.End();
        }
    }

    /// <summary>
    /// Clean up via unsubscribing performance profiling events.
    /// </summary>
    public override void Dispose()
    {
        Entity.Engine.OnPreDraw -= RenderStart;
        Entity.Engine.OnPostDraw -= RenderEnd;
        Entity.Engine.OnPreUpdate -= UpdateStart;
        Entity.Engine.OnPostUpdate -= UpdateEnd;
    }

    [Pure]
    private static void PlotValues(
        in string label,
        in LinearBuffer<float> frameTimes,
        in float maxValue,
        in string unit = "ms"
    )
    {
        var windowWidth = ImGui.GetContentRegionAvail().X;
        var averageFrameTime = frameTimes.Buffer.Average();
        var minFrameTime = frameTimes.Buffer.Min();
        var maxFrameTime = frameTimes.Buffer.Max();

        ImGui.PlotLines(
            "",
            ref frameTimes.Buffer[0],
            frameTimes.Length,
            frameTimes.Index,
            $"{label} - Avg: {averageFrameTime:0.00} {unit} - Min: {minFrameTime:0.00} {unit} - Max: {maxFrameTime:0.00} {unit}",
            0.0f,
            maxValue * 1.2f,
            new Vector2(windowWidth, 80)
        );
    }



    private float CalculateCpuUsage()
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
