using NUnit.Framework;
using DvhProcessing;
using System.Collections.Generic;

namespace DvhProcessing.Tests
{
    [TestFixture]
    public class StructureDataTests
    {
        [Test]
        public void AddVoxelIndex_ShouldAddIndices()
        {
            var structureData = new StructureData("TestStructure");

            structureData.AddVoxelIndex(1, 2, 3);
            structureData.AddVoxelIndex(4, 5, 6);

            Assert.That(structureData.VoxelIndices.Count, Is.EqualTo(2));

            var expectedIndices = new List<(int x, int y, int z)>
            {
                (1, 2, 3),
                (4, 5, 6)
            };

            Assert.That(structureData.VoxelIndices, Is.EquivalentTo(expectedIndices));
        }
    }
}
