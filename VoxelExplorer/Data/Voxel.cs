namespace VoxelExplorer.Data
{
    public struct Voxel
    {
        public uint DataPack0 { get; init; }

        /*
         *  The CPU side data is encoded in a single UINT.   
         * ┌────────────────────────────────────────────┐
         * │                                    (0 - 4) │
         * │                                    ID      │
         * │                                     │      │
         * │                                  ┌──▼─┐    │
         * │     0000 0000 0000 0000 0000 0000│0000│    │
         * │                                  └────┘    │
         * │                                            │
         * │                                            │
         * │                                            │
         * └────────────────────────────────────────────┘
         */

        public Voxel(in byte id)
        {
            DataPack0 = id;
        }

        public static Voxel Empty { get; } = new Voxel(0);
    }
}
