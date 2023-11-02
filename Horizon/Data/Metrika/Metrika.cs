using Horizon.Collections;
using Horizon.GameEntity;
using System.Diagnostics;

namespace Horizon.Data;

/// <summary>
/// A convience class to very quickly capture basic realtime metrics, integrating the ECS.
/// </summary>
public class Metrika : Entity, IDisposable
{
    private const int AVG_LEN = 50;

    public readonly Dictionary<string, Dictionary<string, LinearBuffer<double>>> Categories = new();

    public Dictionary<string, LinearBuffer<double>> this[string category] => Categories[category];

    /// <summary>
    /// You are not required to use this method unless you want to ensure runtime safety.
    /// </summary>
    /// <param name="name"></param>
    public void CreateCategory(in string name)
    {
        Categories.TryAdd(name, new Dictionary<string, LinearBuffer<double>>());
    }

    public void TimeAndTrackMethod(in Action action, in string category, in string name)
    {
        AddCustom(category, name, TimeMethod(action));
    }

    public double TimeMethod(in Action action)
    {
        var startTime = Stopwatch.GetTimestamp();
        action?.Invoke();
        var endTime = Stopwatch.GetTimestamp();
        return (double)(endTime - startTime) / Stopwatch.Frequency;
    }

    public void AddCustom(in string category, in string name, in double time)
    {
        Categories.TryAdd(category, new Dictionary<string, LinearBuffer<double>>());
        Categories[category].TryAdd(name, new LinearBuffer<double>(AVG_LEN));
        Categories[category][name].Append(time);
    }

    public void Aggregate(in string category, in string name, in double time)
    {
        CreateCategory(category);
        Categories[category].TryAdd(name, new LinearBuffer<double>(AVG_LEN));

        Categories[category][name].Append(time);
    }

    public void AddCustom(in string category, in string name, in long start, in long end)
    {
        double time = (double)(end - start) / Stopwatch.Frequency;

        Categories.TryAdd(category, new Dictionary<string, LinearBuffer<double>>());
        Categories[category].TryAdd(name, new LinearBuffer<double>(AVG_LEN));
        Categories[category][name].Append(time);
    }

    public void ResetMetrics()
    {
        foreach (var pair in Categories)
        {
            foreach (var pair2 in Categories[pair.Key])
            {
                Categories[pair.Key][pair2.Key].Clear();
            }
        }
    }

    public void Dispose() { }
}
