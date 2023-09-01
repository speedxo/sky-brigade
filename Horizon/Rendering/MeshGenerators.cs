using Horizon.Data;
using System.Numerics;

namespace Horizon.Rendering
{
    public static class MeshGenerators
    {
        // write code to generate a cube
        public static MeshData CreateCube(float size = 1)
        {
            var verts = new Vertex[4 * 6];
            var indices = new uint[6 * 6];

            // front
            verts[0] = (new Vertex(new Vector3(-size, -size, size), new Vector3(0, 0, 1), new Vector2(0, 0)));
            verts[1] = (new Vertex(new Vector3(size, -size, size), new Vector3(0, 0, 1), new Vector2(1, 0)));
            verts[2] = (new Vertex(new Vector3(size, size, size), new Vector3(0, 0, 1), new Vector2(1, 1)));
            verts[3] = (new Vertex(new Vector3(-size, size, size), new Vector3(0, 0, 1), new Vector2(0, 1)));

            // back
            verts[4] = (new Vertex(new Vector3(-size, -size, -size), new Vector3(0, 0, -1), new Vector2(0, 0)));
            verts[5] = (new Vertex(new Vector3(size, -size, -size), new Vector3(0, 0, -1), new Vector2(1, 0)));
            verts[6] = (new Vertex(new Vector3(size, size, -size), new Vector3(0, 0, -1), new Vector2(1, 1)));
            verts[7] = (new Vertex(new Vector3(-size, size, -size), new Vector3(0, 0, -1), new Vector2(0, 1)));

            // left
            verts[8] = (new Vertex(new Vector3(-size, -size, -size), new Vector3(-1, 0, 0), new Vector2(0, 0)));
            verts[9] = (new Vertex(new Vector3(-size, -size, size), new Vector3(-1, 0, 0), new Vector2(1, 0)));
            verts[10] = (new Vertex(new Vector3(-size, size, size), new Vector3(-1, 0, 0), new Vector2(1, 1)));
            verts[11] = (new Vertex(new Vector3(-size, size, -size), new Vector3(-1, 0, 0), new Vector2(0, 1)));

            // right
            verts[12] = (new Vertex(new Vector3(size, -size, -size), new Vector3(1, 0, 0), new Vector2(0, 0)));
            verts[13] = (new Vertex(new Vector3(size, -size, size), new Vector3(1, 0, 0), new Vector2(1, 0)));
            verts[14] = (new Vertex(new Vector3(size, size, size), new Vector3(1, 0, 0), new Vector2(1, 1)));
            verts[15] = (new Vertex(new Vector3(size, size, -size), new Vector3(1, 0, 0), new Vector2(0, 1)));

            // top
            verts[16] = (new Vertex(new Vector3(-size, size, -size), new Vector3(0, 1, 0), new Vector2(0, 0)));
            verts[17] = (new Vertex(new Vector3(size, size, -size), new Vector3(0, 1, 0), new Vector2(1, 0)));
            verts[18] = (new Vertex(new Vector3(size, size, size), new Vector3(0, 1, 0), new Vector2(1, 1)));
            verts[19] = (new Vertex(new Vector3(-size, size, size), new Vector3(0, 1, 0), new Vector2(0, 1)));

            // bottom
            verts[20] = (new Vertex(new Vector3(-size, -size, -size), new Vector3(0, -1, 0), new Vector2(0, 0)));
            verts[21] = (new Vertex(new Vector3(size, -size, -size), new Vector3(0, -1, 0), new Vector2(1, 0)));
            verts[22] = (new Vertex(new Vector3(size, -size, size), new Vector3(0, -1, 0), new Vector2(1, 1)));
            verts[23] = (new Vertex(new Vector3(-size, -size, size), new Vector3(0, -1, 0), new Vector2(0, 1)));

            // generate indices
            for (int i = 0; i < 6; i++)
            {
                indices[i * 6 + 0] = ((uint)(i * 4));
                indices[i * 6 + 1] = ((uint)(i * 4 + 1));
                indices[i * 6 + 2] = ((uint)(i * 4 + 2));

                indices[i * 6 + 3] = ((uint)(i * 4));
                indices[i * 6 + 4] = ((uint)(i * 4 + 2));
                indices[i * 6 + 5] = ((uint)(i * 4 + 3));
            }

            return new MeshData
            {
                Vertices = verts,
                Elements = indices
            };
        }

        public static MeshData CreateRectangle()
        {
            return new MeshData
            {
                Vertices = new Vertex[] {
                    new Vertex(-1, -1, 0, 0, 0),
                    new Vertex(1, -1, 0, 1, 0),
                    new Vertex(1, 1, 0, 1, 1),
                    new Vertex(-1, 1, 0, 0, 1)
                    },
                Elements = new uint[] {
                    0, 1, 3,
                    1, 2, 3
                }
            };
        }

