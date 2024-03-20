namespace VoxelExplorer.Data
{
    /// <summary>
    /// Helper class for encoding and decoding voxel data.
    /// </summary>
    internal abstract class VoxelCodec
    {
        private const uint ID_MASK = 0b1111;

        public static byte GetID(in Voxel voxel)
        {
            return (byte)(voxel.DataPack0 & ID_MASK);
        }
    }
}
