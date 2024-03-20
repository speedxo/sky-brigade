namespace VoxelExplorer.Data
{
    /// <summary>
    /// Basic chunk storage for testing purposes, n^3 spacial complexity.
    /// </summary>
    internal class BasicChunkDataProvider : IChunkDataProvider
    {
        public int Width { get; init; }
        public int Height { get; init; }
        public int Depth { get; init; }

        private readonly Voxel[] array;

        public Voxel this[in int x, in int y, in int z]
        {
            get
            {
                int index = x + Width * (y + Depth * z);
                if (index >= array.Length || index < 0)
                    return Voxel.Empty;
                return array[index];
            }
            set
            {
                int index = x + Width * (y + Depth * z);
                if (index >= array.Length || index < 0)
                    return;
                array[index] = value;
            }
        }

        public BasicChunkDataProvider(in int width, in int height, in int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;

            array = new Voxel[Width * Depth * Height];
        }

        public void Populate(Func<int, int, int, Voxel>? generator = null)
        {
            if (generator is null)
            {
                Array.Fill(array, new Voxel());
                return;
            }

            for (int i = 0; i < Width * Height * Depth; i++)
            {
                int idx = i;
                int z = idx / (Width * Height);
                idx -= (z * Width * Height);
                int y = idx / Width;
                int x = idx % Width;

                array[i] = generator(x, y, z);
            }
        }
    }
}
