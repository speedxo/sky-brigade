using System.Numerics;

using Bogz.Logging.Loggers;

namespace AutoVoxel.Data;

public class ChunkOctreeNode
{
    private BoundingBox bounds;
    private Tile data;
    private bool isLeaf;
    private ChunkOctreeNode[] children;

    public ChunkOctreeNode(BoundingBox bounds)
    {
        this.bounds = bounds;
        isLeaf = true;
        children = new ChunkOctreeNode[8];
    }

    public void Insert(Vector3 position, Tile data)
    {
        if (!bounds.Contains(position))
        {
            //ConcurrentLogger.Instance.Log(Bogz.Logging.LogLevel.Error, $"{position }");
            //throw new ArgumentException("Position is outside the bounds of this node.");
        }

        if (isLeaf)
        {
            if (this.data.ID == TileID.Air)
            {
                this.data = data;
            }
            else
            {
                Subdivide();
                Insert(position, data);
            }
        }
        else
        {
            int index = GetChildIndex(position);
            children[index].Insert(position, data);
        }
    }

    public Tile Get(Vector3 position)
    {
        if (!bounds.Contains(position))
        {
            return default;
        }

        if (isLeaf)
        {
            return data;
        }
        else
        {
            int index = GetChildIndex(position);
            return children[index].Get(position);
        }
    }

    private void Subdivide()
    {
        Vector3 min = bounds.Min;
        Vector3 max = bounds.Max;
        Vector3 center = (min + max) / 2f;

        children[0] = new ChunkOctreeNode(new BoundingBox(min, center));
        children[1] = new ChunkOctreeNode(new BoundingBox(new Vector3(center.X, min.Y, min.Z), new Vector3(max.X, center.Y, center.Z)));
        children[2] = new ChunkOctreeNode(new BoundingBox(new Vector3(center.X, min.Y, center.Z), new Vector3(max.X, center.Y, max.Z)));
        children[3] = new ChunkOctreeNode(new BoundingBox(new Vector3(min.X, min.Y, center.Z), new Vector3(center.X, center.Y, max.Z)));
        children[4] = new ChunkOctreeNode(new BoundingBox(new Vector3(min.X, center.Y, min.Z), new Vector3(center.X, max.Y, center.Z)));
        children[5] = new ChunkOctreeNode(new BoundingBox(new Vector3(center.X, center.Y, min.Z), new Vector3(max.X, max.Y, center.Z)));
        children[6] = new ChunkOctreeNode(new BoundingBox(center, max));
        children[7] = new ChunkOctreeNode(new BoundingBox(new Vector3(min.X, center.Y, center.Z), new Vector3(center.X, max.Y, max.Z)));

        isLeaf = false;
    }

    private int GetChildIndex(Vector3 position)
    {
        int index = 0;
        if (position.X > bounds.Center.X)
            index |= 1;
        if (position.Y > bounds.Center.Y)
            index |= 2;
        if (position.Z > bounds.Center.Z)
            index |= 4;
        return index;
    }
}