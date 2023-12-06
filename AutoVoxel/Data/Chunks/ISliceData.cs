using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoVoxel.Data.Chunks;

public interface ISliceData
{
    public Tile this[int x, int y, int z] { get; set; }
    public Tile this[int index] { get; set; }
}
