using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AutoVoxel.Data;

public class ChunkOctree
{
    private ChunkOctreeNode root;

    public ChunkOctree()
    {
        root = new ChunkOctreeNode(new BoundingBox(Vector3.Zero, new Vector3(32, 64, 32)));
    }

    public void Insert(Vector3 position, Tile data)
    {
        root.Insert(position, data);
    }

    public Tile Get(Vector3 position)
    {
        return root.Get(position);
    }
}
