using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bogz.Metrika
{
    public abstract class MetricFactory
    {
        /// <summary>
        /// Internal array of stored metrics.
        /// </summary>
        public Dictionary<string, Metric> Metrics { get; init; } = new();

        /// <summary>
        /// Stores and tracks a changing value.
        /// </summary>
        /// <returns>Returns true if the metric was sucessfully commited to monitoring.</returns>
        public bool RegisterMetric(in string name, in Func<object> observe)
            => Metrics.TryAdd(name, new Metric(name, observe));
        
        /// <summary>
        /// Removes a metric.
        /// </summary>
        /// <returns>Returns true if the metric was sucessfully removed.</returns>
        public bool RemoveMetric(in string name)
            => Metrics.Remove(name);
    }
}
