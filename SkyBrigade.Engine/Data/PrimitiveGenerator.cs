using System;
using System.Numerics;

namespace SkyBrigade.Engine.Data;

public class PrimitiveGenerator
{
	public PrimitiveGenerator()
	{
	}
}

public class SphereGenerator
{
    public static void GenerateSphere(float radius, uint longitudeSegments, uint latitudeSegments, out Vertex[] vertices, out uint[] indices)
    {
        List<Vertex> vertexList = new List<Vertex>();
        List<uint> indexList = new List<uint>();

        float phiStep = 2 * MathF.PI / longitudeSegments;
        float thetaStep = MathF.PI / latitudeSegments;

        for (int lat = 0; lat <= latitudeSegments; lat++)
        {
            float theta = lat * thetaStep;
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);

            for (int lon = 0; lon <= longitudeSegments; lon++)
            {
                float phi = lon * phiStep;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;
                float u = 1 - (lon / (float)longitudeSegments);
                float v = 1 - (lat / (float)latitudeSegments);

                Vector3 position = radius * new Vector3(x, y, z);
                Vector3 normal = Vector3.Normalize(position);
                Vector2 texCoords = new Vector2(u, v);

                vertexList.Add(new Vertex(position, normal, texCoords));
            }
        }

        for (uint lat = 0; lat < latitudeSegments; lat++)
        {
            for (uint lon = 0; lon < longitudeSegments; lon++)
            {
                uint first = (lat * (longitudeSegments + 1)) + lon;
                uint second = first + longitudeSegments + 1;

                indexList.Add(first);
                indexList.Add(second);
                indexList.Add(first + 1);

                indexList.Add(second);
                indexList.Add(second + 1);
                indexList.Add(first + 1);
            }
        }

        vertices = vertexList.ToArray();
        indices = indexList.ToArray();
    }
}       