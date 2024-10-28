
using System.Collections.Generic;

namespace DvhProcessing
{
    public class StructureData
    {
        public string StructureName { get; set; }
        public List<(int x, int y, int z)> VoxelIndices { get; set; }

        public StructureData(string structureName)
        {
            StructureName = structureName;
            VoxelIndices = new List<(int x, int y, int z)>();
        }

        public void AddVoxelIndex(int x, int y, int z)
        {
            VoxelIndices.Add((x, y, z));
        }
    }
}
