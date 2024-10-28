using NUnit.Framework;
using DvhProcessing;
using System.IO;

namespace DvhProcessing.Tests
{
    [TestFixture]
    public class RtStructureReaderTests
    {
        private RtStructureReader structureReader;
        private DicomReader dicomReader;

        [SetUp]
        public void Setup()
        {
            structureReader = new RtStructureReader();
            dicomReader = new DicomReader();

            // Change it to local path
            string testDirectory = @"C:\Users\mtu10\source\repos\GroceryShop\TestCases\Pca1";

           
            dicomReader.Read(testDirectory);

            var coordinateMapper = dicomReader.CoordinateMapper;
            if (coordinateMapper == null)
            {
                Assert.Fail("CoordinateMapper is not initialized in DicomReader.");
            }

           
            structureReader.SetCoordinateMapper(coordinateMapper);
        }

        [Test]
        public void Read_ShouldProcessRtStructureFile()
        {
            string filePath = @"C:\Users\mtu10\source\repos\GroceryShop\TestCases\Pca1\HF-01-001\RS.HF-01-001.220901_Becken.dcm";

            structureReader.Read(filePath);

            var structures = structureReader.GetStructures();

            Assert.That(structures, Is.Not.Null);
            Assert.That(structures.Count, Is.GreaterThan(0));
        }

        [Test]
        public void GetBinaryMasks_ShouldReturnMasks()
        {
            string filePath = @"C:\Users\mtu10\source\repos\GroceryShop\TestCases\Pca1\HF-01-001\RS.HF-01-001.220901_Becken.dcm";

            structureReader.Read(filePath);

            var masks = structureReader.GetBinaryMasks();

            Assert.That(masks, Is.Not.Null);
            Assert.That(masks.Count, Is.GreaterThan(0));
        }
    }
}
