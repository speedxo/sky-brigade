namespace VoxelExplorer.Data
{
    /// <summary>
    /// An abstraction allowing for the use of multiple chunk data storage providers, such as layered data for complex chunks, or single voxel types for ambigious chunks.
    /// </summary>
    internal interface IChunkDataProvider
    {
        Voxel this[in int x, in int y, in int z] { get; set; }
        void Populate(Func<int, int, int, Voxel>? generator = null);
    }
}
