using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Horizon.Core.Data;

namespace Horizon.Rendering.Mesh;

public static class MeshGenerator
{
    public static (Vertex3D[], uint[]) GenerateSphere(uint divisions, in Vector3 offset)
    {
        List<Vertex3D> vertices = [];
        List<uint> indices = [];

        for (int lat = 0; lat <= divisions; lat++)
        {
            float theta = lat * MathF.PI / divisions;
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);

            for (int lon = 0; lon <= divisions; lon++)
            {
                float phi = lon * 2 * MathF.PI / divisions;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;

                vertices.Add(new Vertex3D(x + offset.X, y + offset.Y, z + offset.Z, tX: (float)lon / divisions, tY: (float)lat / divisions));
            }
        }

        for (uint lat = 0; lat < divisions; lat++)
        {
            for (uint lon = 0; lon < divisions; lon++)
            {
                uint first = (lat * (divisions + 1)) + lon;
                uint second = first + divisions + 1;

                indices.Add(first);
                indices.Add(second);
                indices.Add(first + 1);

                indices.Add(second);
                indices.Add(second + 1);
                indices.Add(first + 1);
            }
        }

        return (vertices.ToArray(), indices.ToArray());
    }
}
