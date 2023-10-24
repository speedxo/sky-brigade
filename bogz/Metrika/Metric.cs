using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bogz.Metrika
{
    public readonly struct Metric
    {
        public readonly string Name { get; init; }
        public readonly Func<object> Observe { get; init; }
        
        public Metric(in string name, in Func<object> observe)
        {
            this.Name = name;
            this.Observe = observe;
        }
    }
}
