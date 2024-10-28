using NUnit.Framework;
using DvhProcessing;
using System.Collections.Generic;

namespace DvhProcessing.Tests
{
    [TestFixture]
    public class DvhDataTests
    {
        [Test]
        public void CalculateDvh_ShouldComputeDvh()
        {
            var doseValues = new List<double> { 1.0, 2.0, 2.0, 3.0, 4.0, 5.0 };
            var dvhData = new DvhData("TestStructure", doseValues, new List<double>());

            dvhData.CalculateDvh();

            Assert.That(dvhData.DifferentialDvh, Is.Not.Null);
            Assert.That(dvhData.CumulativeDvh, Is.Not.Null);

            // Since number of bins is hardcoded to 100
            Assert.That(dvhData.DifferentialDvh.Count, Is.EqualTo(100));
            Assert.That(dvhData.CumulativeDvh.Count, Is.EqualTo(100));
        }
    }
}
