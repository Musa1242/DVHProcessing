using NUnit.Framework;
using DvhProcessing;
using System.IO;
using System.Linq;

namespace DvhProcessing.Tests
{
    [TestFixture]
    public class DicomReaderTests
    {
        private DicomReader reader;

        [SetUp]
        public void Setup()
        {
            reader = new DicomReader();
        }

        [Test]
        public void Read_ShouldProcessDicomFiles()
        {
            // Change directory according to local path
            string testDirectory = @"C:\Users\mtu10\source\repos\GroceryShop\TestCases\Pca1";

            reader.Read(testDirectory);

            var dvhDataList = reader.GetDvhData();

            Assert.That(dvhDataList, Is.Not.Null);
            Assert.That(dvhDataList.Count, Is.GreaterThan(0));
        }

        [Test]
        public void GetDvhData_ShouldReturnDvhData()
        {
            string testDirectory = @"C:\Users\mtu10\source\repos\GroceryShop\TestCases\Pca1";

            reader.Read(testDirectory);

            var dvhDataList = reader.GetDvhData();

            Assert.That(dvhDataList, Is.Not.Null);
            Assert.That(dvhDataList.All(d => d.CumulativeDvh != null && d.CumulativeDvh.Count > 0), Is.True);
        }
    }
}
