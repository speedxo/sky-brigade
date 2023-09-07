using System.Drawing;
using System.Numerics;

namespace Gravitator2D;

public class QuadTree
{
    private const int MaxObjectsPerNode = 4; // Maximum number of bodies in a single quad
    private const int MaxDepth = 5; // Maximum depth of the quadtree

    private readonly int depth;
    private readonly RectangleF bounds;
    private readonly List<Body> bodies;
    private readonly QuadTree[] nodes;

    public QuadTree(int depth, RectangleF bounds)
    {
        this.depth = depth;
        this.bounds = bounds;
        bodies = new List<Body>();
        nodes = new QuadTree[4];
    }

    public List<Body> GetBodiesInBoundary(RectangleF boundary)
    {
        List<Body> result = new List<Body>();
        GetBodiesInBoundaryRecursive(boundary, result);
        return result;
    }

    private void GetBodiesInBoundaryRecursive(RectangleF boundary, List<Body> result)
    {
        if (!bounds.IntersectsWith(boundary))
            return;

        foreach (var body in bodies)
        {
            if (boundary.Contains(body.Position.X, body.Position.Y))
                result.Add(body);
        }

        if (nodes[0] != null)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].GetBodiesInBoundaryRecursive(boundary, result);
            }
        }
    }

    public void Insert(Body body)
    {
        if (nodes[0] != null)
        {
            int index = GetQuadrantIndex(body.Position);
            if (index != -1)
            {
                nodes[index].Insert(body);
                return;
            }
        }

        bodies.Add(body);

        if (bodies.Count > MaxObjectsPerNode && depth < MaxDepth)
        {
            if (nodes[0] == null)
                Split();

            int i = 0;
            while (i < bodies.Count)
            {
                int index = GetQuadrantIndex(bodies[i].Position);
                if (index != -1)
                {
                    nodes[index].Insert(bodies[i]);
                    bodies.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    private void Split()
    {
        float subWidth = bounds.Width / 2;
        float subHeight = bounds.Height / 2;
        float x = bounds.X;
        float y = bounds.Y;

        nodes[0] = new QuadTree(depth + 1, new RectangleF(x + subWidth, y, subWidth, subHeight));
        nodes[1] = new QuadTree(depth + 1, new RectangleF(x, y, subWidth, subHeight));
        nodes[2] = new QuadTree(depth + 1, new RectangleF(x, y + subHeight, subWidth, subHeight));
        nodes[3] = new QuadTree(
            depth + 1,
            new RectangleF(x + subWidth, y + subHeight, subWidth, subHeight)
        );
    }

    private int GetQuadrantIndex(Vector2 point)
    {
        int index = -1;
        double verticalMidpoint = bounds.X + bounds.Width / 2;
        double horizontalMidpoint = bounds.Y + bounds.Height / 2;

        bool topQuadrant = point.Y < horizontalMidpoint;
        bool bottomQuadrant = point.Y >= horizontalMidpoint;
        bool leftQuadrant = point.X < verticalMidpoint;
        bool rightQuadrant = point.X >= verticalMidpoint;

        if (leftQuadrant)
        {
            if (topQuadrant)
                index = 1;
            else if (bottomQuadrant)
                index = 2;
        }
        else if (rightQuadrant)
        {
            if (topQuadrant)
                index = 0;
            else if (bottomQuadrant)
                index = 3;
        }

        return index;
    }
}
