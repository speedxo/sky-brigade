using Horizon.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Rendering.Spriting
{
    public interface I2DBatchedRenderer<T> : IDrawable
    {
        /// <summary>
        /// Commits an object to be rendered.
        /// </summary>
        public void Add(T input);
    }
}