        // creates a sphere where the vertices are evenly spaced out, normals and texcoords are generated.
        public static MeshData CreateSphere(float radius = 1.0f, int vertexCount = 10)
        {
            int vertexCountSquared = vertexCount * vertexCount;
            Memory<Vertex> verts = new Vertex[vertexCountSquared];
            Memory<uint> indices = new uint[(vertexCount - 1) * (vertexCount - 1) * 6];

            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    // Calculate the position of the vertex
                    float x = (float)Math.Sin(Math.PI * i / (vertexCount - 1)) * (float)Math.Cos(2 * Math.PI * j / (vertexCount - 1));
                    float y = (float)Math.Cos(Math.PI * i / (vertexCount - 1));
                    float z = (float)Math.Sin(Math.PI * i / (vertexCount - 1)) * (float)Math.Sin(2 * Math.PI * j / (vertexCount - 1));

                    // Add the vertex to the memory region of vertices
                    verts.Span[i * vertexCount + j] = new Vertex(new Vector3(x, y, z) * radius, new Vector3(x, y, z), new Vector2((float)j / (vertexCount - 1), (float)i / (vertexCount - 1)));
                }
            }

            // Add the indices for the triangles to the memory region of indices
            int index = 0;
            for (int i = 0; i < vertexCount - 1; i++)
            {
                for (int j = 0; j < vertexCount - 1; j++)
                {
                    indices.Span[index++] = (uint)(i * vertexCount + j);
                    indices.Span[index++] = (uint)(i * vertexCount + j + 1);
                    indices.Span[index++] = (uint)((i + 1) * vertexCount + j);

                    indices.Span[index++] = (uint)(i * vertexCount + j + 1);
                    indices.Span[index++] = (uint)((i + 1) * vertexCount + j + 1);
                    indices.Span[index++] = (uint)((i + 1) * vertexCount + j);
                }
            }
            return new MeshData
            {
                Vertices = verts,
                Elements = indices
            };
        }

        //// dont even bother
        //public static Mesh FromObj(string path)
        //{
        //    var lines = File.ReadAllLines(path);

        //    List<Vertex> verts = new List<Vertex>();
        //    List<Vector3> colors = new List<Vector3>();
        //    List<Vector2> texs = new List<Vector2>();
        //    List<Tuple<int, int, int>> faces = new List<Tuple<int, int, int>>();

        //    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        //    ci.NumberFormat.CurrencyDecimalSeparator = ".";

        //    foreach (string line in lines)
        //    {
        //        if (line.StartsWith("v "))
        //        {
        //            // Cut off beginning of line!!! damnit
        //            string temp = line.Substring(2);
        //            float x = 0, y = 0, z = 0;

        //            if (temp.Count((char c) => c == ' ') == 2)
        //            {
        //                string[] vertparts = temp.Split(' ');

        //                x = (float)double.Parse(vertparts[0].Trim(), NumberStyles.Any, ci);
        //                y = (float)double.Parse(vertparts[1].Trim(), NumberStyles.Any, ci);
        //                z = (float)double.Parse(vertparts[2].Trim(), NumberStyles.Any, ci);
        //            }

        //            verts.Add(new Vertex(new Vector3(x, y, z), Vector3.Zero, new Vector2((float)Math.Sin(x), (float)Math.Sin(z))));
        //        }
        //        else if (line.StartsWith("f "))
        //        {
        //            string temp = line.Substring(2);

        //            Tuple<int, int, int> face = new Tuple<int, int, int>(0, 0, 0);

        //            if (temp.Count((char c) => c == ' ') == 2)
        //            {
        //                string[] faceparts = temp.Split(' ');

        //                int i1, i2, i3;

        //                bool success = int.TryParse(faceparts[0], out i1);
        //                success &= int.TryParse(faceparts[1], out i2);
        //                success &= int.TryParse(faceparts[2], out i3);

        //                if (!success)
        //                {
        //                    GameManager.Instance.Logger.Log(Logging.LogLevel.Error, $"Error parsing face: {line}");
        //                }
        //                else
        //                {
        //                    face = new Tuple<int, int, int>(i1 - 1, i2 - 1, i3 - 1);
        //                    faces.Add(face);
        //                }
        //            }
        //        }
        //        else if (line.StartsWith("vt "))
        //        {
        //            string temp = line.Substring(3);

        //            float x = 0, y = 0;

        //            if (temp.Count((char c) => c == ' ') == 1)
        //            {
        //                string[] texparts = temp.Split(' ');

        //                x = (float)double.Parse(texparts[0].Trim(), NumberStyles.Any, ci);
        //                y = (float)double.Parse(texparts[1].Trim(), NumberStyles.Any, ci);
        //            }

        //            texs.Add(new Vector2(x, y));
        //        }
        //    }

        //    List<uint> indices = new List<uint>();
        //    if (texs.Count > 0)
        //        for (int i = 0; i < verts.Count; i++)
        //            verts[i] = new Vertex(verts[i].Position, verts[i].Normal, texs[i]);

        //    foreach (var face in faces)
        //    {
        //        indices.Add((uint)face.Item1);
        //        indices.Add((uint)face.Item2);
        //        indices.Add((uint)face.Item3);
        //    }

        //    return FromLoaderFunction(() =>
        //    {
        //        return (verts.ToArray(), indices.ToArray());
        //    });
        //}
    }
}