using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Data
{
    /// <summary>
    /// A convience class to very quickly capture basic realtime metrics, integrating the ECS.
    /// </summary>
    public class Metrika : Entity, IDisposable
    {
        public readonly Dictionary<string, Dictionary<string, double>> Categories = new();

        public Dictionary<string, double> this[string category] => Categories[category];


        /// <summary>
        /// You are not required to use this method unless you want to ensure runtime safety.
        /// </summary>
        /// <param name="name"></param>
        public void CreateCategory(in string name)
        {
            Categories.TryAdd(name, new Dictionary<string, double>());
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
            Categories.TryAdd(category, new Dictionary<string, double>());
            if (!Categories[category].TryAdd(name, time))
                Categories[category][name] = time;
        }

        public void Aggregate(in string category, in string name, in double time)
        {
            CreateCategory(category);
            Categories[category].TryAdd(name, 0.0);

            Categories[category][name] += time;
        }

        public void AddCustom(in string category, in string name, in long start, in long end)
        {
            double time = (double)(end - start) / Stopwatch.Frequency;

            Categories.TryAdd(category, new Dictionary<string, double>());
            if (!Categories[category].TryAdd(name, time))
                Categories[category][name] = time;
        }

        public void ResetMetrics()
        {
            foreach (var pair in Categories)
            {
                foreach (var pair2 in Categories[pair.Key])
                {
                    Categories[pair.Key][pair2.Key] = 0.0;
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
